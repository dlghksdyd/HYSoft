using System.Windows.Input;
using HYSoft.Presentation.Interactivity.CommandBehaviors;
using HYSoft.Presentation.Modal;

namespace TestApp.Samples.Modal
{
    public class ModalSampleViewModel
    {
        private ICommand? _modalOpenCommand;
        public ICommand ModalOpenCommand => _modalOpenCommand ??= new RelayCommand(() =>
        {
            ModalManager.Open(new ModalInfoViewModel());
        });
    }
}
