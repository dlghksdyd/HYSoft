using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Hashing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Client.Protocol.FileTransfer
{
    /// <summary>
    /// 파일 업로드(클라이언트→서버) 전용 구현.
    /// - 재개(Resume) 지원(서버가 지원 시)
    /// - 진행률 콜백 지원(IProgress<long>)
    /// - 취소 토큰 지원
    /// </summary>
    public sealed class FileTransferClient
    {
        private readonly IByteTransport _transport;

        public FileTransferClient(IByteTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        /// <summary>
        /// 원격 서버로 파일을 업로드한다.
        /// </summary>
        /// <param name="localFilePath">업로드할 로컬 파일 경로</param>
        /// <param name="remotePath">서버가 저장할 상대/절대 경로(서버 규칙 따름)</param>
        /// <param name="chunkSize">송신 청크 크기(기본 2MiB, 256KiB~4MiB 가드)</param>
        /// <param name="progress">전송 바이트 누계 콜백</param>
        public async Task<(bool Ok, ulong BytesTransferred, uint ServerCrc, byte ResultCode)>
            SendFileAsync(
                string localFilePath,
                string remotePath,
                int chunkSize = 2 * 1024 * 1024,
                IProgress<long>? progress = null,
                CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(localFilePath))
                throw new ArgumentException("localFilePath is required.", nameof(localFilePath));
            if (string.IsNullOrWhiteSpace(remotePath))
                throw new ArgumentException("remotePath is required.", nameof(remotePath));

            // 가드: 청크 크기 범위 (256 KiB ~ 4 MiB)
            chunkSize = Math.Clamp(chunkSize, 256 * 1024, 4 * 1024 * 1024);

            // 파일 정보
            var fi = new FileInfo(localFilePath);
            if (!fi.Exists) throw new FileNotFoundException("Local file not found.", localFilePath);
            ulong fileSize = (ulong)fi.Length;

            // === 1) HELLO 전송 ===
            // [Op(1)=0x10][u16 ver=1][u64 size][u16 pathLen][pathUtf8...]
            var pathUtf8 = Encoding.UTF8.GetBytes(remotePath);
            if (pathUtf8.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(remotePath), "Path too long.");

            int helloLen = 1 + 2 + 8 + 2 + pathUtf8.Length;
            byte[] hello = ArrayPool<byte>.Shared.Rent(helloLen);
            try
            {
                int p = 0;
                hello[p++] = 0x10; // Hello
                BinaryPrimitives.WriteUInt16LittleEndian(hello.AsSpan(p, 2), 1); p += 2;
                BinaryPrimitives.WriteUInt64LittleEndian(hello.AsSpan(p, 8), fileSize); p += 8;
                BinaryPrimitives.WriteUInt16LittleEndian(hello.AsSpan(p, 2), (ushort)pathUtf8.Length); p += 2;
                pathUtf8.AsSpan().CopyTo(hello.AsSpan(p)); p += pathUtf8.Length;

                await _transport.SendAsync(hello.AsMemory(0, p), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(hello);
            }

            // === 2) RESUME 수신 ===
            // [Op(1)=0x11][u64 offset]
            byte[] resumeArr = ArrayPool<byte>.Shared.Rent(1 + 8);
            try
            {
                await _transport.ReceiveExactAsync(resumeArr.AsMemory(0, 1 + 8), cancellationToken).ConfigureAwait(false);

                if (resumeArr[0] != 0x11)
                    throw new IOException($"Protocol error: expected RESUME(0x11), got 0x{resumeArr[0]:X2}.");

                ulong resumeOffset = BinaryPrimitives.ReadUInt64LittleEndian(resumeArr.AsSpan(1, 8));

                // 진행률 초기 보고
                progress?.Report((long)resumeOffset);

                // === 3) 파일 열기(최적화) & 파이프라인 읽기 ===
                long totalSent = (long)resumeOffset;
                var crc32 = new Crc32(); // .NET 8 System.IO.Hashing
                byte[] bufA = ArrayPool<byte>.Shared.Rent(chunkSize);
                byte[] bufB = ArrayPool<byte>.Shared.Rent(chunkSize);

                try
                {
                    using var fs = new FileStream(
                        path: localFilePath,
                        mode: FileMode.Open,
                        access: FileAccess.Read,
                        share: FileShare.Read,
                        bufferSize: 1024 * 1024, // 1 MiB
                        options: FileOptions.Asynchronous | FileOptions.SequentialScan);

                    // 재개 위치로 이동
                    fs.Position = (long)resumeOffset;

                    // 초기 버퍼 채우기
                    int read = await fs.ReadAsync(bufA.AsMemory(0, chunkSize), cancellationToken).ConfigureAwait(false);
                    byte[] cur = bufA;
                    byte[] nxt = bufB;

                    while (read > 0)
                    {
                        // 다음 청크 읽기 시작 (파이프라이닝)
                        var readNextTask = fs.ReadAsync(nxt.AsMemory(0, chunkSize), cancellationToken).AsTask();

                        // === 4) DATA 전송 ===
                        // DATA: [Op(1)=0x20][u32 chunkLen][bytes...]
                        int frameLen = 1 + 4 + read;
                        byte[] frame = ArrayPool<byte>.Shared.Rent(frameLen);
                        try
                        {
                            int q = 0;
                            frame[q++] = 0x20; // DATA
                            BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(q, 4), (uint)read); q += 4;
                            cur.AsSpan(0, read).CopyTo(frame.AsSpan(q)); q += read;

                            await _transport.SendAsync(frame.AsMemory(0, q), cancellationToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(frame);
                        }

                        // CRC 누적(클라이언트 측)
                        crc32.Append(cur.AsSpan(0, read));

                        // 진행률
                        totalSent += read;
                        progress?.Report(totalSent);

                        // 다음 반복 준비
                        read = await readNextTask.ConfigureAwait(false);
                        (cur, nxt) = (nxt, cur);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bufA);
                    ArrayPool<byte>.Shared.Return(bufB);
                }

                // 최종 CRC 추출 (IEEE CRC-32). System.IO.Hashing은 big-endian 바이트를 반환함.
                Span<byte> crcOut = stackalloc byte[4];
                crc32.GetCurrentHash(crcOut);
                uint crcClient = BinaryPrimitives.ReadUInt32BigEndian(crcOut); // ⬅️ BigEndian로 읽기

                // === 5) FINAL 전송 ===
                // FINAL: [Op(1)=0x30][u32 crc32]  (우리는 '리틀엔디언'으로 전송)
                byte[] finalArr = new byte[1 + 4];
                finalArr[0] = 0x30;
                BinaryPrimitives.WriteUInt32LittleEndian(finalArr.AsSpan(1, 4), crcClient);
                await _transport.SendAsync(finalArr.AsMemory(), cancellationToken).ConfigureAwait(false);

                // === 6) RESULT 수신 ===
                // RESULT: [Op(1)=0x31][u8 status][u64 bytesWritten][u32 crc32Server]
                byte[] resultBuf = ArrayPool<byte>.Shared.Rent(1 + 1 + 8 + 4);
                try
                {
                    await _transport.ReceiveExactAsync(resultBuf.AsMemory(0, 1 + 1 + 8 + 4), cancellationToken).ConfigureAwait(false);

                    if (resultBuf[0] != 0x31)
                        throw new IOException($"Protocol error: expected RESULT(0x31), got 0x{resultBuf[0]:X2}.");

                    byte status = resultBuf[1];
                    ulong bytesWritten = BinaryPrimitives.ReadUInt64LittleEndian(resultBuf.AsSpan(2, 8));
                    uint serverCrc = BinaryPrimitives.ReadUInt32LittleEndian(resultBuf.AsSpan(10, 4));

                    bool ok = status == 0 && bytesWritten == fileSize;
                    return (ok, bytesWritten, serverCrc, status);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(resultBuf);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(resumeArr);
            }
        }
    }
}
