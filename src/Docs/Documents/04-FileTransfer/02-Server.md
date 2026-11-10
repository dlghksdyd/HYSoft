# FileTransferServer

`IDisposable` 구현. 네임스페이스: `HYSoft.Communication.FileTransfer`

## API

```csharp
public FileTransferServer(TcpServerOptions options, string? storageRoot = null)
// storageRoot 기본: {AppDomain.BaseDirectory}/received. 생성 시 TcpServer 생성 + 폴더 생성
public Task ReceiveFileAsync()  // 리슨 시작 후 곧바로 반환. 재호출은 무시, Dispose 뒤면 ObjectDisposedException
public void Dispose()           // 리스너·연결 종료, 열린 파일 해제(조각 파일은 남김). 연결당 최대 ~1초 대기
public string StorageRoot { get; }
```

## 저장 방식

- 받는 중: `이름.ftpart` (`FileTransferConstants.PartialFileSuffix`)
- 전체 CRC 일치 시에만 `이름` 으로 교체 — 그 전까지 기존 `이름` 파일은 그대로
- `이름.ftpart` 가 있고 새 파일보다 길지 않으면 그 길이로 **이어받기**(StatusResume)
- CRC 불일치·꼬리 손상 → 0xFF + 조각 파일 삭제(다음은 처음부터). 연결 끊김 → 조각 파일 유지
- 헤더 거부 → 9바이트 `[0xFF][0×8]` 응답 후 연결 종료

## 상태 머신

| 상태 | 설명 | 입력 |
|------|------|------|
| WaitHeader | 헤더 파싱 | 15바이트 |
| WaitFileName | 파일명 → 조각 파일 열기·이어받기 오프셋 응답 | NameLen바이트 |
| ReceivingData | 조각 파일에 기록 | FileSize−offset 바이트 |
| WaitTail | CRC 검증 → 최종 이름으로 교체 | 8바이트 |
| Done / Error | 완료 / 실패 | - |

## 동시성

- 한 연결의 조각은 수신 처리기 안에서 도착 순서대로 끝까지 처리 — 다음 읽기는 그 뒤(순서 보장 + 흐름 제어).
  2026-11-10 이전에는 조각마다 `Task.Run` 이라 순서가 섞여 큰 파일에서 CRC 오류가 났다
- 끊긴 연결은 `TcpServer.ClientDisconnected` 로 세션·파일 핸들 정리
- 같은 이름을 두 연결이 동시에 받으면 뒤 연결은 2초 재시도 후 거부(남의 조각 파일은 건드리지 않음)

## 보안

- **경로 순회 방지**: `..`, `/`, `\`, 잘못된 문자 → `_`. 결과 파일은 storageRoot 바로 아래여야 함

## 사용 예시

```csharp
var server = new FileTransferServer(new TcpServerOptions(IPAddress.Any, 9000), "C:/received");
await server.ReceiveFileAsync();      // 리슨 시작 후 반환
// 종료 시
await Task.Run(server.Dispose);
```
