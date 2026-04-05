using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace HYSoft.Presentation.Interactivity
{
    public static class WindowDragBehavior
    {
        public static readonly DependencyProperty EnableDragMoveProperty =
            DependencyProperty.RegisterAttached(
                "EnableDragMove",
                typeof(bool),
                typeof(WindowDragBehavior),
                new PropertyMetadata(false, OnEnableDragMoveChanged));

        public static void SetEnableDragMove(DependencyObject element, bool value)
            => element.SetValue(EnableDragMoveProperty, value);

        public static bool GetEnableDragMove(DependencyObject element)
            => (bool)element.GetValue(EnableDragMoveProperty);

        private static void OnEnableDragMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement ui)
            {
                if ((bool)e.NewValue)
                {
                    ui.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                    ui.MouseRightButtonUp += OnMouseRightButtonUp;

                    CreateWindowChrome(ui);
                }
                else
                {
                    ui.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                    ui.MouseRightButtonUp -= OnMouseRightButtonUp;
                }
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DependencyObject d) return;
            var window = Window.GetWindow(d);
            if (window == null) return;

            // 더블클릭 (최대화/복원)
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

            try { window.DragMove(); } catch { /* ignore */ }
        }

        // 우클릭: 시스템 메뉴
        private static void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DependencyObject d) return;
            var window = Window.GetWindow(d);
            if (window == null) return;

            var p = e.GetPosition(window);
            SystemCommands.ShowSystemMenu(window, window.PointToScreen(p));
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

        private static void CreateWindowChrome(UIElement ui)
        {
            var window = Window.GetWindow(ui);
            if (window == null) return;

            // WindowChrome이 이미 존재하면 스킵
            if (WindowChrome.GetWindowChrome(window) != null) return;

            var chrome = new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness,
                UseAeroCaptionButtons = false,
            };
            WindowChrome.SetWindowChrome(window, chrome);

            // WndProc hook (윈도우별 로컬 변수로 관리)
            HwndSource? hwndSource = null;

            window.SourceInitialized += (s, e) =>
            {
                hwndSource = (HwndSource)PresentationSource.FromVisual(window);
                hwndSource?.AddHook(WndProc);
            };

            window.Closed += (s, e) =>
            {
                hwndSource?.RemoveHook(WndProc);
                hwndSource = null;
            };
        }
        
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
