using System.Windows;
using System.Windows.Media;
using HYSoft.Presentation.Modal;

namespace TestApp.Samples.Modal
{
    public partial class ModalSampleView : Window
    {
        public ModalSampleView()
        {
            ModalManager.Configure(Brushes.Blue);
            ModalManager.RegisterView<ModalInfoView, ModalInfoViewModel>();

            InitializeComponent();

            DataContext = new ModalSampleViewModel();
        }
    }
}
