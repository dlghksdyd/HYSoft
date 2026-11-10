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
| SendTimeout | TimeSpan | 10초 | 송신 유휴 타임아웃(`InfiniteTimeSpan` = 무기한) |
| ReceiveTimeout | TimeSpan | 10초 | 수신 유휴 타임아웃(`InfiniteTimeSpan` = 무기한) |

## TcpClient

| 속성 | 설명 |
|------|------|
| IsDisposed | 해제 여부 |
| Guid | 인스턴스 고유 ID (읽기 전용) |

### ConnectAsync

IPv4 TCP 소켓 생성, 옵션 적용, `Socket.ConnectAsync(endPoint, token)` + ConnectTimeout CTS.
살아 있는 연결이면 그대로 반환, 끊겼거나 버려진 연결이면 새로 연결. `_connectLock`.

### SendAsync / ReceiveAsync

```csharp
Task SendAsync(byte[] buffer)
Task SendAsync(byte[] buffer, int offset, int count)
Task ReceiveAsync(byte[] buffer)
Task ReceiveAsync(byte[] buffer, int offset, int count)
```

- 부분 전송/수신 루프 (완료까지 반복). ReceiveAsync 는 버퍼가 **가득** 찰 때까지 기다린다
- 소켓 호출마다 유휴 타임아웃 토큰을 넘긴다(진척이 있으면 다시 잰다) → `TimeoutException`
- 타임아웃·`SocketException`·상대 종료(0바이트) 뒤에는 **연결을 버린다** — 이후 호출은
  `InvalidOperationException`, `ConnectAsync` 로 새로 연결 (2026-11-10 이전에는 타임아웃이 적용되지 않았다)
- 각각 `_sendLock`, `_receiveLock` SemaphoreSlim

### Dispose (internal)

Socket.Shutdown → Dispose, SemaphoreSlim 해제, Interlocked.Exchange 원자적.

하위: [Manager & Usage](01-ManagerUsage.md)
