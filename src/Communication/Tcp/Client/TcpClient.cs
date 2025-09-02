using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HYSoft.Communication.Tcp.Client
{
    /// <summary>
    /// 비동기 TCP 클라이언트 구현 클래스입니다.
    /// 서버에 연결하고 데이터를 송수신할 수 있도록 지원합니다.
    /// </summary>
    public class TcpClient
    {
        /// <summary>
        /// 이 인스턴스가 이미 해제(Dispose)되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// 클라이언트 인스턴스의 고유 식별자입니다.
        /// </summary>
        public readonly Guid Guid = Guid.NewGuid();
        
        private readonly TcpClientOptions _options;

        // 내부 소켓과 동시성 제어용 락
        private Socket _socket;
        private readonly SemaphoreSlim _connectLock = new(1, 1);
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly SemaphoreSlim _receiveLock = new(1, 1);

        private TcpClient(TcpClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        internal static TcpClient CreateInternal(TcpClientOptions options) 
            => new TcpClient(options);

        internal void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            // 소켓을 안전하게 교체 후 종료
            var sock = Interlocked.Exchange(ref _socket, null);
            if (sock is not null)
            {
                try
                {
                    try { sock.Shutdown(SocketShutdown.Both); } catch { /* ignore */ }
                    sock.Dispose();
                }
                catch
                {
                    /* ignore */
                }
            }

            // 세마포어 정리: 사용 중일 수 있으므로 non-blocking으로 가능할 때만 Dispose
            static void TryDisposeSemaphore(SemaphoreSlim sem)
            {
                try
                {
                    // 누가 쓰는 중이면 Release 필요할 수 있어 Dispose를 건너뜀
                    if (sem.Wait(0))
                    {
                        sem.Dispose();
                    }
                    else
                    {
                        // 사용 중이면 건너뜀 (ObjectDisposedException 방지)
                    }
                }
                catch
                {
                    /* ignore */
                }
            }

            TryDisposeSemaphore(_sendLock);
            TryDisposeSemaphore(_receiveLock);
            TryDisposeSemaphore(_connectLock);
        }

        private void ThrowIfDisposed()
        {
            if (!IsDisposed) return;
            throw new ObjectDisposedException(nameof(TcpClient));
        }

        /// <summary>
        /// 서버에 비동기적으로 연결을 시도합니다.
        /// 이미 연결된 경우 정상 상태이면 재연결하지 않습니다.
        /// </summary>
        /// <returns>비동기 작업을 나타내는 <see cref="Task"/>.</returns>
        /// <exception cref="TimeoutException">지정된 연결 제한 시간 내에 연결하지 못한 경우 발생합니다.</exception>
        /// <exception cref="InvalidOperationException">객체가 해제되었거나 소켓이 이미 닫힌 경우 발생합니다.</exception>
        public async Task ConnectAsync()
        {
            await _connectLock.WaitAsync().ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();

                // 기존 소켓이 사실상 살아있으면 리턴
                if (_socket is { } existing)
                {
                    // Connected는 신뢰 낮음 → Poll/Available로 죽음 감지
                    var looksDead = existing.Poll(0, SelectMode.SelectRead) && existing.Available == 0;
                    if (existing.Connected && !looksDead) return; // 살아있으면 리턴

                    // 죽었으면 정리
                    try
                    {
                        existing.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                        /* ignored */
                    }

                    try
                    {
                        existing.Dispose();
                    }
                    catch
                    {
                        /* ignored */
                    }

                    _socket = null;
                }

                // 새 소켓 생성 (IPv4 전용)
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = _options.NoDelay,
                    ReceiveBufferSize = _options.ReceiveBufferSize,
                    SendBufferSize = _options.SendBufferSize,
                };

                using var timeoutCts = _options.ConnectTimeout == Timeout.InfiniteTimeSpan
                    ? new CancellationTokenSource()
                    : new CancellationTokenSource(_options.ConnectTimeout);

                try
                {
                    ThrowIfDisposed();
                    if (timeoutCts.IsCancellationRequested)
                        throw new OperationCanceledException();

                    await socket.ConnectAsync(_options.Host, _options.Port, timeoutCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    socket?.Dispose();

                    throw new TimeoutException(
                        $"Connect timed out after {_options.ConnectTimeout.TotalMilliseconds:F0} ms to {_options.Host}:{_options.Port}");
                }
                catch
                {
                    socket?.Dispose();
                    throw;
                }

                var old = Interlocked.Exchange(ref _socket, socket); // 원자성 보장
                if (old is not null && !ReferenceEquals(old, socket))
                {
                    try
                    {
                        old.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                        /* ignore */
                    }

                    try
                    {
                        old.Dispose();
                    }
                    catch
                    {
                        /* ignore */
                    }
                }
            }
            finally
            {
                _connectLock.Release();
            }
        }

        /// <summary>
        /// 지정한 버퍼의 데이터를 서버로 비동기 전송합니다.
        /// </summary>
        /// <param name="buffer">전송할 데이터 버퍼입니다.</param>
        /// <returns>비동기 작업을 나타내는 <see cref="Task"/>.</returns>
        /// <exception cref="InvalidOperationException">소켓이 연결되지 않았거나 이미 해제된 경우 발생합니다.</exception>
        /// <exception cref="TimeoutException">송신이 지정된 제한 시간 내에 완료되지 않은 경우 발생합니다.</exception>
        /// <exception cref="SocketException">네트워크 오류가 발생한 경우 발생합니다.</exception>
        public async Task SendAsync(ReadOnlyMemory<byte> buffer)
        {
            ThrowIfDisposed();

            if (buffer.Length == 0) return;

            // 연결 확인
            var socket = _socket ?? throw new InvalidOperationException("The socket is not connected.");
            if (!socket.Connected)
                throw new InvalidOperationException("The socket is not connected.");

            // 에러 메시지에 목적지 정보를 포함하면 디버깅에 유용
            string remote = null;
            try { remote = socket.RemoteEndPoint?.ToString(); } catch { /* ignore */ }

            await _sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
                using var timeoutCts = new CancellationTokenSource();
                if (_options.SendTimeout != Timeout.InfiniteTimeSpan)
                    timeoutCts.CancelAfter(_options.SendTimeout); // 첫 타임아웃 기동

                int totalSent = 0;

                while (totalSent < buffer.Length)
                {
                    ThrowIfDisposed();

                    ReadOnlyMemory<byte> slice = buffer.Slice(totalSent);

                    int sent = await socket.SendAsync(slice, SocketFlags.None, timeoutCts.Token).ConfigureAwait(false);

                    if (sent <= 0)
                        throw new SocketException((int)SocketError.ConnectionReset);

                    totalSent += sent;

                    // 진행이 있었으니 유휴 타임아웃을 재가동 (Idle 기준)
                    if (_options.SendTimeout != Timeout.InfiniteTimeSpan)
                        timeoutCts.CancelAfter(_options.SendTimeout);
                }
            }
            catch (OperationCanceledException oce)
            {
                throw new TimeoutException(
                    $"Send idle-timed out after {_options.SendTimeout.TotalMilliseconds:F0} ms" +
                    (remote is null ? "." : $" to {remote}."), oce);
            }
            catch (ObjectDisposedException ode)
            {
                throw new InvalidOperationException("The socket is already disposed.", ode);
            }
            catch (SocketException)
            {
                // 필요하면 로깅
                throw;
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// 지정한 여러 버퍼의 데이터를 순차적으로 서버로 전송합니다.
        /// </summary>
        /// <param name="buffers">전송할 데이터 버퍼 목록입니다.</param>
        /// <returns>비동기 작업을 나타내는 <see cref="Task"/>.</returns>
        /// <exception cref="InvalidOperationException">소켓이 연결되지 않았거나 이미 해제된 경우 발생합니다.</exception>
        /// <exception cref="TimeoutException">송신이 지정된 제한 시간 내에 완료되지 않은 경우 발생합니다.</exception>
        /// <exception cref="SocketException">네트워크 오류가 발생한 경우 발생합니다.</exception>
        public async Task SendAsync(IReadOnlyList<ReadOnlyMemory<byte>> buffers)
        {
            foreach (var b in buffers)
                await SendAsync(b).ConfigureAwait(false);
        }

        /// <summary>
        /// 지정된 버퍼 크기만큼 서버에서 데이터를 수신합니다.
        /// </summary>
        /// <param name="buffer">수신한 데이터를 저장할 버퍼입니다.</param>
        /// <returns>비동기 작업을 나타내는 <see cref="Task"/>.</returns>
        /// <exception cref="InvalidOperationException">소켓이 연결되지 않았거나 이미 해제된 경우 발생합니다.</exception>
        /// <exception cref="TimeoutException">수신이 지정된 제한 시간 내에 완료되지 않은 경우 발생합니다.</exception>
        /// <exception cref="SocketException">네트워크 오류 또는 원격 호스트가 연결을 종료한 경우 발생합니다.</exception>
        public async Task ReceiveAsync(Memory<byte> buffer)
        {
            ThrowIfDisposed();

            if (buffer.Length == 0) return;

            var socket = _socket ?? throw new InvalidOperationException("The socket is not connected.");
            if (!socket.Connected)
                throw new InvalidOperationException("The socket is not connected.");

            string remote = null;
            try { remote = socket.RemoteEndPoint?.ToString(); } catch { /* ignore */ }

            await _receiveLock.WaitAsync().ConfigureAwait(false);
            try
            {
                using var timeoutCts = new CancellationTokenSource();
                if (_options.ReceiveTimeout != Timeout.InfiniteTimeSpan)
                    timeoutCts.CancelAfter(_options.ReceiveTimeout); // 첫 유휴 타이머

                int totalRead = 0;

                while (totalRead < buffer.Length)
                {
                    ThrowIfDisposed();

                    var slice = buffer.Slice(totalRead);

                    int read = await socket.ReceiveAsync(slice, SocketFlags.None, timeoutCts.Token).ConfigureAwait(false);

                    if (read == 0)
                        throw new SocketException((int)SocketError.ConnectionReset); // 원격 종료

                    totalRead += read;

                    // 진행이 있었으니 유휴 타임아웃 재가동
                    if (_options.ReceiveTimeout != Timeout.InfiniteTimeSpan)
                        timeoutCts.CancelAfter(_options.ReceiveTimeout);
                }
            }
            catch (OperationCanceledException oce)
            {
                throw new TimeoutException(
                    $"Receive idle-timed out after {_options.ReceiveTimeout.TotalMilliseconds:F0} ms" +
                    (remote is null ? "." : $" from {remote}."), oce);
            }
            catch (ObjectDisposedException ode)
            {
                throw new InvalidOperationException("The socket is already disposed.", ode);
            }
            catch (SocketException)
            {
                throw;
            }
            finally
            {
                _receiveLock.Release();
            }
        }
    }
}
