using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyRadioButton : RadioButton
    {
        public static readonly DependencyProperty ButtonSizeProperty;
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty TextPaddingProperty;

        public double ButtonSize
        {
            get { return (double)GetValue(ButtonSizeProperty); }
            set { SetValue(ButtonSizeProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public GridLength TextPadding
        {
            get { return (GridLength)GetValue(TextPaddingProperty); }
            set { SetValue(TextPaddingProperty, value); }
        }

        static HyRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyRadioButton), new FrameworkPropertyMetadata(typeof(HyRadioButton)));

            ButtonSizeProperty = DependencyProperty.Register("ButtonSize", typeof(double), typeof(HyRadioButton), new FrameworkPropertyMetadata());
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HyRadioButton), new FrameworkPropertyMetadata());
            TextPaddingProperty = DependencyProperty.Register("TextPadding", typeof(GridLength), typeof(HyRadioButton), new FrameworkPropertyMetadata());
        }
    }
}
