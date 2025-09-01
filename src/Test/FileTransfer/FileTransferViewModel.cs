#nullable enable
using HYSoft.Communication.Tcp.Client;
using HYSoft.Communication.Tcp.Client.Protocol.FileTransfer;
using HYSoft.Communication.Tcp.Server;
using HYSoft.Communication.Tcp.Server.Protocol.FileTransfer;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Test.FileTransfer
{
    public class FileTransferViewModel : BindableBase
    {
        // ---- 상태/설정 바인딩용 프로퍼티 ----
        private string _serverHost = "127.0.0.1";
        public string ServerHost
        {
            get => _serverHost;
            set => SetProperty(ref _serverHost, value);
        }

        private int _serverPort = 50500;
        public int ServerPort
        {
            get => _serverPort;
            set => SetProperty(ref _serverPort, value);
        }

        private string _receiveFolder = Path.Combine(AppContext.BaseDirectory, "received");
        public string ReceiveFolder
        {
            get => _receiveFolder;
            set => SetProperty(ref _receiveFolder, value);
        }

        private string _filePath = @"C:\Docker Desktop Installer.exe";
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private string _status = "Idle";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private bool _isServerRunning;
        public bool IsServerRunning
        {
            get => _isServerRunning;
            set => SetProperty(ref _isServerRunning, value);
        }

        private bool _isClientReady;
        public bool IsClientReady
        {
            get => _isClientReady;
            set => SetProperty(ref _isClientReady, value);
        }

        // ---- 내부 필드 ----
        private FileTransferServer? _fileServer;
        private FileTransferClient? _fileClient;

        // 원래 필드가 있었던 TcpClient(저수준)는 FileTransferClient 내부에서 관리하므로 보관하지 않아도 됩니다.
        private TcpClient? _client; // (사용자 코드 호환 위해 유지, 사용하지는 않음)

        // ---- 명령 ----
        private ICommand? _serverStartCommand;
        public ICommand ServerStartCommand => _serverStartCommand ??= new DelegateCommand(async () =>
        {
            try
            {
                // 이미 실행 중이면 무시
                if (IsServerRunning)
                {
                    Status = "Server already running.";
                    return;
                }

                // 서버 옵션 구성 (0.0.0.0 바인딩)
                var serverOptions = new TcpServerOptions
                (
                    listenAddress: IPAddress.Any,
                    port: ServerPort
                )
                {
                    NoDelay = true,
                    BackLog = 100,
                    ReceiveBufferSize = 64 * 1024,
                    SendBufferSize = 64 * 1024,
                    MaxClients = 1024
                };

                _fileServer = new FileTransferServer(serverOptions, ReceiveFolder);
                await _fileServer.ReceiveFileAsync();

                IsServerRunning = true;
                Status = $"Server started on 0.0.0.0:{ServerPort}, saving to: {ReceiveFolder}";
            }
            catch (Exception ex)
            {
                Status = $"Server start failed: {ex.Message}";
            }
        });

        private ICommand? _clientStartCommand;
        public ICommand ClientStartCommand => _clientStartCommand ??= new DelegateCommand(async () =>
        {
            try
            {
                // 클라이언트 옵션 구성
                var clientOptions = new TcpClientOptions(IPAddress.Parse(ServerHost), ServerPort)
                {
                    NoDelay = true,
                    ReceiveBufferSize = 64 * 1024,
                    SendBufferSize = 64 * 1024,
                    ConnectTimeout = TimeSpan.FromSeconds(5),
                    SendTimeout = TimeSpan.FromSeconds(10),
                    ReceiveTimeout = TimeSpan.FromSeconds(10)
                };

                // 파일 전송 클라이언트 준비
                _fileClient = new FileTransferClient(clientOptions);
                IsClientReady = true;
                Status = $"Client ready for {ServerHost}:{ServerPort}";
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                IsClientReady = false;
                Status = $"Client init failed: {ex.Message}";
            }
        });

        private ICommand? _clientSendFileCommand;
        public ICommand ClientSendFileCommand => _clientSendFileCommand ??= new DelegateCommand(async () =>
        {
            if (_fileClient is null)
            {
                Status = "Client not ready. Run ClientStart first.";
                return;
            }
            if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
            {
                Status = "Invalid file path.";
                return;
            }

            try
            {
                Status = $"Sending: {Path.GetFileName(FilePath)} ...";
                await _fileClient.SendFileAsync(FilePath);
                Status = $"Send complete: {Path.GetFileName(FilePath)}";
            }
            catch (Exception ex)
            {
                Status = $"Send failed: {ex.Message}";
            }
        });
    }
}
