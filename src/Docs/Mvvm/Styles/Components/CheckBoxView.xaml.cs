using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class CheckBoxView : UserControl
    {
        public CheckBoxView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
