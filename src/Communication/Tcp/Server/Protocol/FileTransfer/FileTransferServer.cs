#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    /// <summary>
    /// 간단한 파일 전송 서버 구현.
    /// </summary>
    public class FileTransferServer
    {
        private readonly TcpServer _server;
        private readonly string _storageRoot;
        private readonly ConcurrentDictionary<Guid, Session> _sessions = new ConcurrentDictionary<Guid, Session>();

        private const uint MAGIC_HEADER = 0x46543130;
        private const uint MAGIC_TAIL = 0x4654454E;
        private const byte STATUS_OK = 0x00;
        private const byte STATUS_RESUME = 0x01;
        private const byte STATUS_ERROR = 0xFF;

        private bool _receiving;

        public FileTransferServer(TcpServerOptions options, string? storageRoot = null)
        {
            _server = TcpServerManager.Create(options);
            _storageRoot = storageRoot ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "received");
            Directory.CreateDirectory(_storageRoot);
        }

        public async Task ReceiveFileAsync()
        {
            if (!_receiving)
            {
                _receiving = true;
                _server.DataReceived += OnDataReceived;
            }
            await _server.StartAsync().ConfigureAwait(false);
        }

        private void OnDataReceived(TcpDataReceivedContext ctx)
        {
            var session = _sessions.GetOrAdd(ctx.ClientId, _ => new Session(ctx.ClientId, _storageRoot));

            _ = Task.Run(async () =>
            {
                await session.Lock.WaitAsync().ConfigureAwait(false);
                try
                {
                    await ProcessIncomingAsync(session, ctx).ConfigureAwait(false);
                }
                catch
                {
                    try { await ctx.ReplyAsync(new byte[] { STATUS_ERROR }).ConfigureAwait(false); } catch { }
                    CleanupSession(session);
                }
                finally
                {
                    try { session.Lock.Release(); }
                    catch (ObjectDisposedException) { }
                }
            });
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
                        int need = 4 + 1 + 8 + 2; // 15
                        int available = inputLength - inputOffset;
                        int canWrite = Math.Min(available, need - s.HeaderBuffer.Length);
                        if (canWrite > 0)
                        {
                            s.HeaderBuffer.Write(inputData, inputOffset, canWrite);
                            inputOffset += canWrite;
                        }
                        if (s.HeaderBuffer.Length < need) break;

                        byte[] span = s.HeaderBuffer.ToArray();
                        uint magic = ReadUInt32BE(span, 0);
                        if (magic != MAGIC_HEADER) { s.State = SessionState.Error; break; }

                        s.Flags = span[4];
                        s.FileSize = (long)ReadUInt64BE(span, 5);
                        ushort nameLen = ReadUInt16BE(span, 13);

                        if (s.FileSize < 0 || nameLen == 0) { s.State = SessionState.Error; break; }

                        s.State = SessionState.WaitFileName;
                        s.ExpectedNameBytes = nameLen;
                        s.HeaderBuffer.Clear();
                        break;
                    }
                    case SessionState.WaitFileName:
                    {
                        int toCopy = Math.Min(inputLength - inputOffset, s.ExpectedNameBytes - s.HeaderBuffer.Length);
                        if (toCopy > 0)
                        {
                            s.HeaderBuffer.Write(inputData, inputOffset, toCopy);
                            inputOffset += toCopy;
                        }
                        if (s.HeaderBuffer.Length < s.ExpectedNameBytes) break;

                        string fileName = Encoding.UTF8.GetString(s.HeaderBuffer.ToArray(), 0, s.ExpectedNameBytes);
                        s.FileName = SanitizeFileName(fileName);
                        s.FullPath = Path.Combine(s.StorageRoot, s.FileName);

                        long startOffset = 0;
                        Directory.CreateDirectory(Path.GetDirectoryName(s.FullPath)!);
                        if (File.Exists(s.FullPath)) TryDeleteQuiet(s.FullPath);

                        s.Stream = new FileStream(s.FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 64 * 1024, true);
                        if (s.Stream.Length != startOffset) s.Stream.SetLength(startOffset);
                        s.Stream.Seek(startOffset, SeekOrigin.Begin);

                        s.Crc32 = 0xFFFFFFFFu;
                        if (startOffset > 0) await SeedCrcFromExistingAsync(s, startOffset).ConfigureAwait(false);
                        s.ReceivedAfterOffset = startOffset;

                        var resp = new byte[9];
                        resp[0] = startOffset > 0 ? STATUS_RESUME : STATUS_OK;
                        WriteUInt64BE(resp, 1, (ulong)startOffset);
                        await ctx.ReplyAsync(resp).ConfigureAwait(false);

                        s.State = SessionState.ReceivingData;
                        s.HeaderBuffer.Clear();
                        break;
                    }
                    case SessionState.ReceivingData:
                    {
                        long remaining = s.FileSize - s.ReceivedAfterOffset;
                        if (remaining <= 0) { s.State = SessionState.WaitTail; break; }

                        int toWrite = (int)Math.Min(inputLength - inputOffset, remaining);
                        if (toWrite > 0)
                        {
                            await s.Stream!.WriteAsync(inputData, inputOffset, toWrite).ConfigureAwait(false);
                            s.Crc32 = Crc32.Update(s.Crc32, inputData, inputOffset, toWrite);
                            s.ReceivedAfterOffset += toWrite;
                            inputOffset += toWrite;
                        }

                        if (s.ReceivedAfterOffset >= s.FileSize)
                            s.State = SessionState.WaitTail;
                        break;
                    }
                    case SessionState.WaitTail:
                    {
                        int need = 8;
                        int toCopy = Math.Min(need - s.HeaderBuffer.Length, inputLength - inputOffset);
                        if (toCopy > 0)
                        {
                            s.HeaderBuffer.Write(inputData, inputOffset, toCopy);
                            inputOffset += toCopy;
                        }
                        if (s.HeaderBuffer.Length < need) break;

                        byte[] span = s.HeaderBuffer.ToArray();
                        uint magicTail = ReadUInt32BE(span, 0);
                        uint crcFromClient = ReadUInt32BE(span, 4);

                        if (magicTail != MAGIC_TAIL) { s.State = SessionState.Error; break; }

                        s.Crc32 ^= 0xFFFFFFFFu;
                        if (s.Crc32 != crcFromClient)
                        {
                            await ctx.ReplyAsync(new byte[] { STATUS_ERROR }).ConfigureAwait(false);
                            s.State = SessionState.Error;
                            break;
                        }

                        await s.Stream!.FlushAsync().ConfigureAwait(false);
                        s.State = SessionState.Done;
                        await ctx.ReplyAsync(new byte[] { STATUS_OK }).ConfigureAwait(false);
                        break;
                    }
                }
            }

            if (s.State == SessionState.Done || s.State == SessionState.Error)
                CleanupSession(s);
        }

        private static void TryDeleteQuiet(string path) { try { File.Delete(path); } catch { } }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        private static async Task SeedCrcFromExistingAsync(Session s, long upToLength)
        {
            s.Stream!.Flush();
            s.Stream.Seek(0, SeekOrigin.Begin);
            byte[] buf = new byte[64 * 1024];
            long pos = 0;
            while (pos < upToLength)
            {
                int toRead = (int)Math.Min(buf.Length, upToLength - pos);
                int read = await s.Stream.ReadAsync(buf, 0, toRead).ConfigureAwait(false);
                if (read <= 0) throw new EndOfStreamException("Unexpected EOF while CRC seeding.");
                s.Crc32 = Crc32.Update(s.Crc32, buf, 0, read);
                pos += read;
            }
            s.Stream.Seek(upToLength, SeekOrigin.Begin);
        }

        private void CleanupSession(Session s)
        {
            if (_sessions.TryRemove(s.ClientId, out _))
            {
                try { s.Stream?.Dispose(); } catch { }
                try { s.Lock.Dispose(); } catch { }
                s.DisposeBuffers();
            }
        }

        #region Binary Helpers

        private static uint ReadUInt32BE(byte[] buf, int offset)
            => ((uint)buf[offset] << 24) | ((uint)buf[offset + 1] << 16) | ((uint)buf[offset + 2] << 8) | buf[offset + 3];

        private static ulong ReadUInt64BE(byte[] buf, int offset)
            => ((ulong)buf[offset] << 56) | ((ulong)buf[offset + 1] << 48) | ((ulong)buf[offset + 2] << 40) |
               ((ulong)buf[offset + 3] << 32) | ((ulong)buf[offset + 4] << 24) | ((ulong)buf[offset + 5] << 16) |
               ((ulong)buf[offset + 6] << 8) | buf[offset + 7];

        private static ushort ReadUInt16BE(byte[] buf, int offset)
            => (ushort)((buf[offset] << 8) | buf[offset + 1]);

        private static void WriteUInt64BE(byte[] buf, int offset, ulong value)
        {
            buf[offset] = (byte)(value >> 56); buf[offset + 1] = (byte)(value >> 48);
            buf[offset + 2] = (byte)(value >> 40); buf[offset + 3] = (byte)(value >> 32);
            buf[offset + 4] = (byte)(value >> 24); buf[offset + 5] = (byte)(value >> 16);
            buf[offset + 6] = (byte)(value >> 8); buf[offset + 7] = (byte)value;
        }

        #endregion

        private enum SessionState { WaitHeader, WaitFileName, ReceivingData, WaitTail, Done, Error }

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
            public readonly GrowingBuffer HeaderBuffer = new GrowingBuffer();
            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

            public Session(Guid id, string storageRoot) { ClientId = id; StorageRoot = storageRoot; }
            public void DisposeBuffers() { try { HeaderBuffer.Dispose(); } catch { } }
        }

        private sealed class GrowingBuffer : IDisposable
        {
            private byte[] _buf = new byte[0];
            private int _len;
            public int Length => _len;

            public void Write(byte[] src, int offset, int count)
            {
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

            public void Dispose() => _buf = Array.Empty<byte>();
        }

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
                    for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? (Poly ^ (c >> 1)) : (c >> 1);
                    t[i] = c;
                }
                return t;
            }

            public static uint Update(uint crc, byte[] data, int offset, int count)
            {
                uint c = crc;
                for (int i = offset; i < offset + count; i++)
                    c = Table[(c ^ data[i]) & 0xFF] ^ (c >> 8);
                return c;
            }
        }
    }
}
