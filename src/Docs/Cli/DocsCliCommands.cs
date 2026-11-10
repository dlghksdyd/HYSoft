using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Docs.Mvvm;
using Docs.Mvvm.Communication;
using Docs.Mvvm.LeftMenu;
using HYSoft.Presentation.Interactivity.CommandBehaviors;
using MenuItem = Docs.Mvvm.LeftMenu.MenuItem;

namespace Docs.Cli
{
    /// <summary>
    /// CLI 명령 처리기. 모든 명령은 UI 스레드에서 실행된다(<see cref="DocsCliServer"/> 가 넘겨준다).
    /// </summary>
    /// <remarks>
    /// 🔴 화면을 «사람처럼» 조작하지 않는다 — 마우스·포커스·화면 캡처를 쓰지 않고 뷰모델·명령·시각 트리를 직접 다룬다.
    /// 그래서 창이 화면 밖(<c>--background</c>)에 있어도 그대로 동작하고 사람의 작업을 방해하지 않는다.
    /// </remarks>
    public sealed class DocsCliCommands
    {
        private const int DefaultLogLines = 20;

        private static readonly (string Name, string Usage)[] CommandList =
        {
            ("ping", "ping                     - app / pid / ready"),
            ("help", "help                     - this list"),
            ("state", "state                    - current area, page and menu tree"),
            ("area", "area <ui|db|comm>        - switch the top area (opens its first page)"),
            ("open", "open <page title>        - open a page of the current area by its menu title"),
            ("text", "text                     - visible texts and buttons of the current page"),
            ("shot", "shot <absolute.png>      - render the window to PNG (no screen capture)"),
            ("click", "click <button text>      - invoke an enabled button of the current page (does not wait)"),
            ("echo.status", "echo.status              - Live Echo Demo state and recent log"),
            ("echo.start", "echo.start [port]        - start the loopback server (waits)"),
            ("echo.connect", "echo.connect             - connect the client (waits)"),
            ("echo.send", "echo.send <message>      - send and wait for the echo"),
            ("echo.disconnect", "echo.disconnect          - close the client (waits)"),
            ("echo.stop", "echo.stop                - stop the server (waits)"),
            ("echo.log", "echo.log [lines]         - recent demo log lines"),
            ("quit", "quit                     - close the app"),
        };

        private bool _ready;

        /// <summary>첫 화면이 그려진 뒤 부른다. 그 전에는 ping·help 만 답한다 — 반쯤 그려진 화면을 읽지 않게 한다.</summary>
        public void MarkReady() => _ready = true;

        public async Task<DocsCliResponse> ExecuteAsync(string cmd, string[] args)
        {
            string name = cmd.Trim().ToLowerInvariant();
            if (name == "ping") return Ping();
            if (name == "help") return DocsCliResponse.Success(CommandList.Select(c => c.Usage).ToArray());
            if (!_ready) return DocsCliResponse.Fail("Not ready yet - the first page has not been rendered. Retry after ping shows ready=true.");

            return name switch
            {
                "state" => State(),
                "area" => await AreaAsync(args),
                "open" => await OpenAsync(args),
                "text" => Text(),
                "shot" => await ShotAsync(args),
                "click" => Click(args),
                "echo.status" => EchoStatus(DefaultLogLines),
                "echo.log" => EchoStatus(args.Length > 0 && int.TryParse(args[0], out int lines) ? Math.Clamp(lines, 1, 200) : DefaultLogLines),
                "echo.start" => await EchoAsync(EchoDemoAction.StartServer, args),
                "echo.connect" => await EchoAsync(EchoDemoAction.Connect, args),
                "echo.send" => await EchoAsync(EchoDemoAction.Send, args),
                "echo.disconnect" => await EchoAsync(EchoDemoAction.Disconnect, args),
                "echo.stop" => await EchoAsync(EchoDemoAction.StopServer, args),
                "quit" => Quit(),
                _ => DocsCliResponse.Fail($"Unknown command '{cmd}'. Use help."),
            };
        }

        #region 조회

        private DocsCliResponse Ping()
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            return DocsCliResponse.Success(new
            {
                app = "HYSoft.Docs",
                pid = Environment.ProcessId,
                version,
                ready = _ready,
                background = DocsCliOptions.Current.Background,
            });
        }

