using Docs.Mvvm.Popup;
using Docs.Mvvm.Styles.Icons;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Modal;
using System.Windows;
using System.Windows.Input;

namespace Docs
{
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        public MainWindowViewModel()
        {
            var view = new IconsView();
            view.DataContext = new IconsViewModel();
            BottomContent = view;
        }

        private object? _bottomContent;
        public object? BottomContent
        {
            get => _bottomContent;
            set => SetProperty(ref _bottomContent, value);
        }
        
        public ICommand ExitAppCommand => new RelayCommand(() =>
        {
            var result = new PopupInfoViewModel()
            {
                Title = "프로그램 종료",
                Message = "프로그램을 종료하시겠습니까?",
            }.Open();

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
