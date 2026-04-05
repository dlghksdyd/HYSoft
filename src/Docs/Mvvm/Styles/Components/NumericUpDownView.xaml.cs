using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class NumericUpDownView : UserControl
    {
        public NumericUpDownView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