        private DocsCliResponse State()
        {
            if (!TryGetShell(out var shell, out var error)) return DocsCliResponse.Fail(error);

            return DocsCliResponse.Success(new
            {
                area = AreaName(shell.CurrentMenuType),
                page = FindSelected(shell.MenuItems)?.Title,
                pageType = shell.Content?.GetType().Name,
                menu = shell.MenuItems.Select(DescribeMenu).ToArray(),
            });
        }

        private static object DescribeMenu(MenuItem item) => new
        {
            title = item.Title,
            isPage = item.ViewType is not null,
            selected = item.IsSelected,
            children = item.SubItems.Count == 0 ? null : item.SubItems.Select(DescribeMenu).ToArray(),
        };

        /// <summary>현재 페이지의 보이는 글자·입력값·버튼을 화면 순서대로 돌려준다.</summary>
        private DocsCliResponse Text()
        {
            if (!TryGetShell(out var shell, out var error)) return DocsCliResponse.Fail(error);
            if (shell.Content is not DependencyObject page) return DocsCliResponse.Fail("No page is open.");

            var lines = new List<string>();
            foreach (var element in Descendants(page))
            {
                if (element is UIElement ui && !ui.IsVisible) continue;

                switch (element)
                {
                    case Button button:
                        lines.Add($"[button{(button.IsEnabled ? "" : ", disabled")}] {ButtonText(button)}");
                        break;
                    case TextBox textBox:
                        lines.Add($"[input{(textBox.IsEnabled ? "" : ", disabled")}{(textBox.IsReadOnly ? ", readonly" : "")}] {textBox.Text}");
                        break;
                    case TextBlock textBlock when !string.IsNullOrWhiteSpace(textBlock.Text) && !IsInside<Button>(textBlock, page):
                        lines.Add(textBlock.Text);
                        break;
                }
            }

            return DocsCliResponse.Success(new { page = shell.Content.GetType().Name, lines });
        }

        #endregion

        #region 이동

        private async Task<DocsCliResponse> AreaAsync(string[] args)
        {
            if (!TryGetShell(out var shell, out var error)) return DocsCliResponse.Fail(error);
            if (args.Length == 0) return DocsCliResponse.Fail("Usage: area <ui|db|comm>");

            ELeftMenuType? type = args[0].Trim().ToLowerInvariant() switch
            {
                "ui" or "uisupport" => ELeftMenuType.UiSupport,
                "db" or "dbsupport" => ELeftMenuType.DbSupport,
                "comm" or "communication" => ELeftMenuType.Communication,
                _ => null,
            };
            if (type is null) return DocsCliResponse.Fail($"Unknown area '{args[0]}'. Use ui, db or comm.");

            shell.UpdateLeftMenuItemCommand.Execute(new EventPayload(null, null, type.Value));
            await SettleAsync();
            return State();
        }

        private async Task<DocsCliResponse> OpenAsync(string[] args)
        {
            if (!TryGetShell(out var shell, out var error)) return DocsCliResponse.Fail(error);
            string title = string.Join(" ", args).Trim();
            if (title.Length == 0) return DocsCliResponse.Fail("Usage: open <page title>");

            var pages = Flatten(shell.MenuItems).Where(m => m.ViewType is not null).ToList();
            var target = pages.FirstOrDefault(m => string.Equals(m.Title, title, StringComparison.OrdinalIgnoreCase));
            if (target is null)
            {
                return DocsCliResponse.Fail(
                    $"No page '{title}' in area {AreaName(shell.CurrentMenuType)}. Switch the area first if needed.",
                    new { available = pages.Select(p => p.Title).ToArray() });
            }

            shell.SelectMenuItemCommand.Execute(new EventPayload(null, null, target));
            await SettleAsync();
            return State();
        }

        #endregion

        #region 조작

