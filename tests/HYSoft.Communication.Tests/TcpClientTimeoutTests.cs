using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using HYSoft.Communication.Tcp.Client;
using Xunit;
using RawListener = System.Net.Sockets.TcpListener;
using TcpClient = HYSoft.Communication.Tcp.Client.TcpClient;

namespace HYSoft.Communication.Tests
{
    /// <summary>
    /// TcpClient 의 Send/ReceiveTimeout 이 실제로 적용되는가.
    /// 결함(2026-11-10): 타임아웃 CTS 를 만들기만 하고 소켓 호출에 넘기지 않아, 응답 없는 상대에게 무기한 대기했다.
    /// </summary>
    public sealed class TcpClientTimeoutTests : IDisposable
    {
        private readonly RawListener _peer;
        private readonly int _port;
        private readonly List<Socket> _accepted = new();

        public TcpClientTimeoutTests()
        {
            _port = TestNet.FreePort();
            _peer = new RawListener(IPAddress.Loopback, _port);
            _peer.Start();
        }

        public void Dispose()
        {
            foreach (var socket in _accepted) socket.Dispose();
            _peer.Stop();
        }

        private async Task<(TcpClient Client, Socket Peer)> ConnectAsync(Action<TcpClientOptions> configure)
        {
            var options = new TcpClientOptions(IPAddress.Loopback, _port);
            configure(options);
            var client = TcpClientManager.Create(options);
            var acceptTask = _peer.AcceptSocketAsync();
            await client.ConnectAsync();
            var peer = await acceptTask;
            _accepted.Add(peer);
            return (client, peer);
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task ReceiveAsync_peer_silent_throws_TimeoutException_within_bound()
        {
            var (client, _) = await ConnectAsync(o => o.ReceiveTimeout = TimeSpan.FromMilliseconds(300));
            try
            {
                var watch = Stopwatch.StartNew();
                var receive = client.ReceiveAsync(new byte[4]);
                var finished = await Task.WhenAny(receive, Task.Delay(TimeSpan.FromSeconds(5)));

                Assert.Same(receive, finished);   // 종전에는 여기서 5초가 지나도 끝나지 않았다.
                await Assert.ThrowsAsync<TimeoutException>(() => receive);
                Assert.InRange(watch.ElapsedMilliseconds, 250, 3000);
            }
            finally
            {
                TcpClientManager.Dispose(client);
            }
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task After_receive_timeout_the_connection_is_dropped_not_reused()
        {
            // 일부만 읽힌 채 끝났을 수 있다 — 같은 소켓으로 계속 읽으면 프레임이 어긋난다(fail-closed).
            var (client, _) = await ConnectAsync(o => o.ReceiveTimeout = TimeSpan.FromMilliseconds(200));
            try
            {
                await Assert.ThrowsAsync<TimeoutException>(() => client.ReceiveAsync(new byte[4]));
                await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(new byte[] { 1 }));
            }
            finally
            {
                TcpClientManager.Dispose(client);
            }
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task ReceiveTimeout_is_idle_based_and_resets_on_progress()
        {
            var (client, peer) = await ConnectAsync(o => o.ReceiveTimeout = TimeSpan.FromMilliseconds(600));
            try
            {
                // 200 ms 간격으로 4바이트 — 전체 약 800 ms 로 타임아웃보다 길지만 매번 진행이 있다.
                var sender = Task.Run(async () =>
                {
                    for (int i = 0; i < 4; i++)
                    {
                        await Task.Delay(200);
                        await peer.SendAsync(new[] { (byte)(i + 1) }, SocketFlags.None);
                    }
                });

                var buffer = new byte[4];
                await client.ReceiveAsync(buffer);
                await sender;
                Assert.Equal(new byte[] { 1, 2, 3, 4 }, buffer);
            }
            finally
            {
                TcpClientManager.Dispose(client);
            }
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Infinite_ReceiveTimeout_still_waits_for_late_data()
        {
            var (client, peer) = await ConnectAsync(o => o.ReceiveTimeout = Timeout.InfiniteTimeSpan);
            try
            {
                var sender = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await peer.SendAsync(new byte[] { 7, 8 }, SocketFlags.None);
                });

                var buffer = new byte[2];
                await client.ReceiveAsync(buffer);
                await sender;
                Assert.Equal(new byte[] { 7, 8 }, buffer);
            }
            finally
            {
                TcpClientManager.Dispose(client);
            }
        }

        // 송신 타임아웃 재현 시험은 두지 않는다: Windows 는 상대가 읽지 않아도 64 MB 비동기 송신을 커널에 맡기고
        // 29 ms 만에 완료시켰다(2026-11-10 실측). 송신은 수신과 같은 도우미(RunWithIdleTimeoutAsync)를 지나므로
        // 위 수신 시험들이 그 경로를 덮는다.

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Peer_close_during_receive_throws_and_drops_the_connection()
        {
            var (client, peer) = await ConnectAsync(o => o.ReceiveTimeout = TimeSpan.FromSeconds(5));
            try
            {
                var receive = client.ReceiveAsync(new byte[4]);
                peer.Shutdown(SocketShutdown.Both);
                peer.Close();

                await Assert.ThrowsAsync<SocketException>(() => receive);
                await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(new byte[] { 1 }));
            }
            finally
            {
                TcpClientManager.Dispose(client);
            }
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task ConnectAsync_after_dropped_connection_reconnects()
        {
            var (client, _) = await ConnectAsync(o => o.ReceiveTimeout = TimeSpan.FromMilliseconds(200));
            try
            {
                await Assert.ThrowsAsync<TimeoutException>(() => client.ReceiveAsync(new byte[1]));

                var acceptTask = _peer.AcceptSocketAsync();
                await client.ConnectAsync();
                var peer = await acceptTask;
                _accepted.Add(peer);

                await peer.SendAsync(new byte[] { 42 }, SocketFlags.None);
                var buffer = new byte[1];
                await client.ReceiveAsync(buffer);
                Assert.Equal(42, buffer[0]);
            }
            finally
            {
                TcpClientManager.Dispose(client);
            }
        }
    }
}
