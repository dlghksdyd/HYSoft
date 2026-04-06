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
// 특정 클라이언트에 송신. 클라이언트별 SendLock. 성공/실패 bool 반환

async Task<int> BroadcastAsync(byte[] data, CancellationToken ct)
// 전체 클라이언트에 병렬 송신. 성공 수 반환

bool Disconnect(Guid clientId)
// 특정 클라이언트 연결 해제

(Guid Id, EndPoint? Remote)[] GetClientsSnapshot()
// 접속 클라이언트 스냅샷
```

### 이벤트/콜백

```csharp
event TcpDataReceivedEventHandler? DataReceived; // 데이터 수신 시
Action<Exception, string>? LogError;              // 오류 로깅 콜백
```

하위: [Events & Usage](01-EventsUsage.md)
