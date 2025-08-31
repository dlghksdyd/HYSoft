using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Client.Protocol.FileTransfer
{
    /// <summary>
    /// 전송 계층 추상화: 바이트 송수신 기능만 필요.
    /// </summary>
    public interface IByteTransport : IAsyncDisposable, IDisposable
    {
        bool IsConnected { get; }

        Task ConnectAsync(CancellationToken ct = default);

        /// <summary>메시지 전체를 보낸다(Partial 없이).</summary>
        Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default);

        /// <summary>정확히 size 바이트를 수신한다. 연결 종료/EOF면 예외.</summary>
        Task ReceiveExactAsync(Memory<byte> buffer, CancellationToken ct = default);
    }

    internal static class TransportExtensions
    {
        public static async Task SendStreamAsync(
            this IByteTransport transport,
            Stream src,
            int chunkSize,
            IProgress<long>? progress,
            CancellationToken ct)
        {
            var rent = ArrayPool<byte>.Shared.Rent(chunkSize);
            try
            {
                int read;
                long sent = 0;
                while ((read = await src.ReadAsync(rent, 0, chunkSize, ct).ConfigureAwait(false)) > 0)
                {
                    await transport.SendAsync(new ReadOnlyMemory<byte>(rent, 0, read), ct).ConfigureAwait(false);
                    sent += read;
                    progress?.Report(sent);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rent);
            }
        }
    }
}