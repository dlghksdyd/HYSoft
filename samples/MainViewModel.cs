using Samples.Communication.Tcp.Client.Protocol.FileTransfer;
using Samples.Communication.Tcp.Server.Protocol.FileTransfer;
using Samples.Presentation.Modal;
using System.Windows.Input;

namespace Samples
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

        public ICommand ModalSampleCommand => new DelegateCommand(() =>
        {
            var modal = new ModalView();
            modal.Show();
        });
    }
}
