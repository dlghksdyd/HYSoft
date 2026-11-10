using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HYSoft.Communication.Tcp.Client;
using HYSoft.Communication.Tcp.Server;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Interactivity.CommandBehaviors;
using TcpClient = HYSoft.Communication.Tcp.Client.TcpClient;

namespace Docs.Mvvm.Communication
{
    /// <summary>에코 데모의 동작. 버튼과 CLI(<c>echo.*</c>)가 같은 목록을 쓴다.</summary>
    public enum EchoDemoAction
    {
        StartServer,
        StopServer,
        Connect,
        Disconnect,
        Send,
    }

    /// <summary>
    /// TcpServer · TcpClient 를 루프백(127.0.0.1)에서 실제로 돌려 보는 에코 데모.
    /// </summary>
    /// <remarks>
    /// 안전 규칙:
    /// <list type="bullet">
    /// <item>서버는 <see cref="IPAddress.Loopback"/> 에만 바인딩한다 — 외부에서 접속할 수 없다.</item>
    /// <item>이미 리슨 중인 포트면 시작하지 않는다 — 라이브러리가 SO_REUSEADDR 를 켜서 충돌이 오류로 드러나지 않는다.</item>
    /// <item>응답 대기는 <see cref="ReplyTimeout"/> 로 묶는다 — TcpClient 의 ReceiveTimeout 이 현재 적용되지 않는다.</item>
    /// <item>송수신 중 오류·시간 초과면 클라이언트를 버리고 «연결 끊김»으로 둔다(재연결은 사용자가 한다).</item>
    /// <item>명령은 한 번에 하나(<see cref="IsBusy"/>)이고, 페이지를 떠나면 <see cref="Dispose"/> 가 모두 정리한다.</item>
    /// </list>
    /// </remarks>
    public sealed class TcpEchoDemoViewModel : NotifyPropertyChangedBase, IDisposable
    {
        public const int DefaultPort = 50505;
        public static readonly TimeSpan ReplyTimeout = TimeSpan.FromSeconds(3);
        private const int MaxLogLines = 200;

        private readonly Dispatcher _dispatcher;
        private TcpServer? _server;
        private TcpClient? _client;
        private bool _disposed;

        public TcpEchoDemoViewModel()
        {
            _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        }

        #region 바인딩 속성

        private string _portText = DefaultPort.ToString();
        public string PortText
        {
            get => _portText;
            set => SetProperty(ref _portText, value);
        }

        private string _message = "Hello, HYSoft!";
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private bool _isServerRunning;
        public bool IsServerRunning
        {
            get => _isServerRunning;
            private set
            {
                if (!SetProperty(ref _isServerRunning, value)) return;
                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(CanEditPort));
            }
        }

        /// <summary>서버가 멈춰 있고 진행 중인 명령이 없을 때만 포트를 바꿀 수 있다.</summary>
        public bool CanEditPort => !IsServerRunning && !IsBusy;

