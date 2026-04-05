using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    public partial class ModalView : UserControl
    {
        public ModalView(IBottomSharedContext context)
        {
            InitializeComponent();
            this.DataContext = new ComponentViewModel(context);
        }
    }
}
