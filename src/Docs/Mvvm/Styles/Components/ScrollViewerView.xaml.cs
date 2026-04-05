using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ScrollViewerView : UserControl
    {
        public ScrollViewerView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
