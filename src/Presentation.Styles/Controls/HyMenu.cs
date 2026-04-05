using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyMenu : Menu
    {
        static HyMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyMenu), new FrameworkPropertyMetadata(typeof(HyMenu)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyMenu),
                new FrameworkPropertyMetadata(new CornerRadius(0)));
    }

    public class HyContextMenu : ContextMenu
    {
        static HyContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyContextMenu), new FrameworkPropertyMetadata(typeof(HyContextMenu)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyContextMenu),
                new FrameworkPropertyMetadata(new CornerRadius(4)));
    }

    public class HyMenuItem : MenuItem
    {
        static HyMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyMenuItem), new FrameworkPropertyMetadata(typeof(HyMenuItem)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyMenuItem),
                new FrameworkPropertyMetadata(new CornerRadius(0)));
    }
}
