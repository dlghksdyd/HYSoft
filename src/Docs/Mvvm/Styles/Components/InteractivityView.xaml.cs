using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class InteractivityView : UserControl
    {
        public InteractivityView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
