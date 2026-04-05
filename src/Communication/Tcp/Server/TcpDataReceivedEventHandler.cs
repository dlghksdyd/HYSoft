using System;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Server
{
    /// <summary>
    /// TCP 서버에서 클라이언트로부터 데이터가 수신되었을 때 호출되는 이벤트 핸들러를 나타냅니다.
    /// </summary>
    public delegate void TcpDataReceivedEventHandler(TcpDataReceivedContext context);

    /// <summary>
    /// TCP 서버에서 특정 클라이언트로부터 데이터가 수신되었을 때 전달되는 컨텍스트입니다.
    /// </summary>
    public sealed class TcpDataReceivedContext
    {
        private readonly TcpServer _tcpServer;

        public TcpDataReceivedContext(TcpServer tcpServer, Guid clientId, byte[] data)
        {
            _tcpServer = tcpServer;
            ClientId = clientId;
            Data = data;
        }

        /// <summary>
        /// 클라이언트로부터 수신된 데이터 바이트 배열입니다.
        /// </summary>
        public readonly byte[] Data;

        /// <summary>
        /// 클라이언트의 고유 GUID입니다.
        /// </summary>
        public readonly Guid ClientId;

        /// <summary>
        /// 수신한 클라이언트에게 응답 데이터를 비동기적으로 전송합니다.
        /// </summary>
        public async Task<bool> ReplyAsync(byte[] data)
            => await _tcpServer.SendAsync(ClientId, data);
    }
}
