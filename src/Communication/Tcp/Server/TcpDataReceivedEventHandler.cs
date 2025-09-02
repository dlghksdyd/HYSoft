using System;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Server
{
    /// <summary>
    /// TCP 서버에서 클라이언트로부터 데이터가 수신되었을 때 호출되는 이벤트 핸들러를 나타냅니다.
    /// </summary>
    /// <param name="context">
    /// 수신된 데이터와 응답을 보낼 수 있는 기능을 포함한 <see cref="TcpDataReceivedContext"/> 객체입니다.
    /// </param>
    public delegate void TcpDataReceivedEventHandler(TcpDataReceivedContext context);

    /// <summary>
    /// TCP 서버에서 특정 클라이언트로부터 데이터가 수신되었을 때 전달되는 컨텍스트입니다.
    /// </summary>
    /// <param name="tcpServer">데이터를 수신한 <see cref="TcpServer"/> 인스턴스입니다.</param>
    /// <param name="clientId">데이터를 전송한 클라이언트의 고유 식별자입니다.</param>
    /// <param name="data">수신된 원본 데이터입니다.</param>
    public sealed class TcpDataReceivedContext(TcpServer tcpServer, Guid clientId, byte[] data)
    {
        /// <summary>
        /// 클라이언트로부터 수신된 데이터 바이트 배열입니다.
        /// </summary>
        public readonly byte[] Data = data;

        /// <summary>
        /// 클라이언트의 고유 GUID입니다.
        /// </summary>
        public readonly Guid ClientId = clientId;

        /// <summary>
        /// 수신한 클라이언트에게 응답 데이터를 비동기적으로 전송합니다.
        /// </summary>
        /// <param name="data">전송할 데이터 바이트 배열입니다.</param>
        /// <returns>성공 여부를 나타내는 <see cref="Task{Boolean}"/>.</returns>
        public async Task<bool> ReplyAsync(byte[] data)
            => await tcpServer.SendAsync(ClientId, data);
    }
}