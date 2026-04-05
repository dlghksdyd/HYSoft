using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class BreadcrumbView : UserControl
    {
        public BreadcrumbView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
