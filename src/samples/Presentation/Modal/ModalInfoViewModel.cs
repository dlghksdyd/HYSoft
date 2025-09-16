using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HYSoft.Presentation.Modal;

namespace Samples.Presentation.Modal
{
    public class ModalInfoViewModel : BindableBase
    {
        public ICommand OkCommand => new DelegateCommand(() =>
        {
            ModalManager.Close(this, ModalResult.Ok);
        });

        public ICommand CancelCommand => new DelegateCommand(() =>
        {
            ModalManager.Close(this, ModalResult.Cancel);
        });
    }
}
