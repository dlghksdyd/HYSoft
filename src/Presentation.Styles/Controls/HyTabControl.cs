using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyTabControl : TabControl
    {
        public static readonly DependencyProperty CornerRadiusProperty;

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static HyTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTabControl), new FrameworkPropertyMetadata(typeof(HyTabControl)));
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HyTabControl), new FrameworkPropertyMetadata());
        }
    }

    public class HyTabItem : TabItem
    {
        public static readonly DependencyProperty CornerRadiusProperty;

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static HyTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTabItem), new FrameworkPropertyMetadata(typeof(HyTabItem)));
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HyTabItem), new FrameworkPropertyMetadata());
        }
    }
}
