using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ListBoxView : UserControl
    {
        public ListBoxView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
