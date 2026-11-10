using System.Windows.Controls;
using Docs.Mvvm.Styles.Components;

namespace Docs.Mvvm.Communication
{
    /// <summary>
    /// Communication › TCP › TcpServer 문서 페이지.
    /// </summary>
    public partial class TcpServerView : UserControl
    {
        public TcpServerView(IBottomSharedContext context)
        {
            InitializeComponent();
            DataContext = new ComponentViewModel(context);
        }
    }
}
