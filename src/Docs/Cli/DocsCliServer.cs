using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32.SafeHandles;

namespace Docs.Cli
{
    /// <summary>
    /// AI 전용 CLI 채널 — Named Pipe <c>HYSoft.Docs.Cli.&lt;pid&gt;</c>. <b>연결 1회 = 요청 1줄 = 응답 1줄</b>(UTF-8 JSON).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>요청: <c>{"cmd":"open","args":["TcpServer"]}</c> · 응답: <c>{"ok":true,"data":{…}}</c> 또는 <c>{"ok":false,"error":"…"}</c>.</item>
    /// <item>파이프 이름에 PID 를 넣는다 — 사람이 띄운 Docs 나 다른 시험 인스턴스와 섞이지 않는다.</item>
    /// <item>같은 사용자만(<see cref="PipeOptions.CurrentUserOnly"/>), 로컬 경로만 받는다 — SMB(<c>IPC$</c>)로 들어온
    /// 연결은 판정이 애매하면 원격으로 보고 거부한다(fail-closed).</item>
    /// <item>명령은 UI 스레드에서 실행한다. 처리를 기다리지 않고 다음 연결을 받으므로, 오래 걸리는 명령 중에도
    /// <c>ping</c> 은 답한다.</item>
    /// </list>
    /// </remarks>
    public sealed class DocsCliServer
    {
        public const int MaxRequestBytes = 64 * 1024;
        private const int MaxConsecutiveAcceptFailures = 20;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // 한글을 \uXXXX 로 바꾸지 않는다 — 읽는 쪽이 AI 라도 로그·화면 문구는 원문으로 대조해야 한다.
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly DocsCliCommands _commands;
        private readonly CancellationTokenSource _cts = new();

        public DocsCliServer(DocsCliCommands commands)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            PipeName = PipeNameFor(Environment.ProcessId);
        }

        public static string PipeNameFor(int processId) => $"HYSoft.Docs.Cli.{processId}";

        public string PipeName { get; }

        /// <summary>수용 루프가 돌고 있는가. 연속 실패로 포기하면 false.</summary>
        public bool IsListening { get; private set; }

        public void Start()
        {
            IsListening = true;
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            try { _cts.Cancel(); } catch (ObjectDisposedException) { }
            IsListening = false;
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            int consecutiveFailures = 0;
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream? pipe = null;
                try
                {
                    pipe = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                    await pipe.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    consecutiveFailures = 0;

                    var accepted = pipe;
                    pipe = null;   // 수명은 아래 작업이 갖는다.
                    _ = Task.Run(async () =>
                    {
                        try { await ServeOneAsync(accepted, ct).ConfigureAwait(false); }
                        catch (Exception) { /* 한 요청의 실패가 채널을 끝내지 않는다 */ }
                        finally
                        {
                            try { accepted.Dispose(); } catch (Exception) { }
                        }
                    }, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // 이름 선점 등으로 매번 즉시 실패하면 코어를 태운다 — 늘어나는 지연 뒤 상한에서 포기한다.
                    if (++consecutiveFailures >= MaxConsecutiveAcceptFailures) break;
                    try { await Task.Delay(Math.Min(500 * consecutiveFailures, 10_000), ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                }
                finally
                {
                    try { pipe?.Dispose(); } catch (Exception) { }
                }
            }

            IsListening = false;
        }

        private async Task ServeOneAsync(NamedPipeServerStream pipe, CancellationToken ct)
        {
            string requestLine = await ReadLineAsync(pipe, ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(requestLine)) return;

            DocsCliResponse response;
            if (!IsLocalClient(pipe))
            {
                response = DocsCliResponse.Fail("Remote pipe clients are not accepted.");
            }
            else
            {
                try
                {
                    var request = JsonSerializer.Deserialize<DocsCliRequest>(requestLine, JsonOptions);
                    response = request is null || string.IsNullOrWhiteSpace(request.Cmd)
                        ? DocsCliResponse.Fail("Request has no cmd.")
                        : await DispatchOnUiThreadAsync(request).ConfigureAwait(false);
                }
                catch (JsonException ex)
                {
                    response = DocsCliResponse.Fail($"Invalid request JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    response = DocsCliResponse.Fail($"{ex.GetType().Name}: {ex.Message}");
                }
            }

            string json;
            try { json = JsonSerializer.Serialize(response, JsonOptions); }
            catch (Exception ex) { json = JsonSerializer.Serialize(DocsCliResponse.Fail($"Response serialization failed: {ex.Message}"), JsonOptions); }

            byte[] payload = Encoding.UTF8.GetBytes(json + "\n");
            await pipe.WriteAsync(payload, ct).ConfigureAwait(false);
            await pipe.FlushAsync(ct).ConfigureAwait(false);
            pipe.WaitForPipeDrain();
        }

        private async Task<DocsCliResponse> DispatchOnUiThreadAsync(DocsCliRequest request)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.HasShutdownStarted)
                return DocsCliResponse.Fail("The app is shutting down.");

            string[] args = request.Args ?? Array.Empty<string>();
            return await dispatcher.InvokeAsync(() => _commands.ExecuteAsync(request.Cmd!, args)).Task.Unwrap().ConfigureAwait(false);
        }

        /// <summary>
        /// 로컬 직접 경로(<c>\\.\pipe\</c>)인가. <c>ERROR_PIPE_LOCAL</c>(229)로 실패했을 때만 로컬이다 —
        /// 성공(=클라이언트 컴퓨터 이름이 있음)·다른 오류·예외는 모두 원격으로 본다.
        /// </summary>
        private static bool IsLocalClient(NamedPipeServerStream pipe)
        {
            const int ErrorPipeLocal = 229;
            try
            {
                var name = new StringBuilder(256);
                if (GetNamedPipeClientComputerName(pipe.SafePipeHandle, name, (uint)name.Capacity)) return false;
                return Marshal.GetLastWin32Error() == ErrorPipeLocal;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetNamedPipeClientComputerName(SafePipeHandle pipe, StringBuilder clientComputerName, uint clientComputerNameLength);

        private static async Task<string> ReadLineAsync(Stream stream, CancellationToken ct)
        {
            var one = new byte[1];
            var bytes = new List<byte>(256);
            while (!ct.IsCancellationRequested)
            {
                int read = await stream.ReadAsync(one, ct).ConfigureAwait(false);
                if (read == 0 || one[0] == (byte)'\n') break;

                bytes.Add(one[0]);
                if (bytes.Count > MaxRequestBytes)
                    throw new InvalidDataException("Request exceeds 64 KB.");
            }

            return Encoding.UTF8.GetString(bytes.ToArray()).TrimEnd('\r');
        }
    }

    /// <summary>CLI 요청 한 줄.</summary>
    public sealed class DocsCliRequest
    {
        public string? Cmd { get; set; }

        public string[]? Args { get; set; }
    }

    /// <summary>CLI 응답 한 줄.</summary>
    public sealed class DocsCliResponse
    {
        public bool Ok { get; init; }

        public string? Error { get; init; }

        public object? Data { get; init; }

        public static DocsCliResponse Success(object? data = null) => new() { Ok = true, Data = data };

        public static DocsCliResponse Fail(string error, object? data = null) => new() { Ok = false, Error = error, Data = data };
    }
}
