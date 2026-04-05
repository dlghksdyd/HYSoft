using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class FontSizeView : UserControl
    {
        public FontSizeView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
