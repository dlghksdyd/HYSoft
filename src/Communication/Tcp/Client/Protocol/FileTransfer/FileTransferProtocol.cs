using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace HYSoft.Communication.Tcp.Client.Protocol.FileTransfer
{
    /// <summary>
    /// 단순/견고 바이너리 프로토콜 (LE)
    /// Frame:
    ///   4  : Magic 'F','T','0','1' (0x46 0x54 0x30 0x31)
    ///   1  : OpCode (1=PUT, 2=PUT_DONE)
    ///   1  : Flags (bit0: resume 가능)
    ///   2  : PathLen (utf8)
    ///   4  : ChunkSize
    ///   8  : FileSize
    ///   32 : SHA256
    ///   N  : Path(utf8)
    /// 서버 응답:
    ///   1  : Status (0=OK,1=RESUME,2=ERROR)
    ///   8  : ResumeOffset (Status=RESUME일 때 유효, 그 외 0)
    /// 전송 본문:
    ///   반복 { 4:ChunkLen; ChunkLen bytes; 4:CRC32 } … 마지막엔 ChunkLen=0
    /// 마지막 확인:
    ///   1: FinalStatus (0=OK, 2=ERROR)
    /// </summary>
    internal static class FileTransferProtocol
    {
        public const uint Magic = 0x31305446; // 'FT01' LE
        public const byte OpPut = 0x01;
        public const byte OpPutDone = 0x02;

        public const byte StatusOk = 0x00;
        public const byte StatusResume = 0x01;
        public const byte StatusError = 0x02;

        public static byte[] BuildPutHeader(
            string path,
            int chunkSize,
            long fileSize,
            ReadOnlySpan<byte> sha256,
            bool allowResume)
        {
            var pathBytes = Encoding.UTF8.GetBytes(path);
            if (pathBytes.Length > ushort.MaxValue) throw new ArgumentException("Path too long.");

            var buf = new byte[4 + 1 + 1 + 2 + 4 + 8 + 32 + pathBytes.Length];
            var span = buf.AsSpan();
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(0, 4), Magic);
            span[4] = OpPut;
            span[5] = (byte)(allowResume ? 0x01 : 0x00);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(6, 2), (ushort)pathBytes.Length);
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(8, 4), chunkSize);
            BinaryPrimitives.WriteInt64LittleEndian(span.Slice(12, 8), fileSize);
            sha256.CopyTo(span.Slice(20, 32));
            pathBytes.AsSpan().CopyTo(span.Slice(52));
            return buf;
        }

        public static uint Crc32(ReadOnlySpan<byte> data)
        {
            // .NET에 내장 CRC32가 없으니 간단 구현(표준 IEEE 802.3)
            const uint poly = 0xEDB88320u;
            uint crc = 0xFFFFFFFFu;
            for (int i = 0; i < data.Length; i++)
            {
                uint b = data[i];
                crc ^= b;
                for (int j = 0; j < 8; j++)
                {
                    uint mask = (crc & 1) != 0 ? 0xFFFFFFFFu : 0u;
                    crc = (crc >> 1) ^ (poly & mask);
                }
            }
            return ~crc;
        }

        public static byte[] Sha256OfFile(string filePath)
        {
            using var sha = SHA256.Create();
            using var fs = System.IO.File.OpenRead(filePath);
            return sha.ComputeHash(fs);
        }
    }
}
