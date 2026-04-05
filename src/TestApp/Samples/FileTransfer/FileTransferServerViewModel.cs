using System;
using System.Net;
using System.Windows.Input;
using HYSoft.Communication.Tcp.Server;
using HYSoft.Communication.FileTransfer;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Interactivity.CommandBehaviors;

namespace TestApp.Samples.FileTransfer
{
    public class FileTransferServerViewModel : NotifyPropertyChangedBase
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

        private ICommand? _serverStartCommand;
        public ICommand ServerStartCommand => _serverStartCommand ??= new RelayCommand(async () =>
        {
            try
            {
                if (IsServerRunning)
                {
                    Status = "Server already running.";
                    return;
                }

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
                Status = @"Server started on 0.0.0.0:20000, saving to: .\obj\";
            }
            catch (Exception ex)
            {
                Status = $"Server start failed: {ex.Message}";
            }
        });
    }
}
