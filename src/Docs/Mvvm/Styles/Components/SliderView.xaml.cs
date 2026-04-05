using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class SliderView : UserControl
    {
        public SliderView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
