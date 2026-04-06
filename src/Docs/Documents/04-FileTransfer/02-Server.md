# FileTransferServer

네임스페이스: `HYSoft.Communication.FileTransfer`

## 생성자

```csharp
public FileTransferServer(TcpServerOptions options, string? storageRoot = null)
// storageRoot 기본: {AppDomain.BaseDirectory}/received
```

## ReceiveFileAsync

```csharp
public async Task ReceiveFileAsync() // 서버 시작, 무한 대기
```

## 상태 머신

| 상태 | 설명 | 입력 |
|------|------|------|
| WaitHeader | 헤더 파싱 | 15바이트 |
| WaitFileName | 파일명 추출 | NameLen바이트 |
| ReceivingData | 데이터 수신/저장 | FileSize바이트 |
| WaitTail | CRC 검증 | 8바이트 |
| Done / Error | 완료 / 실패 | - |

## 보안

- **경로 순회 방지**: `..`, `/`, `\`, 잘못된 문자 제거
- **경로 검증**: `Path.GetFullPath()`로 storageRoot 내부 제약
- **스레드 안전**: 세션별 SemaphoreSlim, ConcurrentDictionary

## 내부 클래스

- **Session**: ClientId, State, FileSize, FileName, Stream, CrcValue, Lock 등
- **GrowingBuffer**: 동적 버퍼 (256 초기 → 2배 확장). 부분 프레임 누적용

## 사용 예시

```csharp
var options = new TcpServerOptions(IPAddress.Any, 9000);
var server = new FileTransferServer(options, "C:/received");
await server.ReceiveFileAsync(); // 무한 대기
```
