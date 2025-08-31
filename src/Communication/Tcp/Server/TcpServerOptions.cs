using System;
using System.Net;

namespace HYSoft.Communication.Tcp.Server
{
    /// <summary>
    /// TCP 서버 동작을 설정하기 위한 옵션 클래스입니다.
    /// </summary>
    /// <param name="listenAddress">서버가 바인딩할 IP 주소입니다.</param>
    /// <param name="port">서버가 바인딩할 포트 번호입니다.</param>
    public sealed class TcpServerOptions(IPAddress listenAddress, int port)
    {
        /// <summary>
        /// 서버가 바인딩할 IP 주소를 가져옵니다.
        /// </summary>
        public IPAddress ListenAddress { get; init; } = listenAddress ?? throw new ArgumentNullException(nameof(listenAddress));

        /// <summary>
        /// 서버가 바인딩할 포트 번호를 가져옵니다.
        /// </summary>
        public int Port { get; init; } = port;

        /// <summary>
        /// TCP 연결에 Nagle 알고리즘을 사용하지 않도록 설정할지 여부를 나타냅니다.
        /// <para>기본값은 <c>true</c>이며, 지연 없이 데이터를 즉시 전송합니다.</para>
        /// </summary>
        public bool NoDelay { get; set; } = true;

        /// <summary>
        /// 소켓 수신 버퍼의 크기를 가져오거나 설정합니다. (바이트 단위)
        /// <para>기본값은 64KB입니다.</para>
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 64 * 1024;

        /// <summary>
        /// 소켓 송신 버퍼의 크기를 가져오거나 설정합니다. (바이트 단위)
        /// <para>기본값은 64KB입니다.</para>
        /// </summary>
        public int SendBufferSize { get; set; } = 64 * 1024;

        /// <summary>
        /// 대기열(Backlog)의 크기를 가져오거나 설정합니다.
        /// <para>클라이언트 연결 요청을 처리하기 전에 큐에 쌓을 수 있는 최대 개수를 나타냅니다.</para>
        /// <para>기본값은 100입니다.</para>
        /// </summary>
        public int BackLog { get; set; } = 100;

        /// <summary>
        /// 동시에 연결 가능한 최대 클라이언트 수를 가져옵니다.
        /// <para>기본값은 1024입니다.</para>
        /// </summary>
        public int MaxClients { get; init; } = 1024;
    }
}