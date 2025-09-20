using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HYSoft.Presentation.Styles.Icons
{
    internal static class IconGenerator
    {
        /// <summary>
        /// 아이콘 키에 해당하는 PNG(Resource)를 pack URI로 ImageSource로 로드합니다.
        /// </summary>
        public static ImageSource GetIcon(EIconKeys iconKey)
        {
            if (!IconKeys.IconMap.TryGetValue(iconKey, out var relativePath))
                throw new ArgumentOutOfRangeException(nameof(iconKey), iconKey, "Unknown icon key");

            var asmName = typeof(IconGenerator).Assembly.GetName().Name;
            var component = string.IsNullOrEmpty(asmName) ? string.Empty : asmName + ";component/";

            var uriString = "pack://application:,,,/" + component + relativePath;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(uriString, UriKind.Absolute);
            bmp.EndInit();
            if (bmp.CanFreeze) bmp.Freeze();
            return bmp;
        }
    }
}