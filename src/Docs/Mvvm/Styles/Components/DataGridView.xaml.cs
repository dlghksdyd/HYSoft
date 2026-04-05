using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class DataGridView : UserControl
    {
        public DataGridView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
