using System.Windows.Controls;

namespace Docs.Mvvm.Styles.Icons
{
    /// <summary>
    /// IconAssetView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IconAssetView : UserControl
    {
        public IconAssetView(IBottomSharedContext context)
        {
            InitializeComponent();

            this.DataContext = new IconAssetViewModel(context);
        }
    }
}
