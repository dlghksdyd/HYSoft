# TCP Server

네임스페이스: `HYSoft.Communication.Tcp.Server`

## TcpServerOptions

| 속성 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| ListenAddress | IPAddress | (필수) | 바인드 IP |
| Port | int | (필수) | 리슨 포트 |
| NoDelay | bool | true | 클라이언트별 Nagle 비활성 |
| ReceiveBufferSize | int | 64KB | 수신 버퍼 |
| SendBufferSize | int | 64KB | 송신 버퍼 |
| BackLog | int | 100 | 백로그 큐 |
| MaxClients | int | 1024 | 최대 동시 접속 |

## TcpServer

| 속성 | 설명 |
|------|------|
| IsDisposed | 해제 여부 |
| Guid | 인스턴스 고유 ID |
| ConnectedCount | 현재 접속 수 |

### 주요 메서드

```csharp
Task StartAsync()
// 리슨 소켓 초기화, SO_REUSEADDR, 백그라운드 AcceptLoop 시작

Task StopAsync()
// CTS 취소, 리스너 종료, 전체 클라이언트 연결 해제

async Task<bool> SendAsync(Guid clientId, byte[] data, CancellationToken ct)
// 특정 클라이언트에 송신. 클라이언트별 SendLock. 실패(없는/닫히는 클라이언트·취소·소켓 오류)는 예외 없이 false

async Task<int> BroadcastAsync(byte[] data, CancellationToken ct)
// 전체 클라이언트에 병렬 송신. 성공 수 반환

bool Disconnect(Guid clientId)
// 특정 클라이언트 연결 해제

(Guid Id, EndPoint? Remote)[] GetClientsSnapshot()
// 접속 클라이언트 스냅샷
```

### 이벤트/콜백

```csharp
event TcpDataReceivedEventHandler? DataReceived; // 소켓 읽기마다(메시지 단위 아님)
event Action<Guid>? ClientDisconnected;           // 연결당 정확히 1회, 마지막 DataReceived 뒤
Action<Exception, string>? LogError;              // 오류·처리기 예외 로깅 콜백
```

⚠️ `StartAsync` 는 SO_REUSEADDR 를 켠다 — Windows 에서는 이미 쓰는 포트에도 바인딩이 성공할 수 있다.

하위: [Events & Usage](01-EventsUsage.md)
