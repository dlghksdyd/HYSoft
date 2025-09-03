using HYSoft.Presentation.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Samples.Presentation.Modal
{
    public class ModalViewModel
    {
        public ICommand ModalOpenCommand => new DelegateCommand(() =>
        {
            ModalManager.Open(new ModalInfoViewModel());
        });
    }
}
