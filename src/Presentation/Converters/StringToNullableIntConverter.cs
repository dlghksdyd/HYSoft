using System;
using System.Globalization;
using System.Windows.Data;

namespace HYSoft.Presentation.Converters
{
    public sealed class StringToNullableIntConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString();

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s, NumberStyles.Integer, culture, out var n)) return n;
            return null;
        }
    }
}
