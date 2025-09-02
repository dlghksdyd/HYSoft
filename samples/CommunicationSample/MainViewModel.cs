using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Samples.Communication.Tcp.Client.Protocol.FileTransfer;
using Samples.Communication.Tcp.Server.Protocol.FileTransfer;

namespace CommunicationSample
{
    public class MainViewModel : BindableBase
    {
        public ICommand FileTransferSampleCommand => new DelegateCommand(() =>
        {
            var server = new FileTransferServerView();
            server.Show();
            var client = new FileTransferClientView();
            client.Show();
        });
    }
}
