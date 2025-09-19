using HYSoft.Presentation.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HYSoft.Presentation.Interactivity;

namespace Samples.Presentation.Modal
{
    public class ModalViewModel
    {
        public ICommand ModalOpenCommand => new RelayCommand(() =>
        {
            ModalManager.Open(new ModalInfoViewModel());
        });
    }
}
