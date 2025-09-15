using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Icons
{
    public sealed class IconKeyToImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is EIconKeys key)
            {
                return IconGenerator.GetIcon(key);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

}
