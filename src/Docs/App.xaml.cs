using System.Configuration;
using System.Data;
using System.Windows;
using Docs.Mvvm.Popup;
using HYSoft.Presentation.Modal;
using HYSoft.Presentation.Styles.ColorTokens;

namespace Docs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ModalManager.Configure("#33ffffff");
            
            ModalManager.RegisterView<PopupInfoView, PopupInfoViewModel>();
        }
    }

} 
