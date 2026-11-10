using System.Windows.Controls;
using Docs.Mvvm.Styles.Components;

namespace Docs.Mvvm.Communication
{
    /// <summary>
    /// Communication › File Transfer 문서 페이지.
    /// </summary>
    public partial class FileTransferView : UserControl
    {
        public FileTransferView(IBottomSharedContext context)
        {
            InitializeComponent();
            DataContext = new ComponentViewModel(context);
        }
    }
}
