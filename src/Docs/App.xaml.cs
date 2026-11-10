using System.Windows;
using Docs.Cli;
using Docs.Mvvm.Popup;
using HYSoft.Presentation.Modal;

namespace Docs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DocsCliServer? _cliServer;

        /// <summary><c>--cli</c> 로 실행했을 때만 있다. 첫 화면이 그려지면 MainWindow 가 준비 완료를 알린다.</summary>
        internal static DocsCliCommands? CliCommands { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // StartupUri 의 MainWindow 는 OnStartup 뒤에 만들어진다 — 창이 읽기 전에 옵션을 고정한다.
            var options = DocsCliOptions.Initialize(e.Args);

            ModalManager.Configure("#33ffffff");

            ModalManager.RegisterView<PopupInfoView, PopupInfoViewModel>();

            if (options.EnableCli)
            {
                CliCommands = new DocsCliCommands();
                _cliServer = new DocsCliServer(CliCommands);
                _cliServer.Start();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _cliServer?.Stop();
            base.OnExit(e);
        }
    }

}
