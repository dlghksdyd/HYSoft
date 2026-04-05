using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ConverterView : UserControl
    {
        public ConverterView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
