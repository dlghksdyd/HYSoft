using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyToggleSwitch : ToggleButton
    {
        // Converter: double -> CornerRadius (half value)
        public static readonly IValueConverter HalfConverter = new HalfToCornerRadiusConverter();
        // Converter: (SwitchWidth, ThumbSize) -> thumb slide distance
        public static readonly IMultiValueConverter ThumbOffsetConverter = new ThumbSlideOffsetConverter();

        static HyToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyToggleSwitch), new FrameworkPropertyMetadata(typeof(HyToggleSwitch)));
        }

        public double SwitchWidth
        {
            get => (double)GetValue(SwitchWidthProperty);
            set => SetValue(SwitchWidthProperty, value);
        }
        public static readonly DependencyProperty SwitchWidthProperty =
            DependencyProperty.Register(nameof(SwitchWidth), typeof(double), typeof(HyToggleSwitch),
                new PropertyMetadata(44.0));

        public double SwitchHeight
        {
            get => (double)GetValue(SwitchHeightProperty);
            set => SetValue(SwitchHeightProperty, value);
        }
        public static readonly DependencyProperty SwitchHeightProperty =
            DependencyProperty.Register(nameof(SwitchHeight), typeof(double), typeof(HyToggleSwitch),
                new PropertyMetadata(22.0));

        public double ThumbSize
        {
            get => (double)GetValue(ThumbSizeProperty);
            set => SetValue(ThumbSizeProperty, value);
        }
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(nameof(ThumbSize), typeof(double), typeof(HyToggleSwitch),
                new PropertyMetadata(16.0));

        public Brush OnBackground
        {
            get => (Brush)GetValue(OnBackgroundProperty);
            set => SetValue(OnBackgroundProperty, value);
        }
        public static readonly DependencyProperty OnBackgroundProperty =
            DependencyProperty.Register(nameof(OnBackground), typeof(Brush), typeof(HyToggleSwitch),
                new PropertyMetadata(null));

        public Brush OffBackground
        {
            get => (Brush)GetValue(OffBackgroundProperty);
            set => SetValue(OffBackgroundProperty, value);
        }
        public static readonly DependencyProperty OffBackgroundProperty =
            DependencyProperty.Register(nameof(OffBackground), typeof(Brush), typeof(HyToggleSwitch),
                new PropertyMetadata(null));

        private sealed class HalfToCornerRadiusConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double d)
                    return new CornerRadius(d / 2.0);
                return new CornerRadius(0);
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotSupportedException();
        }

        private sealed class ThumbSlideOffsetConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                // values[0] = SwitchWidth, values[1] = ThumbSize
                if (values.Length >= 2 && values[0] is double width && values[1] is double thumb)
                    return width - thumb - 6.0; // 6 = left margin (3) + right margin (3)
                return 0.0;
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotSupportedException();
        }
    }
}
