using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class IconGuideView : UserControl
    {
        public IconGuideView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
