using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing; // ← 통일: System.IO.Hashing.Crc32
using System.Text;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    internal sealed class FileTransferSession : IAsyncDisposable
    {
        private readonly FileTransferServerOptions _options;
        private readonly object _gate = new();
        private readonly MemoryStream _rxBuffer = new(); // 메시지 파서용 누적 버퍼

        private FileStream? _stream;
        private string? _targetAbsPath;
        private string? _tempAbsPath;

        private ulong _expectedSize;
        private ulong _written;
        private SessionState _state = SessionState.Init;
        private bool _disposed;

        // CRC32 해시 (증분)
        private readonly Crc32 _crc32 = new();

        public Guid ClientId { get; }

        public FileTransferSession(Guid clientId, FileTransferServerOptions options)
        {
            ClientId = clientId;
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            try { _stream?.Dispose(); }
            catch { /* ignore */ }
            await Task.CompletedTask;
        }

        /// <summary>
        /// TcpServer 수신 바이트를 계속 밀어넣는다. 그에 따른 "응답 패킷들"을 0개 이상 돌려준다.
        /// </summary>
        public List<byte[]> OnBytes(ReadOnlySpan<byte> data)
        {
            lock (_gate)
            {
                _rxBuffer.Position = _rxBuffer.Length;
                _rxBuffer.Write(data);
                _rxBuffer.Position = 0;

                var replies = new List<byte[]>();

                while (TryParseOneMessage(_rxBuffer, out var reply))
                {
                    if (reply is { Length: > 0 })
                        replies.Add(reply);
                }

                // 처리 후 남은 바이트들을 앞당기기(consume된 만큼 제거)
                if (_rxBuffer.Position > 0)
                {
                    var remain = (int)(_rxBuffer.Length - _rxBuffer.Position);
                    if (remain > 0)
                    {
                        var buf = _rxBuffer.GetBuffer();
                        Buffer.BlockCopy(buf, (int)_rxBuffer.Position, buf, 0, remain);
                        _rxBuffer.SetLength(remain);
                        _rxBuffer.Position = 0;
                    }
                    else
                    {
                        _rxBuffer.SetLength(0);
                        _rxBuffer.Position = 0;
                    }
                }

                return replies;
            }
        }

        private bool TryParseOneMessage(MemoryStream ms, out byte[]? reply)
        {
            reply = null;

            if (ms.Length - ms.Position < 1)
                return false;

            var buf = ms.GetBuffer();
            int p = (int)ms.Position;
            var op = (OpCode)buf[p++];

            switch (op)
            {
                case OpCode.Hello:
                    {
                        // [u16 ver][u64 size][u16 pathLen][pathUtf8...]
                        if (ms.Length - p < 2 + 8 + 2) { ms.Position = p - 1; return false; }

                        ushort ver = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(p, 2)); p += 2;
                        if (ver != 1) { SetFault(); reply = EncodeResult(ResultCode.BadRequest, 0, 0); ms.Position = p; return true; }

                        ulong size = BinaryPrimitives.ReadUInt64LittleEndian(buf.AsSpan(p, 8)); p += 8;
                        ushort pathLen = BinaryPrimitives.ReadUInt16LittleEndian(buf.AsSpan(p, 2)); p += 2;

                        if (ms.Length - p < pathLen) { ms.Position = p - (2 + 8 + 2 + 1); return false; }

                        var path = Encoding.UTF8.GetString(buf, p, pathLen); p += pathLen;

                        ms.Position = p;

                        reply = HandleHello(size, path);
                        return true;
                    }

                case OpCode.Data:
                    {
                        // [u32 chunkLen][bytes...]
                        if (ms.Length - p < 4) { ms.Position = p - 1; return false; }
                        uint chunkLen = BinaryPrimitives.ReadUInt32LittleEndian(buf.AsSpan(p, 4)); p += 4;

                        if (ms.Length - p < chunkLen) { ms.Position = p - (1 + 4); return false; }

                        var segment = buf.AsSpan(p, (int)chunkLen);
                        p += (int)chunkLen;
                        ms.Position = p;

                        reply = HandleData(segment);
                        return true;
                    }

                case OpCode.Final:
                    {
                        // [u32 crc32Client (LE)]
                        if (ms.Length - p < 4) { ms.Position = p - 1; return false; }
                        uint crcClient = BinaryPrimitives.ReadUInt32LittleEndian(buf.AsSpan(p, 4)); p += 4;
                        ms.Position = p;

                        reply = HandleFinal(crcClient);
                        return true;
                    }

                default:
                    SetFault();
                    reply = EncodeResult(ResultCode.BadRequest, _written, 0);
                    ms.Position = p;
                    return true;
            }
        }

        private byte[] HandleHello(ulong size, string relativePath)
        {
            if (_state != SessionState.Init)
                return EncodeResult(ResultCode.StateError, _written, 0);

            if ((long)size < 0 || (long)size > _options.MaxFileSizeBytes)
            {
                SetFault();
                return EncodeResult(ResultCode.BadRequest, 0, 0);
            }

            // 상대 경로 sanitize
            if (_options.SanitizeRelativePath)
            {
                relativePath = relativePath.Replace('\\', '/');
                while (relativePath.Contains("//")) relativePath = relativePath.Replace("//", "/");
                if (relativePath.StartsWith("/")) relativePath = relativePath.TrimStart('/');
                if (relativePath.Contains("..")) relativePath = relativePath.Replace("..", "");
            }

            var root = Path.GetFullPath(_options.RootDirectory);
            var abs = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!abs.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                SetFault();
                return EncodeResult(ResultCode.BadRequest, 0, 0);
            }

            var dir = Path.GetDirectoryName(abs)!;
            Directory.CreateDirectory(dir);

            var temp = abs + _options.TempExtension;

            _targetAbsPath = abs;
            _tempAbsPath = temp;
            _expectedSize = size;

            long resume = 0;

            try
            {
                // 비동기 + 순차 스캔 + 큰 버퍼로 파일 열기
                _stream = new FileStream(
                    path: temp,
                    mode: FileMode.OpenOrCreate,
                    access: FileAccess.ReadWrite,
                    share: FileShare.None,
                    bufferSize: 1024 * 1024, // 1 MiB
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan
                );

                // 재개 길이 및 사전 할당
                var curLen = _stream.Length;

                if ((ulong)curLen > _expectedSize)
                {
                    _stream.SetLength((long)_expectedSize);
                    resume = (long)_expectedSize;
                }
                else
                {
                    if ((ulong)curLen < _expectedSize)
                        _stream.SetLength((long)_expectedSize);
                    resume = curLen;
                }

                // 기존 데이터 구간의 CRC 누적(재해시)
                if (resume > 0)
                {
                    const int BUFSZ = 1024 * 1024;
                    var buf = new byte[BUFSZ];

                    _stream.Position = 0;
                    long left = resume;
                    while (left > 0)
                    {
                        int take = (int)Math.Min(BUFSZ, left);
                        int r = _stream.Read(buf, 0, take);
                        if (r <= 0) break;
                        _crc32.Append(buf.AsSpan(0, r));
                        left -= r;
                    }
                }

                // 쓰기 시작 위치는 resume
                _stream.Position = resume;

                _written = (ulong)resume;
                _state = SessionState.Receiving;

                // RESUME 응답
                var resp = new byte[1 + 8];
                resp[0] = (byte)OpCode.Resume;
                BinaryPrimitives.WriteUInt64LittleEndian(resp.AsSpan(1, 8), (ulong)resume);
                return resp;
            }
            catch
            {
                SetFault();
                return EncodeResult(ResultCode.IoError, _written, 0);
            }
        }

        private byte[]? HandleData(ReadOnlySpan<byte> chunk)
        {
            if (_state != SessionState.Receiving || _stream is null)
                return EncodeResult(ResultCode.StateError, _written, 0);

            try
            {
                if (_written + (ulong)chunk.Length > _expectedSize)
                {
                    SetFault();
                    return EncodeResult(ResultCode.BadRequest, _written, 0);
                }

                _stream.Write(chunk);
                _written += (ulong)chunk.Length;

                // 진행형 CRC 누적
                _crc32.Append(chunk);

                // DATA는 응답 없음
                return null;
            }
            catch
            {
                SetFault();
                return EncodeResult(ResultCode.IoError, _written, 0);
            }
        }

        private byte[] HandleFinal(uint crcFromClient /* little-endian 값 */)
        {
            if (_state != SessionState.Receiving || _stream is null)
                return EncodeResult(ResultCode.StateError, _written, 0);

            try
            {
                _stream.Flush(true);
                _stream.Dispose();
                _stream = null;

                // 서버 최종 CRC 계산: GetCurrentHash()는 big-endian 바이트를 반환
                Span<byte> hashBytes = stackalloc byte[4];
                _crc32.GetCurrentHash(hashBytes);
                uint crcServer = BinaryPrimitives.ReadUInt32BigEndian(hashBytes);

                if (_written != _expectedSize)
                {
                    SetFault();
                    return EncodeResult(ResultCode.BadRequest, _written, crcServer);
                }

                if (!string.IsNullOrEmpty(_tempAbsPath) && !string.IsNullOrEmpty(_targetAbsPath))
                {
                    if (File.Exists(_targetAbsPath))
                        File.Delete(_targetAbsPath);
                    File.Move(_tempAbsPath, _targetAbsPath);
                }

                _state = SessionState.Completed;

                if (crcServer != crcFromClient)
                    return EncodeResult(ResultCode.CrcMismatch, _written, crcServer);

                return EncodeResult(ResultCode.Ok, _written, crcServer);
            }
            catch
            {
                SetFault();
                return EncodeResult(ResultCode.IoError, _written, 0);
            }
        }

        private void SetFault()
        {
            _state = SessionState.Faulted;
            try { _stream?.Dispose(); } catch { }
            _stream = null;
        }

        private static byte[] EncodeResult(ResultCode code, ulong bytesWritten, uint crc /* 숫자값 */)
        {
            // RESULT: [Op(1)=0x31][u8 status][u64 bytesWritten][u32 crc32Server(LE)]
            var resp = new byte[1 + 1 + 8 + 4];
            resp[0] = (byte)OpCode.Result;
            resp[1] = (byte)code;
            BinaryPrimitives.WriteUInt64LittleEndian(resp.AsSpan(2, 8), bytesWritten);
            BinaryPrimitives.WriteUInt32LittleEndian(resp.AsSpan(10, 4), crc);
            return resp;
        }
    }
}
