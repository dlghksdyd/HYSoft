using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    /// <summary>
    /// HyComboBoxItem — ComboBoxItem 래퍼.
    /// </summary>
    public class HyComboBoxItem : ComboBoxItem
    {
        static HyComboBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyComboBoxItem), new FrameworkPropertyMetadata(typeof(HyComboBoxItem)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyComboBoxItem),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