        /// <summary>
        /// 버튼을 자동화 피어로 누른다. 🔴 결과를 기다리지 않는다 — 버튼이 모달을 띄우면 사람이 닫을 때까지 반환하지
        /// 않으므로, 기다리면 성공한 클릭을 실패로 읽게 된다. 결과는 <c>text</c>·<c>state</c> 로 다시 읽는다.
        /// </summary>
        private DocsCliResponse Click(string[] args)
        {
            if (!TryGetShell(out var shell, out var error)) return DocsCliResponse.Fail(error);
            if (shell.Content is not DependencyObject page) return DocsCliResponse.Fail("No page is open.");
            string label = string.Join(" ", args).Trim();
            if (label.Length == 0) return DocsCliResponse.Fail("Usage: click <button text>");

            var buttons = Descendants(page).OfType<Button>().Where(b => b.IsVisible).ToList();
            var matches = buttons.Where(b => string.Equals(ButtonText(b), label, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0)
                return DocsCliResponse.Fail($"No visible button '{label}'.", new { buttons = buttons.Select(ButtonText).Where(t => t.Length > 0).ToArray() });
            if (matches.Count > 1)
                return DocsCliResponse.Fail($"{matches.Count} buttons are labelled '{label}' - ambiguous.");

            var button = matches[0];
            if (!button.IsEnabled) return DocsCliResponse.Fail($"Button '{label}' is disabled.");

            var invoker = new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            if (invoker is null) return DocsCliResponse.Fail($"Button '{label}' cannot be invoked.");

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new Action(invoker.Invoke));
            return DocsCliResponse.Success(new { clicked = label, waited = false });
        }

        private async Task<DocsCliResponse> EchoAsync(EchoDemoAction action, string[] args)
        {
            if (!TryGetEchoDemo(out var demo, out var error)) return DocsCliResponse.Fail(error);

            if (action == EchoDemoAction.StartServer && args.Length > 0)
            {
                if (!demo.CanEditPort) return DocsCliResponse.Fail("The port cannot be changed now (server running or busy).", DescribeEcho(demo, DefaultLogLines));
                demo.PortText = args[0];
            }
            else if (action == EchoDemoAction.Send)
            {
                string message = string.Join(" ", args);
                if (message.Length == 0) return DocsCliResponse.Fail("Usage: echo.send <message>");
                if (message.Length > 1024) return DocsCliResponse.Fail("Message is limited to 1024 characters (same as the input box).");
                demo.Message = message;
            }

            if (!demo.CanExecute(action))
                return DocsCliResponse.Fail($"{action} is not allowed in the current state.", DescribeEcho(demo, DefaultLogLines));

            await demo.ExecuteAsync(action);
            await SettleAsync();   // 서버 스레드가 예약한 로그 줄이 반영되게 한다.

            // 🔴 「실행했다」가 아니라 «결과 상태»로 성공을 판정한다 — 포트 충돌·형식 오류는 예외 없이 로그만 남기므로
            //    실행 여부만 보면 서버가 안 떴는데 ok=true 가 된다(2026-11-10 실측).
            bool achieved = action switch
            {
                EchoDemoAction.StartServer => demo.IsServerRunning,
                EchoDemoAction.StopServer => !demo.IsServerRunning,
                EchoDemoAction.Connect => demo.IsClientConnected,
                EchoDemoAction.Disconnect => !demo.IsClientConnected,
                EchoDemoAction.Send => demo.LastSendEchoed == true,
                _ => false,
            };

            var state = DescribeEcho(demo, DefaultLogLines);
            return achieved
                ? DocsCliResponse.Success(state)
                : DocsCliResponse.Fail($"{action} did not reach its expected state - see log.", state);
        }

        private DocsCliResponse EchoStatus(int lines)
            => TryGetEchoDemo(out var demo, out var error)
                ? DocsCliResponse.Success(DescribeEcho(demo, lines))
                : DocsCliResponse.Fail(error);

        private static object DescribeEcho(TcpEchoDemoViewModel demo, int lines) => new
        {
            serverRunning = demo.IsServerRunning,
            clientConnected = demo.IsClientConnected,
            busy = demo.IsBusy,
            port = demo.PortText,
            message = demo.Message,
            log = demo.Logs.Skip(Math.Max(0, demo.Logs.Count - lines)).ToArray(),
        };

