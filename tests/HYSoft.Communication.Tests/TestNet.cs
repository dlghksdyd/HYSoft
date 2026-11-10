using System.Net;
using System.Net.Sockets;
using RawListener = System.Net.Sockets.TcpListener;

namespace HYSoft.Communication.Tests
{
    /// <summary>시험 공용 네트워크 도우미. 모든 소켓은 127.0.0.1 에만 연다(방화벽 확인 창이 뜨지 않게).</summary>
    internal static class TestNet
    {
        /// <summary>
        /// 시험 하나의 상한. 결함이 「무기한 대기」라서, 상한이 없으면 회귀가 실패가 아니라 멈춤으로 나타난다
        /// (2026-11-10 수정 전 실행이 10분 넘게 끝나지 않았다).
        /// </summary>
        public const int TestTimeoutMs = 30_000;

        /// <summary>지금 비어 있는 루프백 포트 하나.</summary>
        public static int FreePort()
        {
            var probe = new RawListener(IPAddress.Loopback, 0);
            probe.Start();
            int port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            return port;
        }

        /// <summary>그 포트에 지금 리스너가 있는가(OS 기준).</summary>
        public static bool IsListening(int port)
            => System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners().Any(endPoint => endPoint.Port == port);

        /// <summary>조건이 참이 될 때까지 기다린다. 시간 안에 안 되면 false.</summary>
        public static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (condition()) return true;
                await Task.Delay(20);
            }

            return condition();
        }

        /// <summary>원시 소켓으로 서버에 접속한다(라이브러리 클라이언트를 거치지 않는 시험용).</summary>
        public static async Task<Socket> ConnectRawAsync(int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port));
            return socket;
        }

        /// <summary>정확히 count 바이트를 읽는다. 상대가 먼저 닫으면 읽은 만큼만 돌려준다.</summary>
        public static async Task<byte[]> ReceiveExactlyAsync(Socket socket, int count, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var buffer = new byte[count];
            int total = 0;
            while (total < count)
            {
                int read = await socket.ReceiveAsync(buffer.AsMemory(total), SocketFlags.None, cts.Token);
                if (read == 0) break;
                total += read;
            }

            return buffer.AsSpan(0, total).ToArray();
        }
    }
}
