using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Modal;

namespace Samples.Presentation.Modal
{
    public class ModalInfoViewModel : NotifyPropertyChangedBase
    {
        public ICommand OkCommand => new RelayCommand(() =>
        {
            ModalManager.Close(this, ModalResult.Ok);
        });

        public ICommand CancelCommand => new RelayCommand(() =>
        {
            ModalManager.Close(this, ModalResult.Cancel);
        });
    }
}
