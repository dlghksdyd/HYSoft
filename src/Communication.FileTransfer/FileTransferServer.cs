#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HYSoft.Communication.Tcp.Server;
using static HYSoft.Communication.FileTransfer.BinaryHelper;
using static HYSoft.Communication.FileTransfer.FileTransferConstants;

namespace HYSoft.Communication.FileTransfer
{
    /// <summary>
    /// FT10 파일 수신 서버.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><b>순서</b>: 한 클라이언트의 수신 조각은 도착 순서대로, 수신 처리기 안에서 끝까지 처리한다. 그 클라이언트의 다음
    /// 소켓 읽기는 처리가 끝난 뒤 시작하므로 순서가 섞이지 않고, 디스크가 느리면 TCP 흐름 제어로 송신 측이 늦춰진다.
    /// (종전에는 조각마다 Task.Run 으로 넘겨 순서가 보장되지 않았고, 큰 파일에서 CRC 오류가 실측됐다 — 2026-11-10.)</item>
    /// <item><b>이어받기</b>: 받는 중인 파일은 <c>이름</c>+<see cref="FileTransferConstants.PartialFileSuffix"/> 에 쓴다. 같은 이름의
    /// 조각 파일이 있으면 그 길이부터 이어받는다(<see cref="FileTransferConstants.StatusResume"/>). 전체 CRC 가 맞을 때만
    /// 최종 이름으로 바꾼다 — 기존 완성 파일은 새 파일이 검증될 때까지 그대로 남는다.</item>
    /// <item><b>실패 처리</b>: CRC 불일치·꼬리 손상이면 조각 파일을 지운다(다음 시도는 처음부터). 연결이 끊기면 조각 파일은 남기고
    /// 핸들만 닫는다(이어받기용). 헤더 오류에는 9바이트 오류 응답을 보내고 연결을 끊는다 — 응답 없이 두면 클라이언트가 기다린다.</item>
    /// <item><b>수명</b>: <see cref="Dispose"/> 가 리스너와 모든 연결을 닫는다. 다시 시작할 수 없다.</item>
    /// </list>
    /// </remarks>
    public class FileTransferServer : IDisposable
    {
        private readonly TcpServer _server;
        private readonly string _storageRoot;
        private readonly ConcurrentDictionary<Guid, Session> _sessions = new ConcurrentDictionary<Guid, Session>();
        private readonly object _lifecycleGate = new object();

        private bool _receiving;
        private volatile bool _disposed;

        public FileTransferServer(TcpServerOptions options, string? storageRoot = null)
        {
            _server = TcpServerManager.Create(options);
            _storageRoot = Path.GetFullPath(storageRoot ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "received"));
            Directory.CreateDirectory(_storageRoot);
        }

        /// <summary>파일을 저장하는 폴더(절대 경로).</summary>
        public string StorageRoot => _storageRoot;

