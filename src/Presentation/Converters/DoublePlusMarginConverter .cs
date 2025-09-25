using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HYSoft.Presentation.Converters
{
    /// <summary>
    /// values[0] + (values[1] * 2) 를 반환하는 컨버터
    /// </summary>
    public class DoublePlusMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double v0 = 0d;
            double v1 = 0d;

            if (values.Length > 0 && values[0] is double d0)
                v0 = d0;

            if (values.Length > 1 && values[1] is double d1)
                v1 = d1;

            return v0 + (v1 * 2);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
