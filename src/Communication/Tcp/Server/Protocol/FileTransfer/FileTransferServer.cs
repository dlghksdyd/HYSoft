#nullable enable
using HYSoft.Communication.Tcp.Client;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    /// <summary>
    /// 간단한 파일 전송 서버 구현.
    /// 클라이언트 프로토콜:
    /// 1) [MAGIC_HEADER(4)="FT10"][Flags(1)][FileSize(8)][NameLen(2)][FileName(UTF-8)]
    /// 2) 서버 응답: [Status(1: 0x00=OK,0x01=RESUME)][StartOffset(8)]
    /// 3) 클라: StartOffset부터 파일 바이트 전송
    /// 4) 클라: [MAGIC_TAIL(4)="FTEN"][CRC32(4)]  (CRC32는 파일 전체 기준)
    /// 5) 서버: [FinalStatus(1): 0x00=OK, 0xFF=ERROR]
    /// </summary>
    public class FileTransferServer
    {
        private readonly TcpServer _server;

        // 저장 루트 폴더 (없으면 생성)
        private readonly string _storageRoot;

        // 세션 관리 (클라이언트별 상태)
        private readonly ConcurrentDictionary<Guid, Session> _sessions = new();

        // 프로토콜 상수
        private const uint MAGIC_HEADER = 0x46543130; // "FT10"
        private const uint MAGIC_TAIL = 0x4654454E;   // "FTEN"
        private const byte STATUS_OK = 0x00;
        private const byte STATUS_RESUME = 0x01;
        private const byte STATUS_ERROR = 0xFF;

        public FileTransferServer(TcpServerOptions options, string? storageRoot = null)
        {
            _server = TcpServerManager.Create(options);
            _storageRoot = storageRoot ?? Path.Combine(AppContext.BaseDirectory, "received");
            Directory.CreateDirectory(_storageRoot);
        }

        /// <summary>
        /// 파일 수신 서버를 시작하고, DataReceived 핸들러를 설치합니다.
        /// </summary>
        public async Task ReceiveFileAsync()
        {
            _server.DataReceived += OnDataReceived;
            await _server.StartAsync().ConfigureAwait(false);
        }

        private void OnDataReceived(TcpDataReceivedContext ctx)
        {
            // 세션 가져오기/생성
            var session = _sessions.GetOrAdd(ctx.ClientId, _ => new Session(ctx.ClientId, _storageRoot));

            // 세션 단위 직렬 처리 (동시 수신 시 혼잡 방지)
            _ = Task.Run(async () =>
            {
                await session.Lock.WaitAsync().ConfigureAwait(false);
                try
                {
                    await ProcessIncomingAsync(session, ctx).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 실패 시 에러 응답 및 정리
                    try { await ctx.ReplyAsync(new byte[] { STATUS_ERROR }).ConfigureAwait(false); } catch { /* ignore */ }
                    CleanupSession(session);
                }
                finally
                {
                    session.Lock.Release();
                }
            });
        }

        private async Task ProcessIncomingAsync(Session s, TcpDataReceivedContext ctx)
        {
            var input = ctx.Data.AsMemory();

            while (!input.IsEmpty && s.State != SessionState.Done && s.State != SessionState.Error)
            {
                switch (s.State)
                {
                    case SessionState.WaitHeader:
                        {
                            // 먼저 고정영역 15바이트 확보
                            int need = 4 + 1 + 8 + 2;
                            if (s.HeaderBuffer.Length + input.Length < need)
                            {
                                s.HeaderBuffer.Write(input.Span);
                                input = input.Slice(input.Length);
                                break;
                            }

                            // 필요분만 채움
                            int copy = need - s.HeaderBuffer.Length;
                            s.HeaderBuffer.Write(input.Span.Slice(0, copy));
                            input = input.Slice(copy);

                            var span = s.HeaderBuffer.GetBufferSpan();
                            uint magic = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(0, 4));
                            if (magic != MAGIC_HEADER) { s.State = SessionState.Error; break; }

                            s.Flags = span[4];
                            s.FileSize = (long)BinaryPrimitives.ReadUInt64BigEndian(span.Slice(5, 8));
                            ushort nameLen = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(13, 2));

                            if (s.FileSize < 0 || nameLen == 0)
                            {
                                s.State = SessionState.Error;
                                break;
                            }

                            // 파일명 수신으로 진행
                            s.State = SessionState.WaitFileName;
                            s.ExpectedNameBytes = nameLen;
                            s.HeaderBuffer.Clear(); // 재사용
                            break;
                        }
                    case SessionState.WaitFileName:
                        {
                            int toCopy = (int)Math.Min(input.Length, s.ExpectedNameBytes - s.HeaderBuffer.Length);
                            if (toCopy > 0)
                            {
                                s.HeaderBuffer.Write(input.Span.Slice(0, toCopy));
                                input = input.Slice(toCopy);
                            }

                            if (s.HeaderBuffer.Length < s.ExpectedNameBytes)
                            {
                                // 더 필요
                                break;
                            }

                            // 파일명 파싱
                            string fileName = Encoding.UTF8.GetString(s.HeaderBuffer.GetBufferSpan().Slice(0, s.ExpectedNameBytes));
                            s.FileName = SanitizeFileName(fileName);
                            s.FullPath = Path.Combine(s.StorageRoot, s.FileName);

                            long startOffset = 0;
                            var dir = Path.GetDirectoryName(s.FullPath)!;
                            Directory.CreateDirectory(dir);
                            if (File.Exists(s.FullPath))
                            {
                                TryDeleteQuiet(s.FullPath); // 동일 이름 존재 시 삭제하고 처음부터 받기
                            }

                            // 파일 스트림 열기 (추가쓰기)
                            s.Stream = new FileStream(
                                s.FullPath,
                                FileMode.OpenOrCreate,
                                FileAccess.ReadWrite,
                                FileShare.None,
                                bufferSize: 64 * 1024,
                                useAsync: true);

                            if (s.Stream.Length != startOffset)
                            {
                                s.Stream.SetLength(startOffset);
                            }
                            s.Stream.Seek(startOffset, SeekOrigin.Begin);

                            // CRC 초기화: 전체 파일 기준으로 계산해야 하므로
                            // startOffset > 0 인 경우, 기존 구간의 CRC를 먼저 계산
                            s.Crc32 = 0xFFFFFFFFu;
                            if (startOffset > 0)
                            {
                                await SeedCrcFromExistingAsync(s, startOffset).ConfigureAwait(false);
                            }

                            s.ReceivedAfterOffset = startOffset;

                            // 서버 응답 [Status(1)][StartOffset(8)]
                            var resp = new byte[1 + 8];
                            resp[0] = startOffset > 0 ? STATUS_RESUME : STATUS_OK;
                            BinaryPrimitives.WriteUInt64BigEndian(resp.AsSpan(1, 8), (ulong)startOffset);
                            await ctx.ReplyAsync(resp).ConfigureAwait(false);

                            // 다음 단계로
                            s.State = SessionState.ReceivingData;
                            s.HeaderBuffer.Clear();
                            break;
                        }
                    case SessionState.ReceivingData:
                        {
                            // 남은 데이터량
                            long remaining = s.FileSize - s.ReceivedAfterOffset;
                            if (remaining <= 0)
                            {
                                s.State = SessionState.WaitTail;
                                break;
                            }

                            int toWrite = (int)Math.Min(input.Length, remaining);
                            if (toWrite > 0)
                            {
                                // 디스크 기록
                                await s.Stream!.WriteAsync(input.Slice(0, toWrite)).ConfigureAwait(false);

                                // CRC 갱신
                                s.Crc32 = Crc32.Update(s.Crc32, input.Span.Slice(0, toWrite));

                                s.ReceivedAfterOffset += toWrite;
                                input = input.Slice(toWrite);
                            }

                            if (s.ReceivedAfterOffset >= s.FileSize)
                            {
                                // 파일 본문 완료 → 테일 대기
                                s.State = SessionState.WaitTail;
                            }
                            break;
                        }
                    case SessionState.WaitTail:
                        {
                            // [MAGIC_TAIL(4)][CRC32(4)] 총 8바이트 필요
                            int need = 8;
                            int copy = Math.Min(need - s.HeaderBuffer.Length, input.Length);
                            if (copy > 0)
                            {
                                s.HeaderBuffer.Write(input.Span.Slice(0, copy));
                                input = input.Slice(copy);
                            }
                            if (s.HeaderBuffer.Length < need)
                            {
                                break;
                            }

                            var span = s.HeaderBuffer.GetBufferSpan().Slice(0, need);
                            uint magicTail = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(0, 4));
                            uint crcFromClient = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(4, 4));

                            if (magicTail != MAGIC_TAIL)
                            {
                                s.State = SessionState.Error;
                                break;
                            }

                            // CRC 보수
                            s.Crc32 ^= 0xFFFFFFFFu;

                            if (s.Crc32 != crcFromClient)
                            {
                                // CRC mismatch
                                await ctx.ReplyAsync(new byte[] { STATUS_ERROR }).ConfigureAwait(false);
                                s.State = SessionState.Error;
                                break;
                            }

                            // 성공
                            await s.Stream!.FlushAsync().ConfigureAwait(false);
                            s.State = SessionState.Done;

                            await ctx.ReplyAsync(new byte[] { STATUS_OK }).ConfigureAwait(false);
                            break;
                        }
                }
            }

            if (s.State == SessionState.Done || s.State == SessionState.Error)
            {
                CleanupSession(s);
            }
        }

        private static void TryDeleteQuiet(string path)
        {
            try { File.Delete(path); } catch { /* ignore */ }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        private static async Task SeedCrcFromExistingAsync(Session s, long upToLength)
        {
            // 기존 파일 0..upToLength-1 구간의 CRC 계산
            s.Stream!.Flush(); // ensure on-disk
            s.Stream.Seek(0, SeekOrigin.Begin);

            byte[] buf = new byte[64 * 1024];
            long pos = 0;
            while (pos < upToLength)
            {
                int toRead = (int)Math.Min(buf.Length, upToLength - pos);
                int read = await s.Stream.ReadAsync(buf.AsMemory(0, toRead)).ConfigureAwait(false);
                if (read <= 0) throw new EndOfStreamException("Unexpected EOF while CRC seeding.");
                s.Crc32 = Crc32.Update(s.Crc32, buf.AsSpan(0, read));
                pos += read;
            }

            // 다시 이어쓰기 위치로 이동
            s.Stream.Seek(upToLength, SeekOrigin.Begin);
        }

        private void CleanupSession(Session s)
        {
            if (_sessions.TryRemove(s.ClientId, out _))
            {
                try { s.Stream?.Dispose(); } catch { /* ignore */ }
                s.DisposeBuffers();
            }
        }

        // === 세션/도우미 ===

        private enum SessionState
        {
            WaitHeader,
            WaitFileName,
            ReceivingData,
            WaitTail,
            Done,
            Error
        }

        private sealed class Session
        {
            public Guid ClientId { get; }
            public readonly string StorageRoot;

            public SessionState State = SessionState.WaitHeader;

            public long FileSize;
            public string FileName = string.Empty;
            public string FullPath = string.Empty;
            public byte Flags;

            public long ReceivedAfterOffset;
            public FileStream? Stream;

            public uint Crc32;

            public int ExpectedNameBytes;

            public readonly GrowingBuffer HeaderBuffer = new();
            public readonly SemaphoreSlim Lock = new(1, 1);

            public Session(Guid id, string storageRoot)
            {
                ClientId = id;
                StorageRoot = storageRoot;
            }

            public void DisposeBuffers()
            {
                try { HeaderBuffer.Dispose(); } catch { /* ignore */ }
                try { Lock.Dispose(); } catch { /* ignore */ }
            }
        }

        /// <summary>
        /// 동적으로 증가 가능한 메모리 버퍼
        /// </summary>
        private sealed class GrowingBuffer : IDisposable
        {
            private byte[] _buf = new byte[0];
            private int _len;

            public int Length => _len;

            public void Write(ReadOnlySpan<byte> src)
            {
                EnsureCapacity(_len + src.Length);
                src.CopyTo(_buf.AsSpan(_len));
                _len += src.Length;
            }

            public void Clear() => _len = 0;

            public ReadOnlySpan<byte> GetBufferSpan() => _buf.AsSpan(0, _len);

            private void EnsureCapacity(int need)
            {
                if (_buf.Length >= need) return;
                int cap = _buf.Length == 0 ? 256 : _buf.Length;
                while (cap < need) cap *= 2;
                Array.Resize(ref _buf, cap);
            }

            public void Dispose() => _buf = Array.Empty<byte>();
        }

        /// <summary>
        /// 표준 CRC32 계산기 (poly 0xEDB88320)
        /// </summary>
        private static class Crc32
        {
            private const uint Poly = 0xEDB88320u;
            private static readonly uint[] Table = CreateTable();

            private static uint[] CreateTable()
            {
                var t = new uint[256];
                for (uint i = 0; i < 256; i++)
                {
                    uint c = i;
                    for (int k = 0; k < 8; k++)
                        c = (c & 1) != 0 ? (Poly ^ (c >> 1)) : (c >> 1);
                    t[i] = c;
                }
                return t;
            }

            public static uint Update(uint crc, ReadOnlySpan<byte> data)
            {
                uint c = crc;
                foreach (byte b in data)
                    c = Table[(c ^ b) & 0xFF] ^ (c >> 8);
                return c;
            }
        }
    }
}
