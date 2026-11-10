using Docs.Mvvm.Shared;

namespace Docs.Mvvm.Communication
{
    /// <summary>
    /// Communication 페이지들이 보여 주는 표·코드 예시. 내용은 HYSoft.Communication 의 실제 코드가 기준이다
    /// (Communication/Tcp/Client·Server, Communication.FileTransfer).
    /// </summary>
    public static class CommunicationContent
    {
        #region TcpClient

        public static IReadOnlyList<OptionRow> ClientOptions { get; } = new[]
        {
            OptionRow.Header,
            new OptionRow("Host", "IPAddress", "(required)", "Server IP address. Constructor argument; null throws ArgumentNullException."),
            new OptionRow("Port", "int", "(required)", "Server port. Outside 1-65535 throws ArgumentOutOfRangeException."),
            new OptionRow("NoDelay", "bool", "true", "Disables Nagle's algorithm on the socket."),
            new OptionRow("ReceiveBufferSize", "int", "64 KB", "Socket receive buffer size."),
            new OptionRow("SendBufferSize", "int", "64 KB", "Socket send buffer size."),
            new OptionRow("ConnectTimeout", "TimeSpan", "5 s", "ConnectAsync throws TimeoutException when exceeded."),
            new OptionRow("SendTimeout", "TimeSpan", "10 s", "Declared, but not applied to the socket operation in the current build (see note)."),
            new OptionRow("ReceiveTimeout", "TimeSpan", "10 s", "Declared, but not applied to the socket operation in the current build (see note)."),
        };

        public static IReadOnlyList<MemberRow> ClientMembers { get; } = new[]
        {
            new MemberRow("TcpClientManager.Create(options)", "Creates a client and registers it. The TcpClient constructor is private - always create through the manager."),
            new MemberRow("TcpClientManager.Dispose(client)", "Closes the socket, releases the locks and unregisters the client. A disposed client cannot be reused."),
            new MemberRow("Task ConnectAsync()", "Connects (IPv4). Returns immediately if already connected and alive; replaces a dead socket with a new one."),
            new MemberRow("Task SendAsync(byte[] buffer[, int offset, int count])", "Loops until every byte is sent. Throws InvalidOperationException when not connected."),
            new MemberRow("Task ReceiveAsync(byte[] buffer[, int offset, int count])", "Waits until the buffer is completely filled. Peer close (0 bytes) throws SocketException(ConnectionReset)."),
            new MemberRow("bool IsDisposed", "True after TcpClientManager.Dispose."),
            new MemberRow("Guid Guid", "Instance ID used by the manager."),
        };

        public const string ClientUsage =
"""
using System.Buffers.Binary;
using System.Net;
using System.Text;
using HYSoft.Communication.Tcp.Client;

var options = new TcpClientOptions(IPAddress.Parse("127.0.0.1"), 8080)
{
    ConnectTimeout = TimeSpan.FromSeconds(3),
};

var client = TcpClientManager.Create(options);
try
{
    await client.ConnectAsync();

    await client.SendAsync(Encoding.UTF8.GetBytes("PING"));

    // ReceiveAsync completes only when the buffer is FULL.
    // Read a fixed-size length header first, then the body.
    var header = new byte[4];
    await client.ReceiveAsync(header);
    int length = BinaryPrimitives.ReadInt32BigEndian(header);

    var body = new byte[length];
    await client.ReceiveAsync(body);
}
finally
{
    TcpClientManager.Dispose(client);
}
""";

        public const string ClientTimeoutWorkaround =
"""
// SendTimeout / ReceiveTimeout are not enforced yet - bound the wait yourself.
var receive = client.ReceiveAsync(buffer);
var finished = await Task.WhenAny(receive, Task.Delay(TimeSpan.FromSeconds(3)));
if (finished != receive)
{
    TcpClientManager.Dispose(client);          // closes the socket -> pending receive faults
    _ = receive.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
    throw new TimeoutException("No response within 3 s.");
}
await receive;
""";

        #endregion

        #region TcpServer

        public static IReadOnlyList<OptionRow> ServerOptions { get; } = new[]
        {
            OptionRow.Header,
            new OptionRow("ListenAddress", "IPAddress", "(required)", "Bind address. IPAddress.Loopback = local only, IPAddress.Any = all interfaces."),
            new OptionRow("Port", "int", "(required)", "Listen port. Validated (1-65535) when the server is created."),
            new OptionRow("NoDelay", "bool", "true", "Applied to the listener and to every accepted client socket."),
            new OptionRow("ReceiveBufferSize", "int", "64 KB", "Socket buffer; the per-client read buffer is min(this, 128 KB)."),
            new OptionRow("SendBufferSize", "int", "64 KB", "Socket send buffer size."),
            new OptionRow("BackLog", "int", "100", "Pending connection queue length passed to Listen()."),
            new OptionRow("MaxClients", "int", "1024", "When reached, the accept loop pauses (100 ms polling) until a client leaves."),
        };

