using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class TextBoxView : UserControl
    {
        public TextBoxView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
