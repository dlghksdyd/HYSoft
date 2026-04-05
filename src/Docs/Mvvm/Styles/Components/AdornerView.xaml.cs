using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class AdornerView : UserControl
    {
        public AdornerView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
