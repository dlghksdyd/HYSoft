using System;
using System.IO;
using System.Net;
using System.Windows.Input;
using HYSoft.Communication.Tcp.Client;
using HYSoft.Communication.FileTransfer;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Interactivity.CommandBehaviors;

namespace TestApp.Samples.FileTransfer
{
    public class FileTransferClientViewModel : NotifyPropertyChangedBase
    {
        private string _status = "Idle";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private FileTransferClient? _fileClient;
        private readonly string _filePath = "./FileTransferTestFile.txt";

        private ICommand? _clientStartCommand;
        public ICommand ClientStartCommand => _clientStartCommand ??= new RelayCommand(() =>
        {
            try
            {
                var clientOptions = new TcpClientOptions(IPAddress.Parse("127.0.0.1"), 20000)
                {
                    NoDelay = true,
                    ReceiveBufferSize = 64 * 1024,
                    SendBufferSize = 64 * 1024,
                    ConnectTimeout = TimeSpan.FromSeconds(5),
                    SendTimeout = TimeSpan.FromSeconds(10),
                    ReceiveTimeout = TimeSpan.FromSeconds(10)
                };
                _fileClient = new FileTransferClient(clientOptions);
                Status = "Client ready for 127.0.0.1:20000";
            }
            catch (Exception ex)
            {
                Status = $"Client init failed: {ex.Message}";
            }
        });

        private ICommand? _clientSendFileCommand;
        public ICommand ClientSendFileCommand => _clientSendFileCommand ??= new RelayCommand(async () =>
        {
            if (_fileClient == null)
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
