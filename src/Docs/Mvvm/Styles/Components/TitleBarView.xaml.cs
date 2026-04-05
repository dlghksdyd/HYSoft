using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class TitleBarView : UserControl
    {
        public TitleBarView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
