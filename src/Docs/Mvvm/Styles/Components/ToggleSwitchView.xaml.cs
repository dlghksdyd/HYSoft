using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ToggleSwitchView : UserControl
    {
        public ToggleSwitchView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
