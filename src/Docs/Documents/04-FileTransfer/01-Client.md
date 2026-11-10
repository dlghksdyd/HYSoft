# FileTransferClient

`IDisposable` 구현. 네임스페이스: `HYSoft.Communication.FileTransfer`

## 생성자

```csharp
public FileTransferClient(TcpClientOptions options)
```

## SendFileAsync

```csharp
public async Task SendFileAsync(
    string filePath,
    IProgress<FileTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
```

### 동작 흐름

1. 파일 경로/존재 검증
2. 메타데이터 수집 (크기, 이름)
3. 서버 연결
4. 헤더 전송
5. 서버 응답 수신 (이어받기 오프셋)
6. 이어받기 시 건너뛴 바이트 CRC 사전 계산
7. 64KB 청크 전송 + CRC 누적 + 진행률 콜백
8. 트레일러 전송 (CRC-32)
9. 서버 최종 상태 수신

동시 전송 불가 (`_isSending` 뮤텍스).
실패·취소되면 연결을 버린다 — 다음 호출은 새로 연결하고, 서버에 조각 파일이 있으면 이어받는다.

### 예외

- `ArgumentException`: 잘못된 경로
- `FileNotFoundException`: 파일 없음
- `InvalidOperationException`: 이미 전송 중 / 파일명 65535 바이트 초과
- `IOException`: 서버 거부·CRC/프로토콜 오류
- `OperationCanceledException`: 취소(청크 사이에서 확인)
- `TimeoutException` / `SocketException`: ReceiveTimeout 안에 응답 없음 / 연결 끊김

## 사용 예시

```csharp
var tcpOpts = new TcpClientOptions(IPAddress.Parse("192.168.1.10"), 9000);
using var client = new FileTransferClient(tcpOpts);

var progress = new Progress<FileTransferProgress>(p =>
    Console.WriteLine($"{p.Percentage:P1}"));

await client.SendFileAsync("C:/data/report.pdf", progress);
```
