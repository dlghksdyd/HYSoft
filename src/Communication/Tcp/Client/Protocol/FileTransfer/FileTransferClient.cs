using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Client.Protocol.FileTransfer
{
    public class FileTransferClient
    {
        private readonly TcpClient _client;

        // 프로토콜 상수
        private const uint MAGIC_HEADER = 0x46543130; // "FT10"
        private const uint MAGIC_TAIL = 0x4654454E;   // "FTEN"
        private const byte STATUS_OK = 0x00;
        private const byte STATUS_RESUME = 0x01;
        private const byte STATUS_ERROR = 0xFF;

        // 송신 청크 크기
        private const int DefaultChunkSize = 64 * 1024;

        // 상태 플래그
        private bool _isSending;
        private readonly object _lock = new();

        public FileTransferClient(TcpClientOptions options)
        {
            _client = TcpClientManager.Create(options);
        }

        /// <summary>
        /// 파일을 서버로 전송합니다. 서버는 다음 핸드셰이크를 따릅니다.
        /// 1) 클라이언트 → 서버: [MAGIC_HEADER(4)][Flags(1)][FileSize(8)][NameLen(2)][FileName(UTF-8)]
        /// 2) 서버 → 클라이언트: [Status(1)][StartOffset(8)]  (Status=OK이면 0부터, RESUME면 지정 오프셋부터)
        /// 3) 클라이언트 → 서버: 파일 데이터를 StartOffset부터 순차 전송 (청크 스트리밍)
        /// 4) 클라이언트 → 서버: [MAGIC_TAIL(4)][CRC32(4)]
        /// 5) 서버 → 클라이언트: [FinalStatus(1)] (OK=수신/검증 성공)
        /// </summary>
        /// <param name="filePath">전송할 로컬 파일 경로</param>
        public async Task SendFileAsync(string filePath)
        {
            lock (_lock)
            {
                if (_isSending)
                    throw new InvalidOperationException("A file transfer is already in progress.");
                _isSending = true;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("filePath is null or empty.", nameof(filePath));
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File not found.", filePath);

                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;
                if (fileSize < 0)
                    throw new IOException("Invalid file size.");

                string fileName = fileInfo.Name;
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                if (fileNameBytes.Length > ushort.MaxValue)
                    throw new InvalidOperationException("File name is too long for protocol.");

                // 1) 서버 연결
                await _client.ConnectAsync().ConfigureAwait(false);

                // 2) 헤더 송신
                // [MAGIC_HEADER(4)][Flags(1)][FileSize(8)][NameLen(2)][FileName(?)]
                // Flags: 현재 0 (향후 옵션 확장용)
                byte[] header = new byte[4 + 1 + 8 + 2 + fileNameBytes.Length];
                int offset = 0;
                BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(offset, 4), MAGIC_HEADER); offset += 4;
                header[offset++] = 0x00; // Flags
                BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(offset, 8), (ulong)fileSize); offset += 8;
                BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(offset, 2), (ushort)fileNameBytes.Length); offset += 2;
                fileNameBytes.AsSpan().CopyTo(header.AsSpan(offset));
                await _client.SendAsync(header).ConfigureAwait(false);

                // 3) 서버 응답 수신: [Status(1)][StartOffset(8)]
                byte[] resp = new byte[1 + 8];
                await _client.ReceiveAsync(resp).ConfigureAwait(false);
                byte status = resp[0];
                ulong startOffsetU = BinaryPrimitives.ReadUInt64BigEndian(resp.AsSpan(1, 8));
                long startOffset = (long)startOffsetU;

                if (status != STATUS_OK && status != STATUS_RESUME)
                    throw new IOException($"Server rejected header. Status=0x{status:X2}");

                if (startOffset < 0 || startOffset > fileSize)
                    throw new IOException($"Invalid resume offset from server: {startOffset}");

                // 4) 파일 데이터 전송 (StartOffset부터)
                uint crc32 = 0xFFFFFFFFu;
                const int chunkSize = DefaultChunkSize;

                // 파일 전체 CRC를 검증해야 하므로 전송 범위 외 부분도 CRC에 포함되어야 함.
                //  - 서버가 resume을 지시했더라도 CRC는 파일 전체 기준으로 계산하여 tail에서 보냄.
                //  - 서버도 동일 규칙으로 복구/검증한다고 가정.
                await using (var fs = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: chunkSize,
                    useAsync: true))
                {
                    byte[] buffer = new byte[chunkSize];

                    // (a) 먼저 0 ~ StartOffset-1 구간의 CRC만 계산 (송신은 안 함)
                    long pos = 0;
                    while (pos < startOffset)
                    {
                        int toRead = (int)Math.Min(chunkSize, startOffset - pos);
                        int read = await fs.ReadAsync(buffer.AsMemory(0, toRead)).ConfigureAwait(false);
                        if (read <= 0) throw new EndOfStreamException("Unexpected EOF while pre-CRC calculation.");
                        crc32 = Crc32.Update(crc32, buffer.AsSpan(0, read));
                        pos += read;
                    }

                    // (b) StartOffset ~ 파일 끝까지는 읽으면서 송신 + CRC 누적
                    while (pos < fileSize)
                    {
                        int toRead = (int)Math.Min(chunkSize, fileSize - pos);
                        int read = await fs.ReadAsync(buffer.AsMemory(0, toRead)).ConfigureAwait(false);
                        if (read <= 0) throw new EndOfStreamException("Unexpected EOF while sending.");

                        // 송신
                        await _client.SendAsync(new ReadOnlyMemory<byte>(buffer, 0, read)).ConfigureAwait(false);

                        // CRC
                        crc32 = Crc32.Update(crc32, buffer.AsSpan(0, read));
                        pos += read;
                    }
                }

                // CRC 보수
                crc32 ^= 0xFFFFFFFFu;

                // 5) 테일 송신: [MAGIC_TAIL(4)][CRC32(4)]
                byte[] tail = new byte[8];
                BinaryPrimitives.WriteUInt32BigEndian(tail.AsSpan(0, 4), MAGIC_TAIL);
                BinaryPrimitives.WriteUInt32BigEndian(tail.AsSpan(4, 4), crc32);
                await _client.SendAsync(tail).ConfigureAwait(false);

                // 6) 최종 상태 수신: [FinalStatus(1)]
                byte[] finalResp = new byte[1];
                await _client.ReceiveAsync(finalResp).ConfigureAwait(false);

                if (finalResp[0] != STATUS_OK)
                    throw new IOException($"Server reported error on finalize. Status=0x{finalResp[0]:X2}");
            }
            finally
            {
                lock (_lock)
                {
                    _isSending = false;
                }
            }
        }

        /// <summary>
        /// RFC 1952 형태의 표준 CRC32 (Polynomial 0xEDB88320) 계산기.
        /// </summary>
        private static class Crc32
        {
            private const uint Poly = 0xEDB88320u;
            private static readonly uint[] Table = CreateTable();

            private static uint[] CreateTable()
            {
                var table = new uint[256];
                for (uint i = 0; i < 256; i++)
                {
                    uint c = i;
                    for (int k = 0; k < 8; k++)
                        c = (c & 1) != 0 ? (Poly ^ (c >> 1)) : (c >> 1);
                    table[i] = c;
                }
                return table;
            }

            public static uint Update(uint crc, ReadOnlySpan<byte> data)
            {
                uint c = crc;
                foreach (byte b in data)
                {
                    c = Table[(c ^ b) & 0xFF] ^ (c >> 8);
                }
                return c;
            }
        }
    }
}
