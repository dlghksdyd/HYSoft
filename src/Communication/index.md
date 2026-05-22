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
| TcpServer | 멀티 클라이언트 서버 (StartAsync, StopAsync, SendAsync, BroadcastAsync) |
| TcpServerManager | 정적 팩토리 + 생명주기 (Create, Dispose) |
| TcpDataReceivedContext | 수신 이벤트 컨텍스트 (Data, ClientId, ReplyAsync) |

## 설계 패턴

- **Manager Pattern**: ConcurrentDictionary<Guid, T>로 인스턴스 추적
- **동시성**: Connect/Send/Receive 각각 별도 SemaphoreSlim
- **타임아웃**: 유휴 기반 (부분 ��송 성공 후 리셋)
- **메모리**: Interlocked.Exchange 원자적 소켓 교체, Shutdown→Dispose 순서

## 파일 구조

```
Tcp/Client/ TcpClient.cs, TcpClientManager.cs, TcpClientOptions.cs
Tcp/Server/ TcpServer.cs, TcpServerManager.cs, TcpServerOptions.cs, TcpDataReceivedEventHandler.cs
```

## 상세 문서

[Communication docs](../Docs/Documents/03-Communication/README.md)
