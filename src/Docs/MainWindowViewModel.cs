using Docs.Mvvm.Popup;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Modal;
using System.Windows;
using System.Windows.Input;
using Docs.Mvvm;
using Docs.Mvvm.Styles;

namespace Docs
{
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        public ICommand ExitAppCommand => new RelayCommand(() =>
        {
            var result = ModalManager.Open(new PopupInfoViewModel()
            {
                Title = "Exit",
                Message = "Is this application shutdown?",
            });

            if (result == ModalResult.Ok)
            {
                // 정상 종료 시도
                Application.Current.Shutdown();

                // 보장된 전체 종료
                System.Environment.Exit(0);
            }
        });
    }
}
