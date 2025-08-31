using System;
using System.Net;

namespace HYSoft.Communication.Tcp.Client
{
    /// <summary>
    /// TCP 클라이언트의 동작을 정의하는 옵션 클래스입니다.
    /// </summary>
    /// <param name="host">서버 호스트 IP 주소입니다.</param>
    /// <param name="port">서버 포트 번호입니다.</param>
    public sealed class TcpClientOptions(IPAddress host, int port)
    {
        /// <summary>
        /// 연결할 서버의 호스트 IP 주소를 가져옵니다.
        /// </summary>
        public IPAddress Host { get; } = host ?? throw new ArgumentNullException();

        /// <summary>
        /// 연결할 서버의 포트 번호를 가져옵니다.
        /// </summary>
        public int Port { get; } = port;

        /// <summary>
        /// Nagle 알고리즘을 비활성화할지 여부를 나타냅니다.
        /// <para>기본값은 <c>true</c>이며, 작은 패킷도 지연 없이 즉시 전송됩니다.</para>
        /// </summary>
        public bool NoDelay { get; set; } = true;

        /// <summary>
        /// 소켓 수신 버퍼의 크기(바이트)를 가져오거나 설정합니다.
        /// <para>기본값은 64KB입니다.</para>
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 64 * 1024;

        /// <summary>
        /// 소켓 송신 버퍼의 크기(바이트)를 가져오거나 설정합니다.
        /// <para>기본값은 64KB입니다.</para>
        /// </summary>
        public int SendBufferSize { get; set; } = 64 * 1024;

        /// <summary>
        /// 서버에 연결할 때의 최대 대기 시간입니다.
        /// <para>기본값은 5초입니다.</para>
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 송신 작업이 유휴 상태일 때의 최대 대기 시간입니다.
        /// <para>기본값은 10초입니다.</para>
        /// </summary>
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 수신 작업이 유휴 상태일 때의 최대 대기 시간입니다.
        /// <para>기본값은 10초입니다.</para>
        /// </summary>
        public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}