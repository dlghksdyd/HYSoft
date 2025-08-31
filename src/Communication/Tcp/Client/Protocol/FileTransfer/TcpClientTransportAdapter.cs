using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HYSoft.Communication.Tcp.Client;

namespace HYSoft.Communication.Tcp.Client.Protocol.FileTransfer
{
    /// <summary>
    /// 기존 HYSoft Tcp.Client를 IByteTransport로 어댑트.
    /// (아래 _client API 시그니처는 예시입니다. 실제 TcpClient에 맞춰 수정하세요.)
    /// </summary>
    public sealed class TcpClientTransportAdapter : IByteTransport
    {
        private readonly TcpClient _client;

        public TcpClientTransportAdapter(TcpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public bool IsConnected => !_client.IsDisposed; // 필요 시 실제 연결 상태 속성으로 교체

        public Task ConnectAsync(CancellationToken ct = default)
        {
            // 이미 외부에서 Connect 했다면 No-op 하셔도 됩니다.
            // _client.ConnectAsync(...) 형태가 있다면 호출하도록 변경
            return Task.CompletedTask;
        }

        public async Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            // 예시: _client.SendAsync(byte[]) 형태라 가정
            await _client.SendAsync(buffer.ToArray()).ConfigureAwait(false);
        }

        public async Task ReceiveExactAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            // 외부 취소 요청만 즉시 반영 (TcpClient.ReceiveAsync는 자체 타임아웃 사용)
            ct.ThrowIfCancellationRequested();

            // 여러분의 TcpClient.ReceiveAsync(Memory<byte>)는 정확히 buffer.Length만큼 채울 때까지 대기
            await _client.ReceiveAsync(buffer).ConfigureAwait(false);
        }

        public void Dispose() => _client.Dispose();
        public ValueTask DisposeAsync()
        {
            _client.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
