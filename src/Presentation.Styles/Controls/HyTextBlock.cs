using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyTextBlock : Control
    {
        static HyTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTextBlock), new FrameworkPropertyMetadata(typeof(HyTextBlock)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyTextBlock),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(HyTextBlock),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty Argument1Property =
            DependencyProperty.Register(
                nameof(Argument1),
                typeof(object),
                typeof(HyTextBlock),
                new FrameworkPropertyMetadata(null)); // 필요시 BindsTwoWayByDefault 등 옵션 추가

        public object? Argument1
        {
            get => GetValue(Argument1Property);
            set => SetValue(Argument1Property, value);
        }

        public static readonly DependencyProperty Argument2Property =
            DependencyProperty.Register(
                nameof(Argument2),
                typeof(object),
                typeof(HyTextBlock),
                new FrameworkPropertyMetadata(null));

        public object? Argument2
        {
            get => GetValue(Argument2Property);
            set => SetValue(Argument2Property, value);
        }
    }
}
