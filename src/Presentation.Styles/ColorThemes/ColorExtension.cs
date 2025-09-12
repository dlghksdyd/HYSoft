using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Markup.Primitives;

namespace HYSoft.Presentation.Styles.ColorThemes
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public sealed class ColorExtension : MarkupExtension
    {
        public ColorExtension() { }
        public ColorExtension(EColorKeys key) => Key = key;

        [ConstructorArgument("key")]
        public EColorKeys Key { get; set; }

        // (타깃 객체, DP) → 릴레이 엔트리
        private static readonly ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, Entry>> _table = new();

        // 팔레트 변경 → 릴레이 갱신
        static ColorExtension()
        {
            ColorPalette.Changed += OnPaletteChanged;
        }

        private static void OnPaletteChanged(EColorKeys changedKey, Color color, double? opacity)
        {
            foreach (var kvObj in _table)
            {
                var obj = kvObj.Key;
                var map = kvObj.Value;
                foreach (var kvDp in map)
                {
                    var entry = kvDp.Value;
                    if (entry.Key == changedKey)
                    {
                        // 같은 릴레이 브러시 인스턴스의 Color만 업데이트 (in-place)
                        void Apply()
                        {
                            entry.Brush.Color = color;
                            if (opacity.HasValue) entry.Brush.Opacity = Clamp01(opacity.Value);
                        }
                        var disp = entry.Brush.Dispatcher ?? Application.Current?.Dispatcher;
                        if (disp?.CheckAccess() == true) Apply();
                        else disp?.Invoke(Apply);
                    }
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 타깃 정보 획득
            var pvt = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var targetObj = pvt?.TargetObject as DependencyObject;
            var targetDp = pvt?.TargetProperty as DependencyProperty;

            // 템플릿/BAML 중간 단계에서는 마크업 체인 반환이 필요할 수 있음
            if (targetObj == null || targetDp == null)
                return this;

            // 타깃 엔트리 확보 (있으면 재사용)
            var map = _table.GetOrCreateValue(targetObj);
            if (!map.TryGetValue(targetDp, out var entry))
            {
                // 새 릴레이 브러시 생성 (이 브러시는 "복사본"이며 Freeze하지 않음)
                var brush = new SolidColorBrush(ColorPalette.GetColor(Key))
                {
                    Opacity = ColorPalette.GetBrush(Key).Opacity
                };
                entry = new Entry { Brush = brush, Key = Key };
                map[targetDp] = entry;
            }
            else
            {
                // 기존 인스턴스 유지 + 색만 새 Key 값으로 갱신
                var src = ColorPalette.GetBrush(Key);
                void Apply()
                {
                    entry.Brush.Color = src.Color;
                    entry.Brush.Opacity = src.Opacity;
                }
                var disp = entry.Brush.Dispatcher ?? Application.Current?.Dispatcher;
                if (disp?.CheckAccess() == true) Apply();
                else disp?.Invoke(Apply);

                entry.Key = Key;
            }

            return entry.Brush;
        }

        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        private sealed class Entry
        {
            public SolidColorBrush Brush { get; set; }
            public EColorKeys Key { get; set; }
        }
    }
}