        private bool _isClientConnected;
        public bool IsClientConnected
        {
            get => _isClientConnected;
            private set
            {
                if (SetProperty(ref _isClientConnected, value)) RaisePropertyChanged(nameof(StatusText));
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (!SetProperty(ref _isBusy, value)) return;
                RaisePropertyChanged(nameof(CanEditPort));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string StatusText =>
            $"Server: {(IsServerRunning ? "listening" : "stopped")}   |   Client: {(IsClientConnected ? "connected" : "disconnected")}";

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        /// <summary>마지막 Send 가 같은 바이트 수의 에코를 받았는가(전송 전에는 null).</summary>
        public bool? LastSendEchoed { get; private set; }

        #endregion

        #region 커맨드

        private ICommand? _startServerCommand;
        public ICommand StartServerCommand => _startServerCommand ??= new RelayCommand(
            () => _ = ExecuteAsync(EchoDemoAction.StartServer),
            () => CanExecute(EchoDemoAction.StartServer));

        private ICommand? _stopServerCommand;
        public ICommand StopServerCommand => _stopServerCommand ??= new RelayCommand(
            () => _ = ExecuteAsync(EchoDemoAction.StopServer),
            () => CanExecute(EchoDemoAction.StopServer));

        private ICommand? _connectCommand;
        public ICommand ConnectCommand => _connectCommand ??= new RelayCommand(
            () => _ = ExecuteAsync(EchoDemoAction.Connect),
            () => CanExecute(EchoDemoAction.Connect));

        private ICommand? _disconnectCommand;
        public ICommand DisconnectCommand => _disconnectCommand ??= new RelayCommand(
            () => _ = ExecuteAsync(EchoDemoAction.Disconnect),
            () => CanExecute(EchoDemoAction.Disconnect));

        private ICommand? _sendCommand;
        public ICommand SendCommand => _sendCommand ??= new RelayCommand(
            () => _ = ExecuteAsync(EchoDemoAction.Send),
            () => CanExecute(EchoDemoAction.Send));

        private ICommand? _clearLogCommand;
        public ICommand ClearLogCommand => _clearLogCommand ??= new RelayCommand(() => Logs.Clear());

        #endregion

        /// <summary>
        /// 동작을 지금 실행할 수 있는가. 버튼 활성 조건과 CLI(<c>echo.*</c>)가 같은 판정을 쓴다.
        /// </summary>
        public bool CanExecute(EchoDemoAction action)
        {
            if (IsBusy || _disposed) return false;
            return action switch
            {
                EchoDemoAction.StartServer => !IsServerRunning,
                EchoDemoAction.StopServer => IsServerRunning,
                EchoDemoAction.Connect => IsServerRunning && !IsClientConnected,
                EchoDemoAction.Disconnect => IsClientConnected,
                EchoDemoAction.Send => IsClientConnected && !string.IsNullOrEmpty(Message),
                _ => false,
            };
        }

        /// <summary>
        /// 동작 하나를 실행하고 끝날 때까지 기다린다. 겹쳐 실행되지 않고(IsBusy), 예외는 로그로만 남긴다.
        /// 지금 실행할 수 없으면(<see cref="CanExecute"/> 거짓) 아무것도 하지 않고 false 를 돌려준다.
        /// </summary>
        public async Task<bool> ExecuteAsync(EchoDemoAction action)
        {
            if (!CanExecute(action)) return false;

            Func<Task> work = action switch
            {
                EchoDemoAction.StartServer => StartServerAsync,
                EchoDemoAction.StopServer => StopServerAsync,
                EchoDemoAction.Connect => ConnectAsync,
                EchoDemoAction.Disconnect => DisconnectAsync,
                EchoDemoAction.Send => SendAsync,
                _ => () => Task.CompletedTask,
            };

            IsBusy = true;
            try
            {
                await work();
            }
            catch (Exception ex)
            {
                AddLog($"[error] {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }

            return true;
        }

        private async Task StartServerAsync()
        {
            if (!int.TryParse(PortText.Trim(), out int port) || port < 1 || port > 65535)
            {
                AddLog("[server] Port must be a number between 1 and 65535.");
                return;
            }

            if (IsPortListening(port))
            {
                AddLog($"[server] Port {port} is already in use by another listener. Choose a different port.");
                return;
            }

            var server = TcpServerManager.Create(new TcpServerOptions(IPAddress.Loopback, port) { MaxClients = 8 });
            server.LogError = (ex, message) => AddLog($"[server] {message}: {ex.Message}");
            server.DataReceived += OnServerDataReceived;

            try
            {
                await server.StartAsync();
            }
            catch
            {
                server.DataReceived -= OnServerDataReceived;
                await Task.Run(() => TcpServerManager.Dispose(server));
                throw;
            }

            if (_disposed)
            {
                // 시작하는 동안 페이지를 떠났다 — 만든 것을 바로 정리한다.
                server.DataReceived -= OnServerDataReceived;
                await Task.Run(() => TcpServerManager.Dispose(server));
                return;
            }

            _server = server;
            IsServerRunning = true;
            AddLog($"[server] Listening on 127.0.0.1:{port}");
        }

        private async Task StopServerAsync()
        {
            ReleaseClient("[client] Disconnected (server stopping).");

            var server = _server;
            _server = null;
            IsServerRunning = false;
            if (server is null) return;

            server.DataReceived -= OnServerDataReceived;
            await Task.Run(() => TcpServerManager.Dispose(server));
            AddLog("[server] Stopped.");
        }

        private async Task ConnectAsync()
        {
            if (_server is null) return;
            if (!int.TryParse(PortText.Trim(), out int port)) return;

            var client = TcpClientManager.Create(new TcpClientOptions(IPAddress.Loopback, port)
            {
                ConnectTimeout = ReplyTimeout,
            });

            try
            {
                await client.ConnectAsync();
            }
            catch
            {
                TcpClientManager.Dispose(client);
                throw;
            }

            if (_disposed)
            {
                TcpClientManager.Dispose(client);
                return;
            }

            _client = client;
            IsClientConnected = true;
            AddLog($"[client] Connected to 127.0.0.1:{port}");
        }

        private Task DisconnectAsync()
        {
            ReleaseClient("[client] Disconnected.");
            return Task.CompletedTask;
        }

        private async Task SendAsync()
        {
            LastSendEchoed = false;
            var client = _client;
            if (client is null) return;

            byte[] request = Encoding.UTF8.GetBytes(Message);
            AddLog($"[client] Send {request.Length} bytes: {Message}");

            try
            {
                await client.SendAsync(request);

                // 에코 서버는 받은 바이트를 그대로 돌려준다 — 보낸 길이만큼 정확히 읽는다.
                byte[] reply = new byte[request.Length];
                Task receive = client.ReceiveAsync(reply);
                Task finished = await Task.WhenAny(receive, Task.Delay(ReplyTimeout));
                if (finished != receive)
                {
                    ObserveFault(receive);
                    ReleaseClient($"[client] No echo within {ReplyTimeout.TotalSeconds:0} s - connection dropped.");
                    return;
                }

                await receive;
                LastSendEchoed = reply.AsSpan().SequenceEqual(request);
                AddLog($"[client] Received {reply.Length} bytes: {Encoding.UTF8.GetString(reply)}" +
                       (LastSendEchoed == true ? "" : " (differs from what was sent)"));
            }
            catch (Exception ex)
            {
                // 송수신 실패 후의 연결 상태는 믿을 수 없다 — 버리고 다시 연결하게 한다.
                ReleaseClient($"[client] {ex.GetType().Name}: {ex.Message} - connection dropped.");
            }
        }

        /// <summary>
        /// 서버 수신 이벤트. I/O 스레드에서 불리므로 막지 않고, 응답은 별도 작업으로 보낸다.
        /// </summary>
        private void OnServerDataReceived(TcpDataReceivedContext context)
        {
            AddLog($"[server] Received {context.Data.Length} bytes from {ShortId(context.ClientId)} - echoing");
            _ = EchoAsync(context);
        }

        private async Task EchoAsync(TcpDataReceivedContext context)
        {
            try
            {
                if (!await context.ReplyAsync(context.Data).ConfigureAwait(false))
                    AddLog($"[server] Echo to {ShortId(context.ClientId)} failed (client gone).");
            }
            catch (Exception ex)
            {
                AddLog($"[server] Echo to {ShortId(context.ClientId)} failed: {ex.Message}");
            }
        }

        private void ReleaseClient(string reason)
        {
            var client = _client;
            _client = null;
            IsClientConnected = false;
            if (client is null) return;

            try { TcpClientManager.Dispose(client); }
            catch (Exception ex) { AddLog($"[client] Dispose failed: {ex.Message}"); }
            AddLog(reason);
        }

        /// <summary>
        /// 페이지를 떠날 때 부른다. 클라이언트를 닫고 서버는 UI 스레드를 막지 않도록 백그라운드에서 정리한다.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            var client = _client;
            _client = null;
            if (client is not null)
            {
                try { TcpClientManager.Dispose(client); } catch { }
            }

            var server = _server;
            _server = null;
            if (server is not null)
            {
                server.DataReceived -= OnServerDataReceived;
                _ = Task.Run(() =>
                {
                    try { TcpServerManager.Dispose(server); } catch { }
                });
            }

            IsClientConnected = false;
            IsServerRunning = false;
        }

        private static bool IsPortListening(int port)
        {
            try
            {
                return IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(endPoint => endPoint.Port == port);
            }
            catch (NetworkInformationException)
            {
                // 확인할 수 없으면 사용 중으로 본다(fail-closed).
                return true;
            }
        }

        private static void ObserveFault(Task task)
            => _ = task.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);

        private static string ShortId(Guid id) => id.ToString("N").Substring(0, 8);

        private void AddLog(string line)
        {
            string stamped = $"{DateTime.Now:HH:mm:ss.fff}  {line}";
            if (_dispatcher.CheckAccess())
            {
                AppendLog(stamped);
            }
            else
            {
                _dispatcher.BeginInvoke(new Action(() => AppendLog(stamped)));
            }
        }

        private void AppendLog(string line)
        {
            Logs.Add(line);
            while (Logs.Count > MaxLogLines) Logs.RemoveAt(0);
        }
    }
}
