using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    /// <summary>
    /// TcpServer의 연결/해제/수신 이벤트에 연결해 쓰는 매니저.
    /// </summary>
    public sealed class FileTransferProtocolManager : IAsyncDisposable
    {
        private readonly FileTransferServerOptions _options;
        private readonly ConcurrentDictionary<Guid, FileTransferSession> _sessions = new();

        public FileTransferProtocolManager(FileTransferServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void OnClientConnected(Guid clientId)
        {
            var session = new FileTransferSession(clientId, _options);
            _sessions[clientId] = session;
        }

        public async ValueTask OnClientDisconnectedAsync(Guid clientId)
        {
            if (_sessions.TryRemove(clientId, out var s))
                await s.DisposeAsync();
        }

        /// <summary>
        /// TcpServer 수신 바이트를 전달하면, "즉시 전송해야 할 응답 패킷 바이트들"을 0개 이상 반환한다.
        /// </summary>
        public IReadOnlyList<byte[]> OnDataReceived(Guid clientId, ReadOnlySpan<byte> data)
        {
            if (!_sessions.TryGetValue(clientId, out var s))
                return Array.Empty<byte[]>();

            return s.OnBytes(data);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var kv in _sessions)
                await kv.Value.DisposeAsync();
            _sessions.Clear();
        }
    }
}