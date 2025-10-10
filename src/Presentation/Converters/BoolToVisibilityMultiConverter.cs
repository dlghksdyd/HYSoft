using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HYSoft.Presentation.Converters
{
    public class BoolToVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // null 체크
            if (values == null || values.Length == 0)
                return Visibility.Collapsed;

            // 모든 바인딩 값이 bool일 때만 검사
            foreach (var v in values)
            {
                if (v is bool b)
                {
                    if (!b) // 하나라도 false면 숨김
                        return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
