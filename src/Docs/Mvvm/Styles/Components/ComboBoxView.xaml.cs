using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ComboBoxView : UserControl
    {
        public ComboBoxView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
