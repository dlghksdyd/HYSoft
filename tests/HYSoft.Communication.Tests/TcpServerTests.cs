using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using HYSoft.Communication.Tcp.Server;
using Xunit;

namespace HYSoft.Communication.Tests
{
    /// <summary>TcpServer 의 송신 실패 계약과 연결 종료 알림.</summary>
    public sealed class TcpServerTests : IAsyncLifetime
    {
        private readonly int _port = TestNet.FreePort();
        private TcpServer _server = null!;

        public async Task InitializeAsync()
        {
            _server = TcpServerManager.Create(new TcpServerOptions(IPAddress.Loopback, _port));
            await _server.StartAsync();
        }

        public Task DisposeAsync()
        {
            TcpServerManager.Dispose(_server);
            return Task.CompletedTask;
        }

        private async Task<(Socket Client, Guid ClientId)> ConnectAsync()
        {
            var client = await TestNet.ConnectRawAsync(_port);
            Assert.True(await TestNet.WaitUntilAsync(() => _server.ConnectedCount > 0, TimeSpan.FromSeconds(3)));
            return (client, _server.GetClientsSnapshot()[0].Id);
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task SendAsync_with_cancelled_token_returns_false_instead_of_throwing()
        {
            // 결함(2026-11-10): 송신 잠금 대기가 try 밖에 있어 OperationCanceledException 이 새어 나갔다 — 계약은 bool.
            var (client, clientId) = await ConnectAsync();
            using (client)
            {
                using var cancelled = new CancellationTokenSource();
                cancelled.Cancel();

                bool sent = await _server.SendAsync(clientId, new byte[] { 1 }, cancelled.Token);
                Assert.False(sent);
            }
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task SendAsync_to_unknown_client_returns_false()
        {
            Assert.False(await _server.SendAsync(Guid.NewGuid(), new byte[] { 1 }));
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task ClientDisconnected_is_raised_once_when_the_client_closes()
        {
            var raised = new ConcurrentQueue<Guid>();
            _server.ClientDisconnected += id => raised.Enqueue(id);

            var (client, clientId) = await ConnectAsync();
            client.Shutdown(SocketShutdown.Both);
            client.Close();

            Assert.True(await TestNet.WaitUntilAsync(() => raised.Count >= 1, TimeSpan.FromSeconds(3)));
            await Task.Delay(200);
            Assert.Equal(new[] { clientId }, raised.ToArray());
            Assert.Equal(0, _server.ConnectedCount);
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task ClientDisconnected_is_raised_when_the_server_disconnects_the_client()
        {
            var raised = new ConcurrentQueue<Guid>();
            _server.ClientDisconnected += id => raised.Enqueue(id);

            var (client, clientId) = await ConnectAsync();
            using (client)
            {
                Assert.True(_server.Disconnect(clientId));
                Assert.True(await TestNet.WaitUntilAsync(() => raised.Count >= 1, TimeSpan.FromSeconds(3)));
                await Task.Delay(200);
                Assert.Equal(new[] { clientId }, raised.ToArray());
            }
        }
    }
}
