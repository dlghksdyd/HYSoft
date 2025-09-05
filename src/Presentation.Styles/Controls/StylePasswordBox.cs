using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class StylePasswordBox : TextBox
    {
        public static readonly DependencyProperty WaterMarkProperty;
        public static readonly DependencyProperty WaterMarkForegroundProperty;
        public static readonly DependencyProperty CornerRadiusProperty;

        public string WaterMark
        {
            get { return (string)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }

        public SolidColorBrush WaterMarkForeground
        {
            get { return (SolidColorBrush)GetValue(WaterMarkForegroundProperty); }
            set { SetValue(WaterMarkForegroundProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private PasswordBox? _passwordBox;

        static StylePasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StylePasswordBox), new FrameworkPropertyMetadata(typeof(StylePasswordBox)));

            WaterMarkProperty = DependencyProperty.Register("WaterMark", typeof(string), typeof(StylePasswordBox), new FrameworkPropertyMetadata());
            WaterMarkForegroundProperty = DependencyProperty.Register("WaterMarkForeground", typeof(SolidColorBrush), typeof(StylePasswordBox), new FrameworkPropertyMetadata());
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(StylePasswordBox), new FrameworkPropertyMetadata());
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_passwordBox != null)
                _passwordBox.PasswordChanged -= _passwordBox_PasswordChanged;

            this.IsTabStop = false;

            _passwordBox = GetTemplateChild("xPasswordBox") as PasswordBox;
            this.TextChanged += StylePasswordBox_TextChanged;

            if (_passwordBox != null)
            {
                _passwordBox.PasswordChanged += _passwordBox_PasswordChanged;
            }
        }

        private void StylePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            StylePasswordBox element = sender as StylePasswordBox;

            //element._passwordBox.Password = element.Text;
        }

        private void _passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox element = sender as PasswordBox;

            this.Text = element.Password;
        }
    }
}
