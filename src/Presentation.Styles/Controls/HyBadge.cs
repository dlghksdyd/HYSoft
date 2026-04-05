using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public enum EBadgeType
    {
        Neutral,
        Info,
        Success,
        Warning,
        Error,
        Highlight
    }

    public class HyBadge : Control
    {
        static HyBadge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyBadge), new FrameworkPropertyMetadata(typeof(HyBadge)));
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(HyBadge),
                new FrameworkPropertyMetadata(null));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty BadgeTypeProperty =
            DependencyProperty.Register(
                nameof(BadgeType),
                typeof(EBadgeType),
                typeof(HyBadge),
                new FrameworkPropertyMetadata(EBadgeType.Neutral));

        public EBadgeType BadgeType
        {
            get => (EBadgeType)GetValue(BadgeTypeProperty);
            set => SetValue(BadgeTypeProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyBadge),
                new FrameworkPropertyMetadata(new CornerRadius(8)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
