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
| SendTimeout | TimeSpan | 10초 | ⚠️ 현재 미적용(아래) |
| ReceiveTimeout | TimeSpan | 10초 | ⚠️ 현재 미적용(아래) |

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

- 부분 전송/수신 루프 (완료까지 반복). ReceiveAsync 는 버퍼가 **가득** 찰 때까지 기다린다
- ⚠️ 타임아웃 CTS 를 만들지만 그 토큰을 소켓 호출에 넘기지 않는다 — Send/ReceiveTimeout 이 실제로
  적용되지 않아, 응답 없는 상대에게는 무기한 대기한다. 호출부에서 `Task.WhenAny` 로 묶을 것
- 각각 `_sendLock`, `_receiveLock` SemaphoreSlim

### Dispose (internal)

Socket.Shutdown → Dispose, SemaphoreSlim 해제, Interlocked.Exchange 원자적.

하위: [Manager & Usage](01-ManagerUsage.md)
