using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HYSoft.Presentation.Modal;

namespace Samples.Presentation.Modal
{
    /// <summary>
    /// ModalView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModalView : Window
    {
        public ModalView()
        {
            // 반드시 InitializeComponent 전에 호출해야 한다.
            PopupManager.Configure(Brushes.Blue);
            PopupManager.RegisterView<ModalInfoView, ModalInfoViewModel>();

            InitializeComponent();

            DataContext = new ModalViewModel();
        }
    }
}
