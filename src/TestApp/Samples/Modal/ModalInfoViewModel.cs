using System.Windows.Input;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Interactivity.CommandBehaviors;
using HYSoft.Presentation.Modal;

namespace TestApp.Samples.Modal
{
    public class ModalInfoViewModel : NotifyPropertyChangedBase
    {
        private ICommand? _okCommand;
        public ICommand OkCommand => _okCommand ??= new RelayCommand(() =>
        {
            ModalManager.Close(this, ModalResult.Ok);
        });

        private ICommand? _cancelCommand;
        public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(() =>
        {
            ModalManager.Close(this, ModalResult.Cancel);
        });
    }
}
