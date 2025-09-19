using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HYSoft.Presentation.Converters
{
    public class WidthHeightToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = values[0] is double dw ? dw : 0;
            double h = values[1] is double dh ? dh : 0;
            return new Rect(0, 0, Math.Max(0, w), Math.Max(0, h));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => Binding.DoNothing as object[];
    }
}
