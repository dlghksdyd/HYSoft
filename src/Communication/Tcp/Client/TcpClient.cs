using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Client
{
    /// <summary>
    /// 비동기 TCP 클라이언트 구현 클래스입니다.
    /// </summary>
    public class TcpClient
    {
        public bool IsDisposed { get; private set; } = false;
        public readonly Guid Guid = Guid.NewGuid();

        private readonly TcpClientOptions _options;
        private Socket? _socket;
        private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _receiveLock = new SemaphoreSlim(1, 1);

        private TcpClient(TcpClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        internal static TcpClient CreateInternal(TcpClientOptions options)
            => new TcpClient(options);

        internal void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            var sock = Interlocked.Exchange(ref _socket, null);
            if (sock != null)
            {
                try { sock.Shutdown(SocketShutdown.Both); } catch { }
                try { sock.Dispose(); } catch { }
            }

            try { _sendLock.Dispose(); } catch { }
            try { _receiveLock.Dispose(); } catch { }
            try { _connectLock.Dispose(); } catch { }
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(TcpClient));
        }

        public async Task ConnectAsync()
        {
            await _connectLock.WaitAsync().ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();

                if (_socket != null)
                {
                    var existing = _socket;
                    var looksDead = existing.Poll(0, SelectMode.SelectRead) && existing.Available == 0;
                    if (existing.Connected && !looksDead) return;

                    try { existing.Shutdown(SocketShutdown.Both); } catch { }
                    try { existing.Dispose(); } catch { }
                    _socket = null;
                }

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = _options.NoDelay,
                    ReceiveBufferSize = _options.ReceiveBufferSize,
                    SendBufferSize = _options.SendBufferSize,
                };

                using (var timeoutCts = _options.ConnectTimeout == Timeout.InfiniteTimeSpan
                    ? new CancellationTokenSource()
                    : new CancellationTokenSource(_options.ConnectTimeout))
                {
                    try
                    {
                        ThrowIfDisposed();
                        var connectTask = Task.Factory.FromAsync(
                            socket.BeginConnect(new IPEndPoint(_options.Host, _options.Port), null, null),
                            socket.EndConnect);
                        var delayTask = Task.Delay(_options.ConnectTimeout, timeoutCts.Token);
                        var completed = await Task.WhenAny(connectTask, delayTask).ConfigureAwait(false);
                        if (completed == delayTask)
                        {
                            socket.Dispose();
                            throw new TimeoutException(
                                $"Connect timed out after {_options.ConnectTimeout.TotalMilliseconds:F0} ms to {_options.Host}:{_options.Port}");
                        }
                        await connectTask.ConfigureAwait(false);
                    }
                    catch (TimeoutException) { throw; }
                    catch (OperationCanceledException)
                    {
                        socket.Dispose();
                        throw new TimeoutException(
                            $"Connect timed out after {_options.ConnectTimeout.TotalMilliseconds:F0} ms to {_options.Host}:{_options.Port}");
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }

                var old = Interlocked.Exchange(ref _socket, socket);
                if (old != null && !ReferenceEquals(old, socket))
                {
                    try { old.Shutdown(SocketShutdown.Both); } catch { }
                    try { old.Dispose(); } catch { }
                }
            }
            finally
            {
                _connectLock.Release();
            }
        }

        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (count == 0) return;

            var socket = _socket ?? throw new InvalidOperationException("The socket is not connected.");
            if (!socket.Connected)
                throw new InvalidOperationException("The socket is not connected.");

            await _sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var timeoutCts = new CancellationTokenSource())
                {
                    if (_options.SendTimeout != Timeout.InfiniteTimeSpan)
                        timeoutCts.CancelAfter(_options.SendTimeout);

                    int totalSent = 0;
                    while (totalSent < count)
                    {
                        ThrowIfDisposed();
                        var segment = new ArraySegment<byte>(buffer, offset + totalSent, count - totalSent);
                        int sent = await Task.Factory.FromAsync(
                            socket.BeginSend(segment.Array!, segment.Offset, segment.Count, SocketFlags.None, null, null),
                            socket.EndSend).ConfigureAwait(false);

                        if (sent <= 0)
                            throw new SocketException((int)SocketError.ConnectionReset);

                        totalSent += sent;

                        if (_options.SendTimeout != Timeout.InfiniteTimeSpan)
                            timeoutCts.CancelAfter(_options.SendTimeout);
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                throw new TimeoutException($"Send idle-timed out after {_options.SendTimeout.TotalMilliseconds:F0} ms.", oce);
            }
            catch (ObjectDisposedException ode)
            {
                throw new InvalidOperationException("The socket is already disposed.", ode);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public Task SendAsync(byte[] buffer)
            => SendAsync(buffer, 0, buffer.Length);

        public async Task ReceiveAsync(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (count == 0) return;

            var socket = _socket ?? throw new InvalidOperationException("The socket is not connected.");
            if (!socket.Connected)
                throw new InvalidOperationException("The socket is not connected.");

            await _receiveLock.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var timeoutCts = new CancellationTokenSource())
                {
                    if (_options.ReceiveTimeout != Timeout.InfiniteTimeSpan)
                        timeoutCts.CancelAfter(_options.ReceiveTimeout);

                    int totalRead = 0;
                    while (totalRead < count)
                    {
                        ThrowIfDisposed();
                        int read = await Task.Factory.FromAsync(
                            socket.BeginReceive(buffer, offset + totalRead, count - totalRead, SocketFlags.None, null, null),
                            socket.EndReceive).ConfigureAwait(false);

                        if (read == 0)
                            throw new SocketException((int)SocketError.ConnectionReset);

                        totalRead += read;

                        if (_options.ReceiveTimeout != Timeout.InfiniteTimeSpan)
                            timeoutCts.CancelAfter(_options.ReceiveTimeout);
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                throw new TimeoutException($"Receive idle-timed out after {_options.ReceiveTimeout.TotalMilliseconds:F0} ms.", oce);
            }
            catch (ObjectDisposedException ode)
            {
                throw new InvalidOperationException("The socket is already disposed.", ode);
            }
            finally
            {
                _receiveLock.Release();
            }
        }

        public Task ReceiveAsync(byte[] buffer)
            => ReceiveAsync(buffer, 0, buffer.Length);
    }
}
