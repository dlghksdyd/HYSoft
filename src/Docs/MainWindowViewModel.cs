using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Docs.Mvvm.Popup;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Modal;

namespace Docs
{
    public class MainWindowViewModel
    {
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
