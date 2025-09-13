using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.ColorTokens
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public sealed class ColorExtension : MarkupExtension
    {
        public ColorExtension() { }
        public ColorExtension(EColorKeys key) => Key = key;

        [ConstructorArgument("key")]
        public EColorKeys Key { get; set; }

        // (타깃 객체, DP) → 릴레이 엔트리 (향후 실시간 테마 전환용 확장 시 사용 가능)
        private static readonly ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, Entry>> _table = new();

        private sealed class Entry
        {
            public SolidColorBrush RelayBrush { get; set; } = new SolidColorBrush();
            public object ResourceKey { get; set; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 1) 타깃 정보 가져오기
            var pvt = serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var targetObject = pvt?.TargetObject;
            var targetProperty = pvt?.TargetProperty as DependencyProperty;

            // 템플릿/BAML 컴파일 중에는 확정 대상이 아닌 경우가 있음 → 마크업 확장 자신을 반환해 재평가 유도
            if (targetObject is not DependencyObject d || targetProperty is null)
                return this;

            // 2) Brush 키 구성: ComponentResourceKey(typeof(ColorKeys), EColorKeys.Key)
            var brushKey = new ComponentResourceKey(typeof(ColorKeys), Key);

            // 3) 리소스 탐색(우선: 요소의 TryFindResource → 전역 Application)
            if (TryResolveBrush(d, brushKey, out var brush))
                return brush;

            // 4) 폴백: 동일한 이름의 Color 리소스를 찾아서 Brush 생성 시도
            if (TryResolveColorThenMakeBrush(d, Key.ToString(), out var madeBrush))
                return madeBrush;

            // 5) 최종 폴백: 투명 브러시 (누락을 시각적으로 구분하고 싶다면 Debug용 색으로 바꿔도 됨)
            return new SolidColorBrush(Colors.Transparent);
        }

        private static bool TryResolveBrush(DependencyObject d, object resourceKey, out Brush brush)
        {
            // 요소 기준 탐색
            brush = TryFindResourceOnObject(d, resourceKey) as Brush;
            if (brush != null) return true;

            // Application 전역
            if (Application.Current != null)
            {
                brush = Application.Current.TryFindResource(resourceKey) as Brush;
                if (brush != null) return true;
            }

            brush = null;
            return false;
        }

        private static bool TryResolveColorThenMakeBrush(DependencyObject d, string colorKey, out Brush brush)
        {
            object found = TryFindResourceOnObject(d, colorKey);
            found ??= Application.Current?.TryFindResource(colorKey);

            if (found is Color color)
            {
                // Freeze하지 않음: 나중에 런타임에서 색 변경 시 in-place 업데이트 패턴 확장 가능
                var b = new SolidColorBrush(color);
                brush = b;
                return true;
            }

            brush = null;
            return false;
        }

        private static object TryFindResourceOnObject(DependencyObject d, object key)
        {
            // FrameworkElement / FrameworkContentElement 우선
            if (d is FrameworkElement fe)
                return fe.TryFindResource(key);
            if (d is FrameworkContentElement fce)
                return fce.TryFindResource(key);

            // 의존 객체만 있고 요소가 아니면, 상위 트리를 못타므로 null
            return null;
        }
    }
}
