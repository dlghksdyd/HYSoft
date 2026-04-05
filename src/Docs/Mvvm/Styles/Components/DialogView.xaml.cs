using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class DialogView : UserControl
    {
        public DialogView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
