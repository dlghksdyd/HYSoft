using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HYSoft.Communication.FileTransfer;
using HYSoft.Communication.Tcp.Client;
using HYSoft.Communication.Tcp.Server;
using Xunit;

namespace HYSoft.Communication.Tests
{
    /// <summary>
    /// FileTransferServer / Client 의 무결성·이어받기·수명.
    /// 결함(2026-11-10): ⓐ 수신 조각마다 Task.Run 으로 처리해 조각 순서가 보장되지 않았다
    /// ⓑ 서버가 항상 offset 0 으로 답해 이어받기가 동작하지 않았다 ⓒ Stop·Dispose 가 없어 포트를 영영 쥐었다
    /// ⓓ 헤더 오류에 답하지 않아 클라이언트가 무기한 기다렸다 ⓔ 전송 중 끊긴 세션의 파일 핸들이 남았다.
    /// </summary>
    public sealed class FileTransferTests : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "hysoft-ft-tests", Guid.NewGuid().ToString("N"));
        private readonly string _sourceDir;
        private readonly string _storageDir;
        private readonly int _port = TestNet.FreePort();
        private readonly FileTransferServer _server;

        public FileTransferTests()
        {
            _sourceDir = Directory.CreateDirectory(Path.Combine(_root, "source")).FullName;
            _storageDir = Path.Combine(_root, "storage");
            _server = new FileTransferServer(new TcpServerOptions(IPAddress.Loopback, _port), _storageDir);
        }

        public void Dispose()
        {
            _server.Dispose();
            try { Directory.Delete(_root, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }

        private string CreateSource(string name, int size, int seed)
        {
            var bytes = new byte[size];
            new Random(seed).NextBytes(bytes);
            string path = Path.Combine(_sourceDir, name);
            File.WriteAllBytes(path, bytes);
            return path;
        }

        private FileTransferClient NewClient() => new(new TcpClientOptions(IPAddress.Loopback, _port)
        {
            ReceiveTimeout = TimeSpan.FromSeconds(10),
        });

        private string StoredPath(string name) => Path.Combine(_storageDir, name);

        private string PartialPath(string name) => StoredPath(name) + FileTransferConstants.PartialFileSuffix;

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Large_files_arrive_byte_for_byte_repeatedly()
        {
            // 조각 순서가 섞이면 CRC 오류(IOException) 또는 내용 불일치로 드러난다.
            await _server.ReceiveFileAsync();
            using var client = NewClient();
            for (int round = 0; round < 3; round++)
            {
                string source = CreateSource($"big-{round}.bin", 24 * 1024 * 1024 + round, seed: round);
                await client.SendFileAsync(source);
                Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(StoredPath($"big-{round}.bin")));
                Assert.False(File.Exists(PartialPath($"big-{round}.bin")));
            }
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Partial_file_is_resumed_from_its_length()
        {
            string source = CreateSource("resume.bin", 3 * 1024 * 1024, seed: 7);
            byte[] all = File.ReadAllBytes(source);
            const int already = 1_000_000;
            Directory.CreateDirectory(_storageDir);
            File.WriteAllBytes(PartialPath("resume.bin"), all.AsSpan(0, already).ToArray());

            await _server.ReceiveFileAsync();
            var reports = new List<FileTransferProgress>();
            using var client = NewClient();
            await client.SendFileAsync(source, new SyncProgress(reports));

            Assert.Equal(all, File.ReadAllBytes(StoredPath("resume.bin")));
            Assert.False(File.Exists(PartialPath("resume.bin")));
            Assert.Equal(all.Length - already, reports[^1].TotalBytes);   // 남은 부분만 보냈다.
            Assert.Equal(all.Length - already, reports[^1].BytesTransferred);
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Cancelled_transfer_resumes_on_the_same_client_instance()
        {
            // 결함: 취소된 연결을 그대로 다시 쓰면 새 헤더가 이전 파일의 데이터로 읽혔다.
            string source = CreateSource("cancel.bin", 16 * 1024 * 1024, seed: 11);
            await _server.ReceiveFileAsync();
            using var client = NewClient();

            using (var cts = new CancellationTokenSource())
            {
                var cancelAfterSomeData = new CallbackProgress(p =>
                {
                    if (p.BytesTransferred >= 2 * 1024 * 1024) cts.Cancel();
                });
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.SendFileAsync(source, cancelAfterSomeData, cts.Token));
            }

            Assert.True(await TestNet.WaitUntilAsync(() => File.Exists(PartialPath("cancel.bin")), TimeSpan.FromSeconds(3)));

            var reports = new List<FileTransferProgress>();
            await client.SendFileAsync(source, new SyncProgress(reports));

            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(StoredPath("cancel.bin")));
            Assert.True(reports[^1].TotalBytes < new FileInfo(source).Length, "the second transfer should resume, not restart");
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Corrupt_partial_fails_once_is_discarded_and_the_retry_succeeds()
        {
            string source = CreateSource("corrupt.bin", 2 * 1024 * 1024, seed: 9);
            Directory.CreateDirectory(_storageDir);
            File.WriteAllBytes(PartialPath("corrupt.bin"), new byte[500_000]);   // 원본과 다른 내용

            await _server.ReceiveFileAsync();
            using var client = NewClient();

            await Assert.ThrowsAsync<IOException>(() => client.SendFileAsync(source));
            Assert.True(await TestNet.WaitUntilAsync(() => !File.Exists(PartialPath("corrupt.bin")), TimeSpan.FromSeconds(3)));
            Assert.False(File.Exists(StoredPath("corrupt.bin")));

            await client.SendFileAsync(source);
            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(StoredPath("corrupt.bin")));
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Existing_file_is_kept_until_a_new_copy_is_verified()
        {
            Directory.CreateDirectory(_storageDir);
            File.WriteAllText(StoredPath("keep.txt"), "old content");
            File.WriteAllBytes(PartialPath("keep.txt"), new byte[10]);   // 틀린 이어받기 → 실패해야 한다

            string source = CreateSource("keep.txt", 64 * 1024, seed: 3);
            await _server.ReceiveFileAsync();
            using var client = NewClient();

            await Assert.ThrowsAsync<IOException>(() => client.SendFileAsync(source));
            Assert.Equal("old content", File.ReadAllText(StoredPath("keep.txt")));

            await client.SendFileAsync(source);
            Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(StoredPath("keep.txt")));
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Dispose_releases_the_port()
        {
            await _server.ReceiveFileAsync();
            Assert.True(TestNet.IsListening(_port));

            _server.Dispose();

            Assert.True(await TestNet.WaitUntilAsync(() => !TestNet.IsListening(_port), TimeSpan.FromSeconds(3)));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _server.ReceiveFileAsync());
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Bad_header_is_answered_with_error_and_the_connection_is_closed()
        {
            await _server.ReceiveFileAsync();
            using var socket = await TestNet.ConnectRawAsync(_port);
            await socket.SendAsync(new byte[15], SocketFlags.None);   // 매직이 0 — 프로토콜 위반

            byte[] reply = await TestNet.ReceiveExactlyAsync(socket, 9, TimeSpan.FromSeconds(3));
            Assert.Equal(9, reply.Length);
            Assert.Equal(FileTransferConstants.StatusError, reply[0]);

            byte[] after = await TestNet.ReceiveExactlyAsync(socket, 1, TimeSpan.FromSeconds(3));
            Assert.Empty(after);   // 서버가 연결을 닫았다.
        }

        [Fact(Timeout = TestNet.TestTimeoutMs)]
        public async Task Disconnect_mid_transfer_keeps_the_partial_and_releases_the_file()
        {
            await _server.ReceiveFileAsync();
            byte[] name = Encoding.UTF8.GetBytes("cut.bin");
            var header = new byte[15 + name.Length];
            BinaryPrimitives.WriteUInt32BigEndian(header, FileTransferConstants.MagicHeader);
            BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(5), 1000);
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(13), (ushort)name.Length);
            name.CopyTo(header, 15);

            using (var socket = await TestNet.ConnectRawAsync(_port))
            {
                await socket.SendAsync(header, SocketFlags.None);
                byte[] reply = await TestNet.ReceiveExactlyAsync(socket, 9, TimeSpan.FromSeconds(3));
                Assert.Equal(FileTransferConstants.StatusOk, reply[0]);

                // 서버는 버퍼링해서 쓰므로 연결 중에는 디스크 길이가 아직 0 일 수 있다 — 길이는 끊긴 뒤에 본다.
                // 같은 연결 안에서 데이터가 FIN 보다 먼저 처리되므로 400바이트는 닫히기 전에 쓰인다.
                await socket.SendAsync(Enumerable.Repeat((byte)0xAB, 400).ToArray(), SocketFlags.None);
                Assert.True(File.Exists(PartialPath("cut.bin")));
                socket.Shutdown(SocketShutdown.Both);
            }

            // 끊긴 뒤 핸들이 풀려야 한다 — 단독으로 열 수 있어야 이어받기·삭제가 가능하다.
            Assert.True(await TestNet.WaitUntilAsync(() =>
            {
                try
                {
                    using var exclusive = new FileStream(PartialPath("cut.bin"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    return exclusive.Length == 400;
                }
                catch (IOException)
                {
                    return false;
                }
            }, TimeSpan.FromSeconds(3)));
            Assert.False(File.Exists(StoredPath("cut.bin")));
        }

        private sealed class CallbackProgress : IProgress<FileTransferProgress>
        {
            private readonly Action<FileTransferProgress> _onReport;

            public CallbackProgress(Action<FileTransferProgress> onReport) => _onReport = onReport;

            public void Report(FileTransferProgress value) => _onReport(value);
        }

        /// <summary>Progress&lt;T&gt; 는 콜백을 비동기로 보내 마지막 보고가 늦게 올 수 있다 — 동기로 모은다.</summary>
        private sealed class SyncProgress : IProgress<FileTransferProgress>
        {
            private readonly List<FileTransferProgress> _reports;

            public SyncProgress(List<FileTransferProgress> reports) => _reports = reports;

            public void Report(FileTransferProgress value)
            {
                lock (_reports) _reports.Add(value);
            }
        }
    }
}
