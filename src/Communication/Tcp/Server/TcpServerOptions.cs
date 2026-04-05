using System;
using System.Net;

namespace HYSoft.Communication.Tcp.Server
{
    /// <summary>
    /// TCP 서버 동작을 설정하기 위한 옵션 클래스입니다.
    /// </summary>
    public sealed class TcpServerOptions
    {
        /// <summary>
        /// 서버가 바인딩할 IP 주소를 가져옵니다.
        /// </summary>
        public IPAddress ListenAddress { get; }

        /// <summary>
        /// 서버가 바인딩할 포트 번호를 가져옵니다.
        /// </summary>
        public int Port { get; }

        public TcpServerOptions(IPAddress listenAddress, int port)
        {
            ListenAddress = listenAddress ?? throw new ArgumentNullException(nameof(listenAddress));
            Port = port;
        }

        public bool NoDelay { get; set; } = true;
        public int ReceiveBufferSize { get; set; } = 64 * 1024;
        public int SendBufferSize { get; set; } = 64 * 1024;
        public int BackLog { get; set; } = 100;
        public int MaxClients { get; set; } = 1024;
    }
}
