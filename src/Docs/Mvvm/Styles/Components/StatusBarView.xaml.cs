using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class StatusBarView : UserControl
    {
        public StatusBarView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
