using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class DragDropView : UserControl
    {
        public DragDropView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
