using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyToolTip : ToolTip
    {
        static HyToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyToolTip), new FrameworkPropertyMetadata(typeof(HyToolTip)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyToolTip),
                new FrameworkPropertyMetadata(new CornerRadius(4)));
    }
}
