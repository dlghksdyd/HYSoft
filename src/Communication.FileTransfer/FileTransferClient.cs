using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HYSoft.Communication.Tcp.Client;
using static HYSoft.Communication.FileTransfer.BinaryHelper;
using static HYSoft.Communication.FileTransfer.FileTransferConstants;

namespace HYSoft.Communication.FileTransfer
{
    /// <summary>
    /// 파일 전송 클라이언트 구현 클래스입니다.
    /// </summary>
    public class FileTransferClient : IDisposable
    {
        private readonly TcpClient _client;
        private bool _disposed;

        private bool _isSending;
        private readonly object _lock = new object();

        public FileTransferClient(TcpClientOptions options)
        {
            _client = TcpClientManager.Create(options);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            TcpClientManager.Dispose(_client);
        }

        /// <summary>
        /// 파일을 서버로 비동기 전송합니다.
        /// </summary>
        /// <param name="filePath">전송할 파일 경로.</param>
        /// <param name="progress">진행률 콜백 (선택).</param>
        /// <param name="cancellationToken">취소 토큰 (선택).</param>
        public async Task SendFileAsync(
            string filePath,
            IProgress<FileTransferProgress>? progress = null,
            CancellationToken cancellationToken = default)
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

                cancellationToken.ThrowIfCancellationRequested();

                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                string fileName = fileInfo.Name;
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                if (fileNameBytes.Length > ushort.MaxValue)
                    throw new InvalidOperationException("File name is too long for protocol.");

                await _client.ConnectAsync().ConfigureAwait(false);

                // Header: [MAGIC(4)][Flags(1)][FileSize(8)][NameLen(2)][FileName(?)]
                byte[] header = new byte[4 + 1 + 8 + 2 + fileNameBytes.Length];
                int offset = 0;
                WriteUInt32BE(header, offset, MagicHeader); offset += 4;
                header[offset++] = 0x00;
                WriteUInt64BE(header, offset, (ulong)fileSize); offset += 8;
                WriteUInt16BE(header, offset, (ushort)fileNameBytes.Length); offset += 2;
                Array.Copy(fileNameBytes, 0, header, offset, fileNameBytes.Length);
                await _client.SendAsync(header).ConfigureAwait(false);

                // Server response: [Status(1)][StartOffset(8)]
                byte[] resp = new byte[9];
                await _client.ReceiveAsync(resp).ConfigureAwait(false);
                byte status = resp[0];
                long startOffset = (long)ReadUInt64BE(resp, 1);

                if (status != StatusOk && status != StatusResume)
                    throw new IOException($"Server rejected header. Status=0x{status:X2}");
                if (startOffset < 0 || startOffset > fileSize)
                    throw new IOException($"Invalid resume offset from server: {startOffset}");

                // Send file data
                uint crc32 = 0xFFFFFFFFu;
                byte[] buffer = new byte[DefaultChunkSize];
                long totalToSend = fileSize - startOffset;
                long sentSoFar = 0;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultChunkSize, true))
                {
                    // CRC for 0..startOffset (no send)
                    long pos = 0;
                    while (pos < startOffset)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        int toRead = (int)Math.Min(DefaultChunkSize, startOffset - pos);
                        int read = await fs.ReadAsync(buffer, 0, toRead).ConfigureAwait(false);
                        if (read <= 0) throw new EndOfStreamException("Unexpected EOF while pre-CRC calculation.");
                        crc32 = Crc32.Update(crc32, buffer, 0, read);
                        pos += read;
                    }

                    // Send startOffset..end + CRC
                    while (pos < fileSize)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        int toRead = (int)Math.Min(DefaultChunkSize, fileSize - pos);
                        int read = await fs.ReadAsync(buffer, 0, toRead).ConfigureAwait(false);
                        if (read <= 0) throw new EndOfStreamException("Unexpected EOF while sending.");
                        await _client.SendAsync(buffer, 0, read).ConfigureAwait(false);
                        crc32 = Crc32.Update(crc32, buffer, 0, read);
                        pos += read;
                        sentSoFar += read;
                        progress?.Report(new FileTransferProgress(sentSoFar, totalToSend));
                    }
                }

                crc32 ^= 0xFFFFFFFFu;

                // Tail: [MAGIC_TAIL(4)][CRC32(4)]
                byte[] tail = new byte[8];
                WriteUInt32BE(tail, 0, MagicTail);
                WriteUInt32BE(tail, 4, crc32);
                await _client.SendAsync(tail).ConfigureAwait(false);

                // Final status
                byte[] finalResp = new byte[1];
                await _client.ReceiveAsync(finalResp).ConfigureAwait(false);
                if (finalResp[0] != StatusOk)
                    throw new IOException($"Server reported error. Status=0x{finalResp[0]:X2}");
            }
            finally
            {
                lock (_lock) { _isSending = false; }
            }
        }
    }
}
