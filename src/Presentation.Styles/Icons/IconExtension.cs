using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HYSoft.Presentation.Styles.Icons
{
    [MarkupExtensionReturnType(typeof(ImageSource))]
    public sealed class IconExtension : MarkupExtension
    {
        public IconExtension() { }
        public IconExtension(EIconKeys iconKey) => IconKey = iconKey;

        [ConstructorArgument("iconKey")]
        public EIconKeys IconKey { get; set; }

        /// <summary>
        /// 직접 색 지정 (#RRGGBB 문자열 또는 Color). 지정 시 ColorKey보다 우선.
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// 디자인 토큰(EColorKeys)을 통한 간접 색 지정. SolidColorBrush.Color를 사용.
        /// </summary>
        public HYSoft.Presentation.Styles.ColorTokens.EColorKeys? ColorKey { get; set; }

        // ColorTokens → Brush 맵을 1회 생성/캐시
        private static readonly Lazy<Dictionary<HYSoft.Presentation.Styles.ColorTokens.EColorKeys, Brush>>
            _colorMap = new(() => HYSoft.Presentation.Styles.ColorTokens.ColorGenerator.Generate());

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var src = IconGenerator.GetIcon(IconKey);

            // 1) Color가 지정되면 최우선 적용
            if (Color is { } directColor)
                return TintImage(src, directColor);

            // 2) ColorKey가 지정되면 해당 Brush에서 Color 추출
            if (ColorKey is { } key &&
                _colorMap.Value.TryGetValue(key, out var brush) &&
                brush is SolidColorBrush scb)
            {
                return TintImage(src, scb.Color);
            }

            // 3) 색 미지정 시 원본 반환
            return src;
        }

        private static ImageSource TintImage(ImageSource source, Color color)
        {
            if (source is not BitmapSource bmp) return source;

            // 1) 원본 DPI와 픽셀 크기
            double srcDpiX = bmp.DpiX > 0 ? bmp.DpiX : 96.0;
            double srcDpiY = bmp.DpiY > 0 ? bmp.DpiY : 96.0;
            int pxW = Math.Max(1, bmp.PixelWidth);
            int pxH = Math.Max(1, bmp.PixelHeight);

            // 2) DIP 크기 = 픽셀 * (96 / DPI)
            double dipW = pxW * 96.0 / srcDpiX;
            double dipH = pxH * 96.0 / srcDpiY;

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var rect = new System.Windows.Rect(0, 0, dipW, dipH);

                // 원본 비트맵을 알파 마스크로 사용
                var mask = new ImageBrush(bmp) { Stretch = Stretch.Fill };
                dc.PushOpacityMask(mask);

                var fill = new SolidColorBrush(color);
                if (fill.CanFreeze) fill.Freeze();
                dc.DrawRectangle(fill, null, rect);

                dc.Pop();
            }

            // 3) RTB는 '픽셀 크기'와 '원본 DPI'로 생성 (DIP↔px 매핑이 정확)
            var rtb = new RenderTargetBitmap(pxW, pxH, srcDpiX, srcDpiY, PixelFormats.Pbgra32);
            rtb.Render(dv);
            if (rtb.CanFreeze) rtb.Freeze();
            return rtb;
        }
    }
}
