using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class MenuView : UserControl
    {
        public MenuView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