        private DocsCliResponse Quit()
        {
            var app = Application.Current;
            _ = Task.Delay(300).ContinueWith(_ => app?.Dispatcher.BeginInvoke(new Action(() => app.Shutdown())));
            return DocsCliResponse.Success(new { quitting = true });
        }

        #endregion

        #region 스크린샷

        /// <summary>
        /// 창 내용을 <see cref="RenderTargetBitmap"/> 으로 직접 그린다. 화면 캡처가 아니므로 창이 가려져 있거나
        /// 화면 밖에 있어도 되고, 창을 앞으로 가져오지 않는다.
        /// </summary>
        private async Task<DocsCliResponse> ShotAsync(string[] args)
        {
            string path = string.Join(" ", args).Trim().Trim('"');
            if (path.Length == 0 || !Path.IsPathFullyQualified(path)) return DocsCliResponse.Fail("Usage: shot <absolute path .png>");
            if (!string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase)) return DocsCliResponse.Fail("Only .png is supported.");

            if (Application.Current?.MainWindow is not Window window || window.Content is not FrameworkElement root)
                return DocsCliResponse.Fail("Main window is not available.");

            await SettleAsync();

            var dpi = VisualTreeHelper.GetDpi(root);
            int width = (int)Math.Ceiling(root.ActualWidth * dpi.DpiScaleX);
            int height = (int)Math.Ceiling(root.ActualHeight * dpi.DpiScaleY);
            if (width <= 0 || height <= 0) return DocsCliResponse.Fail("Window has no size yet.");

            var bitmap = new RenderTargetBitmap(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
            bitmap.Render(root);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temp = path + ".tmp";
            using (var stream = File.Create(temp)) encoder.Save(stream);
            File.Move(temp, path, overwrite: true);

            return DocsCliResponse.Success(new { path, width, height });
        }

        #endregion

        #region 도우미

        /// <summary>바인딩·레이아웃·렌더가 끝날 때까지 한 번 양보한다.</summary>
        private static async Task SettleAsync()
        {
            await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        }

        private static bool TryGetShell(out IBottomSharedContext shell, out string error)
        {
            shell = null!;
            if (Application.Current?.MainWindow is not Window window)
            {
                error = "Main window is not available.";
                return false;
            }

            var view = Descendants(window).OfType<BottomContentsView>().FirstOrDefault();
            if (view?.DataContext is not BottomContentsViewModel viewModel)
            {
                error = "Content view is not available.";
                return false;
            }

            shell = viewModel.SharedContext;
            error = "";
            return true;
        }

        private static bool TryGetEchoDemo(out TcpEchoDemoViewModel demo, out string error)
        {
            demo = null!;
            if (!TryGetShell(out var shell, out error)) return false;
            if (shell.Content is not TcpEchoDemoView view)
            {
                error = "The Live Echo Demo page is not open. Run: area comm, then open \"Live Echo Demo\".";
                return false;
            }

            demo = view.ViewModel;
            return true;
        }

        private static string AreaName(ELeftMenuType? type) => type switch
        {
            ELeftMenuType.UiSupport => "ui",
            ELeftMenuType.DbSupport => "db",
            ELeftMenuType.Communication => "comm",
            _ => "none",
        };

        private static MenuItem? FindSelected(IEnumerable<MenuItem> items)
            => Flatten(items).FirstOrDefault(m => m.IsSelected);

        private static IEnumerable<MenuItem> Flatten(IEnumerable<MenuItem> items)
        {
            foreach (var item in items)
            {
                yield return item;
                foreach (var sub in Flatten(item.SubItems)) yield return sub;
            }
        }

        private static string ButtonText(Button button) => button.Content switch
        {
            string text => text,
            TextBlock textBlock => textBlock.Text,
            _ => "",
        };

        private static bool IsInside<T>(DependencyObject element, DependencyObject stopAt) where T : DependencyObject
        {
            var current = VisualTreeHelper.GetParent(element);
            while (current is not null && !ReferenceEquals(current, stopAt))
            {
                if (current is T) return true;
                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        /// <summary>시각 트리를 깊이 우선(화면 순서)으로 훑는다.</summary>
        private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                foreach (var grandChild in Descendants(child)) yield return grandChild;
            }
        }

        #endregion
    }
}
