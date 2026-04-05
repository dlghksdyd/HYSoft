using System.Windows.Input;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Interactivity.CommandBehaviors;
using TestApp.Samples.FileTransfer;
using TestApp.Samples.Modal;
using TestApp.Samples.Icons;

namespace TestApp
{
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        private ICommand? _fileTransferSampleCommand;
        public ICommand FileTransferSampleCommand => _fileTransferSampleCommand ??= new RelayCommand(() =>
        {
            var server = new FileTransferServerView();
            server.Show();
            var client = new FileTransferClientView();
            client.Show();
        });

        private ICommand? _modalSampleCommand;
        public ICommand ModalSampleCommand => _modalSampleCommand ??= new RelayCommand(() =>
        {
            var modal = new ModalSampleView();
            modal.Show();
        });

        private ICommand? _iconSampleCommand;
        public ICommand IconSampleCommand => _iconSampleCommand ??= new RelayCommand(() =>
        {
            var icons = new IconSampleView();
            icons.Show();
        });
    }
}
