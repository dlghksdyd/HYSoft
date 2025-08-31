using System;
using System.Collections.Concurrent;

namespace HYSoft.Communication.Tcp.Server
{
    /// <summary>
    /// <see cref="TcpServer"/> 인스턴스를 생성하고 관리하는 정적 매니저 클래스입니다.
    /// </summary>
    public static class TcpServerManager
    {
        private static readonly ConcurrentDictionary<Guid, TcpServer> Servers = new();

        /// <summary>
        /// 지정된 옵션을 사용하여 새로운 <see cref="TcpServer"/> 인스턴스를 생성하고 등록합니다.
        /// </summary>
        /// <param name="options">서버 생성 시 사용할 <see cref="TcpServerOptions"/>입니다.</param>
        /// <returns>생성된 <see cref="TcpServer"/> 인스턴스입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/>가 <c>null</c>인 경우 발생합니다.</exception>
        /// <exception cref="InvalidOperationException">서버를 내부 딕셔너리에 등록하지 못한 경우 발생합니다.</exception>
        public static TcpServer Create(TcpServerOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));

            var server = TcpServer.CreateInternal(options);

            if (!Servers.TryAdd(server.Guid, server))
            {
                server.Dispose();
                throw new InvalidOperationException("Failed to register server.");
            }

            return server;
        }

        /// <summary>
        /// 지정된 <see cref="TcpServer"/> 인스턴스를 해제(dispose)하고 내부 관리 목록에서 제거합니다.
        /// </summary>
        /// <param name="server">해제할 <see cref="TcpServer"/> 인스턴스입니다.</param>
        /// <exception cref="ArgumentNullException"><paramref name="server"/>가 <c>null</c>인 경우 발생합니다.</exception>
        public static void Dispose(TcpServer server)
        {
            if (server is null) throw new ArgumentNullException(nameof(server));

            _ = Servers.TryRemove(server.Guid, out _);
            server.Dispose();
        }
    }
}