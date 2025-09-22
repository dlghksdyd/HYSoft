using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Components
{
    /// <summary>
    /// IconView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IconView : UserControl
    {
        public IconView(IBottomSharedContext context)
        {
            InitializeComponent();

            this.DataContext = new IconViewModel(context);
        }
    }
}
