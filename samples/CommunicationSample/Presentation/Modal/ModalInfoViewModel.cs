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
            PopupManager.Close(this, PopupResult.Ok);
        });

        public ICommand CancelCommand => new DelegateCommand(() =>
        {
            PopupManager.Close(this, PopupResult.Cancel);
        });
    }
}
