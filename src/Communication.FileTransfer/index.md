# Communication.FileTransfer

FT10 파일 전송 프로토콜 구현. Communication 라이브러리 기반.

- **네임스페이스**: `HYSoft.Communication.FileTransfer`
- **의존성**: Communication (프로젝트 참조)

## FT10 프로토콜

Magic: Header=0x46543130("FT10"), Tail=0x4654454E("FTEN"). 64KB 청크 전송.

```
CLIENT->SERVER: [MAGIC(4)][FLAGS(1)][FILESIZE(8)][NAMELEN(2)][FILENAME(가변)]
SERVER->CLIENT: [STATUS(1)][START_OFFSET(8)]  (0x00=신규, 0x01=이어받기)
CLIENT->SERVER: [FILE_DATA] (64KB 청크)
CLIENT->SERVER: [MAGIC_TAIL(4)][CRC32(4)]
SERVER->CLIENT: [STATUS(1)]  (0x00=성공, 0xFF=오류)
```

## 핵심 클래스

| 클래스 | 설명 |
|--------|------|
| FileTransferClient | 파일 송신 (SendFileAsync, IProgress, CancellationToken). 실패 시 연결을 버리고 다음 호출에서 이어받기 |
| FileTransferServer | 파일 수신 서버 (ReceiveFileAsync, Dispose). `.ftpart` 에 받고 CRC 일치 시 교체·이어받기 |
| FileTransferProgress | 진행률 (BytesTransferred, TotalBytes, Percentage) |
| FileTransferConstants | 프로토콜 상수 (Magic, Status, ChunkSize) |
| Crc32 | CRC-32 IEEE 802.3 (Compute, Update) |
| BinaryHelper | Big-Endian 직렬화 (internal) |

## 보안

- 파일명 경로 순회 방지 (`..`, `/`, `\` 제거)
- 결과 파일은 storageRoot 바로 아래여야 한다(`Path.GetFullPath` 후 폴더 비교)

## 동시성

- 한 연결의 조각은 도착 순서대로 처리(다음 읽기는 그 뒤) — 2026-11-10 이전에는 순서가 섞여 CRC 오류가 났다

## 상세 문서

[FileTransfer docs](../Docs/Documents/04-FileTransfer/README.md)
