using System;

namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    internal enum OpCode : byte
    {
        Hello = 0x10,
        Resume = 0x11,
        Data = 0x20,
        Final = 0x30,
        Result = 0x31,
    }
}