# TCP Client

네임스페이스: `HYSoft.Communication.Tcp.Client`

## TcpClientOptions

| 속성 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| Host | IPAddress | (필수) | 서버 IP |
| Port | int | (필수) | 서버 포트 |
| NoDelay | bool | true | Nagle 비활성 |
| ReceiveBufferSize | int | 64KB | 수신 버퍼 |
| SendBufferSize | int | 64KB | 송신 버퍼 |
| ConnectTimeout | TimeSpan | 5초 | 연결 타임아웃 |
| SendTimeout | TimeSpan | 10초 | 송신 유휴 타임아웃 |
| ReceiveTimeout | TimeSpan | 10초 | 수신 유휴 타임아웃 |

## TcpClient

| 속성 | 설명 |
|------|------|
| IsDisposed | 해제 여부 |
| Guid | 인스턴스 고유 ID (읽기 전용) |

### ConnectAsync

IPv4 TCP 소켓 생성, 옵션 적용, BeginConnect/EndConnect TAP 래퍼.
Task.WhenAny+CTS로 타임아웃. 죽은 소켓 자동 재연결. `_connectLock`.

### SendAsync / ReceiveAsync

```csharp
Task SendAsync(byte[] buffer)
Task SendAsync(byte[] buffer, int offset, int count)
Task ReceiveAsync(byte[] buffer)
Task ReceiveAsync(byte[] buffer, int offset, int count)
```

- 부분 전송/수신 루프 (완료까지 반복)
- 청크별 유휴 타임아웃 (성공 후 리셋)
- 각각 `_sendLock`, `_receiveLock` SemaphoreSlim

### Dispose (internal)

Socket.Shutdown → Dispose, SemaphoreSlim 해제, Interlocked.Exchange 원자적.

하위: [Manager & Usage](01-ManagerUsage.md)
