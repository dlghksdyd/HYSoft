using System.Windows;
using Docs.Cli;

namespace Docs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainWindowViewModel();

            if (DocsCliOptions.Current.Background)
            {
                // AI 시험용: 화면 밖·작업 표시줄 없음·포커스 가져가지 않음. 내용은 CLI 의 shot 이 직접 그린다.
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = SystemParameters.VirtualScreenLeft - Width - 200;
                Top = SystemParameters.VirtualScreenTop;
                ShowActivated = false;
                ShowInTaskbar = false;
            }

            ContentRendered += (_, _) => App.CliCommands?.MarkReady();
        }
    }
}
