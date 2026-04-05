using System;
using System.Net;

namespace HYSoft.Communication.Tcp.Client
{
    /// <summary>
    /// TCP 클라이언트의 동작을 정의하는 옵션 클래스입니다.
    /// </summary>
    public sealed class TcpClientOptions
    {
        /// <summary>
        /// 연결할 서버의 호스트 IP 주소를 가져옵니다.
        /// </summary>
        public IPAddress Host { get; }

        /// <summary>
        /// 연결할 서버의 포트 번호를 가져옵니다.
        /// </summary>
        public int Port { get; }

        public TcpClientOptions(IPAddress host, int port)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
        }

        public bool NoDelay { get; set; } = true;
        public int ReceiveBufferSize { get; set; } = 64 * 1024;
        public int SendBufferSize { get; set; } = 64 * 1024;
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
