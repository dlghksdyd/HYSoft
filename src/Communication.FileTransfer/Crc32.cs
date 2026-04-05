namespace HYSoft.Communication.FileTransfer
{
    /// <summary>
    /// CRC-32 (IEEE 802.3) 체크섬 유틸리티.
    /// </summary>
    public static class Crc32
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

        public static uint Update(uint crc, byte[] data, int offset, int count)
        {
            uint c = crc;
            for (int i = offset; i < offset + count; i++)
                c = Table[(c ^ data[i]) & 0xFF] ^ (c >> 8);
            return c;
        }

        /// <summary>
        /// 전체 데이터에 대한 CRC-32 계산 (Init → Update → Finalize).
        /// </summary>
        public static uint Compute(byte[] data, int offset, int count)
        {
            return Update(0xFFFFFFFFu, data, offset, count) ^ 0xFFFFFFFFu;
        }
    }
}
