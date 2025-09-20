using System.Windows.Controls;

namespace Docs.Mvvm.Styles
{
    /// <summary>
    /// IconsView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IconsView : UserControl
    {
        public IconsView()
        {
            InitializeComponent();

            this.DataContext = new IconsViewModel();
        }
    }
}
