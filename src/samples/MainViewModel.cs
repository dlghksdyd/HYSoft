using Samples.Communication.Tcp.Client.Protocol.FileTransfer;
using Samples.Communication.Tcp.Server.Protocol.FileTransfer;
using Samples.Presentation.Modal;
using System.Windows.Input;
using HYSoft.Presentation.Interactivity;
using Samples.Presentation.Styles.Icons;

namespace Samples
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        public ICommand FileTransferSampleCommand => new RelayCommand(() =>
        {
            var server = new FileTransferServerView();
            server.Show();
            var client = new FileTransferClientView();
            client.Show();
        });

        public ICommand ModalSampleCommand => new RelayCommand(() =>
        {
            var modal = new ModalView();
            modal.Show();
        });

        public ICommand IconSampleCommand => new RelayCommand(() =>
        {
            var modal = new IconsView();
            modal.Show();
        });
    }
}
