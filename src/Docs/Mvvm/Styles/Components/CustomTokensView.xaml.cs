using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class CustomTokensView : UserControl
    {
        public CustomTokensView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
