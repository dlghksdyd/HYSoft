namespace HYSoft.Communication.Tcp.Server.Protocol.FileTransfer
{
    internal enum SessionState
    {
        Init,
        Receiving,
        Completed,
        Faulted
    }
}