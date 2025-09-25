using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HYSoft.Presentation.Converters
{
    public class CornerRadiusToClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = values[0] is double dw ? dw : 0;
            double h = values[1] is double dh ? dh : 0;
            CornerRadius cr = values[2] is CornerRadius c ? c : new CornerRadius(0);

            double r = cr.TopLeft; // 유니폼 반경 사용
            // 반경이 크기를 넘지 않도록 클램프
            r = Math.Max(0, Math.Min(r, Math.Min(w, h) / 2.0));

            return new RectangleGeometry(new Rect(0, 0, w, h), r, r);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
