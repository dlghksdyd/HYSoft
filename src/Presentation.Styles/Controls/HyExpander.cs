using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyExpander : Expander
    {
        static HyExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyExpander), new FrameworkPropertyMetadata(typeof(HyExpander)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyExpander),
                new FrameworkPropertyMetadata(new CornerRadius(4)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }

    public class HyGroupBox : GroupBox
    {
        static HyGroupBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyGroupBox), new FrameworkPropertyMetadata(typeof(HyGroupBox)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyGroupBox),
                new FrameworkPropertyMetadata(new CornerRadius(4)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
