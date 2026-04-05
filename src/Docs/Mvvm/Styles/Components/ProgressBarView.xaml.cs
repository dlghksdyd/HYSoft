using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ProgressBarView : UserControl
    {
        public ProgressBarView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
