using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class WindowView : UserControl
    {
        public WindowView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
