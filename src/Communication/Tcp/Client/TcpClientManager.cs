using System;
using System.Collections.Concurrent;

namespace HYSoft.Communication.Tcp.Client
{
    /// <summary>
    /// <see cref="TcpClient"/> 인스턴스를 생성하고 관리하는 정적 매니저 클래스입니다.
    /// </summary>
    public static class TcpClientManager
    {
        private static readonly ConcurrentDictionary<Guid, TcpClient> _clients = new();

        /// <summary>
        /// 지정된 옵션을 사용하여 새로운 <see cref="TcpClient"/> 인스턴스를 생성하고 내부 관리 목록에 등록합니다.
        /// </summary>
        /// <param name="options">생성 시 사용할 <see cref="TcpClientOptions"/> 설정입니다.</param>
        /// <returns>생성된 <see cref="TcpClient"/> 인스턴스입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/>가 <c>null</c>인 경우 발생합니다.</exception>
        /// <exception cref="InvalidOperationException">클라이언트를 내부 딕셔너리에 등록하지 못한 경우 발생합니다.</exception>
        public static TcpClient Create(TcpClientOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));

            var client = TcpClient.CreateInternal(options);

            if (!_clients.TryAdd(client.Guid, client))
            {
                client.Dispose();
                throw new InvalidOperationException("Failed to register client.");
            }

            return client;
        }

        /// <summary>
        /// 지정된 <see cref="TcpClient"/> 인스턴스를 해제(dispose)하고 내부 관리 목록에서 제거합니다.
        /// </summary>
        /// <param name="client">해제할 <see cref="TcpClient"/> 인스턴스입니다.</param>
        /// <exception cref="ArgumentNullException"><paramref name="client"/>가 <c>null</c>인 경우 발생합니다.</exception>
        public static void Dispose(TcpClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            _ = _clients.TryRemove(client.Guid, out _);
            client.Dispose();
        }
    }
}