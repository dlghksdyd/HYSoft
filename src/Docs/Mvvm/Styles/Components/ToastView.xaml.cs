using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ToastView : UserControl
    {
        public ToastView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
