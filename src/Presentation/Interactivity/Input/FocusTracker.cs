using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HYSoft.Presentation.Interactivity.Input
{
    public static class FocusTracker
    {
        public static IInputElement? Current { get; private set; }
        public static IInputElement? Previous { get; private set; }

        private static bool _initialized;

        public static void InitOnce()
        {
            if (_initialized) return;
            _initialized = true;

            // 요소에서 포커스를 얻기 직전에 호출(부모부터 터널링)
            EventManager.RegisterClassHandler(typeof(UIElement),
                Keyboard.GotKeyboardFocusEvent,
                new KeyboardFocusChangedEventHandler(OnGotFocus), true);
        }

        private static void OnGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // e.OldFocus가 null일 수도 있음
            Previous = e.OldFocus;
            Current = e.NewFocus;
        }

        public static void RestorePrevious()
        {
            if (Previous is IInputElement ie && ie is DependencyObject d && IsFocusable(d))
            {
                // 레이아웃/활성화 이후로 미뤄야 할 때도 있음
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => Keyboard.Focus(ie)),
                    System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private static bool IsFocusable(DependencyObject d)
        {
            return d is UIElement u && u.IsVisible && u.IsEnabled && u.Focusable;
        }
    }
}
