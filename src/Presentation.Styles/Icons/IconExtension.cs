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
                return IconTintHelper.TintImage(src, directColor);

            // 2) ColorKey가 지정되면 해당 Brush에서 Color 추출
            if (ColorKey is { } key &&
                _colorMap.Value.TryGetValue(key, out var brush) &&
                brush is SolidColorBrush scb)
            {
                return IconTintHelper.TintImage(src, scb.Color);
            }

            // 3) 색 미지정 시 원본 반환
            return src;
        }
    }
}
