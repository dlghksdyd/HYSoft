using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyStatusBar : StatusBar
    {
        static HyStatusBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyStatusBar), new FrameworkPropertyMetadata(typeof(HyStatusBar)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyStatusBar),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
