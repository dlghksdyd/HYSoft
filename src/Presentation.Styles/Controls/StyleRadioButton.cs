using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class StyleRadioButton : RadioButton
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

        static StyleRadioButton()
        {
            ButtonSizeProperty = DependencyProperty.Register("ButtonSize", typeof(double), typeof(StyleRadioButton), new FrameworkPropertyMetadata());
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(StyleRadioButton), new FrameworkPropertyMetadata());
            TextPaddingProperty = DependencyProperty.Register("TextPadding", typeof(GridLength), typeof(StyleRadioButton), new FrameworkPropertyMetadata());
        }
    }
}
