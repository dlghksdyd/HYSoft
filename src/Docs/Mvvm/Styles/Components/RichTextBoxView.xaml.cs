using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class RichTextBoxView : UserControl
    {
        public RichTextBoxView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
