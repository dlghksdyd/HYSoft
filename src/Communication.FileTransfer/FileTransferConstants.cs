namespace HYSoft.Communication.FileTransfer
{
    /// <summary>
    /// 파일 전송 프로토콜 공용 상수.
    /// </summary>
    public static class FileTransferConstants
    {
        /// <summary>헤더 매직 바이트 "FT10"</summary>
        public const uint MagicHeader = 0x46543130;

        /// <summary>테일 매직 바이트 "FTEN"</summary>
        public const uint MagicTail = 0x4654454E;

        public const byte StatusOk = 0x00;
        public const byte StatusResume = 0x01;
        public const byte StatusError = 0xFF;

        /// <summary>기본 전송 청크 크기 (64 KB)</summary>
        public const int DefaultChunkSize = 64 * 1024;
    }
}
