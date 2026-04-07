using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyLabel : Label
    {
        static HyLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyLabel), new FrameworkPropertyMetadata(typeof(HyLabel)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyLabel),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
