using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class DefaultTokensView : UserControl
    {
        public DefaultTokensView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
