using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ExpanderView : UserControl
    {
        public ExpanderView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
