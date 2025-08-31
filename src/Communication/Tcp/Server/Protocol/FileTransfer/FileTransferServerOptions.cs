using System;
using System.IO;

namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    public sealed class FileTransferServerOptions
    {
        /// <summary>저장 루트 디렉터리(필수)</summary>
        public string RootDirectory { get; }

        /// <summary>임시 확장자(.part)</summary>
        public string TempExtension { get; init; } = ".part";

        /// <summary>최대 허용 파일 크기(바이트). 기본: 64GB</summary>
        public long MaxFileSizeBytes { get; init; } = 64L * 1024 * 1024 * 1024;

        /// <summary>상대 경로 내 금지 문자를 간단 필터링</summary>
        public bool SanitizeRelativePath { get; init; } = true;

        public FileTransferServerOptions(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
                throw new ArgumentException("Root directory required.", nameof(rootDirectory));

            RootDirectory = rootDirectory;
            Directory.CreateDirectory(RootDirectory);
        }
    }
}