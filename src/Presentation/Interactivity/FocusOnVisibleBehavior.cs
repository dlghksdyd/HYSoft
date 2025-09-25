using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace HYSoft.Presentation.Interactivity
{
    public static class FocusOnVisibleBehavior
    {
        public static readonly DependencyProperty WhenTrueProperty =
            DependencyProperty.RegisterAttached(
                "WhenTrue", typeof(bool), typeof(FocusOnVisibleBehavior),
                new PropertyMetadata(false, OnChanged));

        public static void SetWhenTrue(DependencyObject d, bool v) => d.SetValue(WhenTrueProperty, v);
        public static bool GetWhenTrue(DependencyObject d) => (bool)d.GetValue(WhenTrueProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Control c) return;
            if (!(bool)e.NewValue) return;

            void TryFocus()
            {
                c.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!c.IsVisible || !c.IsEnabled) return;
                    if (c.IsKeyboardFocusWithin) return;

                    c.Focus();
                    Keyboard.Focus(c);

                    if (c is TextBox tb)
                    {
                        // 필요에 따라 전체 선택 / 캐럿 이동 택1
                        // tb.SelectAll();
                        tb.CaretIndex = tb.Text?.Length ?? 0;
                    }
                }), DispatcherPriority.Input);
            }

            if (c.IsLoaded && c.IsVisible) TryFocus();
            else
            {
                RoutedEventHandler loaded = null!;
                DependencyPropertyChangedEventHandler vis = null!;

                loaded = (_, __) =>
                {
                    c.Loaded -= loaded;
                    if (c.IsVisible) TryFocus();
                };
                vis = (_, args) =>
                {
                    if (args.NewValue is bool v && v)
                    {
                        c.IsVisibleChanged -= vis;
                        if (c.IsLoaded) TryFocus();
                    }
                };

                c.Loaded += loaded;
                c.IsVisibleChanged += vis;
            }
        }
    }
}
