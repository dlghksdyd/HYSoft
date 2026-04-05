using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyListBox : ListBox
    {
        static HyListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyListBox), new FrameworkPropertyMetadata(typeof(HyListBox)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyListBox),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }

    public class HyListBoxItem : ListBoxItem
    {
        static HyListBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyListBoxItem), new FrameworkPropertyMetadata(typeof(HyListBoxItem)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyListBoxItem),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
