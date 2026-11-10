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
- **HandleClient**: 수신 루프(min(128KB, BufferSize)) → DataReceived, 0바이트·오류→종료 → ClientDisconnected 1회

## 사용 예시

```csharp
var options = new TcpServerOptions(IPAddress.Any, 8080);
var server = TcpServerManager.Create(options);
server.LogError = (ex, msg) => Console.WriteLine($"{msg}: {ex}");
var buffers = new ConcurrentDictionary<Guid, MemoryStream>(); // 클라이언트별 미완 메시지

// I/O 스레드에서 읽기마다 불린다. 막지 말고, 비동기 작업의 예외는 직접 잡는다(async void 금지)
server.DataReceived += ctx => _ = ReplyAckAsync(ctx);
server.ClientDisconnected += id => buffers.TryRemove(id, out _); // 클라이언트별 상태 정리

await server.StartAsync();
await server.BroadcastAsync(Encoding.UTF8.GetBytes("공지"));
await server.StopAsync();
TcpServerManager.Dispose(server);

static async Task ReplyAckAsync(TcpDataReceivedContext ctx)
{
    try { await ctx.ReplyAsync(Encoding.UTF8.GetBytes("ACK")); } // 실패는 false
    catch (Exception ex) { Debug.WriteLine(ex); }
}
```
