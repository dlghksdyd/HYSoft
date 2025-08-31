using System;

namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    internal enum ResultCode : byte
    {
        Ok = 0,
        CrcMismatch = 1,
        IoError = 2,
        BadRequest = 3,
        StateError = 4,
    }
}