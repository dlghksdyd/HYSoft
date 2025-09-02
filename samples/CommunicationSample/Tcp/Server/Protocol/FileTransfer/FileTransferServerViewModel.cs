#nullable enable
using HYSoft.Communication.Tcp.Client;
using HYSoft.Communication.Tcp.Client.Protocol.FileTransfer;
using HYSoft.Communication.Tcp.Server;
using HYSoft.Communication.Tcp.Server.Protocol.FileTransfer;
using System.IO;
using System.Net;
using System.Windows.Input;

namespace CommunicationSample.Tcp.Server.Protocol.FileTransfer
{
    public class FileTransferServerViewModel : BindableBase
    {
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
                var serverOptions = new TcpServerOptions(listenAddress: IPAddress.Any, port: 20000)
                {
                    NoDelay = true,
                    BackLog = 100,
                    ReceiveBufferSize = 64 * 1024,
                    SendBufferSize = 64 * 1024,
                    MaxClients = 1024
                };
                var fileServer = new FileTransferServer(serverOptions, @".\obj\");
                await fileServer.ReceiveFileAsync();

                IsServerRunning = true;
                
                Status = @$"Server started on 0.0.0.0:20000, saving to: .\obj\";
            }
            catch (Exception ex)
            {
                Status = $"Server start failed: {ex.Message}";
            }
        });
    }
}
