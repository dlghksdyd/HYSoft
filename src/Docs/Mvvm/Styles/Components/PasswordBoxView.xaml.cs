using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class PasswordBoxView : UserControl
    {
        public PasswordBoxView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
