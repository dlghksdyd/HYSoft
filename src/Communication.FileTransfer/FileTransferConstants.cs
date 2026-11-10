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

        /// <summary>
        /// 받는 중인 파일의 접미사. 서버는 <c>이름.ftpart</c> 에 받고 전체 CRC 가 맞을 때만 <c>이름</c> 으로 바꾼다.
        /// 남아 있는 조각 파일은 다음 전송의 이어받기 시작점이다.
        /// </summary>
        public const string PartialFileSuffix = ".ftpart";

        /// <summary>기본 전송 청크 크기 (64 KB)</summary>
        public const int DefaultChunkSize = 64 * 1024;
    }
}
