using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ToolTipView : UserControl
    {
        public ToolTipView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
