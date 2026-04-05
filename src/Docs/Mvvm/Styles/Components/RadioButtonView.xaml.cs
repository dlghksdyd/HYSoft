using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class RadioButtonView : UserControl
    {
        public RadioButtonView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
