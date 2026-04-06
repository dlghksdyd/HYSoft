# HYSoft.Communication.FileTransfer - Protocol

네임스페이스: `HYSoft.Communication.FileTransfer`. 의존성: Communication.

## FT10 프로토콜

Magic: Header=0x46543130("FT10"), Tail=0x4654454E("FTEN")

```
CLIENT->SERVER: [MAGIC(4)][FLAGS(1)][FILESIZE(8)][NAMELEN(2)][FILENAME(가변)]
SERVER->CLIENT: [STATUS(1)][START_OFFSET(8)]
                 STATUS: 0x00=신규, 0x01=이어받기, 0xFF=오류
CLIENT->SERVER: [FILE_PAYLOAD(FILESIZE-OFFSET 바이트)] (64KB 청크)
CLIENT->SERVER: [MAGIC_TAIL(4)][CRC32(4)]
SERVER->CLIENT: [STATUS(1)] 0x00=성공, 0xFF=오류
```

모든 정수: Big-Endian. 파일명: UTF-8.

## FileTransferConstants

```csharp
MagicHeader = 0x46543130;  MagicTail = 0x4654454E;
StatusOk = 0x00;  StatusResume = 0x01;  StatusError = 0xFF;
DefaultChunkSize = 65_536; // 64KB
```

## Crc32

CRC-32 IEEE 802.3. 다항식 0xEDB88320.

```csharp
uint Update(uint crc, byte[] data, int offset, int count); // 증분
uint Compute(byte[] data, int offset, int count);           // 전체
```

## BinaryHelper (internal)

Big-Endian 직렬화. WriteUInt16/32/64BE, ReadUInt16/32/64BE.

## FileTransferProgress

```csharp
public readonly struct FileTransferProgress
{
    public long BytesTransferred { get; }
    public long TotalBytes { get; }
    public double Percentage { get; } // 0.0 ~ 1.0
}
```

하위: [Client](01-Client.md) | [Server](02-Server.md)
