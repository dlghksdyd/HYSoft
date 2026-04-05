using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ToolBarView : UserControl
    {
        public ToolBarView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
