using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class BadgeView : UserControl
    {
        public BadgeView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
