# TcpServer - Events, Context, Usage

## TcpDataReceivedContext

```csharp
public sealed class TcpDataReceivedContext
{
    public byte[] Data { get; }      // 수신 데이터 (버퍼 복사본)
    public Guid ClientId { get; }    // 송신 클라이언트 ID
    public async Task<bool> ReplyAsync(byte[] data); // 응답 송신
}
```

## TcpClientContext (내부)

서버측 클라이언트별 상태.

| 속성 | 설명 |
|------|------|
| Id (Guid) | 클라이언트 ID |
| Socket | 네트워크 소켓 |
| RemoteEndPoint | 원격 주소 |
| ConnectedAtUtc | 접속 시각 (UTC) |
| Cts | 클라이언트별 CancellationTokenSource |
| SendLock | 송신 SemaphoreSlim |

## TcpServerManager

```csharp
public static TcpServer Create(TcpServerOptions options);
public static void Dispose(TcpServer server);
```

## 내부 동작

- **AcceptLoop**: 무한 수락, MaxClients 초과 시 대기, 클라이언트별 HandleClientAsync 시작
- **HandleClient**: 수신 루프, min(128KB, BufferSize) 버퍼, DataReceived 이벤트 발생, 0바이트→종료

## 사용 예시

```csharp
var options = new TcpServerOptions(IPAddress.Any, 8080);
var server = TcpServerManager.Create(options);
server.LogError = (ex, msg) => Console.WriteLine($"{msg}: {ex}");

server.DataReceived += async ctx =>
{
    var text = Encoding.UTF8.GetString(ctx.Data);
    await ctx.ReplyAsync(Encoding.UTF8.GetBytes("ACK"));
};

await server.StartAsync();
await server.BroadcastAsync(Encoding.UTF8.GetBytes("공지"));
await server.StopAsync();
TcpServerManager.Dispose(server);
```