        public static IReadOnlyList<MemberRow> ServerMembers { get; } = new[]
        {
            new MemberRow("TcpServerManager.Create(options)", "Creates and registers a server (validates options). The constructor is private."),
            new MemberRow("TcpServerManager.Dispose(server)", "Stops the server synchronously, closes every client and unregisters it."),
            new MemberRow("Task StartAsync()", "Binds, listens and starts the background accept loop, then returns. No-op when already started."),
            new MemberRow("Task StopAsync()", "Stops accepting and disconnects every client. No-op when not started."),
            new MemberRow("Task<bool> SendAsync(Guid clientId, byte[] data, CancellationToken ct = default)", "Sends to one client. Returns false for an unknown client or a socket error."),
            new MemberRow("Task<int> BroadcastAsync(byte[] data, CancellationToken ct = default)", "Sends to every client in parallel; returns the number of successful sends."),
            new MemberRow("bool Disconnect(Guid clientId)", "Closes one client. False when the ID is unknown."),
            new MemberRow("(Guid Id, EndPoint? Remote)[] GetClientsSnapshot()", "Copy of the currently connected clients."),
            new MemberRow("int ConnectedCount", "Number of connected clients."),
            new MemberRow("event TcpDataReceivedEventHandler DataReceived", "Raised for every socket read with a TcpDataReceivedContext (Data, ClientId, ReplyAsync)."),
            new MemberRow("Action<Exception, string>? LogError", "Receives socket errors and exceptions thrown by DataReceived handlers."),
        };

        public const string ServerUsage =
"""
using System.Diagnostics;
using System.Net;
using System.Text;
using HYSoft.Communication.Tcp.Server;

var options = new TcpServerOptions(IPAddress.Loopback, 8080) { MaxClients = 64 };
var server = TcpServerManager.Create(options);
server.LogError = (ex, message) => Debug.WriteLine($"{message}: {ex.Message}");

// Runs on an I/O thread, once per socket read - NOT once per message.
server.DataReceived += ctx => _ = EchoAsync(ctx);

await server.StartAsync();

int delivered = await server.BroadcastAsync(Encoding.UTF8.GetBytes("NOTICE"));
foreach (var (id, remote) in server.GetClientsSnapshot())
    Debug.WriteLine($"{id} {remote}");

await server.StopAsync();
TcpServerManager.Dispose(server);

static async Task EchoAsync(TcpDataReceivedContext ctx)
{
    try
    {
        bool sent = await ctx.ReplyAsync(ctx.Data);   // false = client gone / socket error
    }
    catch (Exception)
    {
        // The client was closed while replying (lock disposed / cancelled).
        // Never let an exception escape a fire-and-forget task.
    }
}
""";

        #endregion

        #region FileTransfer

        public const string FileTransferProtocol =
"""
CLIENT -> SERVER  [MAGIC "FT10" (4)][FLAGS (1)][FILE_SIZE (8)][NAME_LEN (2)][FILE_NAME (UTF-8)]
SERVER -> CLIENT  [STATUS (1)][START_OFFSET (8)]      0x00 = OK, 0x01 = RESUME, 0xFF = ERROR
CLIENT -> SERVER  [PAYLOAD (FILE_SIZE - START_OFFSET)]  sent in 64 KB chunks
CLIENT -> SERVER  [MAGIC_TAIL "FTEN" (4)][CRC32 (4)]  CRC-32 (IEEE) of the whole file
SERVER -> CLIENT  [STATUS (1)]                        0x00 = saved, 0xFF = CRC / protocol error

All integers are big-endian.
""";

        public static IReadOnlyList<MemberRow> FileTransferMembers { get; } = new[]
        {
            new MemberRow("new FileTransferClient(TcpClientOptions options)", "Creates its own TcpClient through TcpClientManager. IDisposable."),
            new MemberRow("Task SendFileAsync(string filePath, IProgress<FileTransferProgress>? progress = null, CancellationToken ct = default)", "Connects, sends one file and waits for the final status. One transfer at a time per instance."),
            new MemberRow("new FileTransferServer(TcpServerOptions options, string? storageRoot = null)", "Creates the TCP server immediately and creates storageRoot (default: <app>\\received)."),
            new MemberRow("Task ReceiveFileAsync()", "Subscribes to DataReceived and starts listening, then returns (it does not wait for a file)."),
            new MemberRow("FileTransferProgress", "BytesTransferred, TotalBytes, Percentage (0.0 - 1.0)."),
            new MemberRow("FileTransferConstants", "MagicHeader, MagicTail, StatusOk/Resume/Error, DefaultChunkSize (64 KB)."),
        };

        public const string FileTransferUsage =
"""
using System.Net;
using HYSoft.Communication.FileTransfer;
using HYSoft.Communication.Tcp.Client;
using HYSoft.Communication.Tcp.Server;

// Receiving side
var server = new FileTransferServer(
    new TcpServerOptions(IPAddress.Any, 20000),
    storageRoot: @"D:\Inbox");
await server.ReceiveFileAsync();            // returns once listening

// Sending side
using var client = new FileTransferClient(
    new TcpClientOptions(IPAddress.Parse("192.168.0.10"), 20000));

var progress = new Progress<FileTransferProgress>(p =>
    StatusText = $"{p.Percentage:P0} ({p.BytesTransferred:N0} / {p.TotalBytes:N0} bytes)");

await client.SendFileAsync(@"C:\Data\report.csv", progress);   // throws IOException on server error
""";

        #endregion
    }
}
