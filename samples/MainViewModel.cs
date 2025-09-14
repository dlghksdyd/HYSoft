using Samples.Communication.Tcp.Client.Protocol.FileTransfer;
using Samples.Communication.Tcp.Server.Protocol.FileTransfer;
using Samples.Presentation.Modal;
using System.Windows.Input;
using Samples.Presentation.Styles.Icons;

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

        public ICommand IconSampleCommand => new DelegateCommand(() =>
        {
            var modal = new IconsView();
            modal.Show();
        });
    }
}