        /// <summary>
        /// 수신을 시작한다. 리슨이 시작되면 곧바로 반환한다(파일을 기다리지 않는다). 여러 번 불러도 한 번만 시작한다.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> 뒤에 불렀다.</exception>
        public async Task ReceiveFileAsync()
        {
            lock (_lifecycleGate)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(FileTransferServer));
                if (!_receiving)
                {
                    _receiving = true;
                    _server.DataReceived += OnDataReceived;
                    _server.ClientDisconnected += OnClientDisconnected;
                }
            }

            await _server.StartAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 리스너와 모든 연결을 닫고 받던 파일의 핸들을 놓는다(조각 파일은 이어받기용으로 남긴다).
        /// 연결마다 최대 1초까지 송신 정리를 기다릴 수 있으므로 UI 스레드에서는 <c>Task.Run</c> 으로 부르는 것이 좋다.
        /// </summary>
        public void Dispose()
        {
            lock (_lifecycleGate)
            {
                if (_disposed) return;
                _disposed = true;
            }

            _server.DataReceived -= OnDataReceived;
            _server.ClientDisconnected -= OnClientDisconnected;
            TcpServerManager.Dispose(_server);

            foreach (var session in _sessions.Values)
            {
                lock (session.Gate) CloseSession(session, deletePartial: false);
            }
        }

        private void OnDataReceived(TcpDataReceivedContext ctx)
        {
            if (_disposed) return;

            var session = _sessions.GetOrAdd(ctx.ClientId, id => new Session(id));
            lock (session.Gate)
            {
                if (session.Closed) return;

                try
                {
                    // 🔴 기다린다 — 이 클라이언트의 다음 조각은 이 처리가 끝난 뒤에야 읽힌다(순서 보장 + 흐름 제어).
                    //    처리기는 동기화 컨텍스트가 없는 I/O 스레드에서 불리고 내부 await 은 모두 ConfigureAwait(false) 다.
                    ProcessIncomingAsync(session, ctx).GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                    // 디스크 오류 등 — 상태를 알 수 없으니 오류로 답하고 연결을 끊는다. 조각 파일은 남긴다(다음 CRC 가 가린다).
                    TryReply(ctx, session.HeaderAnswered ? new[] { StatusError } : ErrorHeaderReply());
                    session.Desynchronized = true;
                    session.State = SessionState.Error;
                }

                if (session.State == SessionState.Done || session.State == SessionState.Error)
                {
                    CloseSession(session, session.DeletePartialOnClose);
                    if (session.Desynchronized) _server.Disconnect(ctx.ClientId);
                }
            }
        }

        private void OnClientDisconnected(Guid clientId)
        {
            if (_sessions.TryGetValue(clientId, out var session))
            {
                lock (session.Gate) CloseSession(session, deletePartial: false);
            }
        }

        private async Task ProcessIncomingAsync(Session s, TcpDataReceivedContext ctx)
        {
            byte[] inputData = ctx.Data;
            int inputOffset = 0;
            int inputLength = inputData.Length;

            while (inputOffset < inputLength && s.State != SessionState.Done && s.State != SessionState.Error)
            {
                switch (s.State)
                {
                    case SessionState.WaitHeader:
                    {
                        const int need = 4 + 1 + 8 + 2; // 15
                        int canWrite = Math.Min(inputLength - inputOffset, need - s.HeaderBuffer.Length);
                        s.HeaderBuffer.Write(inputData, inputOffset, canWrite);
                        inputOffset += canWrite;
                        if (s.HeaderBuffer.Length < need) break;

                        byte[] span = s.HeaderBuffer.ToArray();
                        uint magic = ReadUInt32BE(span, 0);
                        long fileSize = (long)ReadUInt64BE(span, 5);
                        ushort nameLen = ReadUInt16BE(span, 13);
                        if (magic != MagicHeader || fileSize < 0 || nameLen == 0)
                        {
                            await RejectAsync(s, ctx, deletePartial: false, desynchronized: true).ConfigureAwait(false);
                            break;
                        }

                        s.Flags = span[4];
                        s.FileSize = fileSize;
                        s.ExpectedNameBytes = nameLen;
                        s.HeaderBuffer.Clear();
                        s.State = SessionState.WaitFileName;
                        break;
                    }
                    case SessionState.WaitFileName:
                    {
                        int toCopy = Math.Min(inputLength - inputOffset, s.ExpectedNameBytes - s.HeaderBuffer.Length);
                        s.HeaderBuffer.Write(inputData, inputOffset, toCopy);
                        inputOffset += toCopy;
                        if (s.HeaderBuffer.Length < s.ExpectedNameBytes) break;

                        string fileName = SanitizeFileName(Encoding.UTF8.GetString(s.HeaderBuffer.ToArray(), 0, s.ExpectedNameBytes));
                        s.HeaderBuffer.Clear();
                        if (!TryResolvePaths(fileName, out string finalPath, out string partialPath))
                        {
                            await RejectAsync(s, ctx, deletePartial: false, desynchronized: true).ConfigureAwait(false);
                            break;
                        }

                        s.FinalPath = finalPath;
                        s.PartialPath = partialPath;

                        s.Stream = await OpenPartialAsync(partialPath).ConfigureAwait(false);
                        if (s.Stream is null)
                        {
                            // 같은 이름을 다른 연결이 받는 중이다 — 그 조각 파일은 남의 것이므로 건드리지 않는다.
                            await RejectAsync(s, ctx, deletePartial: false, desynchronized: false).ConfigureAwait(false);
                            break;
                        }

                        s.OwnsPartial = true;

                        long startOffset = s.Stream.Length;
                        if (startOffset > s.FileSize)
                        {
                            // 이번 파일보다 긴 조각은 다른 파일의 것이다 — 처음부터 받는다.
                            s.Stream.SetLength(0);
                            startOffset = 0;
                        }

                        s.CrcValue = 0xFFFFFFFFu;
                        if (startOffset > 0) await SeedCrcFromExistingAsync(s, startOffset).ConfigureAwait(false);
                        s.Stream.Seek(startOffset, SeekOrigin.Begin);
                        s.ReceivedAfterOffset = startOffset;

                        var resp = new byte[9];
                        resp[0] = startOffset > 0 ? StatusResume : StatusOk;
                        WriteUInt64BE(resp, 1, (ulong)startOffset);
                        await ctx.ReplyAsync(resp).ConfigureAwait(false);
                        s.HeaderAnswered = true;

                        s.State = s.ReceivedAfterOffset >= s.FileSize ? SessionState.WaitTail : SessionState.ReceivingData;
                        break;
                    }
                    case SessionState.ReceivingData:
                    {
                        long remaining = s.FileSize - s.ReceivedAfterOffset;
                        int toWrite = (int)Math.Min(inputLength - inputOffset, remaining);
                        if (toWrite > 0)
                        {
                            await s.Stream!.WriteAsync(inputData, inputOffset, toWrite).ConfigureAwait(false);
                            s.CrcValue = Crc32.Update(s.CrcValue, inputData, inputOffset, toWrite);
                            s.ReceivedAfterOffset += toWrite;
                            inputOffset += toWrite;
                        }

                        if (s.ReceivedAfterOffset >= s.FileSize)
                            s.State = SessionState.WaitTail;
                        break;
                    }
                    case SessionState.WaitTail:
                    {
                        const int need = 8;
                        int toCopy = Math.Min(need - s.HeaderBuffer.Length, inputLength - inputOffset);
                        s.HeaderBuffer.Write(inputData, inputOffset, toCopy);
                        inputOffset += toCopy;
                        if (s.HeaderBuffer.Length < need) break;

                        byte[] span = s.HeaderBuffer.ToArray();
                        s.HeaderBuffer.Clear();
                        uint magicTail = ReadUInt32BE(span, 0);
                        uint crcFromClient = ReadUInt32BE(span, 4);

                        if (magicTail != MagicTail)
                        {
                            await RejectAsync(s, ctx, deletePartial: true, desynchronized: true).ConfigureAwait(false);
                            break;
                        }

                        if ((s.CrcValue ^ 0xFFFFFFFFu) != crcFromClient)
                        {
                            // 받은 내용(또는 이어받은 앞부분)이 원본과 다르다 — 이 조각 파일로는 다시 이어받으면 안 된다.
                            await RejectAsync(s, ctx, deletePartial: true, desynchronized: false).ConfigureAwait(false);
                            break;
                        }

                        await s.Stream!.FlushAsync().ConfigureAwait(false);
                        s.Stream.Dispose();
                        s.Stream = null;

                        try
                        {
                            File.Move(s.PartialPath, s.FinalPath, overwrite: true);
                        }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                        {
                            // 내용은 검증됐다 — 조각 파일을 남겨 두면 다음 시도가 끝에서 이어받아 다시 교체를 시도한다.
                            await RejectAsync(s, ctx, deletePartial: false, desynchronized: false).ConfigureAwait(false);
                            break;
                        }

                        s.OwnsPartial = false;
                        s.State = SessionState.Done;
                        await ctx.ReplyAsync(new[] { StatusOk }).ConfigureAwait(false);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 오류로 답하고 세션을 오류 상태로 둔다(정리는 호출부가 한다). 헤더에 아직 답하지 않았으면 클라이언트는 9바이트를
        /// 기다리므로 9바이트로, 그 뒤면 1바이트로 답한다.
        /// </summary>
        private static async Task RejectAsync(Session s, TcpDataReceivedContext ctx, bool deletePartial, bool desynchronized)
        {
            s.State = SessionState.Error;
            s.DeletePartialOnClose = deletePartial;
            s.Desynchronized = desynchronized;
            try
            {
                await ctx.ReplyAsync(s.HeaderAnswered ? new[] { StatusError } : ErrorHeaderReply()).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // 응답을 못 보내도 세션 정리·연결 종료는 계속한다.
            }
        }

        /// <summary>
        /// 조각 파일을 단독으로 연다. 방금 끊긴 연결이 아직 핸들을 쥐고 있을 수 있어(끊김 알림은 소켓 종료를 감지한 뒤에 온다)
        /// <see cref="PartialOpenRetryWindow"/> 동안 다시 시도한다. 그래도 열리지 않으면 null — 다른 연결이 받는 중이다.
        /// </summary>
        private static async Task<FileStream?> OpenPartialAsync(string partialPath)
        {
            var deadline = DateTime.UtcNow + PartialOpenRetryWindow;
            while (true)
            {
                try
                {
                    return new FileStream(partialPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, DefaultChunkSize, true);
                }
                catch (IOException) when (DateTime.UtcNow < deadline)
                {
                    await Task.Delay(50).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    return null;
                }
            }
        }

        private static readonly TimeSpan PartialOpenRetryWindow = TimeSpan.FromSeconds(2);

        private static byte[] ErrorHeaderReply()
        {
            var reply = new byte[9];
            reply[0] = StatusError;
            return reply;
        }

        private static void TryReply(TcpDataReceivedContext ctx, byte[] reply)
        {
            try { ctx.ReplyAsync(reply).GetAwaiter().GetResult(); } catch (Exception) { }
        }

        /// <summary>세션을 닫는다. 호출부가 <c>session.Gate</c> 를 쥐고 부른다. 여러 번 불러도 한 번만 정리한다.</summary>
        private void CloseSession(Session s, bool deletePartial)
        {
            if (s.Closed) return;
            s.Closed = true;

            _sessions.TryRemove(new KeyValuePair<Guid, Session>(s.ClientId, s));

            try { s.Stream?.Dispose(); } catch (Exception) { }
            s.Stream = null;

            if (deletePartial && s.OwnsPartial && s.PartialPath.Length > 0)
            {
                try { File.Delete(s.PartialPath); } catch (Exception) { }
            }

            s.HeaderBuffer.Dispose();
        }

        /// <summary>최종·조각 파일 경로를 만든다. 둘 다 저장 폴더 바로 아래여야 한다(경로 순회 차단).</summary>
        private bool TryResolvePaths(string fileName, out string finalPath, out string partialPath)
        {
            finalPath = string.Empty;
            partialPath = string.Empty;
            if (fileName.Length == 0 || fileName == "." ) return false;

            string candidate = Path.GetFullPath(Path.Combine(_storageRoot, fileName));
            string? directory = Path.GetDirectoryName(candidate);
            if (directory is null || !string.Equals(directory.TrimEnd(Path.DirectorySeparatorChar), _storageRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                return false;

            finalPath = candidate;
            partialPath = candidate + PartialFileSuffix;
            return true;
        }

        private static string SanitizeFileName(string name)
        {
            // Path traversal 방지
            name = name.Replace("..", "__");
            name = name.Replace("/", "_");
            name = name.Replace("\\", "_");
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        private static async Task SeedCrcFromExistingAsync(Session s, long upToLength)
        {
            s.Stream!.Seek(0, SeekOrigin.Begin);
            byte[] buf = new byte[DefaultChunkSize];
            long pos = 0;
            while (pos < upToLength)
            {
                int toRead = (int)Math.Min(buf.Length, upToLength - pos);
                int read = await s.Stream.ReadAsync(buf, 0, toRead).ConfigureAwait(false);
                if (read <= 0) throw new EndOfStreamException("Unexpected EOF while CRC seeding.");
                s.CrcValue = Crc32.Update(s.CrcValue, buf, 0, read);
                pos += read;
            }
        }

        private enum SessionState { WaitHeader, WaitFileName, ReceivingData, WaitTail, Done, Error }

        private sealed class Session
        {
            public Session(Guid id) { ClientId = id; }

            public Guid ClientId { get; }
            public readonly object Gate = new object();
            public SessionState State = SessionState.WaitHeader;
            public bool Closed;
            public bool HeaderAnswered;
            public bool OwnsPartial;
            public bool DeletePartialOnClose;
            public bool Desynchronized;
            public long FileSize;
            public string FinalPath = string.Empty;
            public string PartialPath = string.Empty;
            public byte Flags;
            public long ReceivedAfterOffset;
            public FileStream? Stream;
            public uint CrcValue;
            public int ExpectedNameBytes;
            public readonly GrowingBuffer HeaderBuffer = new GrowingBuffer();
        }

        private sealed class GrowingBuffer : IDisposable
        {
            private byte[] _buf = new byte[0];
            private int _len;
            public int Length => _len;

            public void Write(byte[] src, int offset, int count)
            {
                if (count <= 0) return;
                EnsureCapacity(_len + count);
                Array.Copy(src, offset, _buf, _len, count);
                _len += count;
            }

            public void Clear() => _len = 0;
            public byte[] ToArray() { var arr = new byte[_len]; Array.Copy(_buf, arr, _len); return arr; }

            private void EnsureCapacity(int need)
            {
                if (_buf.Length >= need) return;
                int cap = _buf.Length == 0 ? 256 : _buf.Length;
                while (cap < need) cap *= 2;
                Array.Resize(ref _buf, cap);
            }

            public void Dispose() { _buf = Array.Empty<byte>(); _len = 0; }
        }
    }
}
