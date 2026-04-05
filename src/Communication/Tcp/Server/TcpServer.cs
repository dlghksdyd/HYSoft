#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Server
{
    /// <summary>
    /// 비동기 TCP 서버.
    /// </summary>
    public sealed class TcpServer
    {
        public bool IsDisposed { get; private set; }
        public readonly Guid Guid = Guid.NewGuid();

        private readonly TcpServerOptions _options;
        private Socket? _listener;
        private readonly SemaphoreSlim _startStopLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource? _acceptCts;
        private Task? _acceptTask;

        public int ConnectedCount => _clients.Count;

        public (Guid Id, EndPoint? Remote)[] GetClientsSnapshot()
            => _clients.Values.Select(c => (c.Id, c.RemoteEndPoint)).ToArray();

        private readonly ConcurrentDictionary<Guid, TcpClientContext> _clients = new ConcurrentDictionary<Guid, TcpClientContext>();

        #region Events

        public event TcpDataReceivedEventHandler? DataReceived;

        private void RaiseDataReceived(TcpClientContext ctx, byte[] data)
        {
            try
            {
                var dataCtx = new TcpDataReceivedContext(this, ctx.Id, data);
                DataReceived?.Invoke(dataCtx);
            }
            catch { }
        }

        #endregion

        #region Log

        public Action<Exception, string>? LogError { get; set; }

        private void Log(Exception ex, string msg)
        {
            try { LogError?.Invoke(ex, msg); } catch { }
        }

        #endregion

        private TcpServer(TcpServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            ValidateOptions();
        }

        private void ValidateOptions()
        {
            if (_options.Port < 1 || _options.Port > 65535) throw new ArgumentOutOfRangeException(nameof(_options.Port));
            if (_options.BackLog <= 0) throw new ArgumentOutOfRangeException(nameof(_options.BackLog));
            if (_options.ReceiveBufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(_options.ReceiveBufferSize));
            if (_options.SendBufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(_options.SendBufferSize));
            if (_options.MaxClients <= 0) throw new ArgumentOutOfRangeException(nameof(_options.MaxClients));
        }

        internal static TcpServer CreateInternal(TcpServerOptions options)
            => new TcpServer(options);

        internal void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            try { StopAsync().GetAwaiter().GetResult(); } catch { }
            try { _acceptCts?.Dispose(); } catch { }
            try { _startStopLock.Dispose(); } catch { }

            foreach (var kv in _clients)
            {
                if (_clients.TryRemove(kv.Key, out var ctx))
                {
                    try { ctx.Cts.Cancel(); } catch { }
                    try { ctx.Cts.Dispose(); } catch { }
                    try { ctx.SendLock.Dispose(); } catch { }
                    CloseSocketSafe(ctx.Socket);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(TcpServer));
        }

        public async Task StartAsync()
        {
            await _startStopLock.WaitAsync().ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();
                if (_listener != null) return;

                var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = _options.NoDelay,
                    ReceiveBufferSize = _options.ReceiveBufferSize,
                    SendBufferSize = _options.SendBufferSize,
                };

                try
                {
                    sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    sock.Bind(new IPEndPoint(_options.ListenAddress, _options.Port));
                    sock.Listen(_options.BackLog);

                    _listener = sock;
                    _acceptCts = new CancellationTokenSource();
                    _acceptTask = AcceptLoopAsync(_acceptCts.Token);
                }
                catch
                {
                    try { sock.Dispose(); } catch { }
                    throw;
                }
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        public async Task StopAsync()
        {
            await _startStopLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var listener = _listener;
                if (listener == null) return;

                _acceptCts?.Cancel();
                CloseSocketSafe(listener);
                _listener = null;

                if (_acceptTask != null)
                {
                    try { await _acceptTask.ConfigureAwait(false); }
                    catch { }
                    _acceptTask = null;
                }

                foreach (var kv in _clients)
                {
                    if (_clients.TryRemove(kv.Key, out var ctx))
                        CloseClient(ctx);
                }
            }
            finally
            {
                _startStopLock.Release();
                _acceptCts?.Dispose();
            }
        }

        public bool Disconnect(Guid clientId)
        {
            if (_clients.TryRemove(clientId, out var ctx))
            {
                CloseClient(ctx);
                return true;
            }
            return false;
        }

        public async Task<bool> SendAsync(Guid clientId, byte[] data, CancellationToken ct = default)
        {
            if (data.Length == 0) return true;
            if (!_clients.TryGetValue(clientId, out var ctx)) return false;

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, ctx.Cts.Token))
            {
                var token = linkedCts.Token;
                await ctx.SendLock.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    var sock = ctx.Socket;
                    int offset = 0;
                    while (offset < data.Length)
                    {
                        int sent = await Task.Factory.FromAsync(
                            sock.BeginSend(data, offset, data.Length - offset, SocketFlags.None, null, null),
                            sock.EndSend).ConfigureAwait(false);
                        if (sent <= 0) throw new SocketException((int)SocketError.ConnectionReset);
                        offset += sent;
                    }
                    return true;
                }
                catch (SocketException ex)
                {
                    Log(ex, $"Send socket error ({ex.SocketErrorCode})");
                    try { ctx.Cts.Cancel(); } catch { }
                    return false;
                }
                catch (OperationCanceledException) { return false; }
                catch (Exception ex)
                {
                    Log(ex, "Send unexpected");
                    try { ctx.Cts.Cancel(); } catch { }
                    return false;
                }
                finally
                {
                    ctx.SendLock.Release();
                }
            }
        }

        public async Task<int> BroadcastAsync(byte[] data, CancellationToken ct = default)
        {
            if (data.Length == 0) return 0;

            var targets = _clients.Values.ToArray();
            var tasks = new Task<bool>[targets.Length];
            for (int i = 0; i < targets.Length; i++)
                tasks[i] = SendAsync(targets[i].Id, data, ct);

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.Count(r => r);
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            var listener = _listener!;
            while (!token.IsCancellationRequested)
            {
                Socket clientSock;
                try
                {
                    if (_clients.Count >= _options.MaxClients)
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                        continue;
                    }

                    clientSock = await Task.Factory.FromAsync(listener.BeginAccept, listener.EndAccept, null)
                        .ConfigureAwait(false);
                }
                catch (ObjectDisposedException ex)
                {
                    Log(ex, $"AcceptLoop: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested) break;
                    Log(ex, "AcceptLoop: unexpected exception");
                    await Task.Delay(100).ConfigureAwait(false);
                    continue;
                }

                try
                {
                    clientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    clientSock.NoDelay = _options.NoDelay;
                    clientSock.ReceiveBufferSize = _options.ReceiveBufferSize;
                    clientSock.SendBufferSize = _options.SendBufferSize;

                    var ctx = new TcpClientContext(clientSock);
                    if (!_clients.TryAdd(ctx.Id, ctx))
                    {
                        CloseSocketSafe(clientSock);
                        continue;
                    }

                    _ = HandleClientAsync(ctx, token);
                }
                catch (Exception ex)
                {
                    Log(ex, "AcceptLoop: unexpected exception");
                    CloseSocketSafe(clientSock);
                }
            }
        }

        private async Task HandleClientAsync(TcpClientContext ctx, CancellationToken serverToken)
        {
            var sock = ctx.Socket;
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(serverToken, ctx.Cts.Token))
            {
                var token = linkedCts.Token;
                var buffer = new byte[_options.ReceiveBufferSize > 0 ? Math.Min(_options.ReceiveBufferSize, 128 * 1024) : 8192];

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        int received = await Task.Factory.FromAsync(
                            sock.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                            sock.EndReceive).ConfigureAwait(false);
                        if (received <= 0) break;

                        var copy = new byte[received];
                        Buffer.BlockCopy(buffer, 0, copy, 0, received);
                        RaiseDataReceived(ctx, copy);
                    }
                }
                catch (OperationCanceledException) { }
                catch (ObjectDisposedException) { }
                catch (SocketException ex)
                {
                    Log(ex, $"Receive socket error ({ex.SocketErrorCode})");
                }
                catch (Exception ex)
                {
                    Log(ex, "Receive unexpected");
                }
                finally
                {
                    if (_clients.TryRemove(ctx.Id, out _))
                        CloseClient(ctx);
                }
            }
        }

        private void CloseClient(TcpClientContext ctx)
        {
            try { ctx.Cts.Cancel(); } catch { }

            bool sendLockAcquired = false;
            try { sendLockAcquired = ctx.SendLock.Wait(TimeSpan.FromSeconds(1)); }
            catch { }
            finally
            {
                if (sendLockAcquired)
                {
                    try { ctx.SendLock.Dispose(); } catch { }
                }
            }

            CloseSocketSafe(ctx.Socket);
            try { ctx.Cts.Dispose(); } catch { }
        }

        private static void CloseSocketSafe(Socket? s)
        {
            if (s == null) return;
            try { s.Shutdown(SocketShutdown.Both); } catch { }
            try { s.Dispose(); } catch { }
        }
    }

    internal sealed class TcpClientContext
    {
        public Guid Id { get; }
        public Socket Socket { get; }
        public EndPoint? RemoteEndPoint { get; }
        public DateTime ConnectedAtUtc { get; }
        public CancellationTokenSource Cts { get; }
        public SemaphoreSlim SendLock { get; }

        public TcpClientContext(Socket socket)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Id = Guid.NewGuid();
            RemoteEndPoint = socket.RemoteEndPoint;
            ConnectedAtUtc = DateTime.UtcNow;
            Cts = new CancellationTokenSource();
            SendLock = new SemaphoreSlim(1, 1);
        }
    }
}
