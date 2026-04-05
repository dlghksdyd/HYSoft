using System.Windows;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyWindow : Window
    {
        static HyWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyWindow), new FrameworkPropertyMetadata(typeof(HyWindow)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyWindow),
                new PropertyMetadata(new CornerRadius(0)));
    }
}
