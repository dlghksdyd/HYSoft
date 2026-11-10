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
    /// <remarks>
    /// <para><b>타임아웃</b>: <see cref="TcpClientOptions.SendTimeout"/>·<see cref="TcpClientOptions.ReceiveTimeout"/> 은
    /// «유휴» 시간이다 — 소켓 호출 한 번이 그 시간 안에 한 바이트도 진척이 없으면 끝낸다. 진척이 있으면 다시 잰다.
    /// <see cref="Timeout.InfiniteTimeSpan"/> 이면 무기한 기다린다.</para>
    /// <para><b>타임아웃·소켓 오류·상대 종료 뒤에는 연결을 버린다</b>(fail-closed). 일부만 보내거나 읽은 채 끝났을 수
    /// 있어 같은 소켓을 계속 쓰면 프레임이 어긋난다. 이후 Send/Receive 는 <see cref="InvalidOperationException"/> 이고,
    /// <see cref="ConnectAsync"/> 로 새로 연결한다.</para>
    /// </remarks>
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
                CloseSocket(sock);
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

                    InvalidateSocket(existing);
                }

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = _options.NoDelay,
                    ReceiveBufferSize = _options.ReceiveBufferSize,
                    SendBufferSize = _options.SendBufferSize,
                };

                using (var timeoutCts = CreateTimeout(_options.ConnectTimeout))
                {
                    try
                    {
                        await socket.ConnectAsync(new IPEndPoint(_options.Host, _options.Port), timeoutCts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
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

                if (IsDisposed)
                {
                    socket.Dispose();
                    ThrowIfDisposed();
                }

                var old = Interlocked.Exchange(ref _socket, socket);
                if (old != null && !ReferenceEquals(old, socket))
                {
                    CloseSocket(old);
                }
            }
            finally
            {
                ReleaseQuietly(_connectLock);
            }
        }

        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (count == 0) return;

            var socket = RequireConnectedSocket();

            await _sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
                int totalSent = 0;
                while (totalSent < count)
                {
                    ThrowIfDisposed();
                    var chunk = new ReadOnlyMemory<byte>(buffer, offset + totalSent, count - totalSent);
                    int sent = await RunWithIdleTimeoutAsync(
                        socket, _options.SendTimeout, "Send",
                        token => socket.SendAsync(chunk, SocketFlags.None, token)).ConfigureAwait(false);

                    if (sent <= 0)
                    {
                        InvalidateSocket(socket);
                        throw new SocketException((int)SocketError.ConnectionReset);
                    }

                    totalSent += sent;
                }
            }
            finally
            {
                ReleaseQuietly(_sendLock);
            }
        }

        public Task SendAsync(byte[] buffer)
            => SendAsync(buffer, 0, buffer.Length);

        public async Task ReceiveAsync(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (count == 0) return;

            var socket = RequireConnectedSocket();

            await _receiveLock.WaitAsync().ConfigureAwait(false);
            try
            {
                int totalRead = 0;
                while (totalRead < count)
                {
                    ThrowIfDisposed();
                    var chunk = new Memory<byte>(buffer, offset + totalRead, count - totalRead);
                    int read = await RunWithIdleTimeoutAsync(
                        socket, _options.ReceiveTimeout, "Receive",
                        token => socket.ReceiveAsync(chunk, SocketFlags.None, token)).ConfigureAwait(false);

                    if (read == 0)
                    {
                        // 상대가 연결을 닫았다 — 이 소켓으로는 더 받을 수 없다.
                        InvalidateSocket(socket);
                        throw new SocketException((int)SocketError.ConnectionReset);
                    }

                    totalRead += read;
                }
            }
            finally
            {
                ReleaseQuietly(_receiveLock);
            }
        }

        public Task ReceiveAsync(byte[] buffer)
            => ReceiveAsync(buffer, 0, buffer.Length);

        /// <summary>
        /// 소켓 호출 한 번을 유휴 타임아웃으로 묶는다. 시간 초과·소켓 오류면 연결을 버리고 예외를 던진다.
        /// Send 와 Receive 가 이 한 곳을 쓴다 — 타임아웃 적용 방식이 둘로 갈라지지 않게 한다.
        /// </summary>
        private async Task<int> RunWithIdleTimeoutAsync(
            Socket socket, TimeSpan idleTimeout, string operation, Func<CancellationToken, ValueTask<int>> socketCall)
        {
            using (var timeoutCts = CreateTimeout(idleTimeout))
            {
                try
                {
                    return await socketCall(timeoutCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException oce) when (timeoutCts.IsCancellationRequested)
                {
                    InvalidateSocket(socket);
                    throw new TimeoutException(
                        $"{operation} idle-timed out after {idleTimeout.TotalMilliseconds:F0} ms - the connection was dropped.", oce);
                }
                catch (SocketException)
                {
                    InvalidateSocket(socket);
                    throw;
                }
                catch (ObjectDisposedException ode)
                {
                    throw new InvalidOperationException("The socket is already disposed.", ode);
                }
            }
        }

        private Socket RequireConnectedSocket()
        {
            var socket = _socket ?? throw new InvalidOperationException("The socket is not connected.");
            if (!socket.Connected)
                throw new InvalidOperationException("The socket is not connected.");
            return socket;
        }

        /// <summary>그 소켓이 아직 현재 연결이면 떼어 내 닫는다(이미 바뀐 새 연결은 건드리지 않는다).</summary>
        private void InvalidateSocket(Socket socket)
        {
            if (ReferenceEquals(Interlocked.CompareExchange(ref _socket, null, socket), socket))
            {
                CloseSocket(socket);
            }
        }

        private static void CloseSocket(Socket socket)
        {
            try { socket.Shutdown(SocketShutdown.Both); } catch { }
            try { socket.Dispose(); } catch { }
        }

        private static CancellationTokenSource CreateTimeout(TimeSpan timeout)
            => timeout == Timeout.InfiniteTimeSpan
                ? new CancellationTokenSource()
                : new CancellationTokenSource(timeout);

        /// <summary>Dispose 와 겹쳐 잠금이 이미 해제(Dispose)됐을 수 있다 — 그때는 풀 것이 없다.</summary>
        private static void ReleaseQuietly(SemaphoreSlim gate)
        {
            try { gate.Release(); } catch (ObjectDisposedException) { }
        }
    }
}
