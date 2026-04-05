using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class TreeViewView : UserControl
    {
        public TreeViewView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
