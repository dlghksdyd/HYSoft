# HYSoft.Communication Library

비동기 TCP 클라이언트/서버 통신. 네임스페이스: `HYSoft.Communication.Tcp`

의존성: System.Memory, System.Buffers, System.Threading.Tasks.Extensions (4.6.0)

## 구성 요소

| 카테고리 | 문서 | 주요 클래스 |
|----------|------|-------------|
| TCP 클라이언트 | [01-TcpClient/](01-TcpClient/README.md) | TcpClient, TcpClientManager, TcpClientOptions |
| TCP 서버 | [02-TcpServer/](02-TcpServer/README.md) | TcpServer, TcpServerManager, TcpServerOptions |

## Manager 패턴

```csharp
var client = TcpClientManager.Create(options);  // 생성 + 내부 등록
TcpClientManager.Dispose(client);               // 해제 + 등록 해제
```

내부: `ConcurrentDictionary<Guid, T>`로 인스턴스 추적.

## 동시성 모델

- **클라이언트**: Connect/Send/Receive 각각 별도 `SemaphoreSlim`
- **서버**: 클라이언트별 SendLock, 전역 StartStopLock
- **타임아웃**: Connect 는 전체, Send/Receive 는 소켓 호출 1회 단위의 유휴 시간 ([TcpClient](01-TcpClient/README.md))

## 오류 처리

- 클라이언트: 타임아웃·소켓 오류·상대 종료 뒤 연결을 버린다(프레임 어긋남 방지). 복구는 `ConnectAsync`
- 서버: 송신 실패는 예외가 아니라 `false`. 연결 종료는 `ClientDisconnected` 로 연결당 1회 알림
- 옵션 `LogError` 콜백으로 로깅
- try/catch/finally + `Interlocked.Exchange`로 안전한 리소스 해제
