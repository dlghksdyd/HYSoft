using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ButtonView : UserControl
    {
        public ButtonView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
