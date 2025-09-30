using HYSoft.Presentation.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using HYSoft.Presentation.Interactivity.CommandBehaviors;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyTitleBar : ContentControl
    {
        static HyTitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTitleBar),
                new FrameworkPropertyMetadata(typeof(HyTitleBar)));
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(
                nameof(IconSize),
                typeof(double),
                typeof(HyTitleBar),
                new FrameworkPropertyMetadata(32.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty ExitAppCommandProperty =
            DependencyProperty.Register(
                nameof(ExitAppCommand),
                typeof(ICommand),
                typeof(HyTitleBar),
                new PropertyMetadata(null));

        public ICommand? ExitAppCommand
        {
            get => (ICommand?)GetValue(ExitAppCommandProperty);
            set => SetValue(ExitAppCommandProperty, value);
        }

        public ICommand MinimizeAppCommand => new RelayCommand<EventPayload>((p) =>
        {
            if (p.Sender is not DependencyObject d) return;

            var window = Window.GetWindow(d);
            if (window == null) return;
            
            SystemCommands.MinimizeWindow(window);
        });


        public ICommand MaximizeAppCommand => new RelayCommand<EventPayload>((p) =>
        {
            if (p.Sender is not DependencyObject d) return;

            var window = Window.GetWindow(d);
            if (window == null) return;

            if (p.Args is not MouseButtonEventArgs e) return;

            if (e.ClickCount == 2)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    SystemCommands.RestoreWindow(window);
                }
                else
                {
                    SystemCommands.MaximizeWindow(window);
                }

                e.Handled = true;
                return;
            }

            // 버튼/Thumb 위에서 시작한 클릭은 드래그 무시
            if (e.Source is DependencyObject src)
            {
                if (FindAncestor<ButtonBase>(src) != null || FindAncestor<Thumb>(src) != null)
                    return;
            }

            try
            {
                if (window.WindowState == WindowState.Normal)
                    window.DragMove();
            }
            catch
            {
                /* ignore */
            }
        });

        public static readonly DependencyProperty IconPaddingProperty =
            DependencyProperty.Register(
                nameof(IconPadding),
                typeof(Thickness),
                typeof(HyTitleBar),
                new FrameworkPropertyMetadata(
                    new Thickness(0),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Thickness IconPadding
        {
            get => (Thickness)GetValue(IconPaddingProperty);
            set => SetValue(IconPaddingProperty, value);
        }

        public static readonly DependencyProperty IconMarginProperty =
            DependencyProperty.Register(
                nameof(IconMargin),
                typeof(Thickness),
                typeof(HyTitleBar),
                new FrameworkPropertyMetadata(
                    new Thickness(0),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static WindowChrome _chrome;
        private HwndSource? _hwndSource;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            return;
            
            var window = Window.GetWindow(this);
            if (window == null) return;

            // WindowChrome 삽입
            _chrome = WindowChrome.GetWindowChrome(window);
            if (_chrome == null)
            {
                _chrome = new WindowChrome
                {
                    CaptionHeight = 0,
                    CornerRadius = new CornerRadius(0),
                    GlassFrameThickness = new Thickness(0),
                    ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness,
                    UseAeroCaptionButtons = false,
                };
                WindowChrome.SetWindowChrome(window, _chrome);
            }

            // WndProc hook 추가
            window.SourceInitialized += (s, e) =>
            {
                _hwndSource = (HwndSource)PresentationSource.FromVisual(window);
                _hwndSource?.AddHook(WndProc);
            };
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_GETMINMAXINFO = 0x0024;

            if (msg == WM_GETMINMAXINFO)
            {
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);

            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                if (GetMonitorInfo(monitor, ref monitorInfo))
                {
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;

                    mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                    mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                    mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                    mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                }
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        #region Native

        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        #endregion
    }
}
