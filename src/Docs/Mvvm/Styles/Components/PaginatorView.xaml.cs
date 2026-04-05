using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class PaginatorView : UserControl
    {
        public PaginatorView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
