#nullable enable
using CommunicationSample;
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

namespace Samples.Communication.Tcp.Client.Protocol.FileTransfer
{
    public class FileTransferClientViewModel : BindableBase
    {
        private string _status = "Idle";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private bool _isClientReady;
        public bool IsClientReady
        {
            get => _isClientReady;
            set => SetProperty(ref _isClientReady, value);
        }

        private FileTransferClient? _fileClient;
        private readonly string _filePath = "./FileTransferTestFile.txt";
        
        public ICommand ClientStartCommand => new DelegateCommand(() =>
        {
            try
            {
                // 클라이언트 옵션 구성
                var clientOptions = new TcpClientOptions(IPAddress.Parse("127.0.0.1"), 20000)
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
                
                Status = $"Client ready for 127.0.0.1:20000";
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

            try
            {
                Status = $"Sending: {Path.GetFileName(_filePath)} ...";
                
                await _fileClient.SendFileAsync(_filePath);
                
                Status = $"Send complete: {Path.GetFileName(_filePath)}";
            }
            catch (Exception ex)
            {
                Status = $"Send failed: {ex.Message}";
            }
        });
    }
}
