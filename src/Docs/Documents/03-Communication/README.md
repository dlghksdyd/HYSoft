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
- **타임아웃**: 유휴 기반 (부분 전송 성공 후 리셋)

## 오류 처리

- 소켓 예외 시 클라이언트 연결 해제/정리
- 옵션 `LogError` 콜백으로 로깅
- try/catch/finally + `Interlocked.Exchange`로 안전한 리소스 해제
