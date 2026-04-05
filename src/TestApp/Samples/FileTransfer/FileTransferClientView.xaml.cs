using System.Windows;

namespace TestApp.Samples.FileTransfer
{
    public partial class FileTransferClientView : Window
    {
        public FileTransferClientView()
        {
            InitializeComponent();
            this.DataContext = new FileTransferClientViewModel();
        }
    }
}
