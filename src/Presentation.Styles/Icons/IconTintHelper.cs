using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HYSoft.Presentation.Styles.Icons
{
    public static class IconTintHelper
    {
        public static ImageSource TintImage(ImageSource source, Color color)
        {
            if (source is not BitmapSource bmp) return source;

            double srcDpiX = bmp.DpiX > 0 ? bmp.DpiX : 96.0;
            double srcDpiY = bmp.DpiY > 0 ? bmp.DpiY : 96.0;
            int pxW = System.Math.Max(1, bmp.PixelWidth);
            int pxH = System.Math.Max(1, bmp.PixelHeight);

            double dipW = pxW * 96.0 / srcDpiX;
            double dipH = pxH * 96.0 / srcDpiY;

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var rect = new System.Windows.Rect(0, 0, dipW, dipH);

                var mask = new ImageBrush(bmp) { Stretch = Stretch.Fill };
                dc.PushOpacityMask(mask);

                var fill = new SolidColorBrush(color);
                if (fill.CanFreeze) fill.Freeze();
                dc.DrawRectangle(fill, null, rect);

                dc.Pop();
            }

            var rtb = new RenderTargetBitmap(pxW, pxH, srcDpiX, srcDpiY, PixelFormats.Pbgra32);
            rtb.Render(dv);
            if (rtb.CanFreeze) rtb.Freeze();
            return rtb;
        }
    }
}