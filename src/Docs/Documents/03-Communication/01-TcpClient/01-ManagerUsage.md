# TcpClientManager & Usage

## TcpClientManager

정적 팩토리/생명주기 관리자. 내부 `ConcurrentDictionary<Guid, TcpClient>`.

```csharp
public static TcpClient Create(TcpClientOptions options); // 생성 + 등록
public static void Dispose(TcpClient client);              // 해제 + 등록 해제
```

## 전체 사용 흐름

```csharp
// 1. 옵션 생성
var options = new TcpClientOptions(IPAddress.Parse("127.0.0.1"), 8080)
{
    ConnectTimeout = TimeSpan.FromSeconds(10),
    SendBufferSize = 128 * 1024
};

// 2. 클라이언트 생성
var client = TcpClientManager.Create(options);

try
{
    // 3. 연결
    await client.ConnectAsync();

    // 4. 송신
    byte[] request = Encoding.UTF8.GetBytes("Hello");
    await client.SendAsync(request);

    // 5. 수신
    byte[] response = new byte[1024];
    await client.ReceiveAsync(response);
}
finally
{
    // 6. 해제
    TcpClientManager.Dispose(client);
}
```
