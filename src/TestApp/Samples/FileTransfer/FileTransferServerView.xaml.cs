using System.Windows;

namespace TestApp.Samples.FileTransfer
{
    public partial class FileTransferServerView : Window
    {
        public FileTransferServerView()
        {
            InitializeComponent();
            this.DataContext = new FileTransferServerViewModel();
        }
    }
}
