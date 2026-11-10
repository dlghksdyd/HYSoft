using System.Windows.Controls;
using Docs.Mvvm.Styles.Components;

namespace Docs.Mvvm.Communication
{
    /// <summary>
    /// Communication › TCP › TcpClient 문서 페이지.
    /// </summary>
    public partial class TcpClientView : UserControl
    {
        public TcpClientView(IBottomSharedContext context)
        {
            InitializeComponent();
            DataContext = new ComponentViewModel(context);
        }
    }
}
