using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HYSoft.Presentation.Styles.Controls
{
    public enum EToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class HyToast : Control
    {
        private DispatcherTimer? _autoCloseTimer;

        static HyToast()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyToast), new FrameworkPropertyMetadata(typeof(HyToast)));
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(HyToast),
                new FrameworkPropertyMetadata(null));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty ToastTypeProperty =
            DependencyProperty.Register(
                nameof(ToastType),
                typeof(EToastType),
                typeof(HyToast),
                new FrameworkPropertyMetadata(EToastType.Info));

        public EToastType ToastType
        {
            get => (EToastType)GetValue(ToastTypeProperty);
            set => SetValue(ToastTypeProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyToast),
                new FrameworkPropertyMetadata(new CornerRadius(4)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register(
                nameof(ShowCloseButton),
                typeof(bool),
                typeof(HyToast),
                new FrameworkPropertyMetadata(true));

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        public static readonly DependencyProperty AutoCloseDelayProperty =
            DependencyProperty.Register(
                nameof(AutoCloseDelay),
                typeof(int),
                typeof(HyToast),
                new FrameworkPropertyMetadata(3000));

        public int AutoCloseDelay
        {
            get => (int)GetValue(AutoCloseDelayProperty);
            set => SetValue(AutoCloseDelayProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(HyToast),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyToast toast)
            {
                if ((bool)e.NewValue)
                    toast.StartAutoCloseTimer();
                else
                    toast.StopAutoCloseTimer();
            }
        }

        private void StartAutoCloseTimer()
        {
            StopAutoCloseTimer();

            if (AutoCloseDelay <= 0)
                return;

            _autoCloseTimer = new DispatcherTimer
            {
                Interval = System.TimeSpan.FromMilliseconds(AutoCloseDelay)
            };
            _autoCloseTimer.Tick += (s, e) =>
            {
                IsOpen = false;
                StopAutoCloseTimer();
            };
            _autoCloseTimer.Start();
        }

        private void StopAutoCloseTimer()
        {
            if (_autoCloseTimer is not null)
            {
                _autoCloseTimer.Stop();
                _autoCloseTimer = null;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_CloseButton") is Button closeButton)
            {
                closeButton.Click += (s, e) => IsOpen = false;
            }
        }
    }
}
