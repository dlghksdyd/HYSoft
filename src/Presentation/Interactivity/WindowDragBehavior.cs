using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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

            // 더블클릭: 최대화/복원
            if (e.ClickCount == 2 && window.ResizeMode != ResizeMode.NoResize)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
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
    }
}
