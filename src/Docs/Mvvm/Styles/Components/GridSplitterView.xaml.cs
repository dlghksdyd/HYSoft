using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class GridSplitterView : UserControl
    {
        public GridSplitterView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
