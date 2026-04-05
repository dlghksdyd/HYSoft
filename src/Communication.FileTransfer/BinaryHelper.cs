namespace HYSoft.Communication.FileTransfer
{
    /// <summary>
    /// Big-Endian 바이트 직렬화/역직렬화 헬퍼.
    /// </summary>
    internal static class BinaryHelper
    {
        public static void WriteUInt16BE(byte[] buf, int offset, ushort value)
        {
            buf[offset] = (byte)(value >> 8);
            buf[offset + 1] = (byte)value;
        }

        public static void WriteUInt32BE(byte[] buf, int offset, uint value)
        {
            buf[offset] = (byte)(value >> 24);
            buf[offset + 1] = (byte)(value >> 16);
            buf[offset + 2] = (byte)(value >> 8);
            buf[offset + 3] = (byte)value;
        }

        public static void WriteUInt64BE(byte[] buf, int offset, ulong value)
        {
            buf[offset] = (byte)(value >> 56);
            buf[offset + 1] = (byte)(value >> 48);
            buf[offset + 2] = (byte)(value >> 40);
            buf[offset + 3] = (byte)(value >> 32);
            buf[offset + 4] = (byte)(value >> 24);
            buf[offset + 5] = (byte)(value >> 16);
            buf[offset + 6] = (byte)(value >> 8);
            buf[offset + 7] = (byte)value;
        }

        public static ushort ReadUInt16BE(byte[] buf, int offset)
            => (ushort)((buf[offset] << 8) | buf[offset + 1]);

        public static uint ReadUInt32BE(byte[] buf, int offset)
            => ((uint)buf[offset] << 24) | ((uint)buf[offset + 1] << 16) |
               ((uint)buf[offset + 2] << 8) | buf[offset + 3];

        public static ulong ReadUInt64BE(byte[] buf, int offset)
            => ((ulong)buf[offset] << 56) | ((ulong)buf[offset + 1] << 48) |
               ((ulong)buf[offset + 2] << 40) | ((ulong)buf[offset + 3] << 32) |
               ((ulong)buf[offset + 4] << 24) | ((ulong)buf[offset + 5] << 16) |
               ((ulong)buf[offset + 6] << 8) | buf[offset + 7];
    }
}
