using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyPasswordBox : TextBox
    {
        public static readonly DependencyProperty WatermarkProperty;
        public static readonly DependencyProperty WatermarkForegroundProperty;
        public static readonly DependencyProperty CornerRadiusProperty;

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public SolidColorBrush WatermarkForeground
        {
            get => (SolidColorBrush)GetValue(WatermarkForegroundProperty);
            set => SetValue(WatermarkForegroundProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private PasswordBox? _passwordBox;

        public HyPasswordBox()
        {
            Unloaded += (_, __) =>
            {
                if (_passwordBox != null)
                {
                    _passwordBox.PasswordChanged -= _passwordBox_PasswordChanged;
                    _passwordBox = null;
                }
            };
        }

        static HyPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyPasswordBox), new FrameworkPropertyMetadata(typeof(HyPasswordBox)));

            WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(HyPasswordBox), new FrameworkPropertyMetadata());
            WatermarkForegroundProperty = DependencyProperty.Register("WatermarkForeground", typeof(SolidColorBrush), typeof(HyPasswordBox), new FrameworkPropertyMetadata());
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HyPasswordBox), new FrameworkPropertyMetadata());
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_passwordBox != null)
                _passwordBox.PasswordChanged -= _passwordBox_PasswordChanged;

            this.IsTabStop = false;

            _passwordBox = GetTemplateChild("xPasswordBox") as PasswordBox;

            if (_passwordBox != null)
            {
                _passwordBox.PasswordChanged += _passwordBox_PasswordChanged;
            }
        }

        private void _passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb && pb.Password != null)
                this.Text = pb.Password;
        }
    }
}
