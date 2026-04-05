using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class TabControlView : UserControl
    {
        public TabControlView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
