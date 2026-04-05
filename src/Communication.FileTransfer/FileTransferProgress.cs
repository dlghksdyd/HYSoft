namespace HYSoft.Communication.FileTransfer
{
    /// <summary>
    /// 파일 전송 진행률 정보.
    /// </summary>
    public readonly struct FileTransferProgress
    {
        /// <summary>현재까지 전송된 바이트 수.</summary>
        public long BytesTransferred { get; }

        /// <summary>전체 전송 바이트 수.</summary>
        public long TotalBytes { get; }

        /// <summary>진행률 (0.0 ~ 1.0).</summary>
        public double Percentage => TotalBytes > 0 ? (double)BytesTransferred / TotalBytes : 0.0;

        public FileTransferProgress(long bytesTransferred, long totalBytes)
        {
            BytesTransferred = bytesTransferred;
            TotalBytes = totalBytes;
        }
    }
}
