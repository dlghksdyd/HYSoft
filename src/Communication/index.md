# Communication

비동기 TCP 클라이언트/서버 통신 라이브러리.

- **네임스페이스**: `HYSoft.Communication.Tcp.Client`, `HYSoft.Communication.Tcp.Server`
- **의존성**: System.Memory, System.Buffers, System.Threading.Tasks.Extensions (4.6.0)

## 핵심 클래스

| 클래스 | 설명 |
|--------|------|
| TcpClientOptions | 클라이언트 설정 (Host, Port, 버퍼, 타임아웃) |
| TcpClient | 비동기 TCP 클라이언트 (ConnectAsync, SendAsync, ReceiveAsync) |
| TcpClientManager | 정적 팩토리 + 생명주기 (Create, Dispose) |
| TcpServerOptions | 서버 설정 (ListenAddress, Port, BackLog, MaxClients) |
| TcpServer | 멀티 클라이언트 서버 (StartAsync, StopAsync, SendAsync, BroadcastAsync, DataReceived, ClientDisconnected) |
| TcpServerManager | 정적 팩토리 + 생명주기 (Create, Dispose) |
| TcpDataReceivedContext | 수신 이벤트 컨텍스트 (Data, ClientId, ReplyAsync) |

## 설계 패턴

- **Manager Pattern**: ConcurrentDictionary<Guid, T>로 인스턴스 추적
- **동시성**: Connect/Send/Receive 각각 별도 SemaphoreSlim
- **타임아웃**: Send/Receive 는 소켓 호출 1회 단위 유휴 시간(진척 시 리셋). 시간 초과·소켓 오류·상대 종료 뒤
  클라이언트는 연결을 버린다 — 복구는 `ConnectAsync`
- **서버 송신 실패**: 예외 없이 `false`. 연결 종료는 `ClientDisconnected` 로 연결당 1회
- **시험**: `tests/HYSoft.Communication.Tests` (실제 루프백 소켓)
- **메모리**: Interlocked.Exchange 원자적 소켓 교체, Shutdown→Dispose 순서

## 파일 구조

```
Tcp/Client/ TcpClient.cs, TcpClientManager.cs, TcpClientOptions.cs
Tcp/Server/ TcpServer.cs, TcpServerManager.cs, TcpServerOptions.cs, TcpDataReceivedEventHandler.cs
```

## 상세 문서

[Communication docs](../Docs/Documents/03-Communication/README.md)
