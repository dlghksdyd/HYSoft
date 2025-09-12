using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Colors
{
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class ColorExtension : MarkupExtension
    {
        public ColorExtension() { }
        public ColorExtension(EColorKeys key) => Key = key;

        /// <summary>매핑할 키 (필수)</summary>
        public EColorKeys Key { get; set; }

        /// <summary>
        /// true면 DynamicResource로 반환하여, 런타임에 리소스 교체(테마 스왑 등) 시 자동 반영.
        /// false면 StaticResource/팔레트 값 반환.
        /// </summary>
        public bool UseDynamic { get; set; } = false;

        /// <summary>
        /// 리소스 딕셔너리에 키가 없을 때 ColorPalette로 폴백할지 여부
        /// (UseDynamic=true인데 리소스가 없다면, 동적 리소스도 의미가 없으므로 팔레트로 즉시 폴백)
        /// </summary>
        public bool FallbackToPalette { get; set; } = true;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Key == 0)
                return Brushes.Transparent;

            // ComponentResourceKey는 ResourceId로 "동일 타입"이 와야 합니다.
            // 리소스 사전에서 ResourceId={x:Static c:EColorKeys.*} 를 썼으므로, 여기서도 enum 값 그대로 사용!
            var resKey = new ComponentResourceKey(typeof(ColorKeys), Key);

            // 1) DynamicResource 모드: 라이브 업데이트가 필요할 때
            if (UseDynamic)
            {
                // 동적 리소스로 바로 넘기면, 아직 리소스가 늦게 로드되더라도 바인딩 유지됨.
                // 다만, 해당 리소스가 "끝내" 존재하지 않을 경우 빈 값이라 Fallback을 원하면 팔레트 즉시 반환.
                if (FallbackToPalette && !ResourceExists(resKey, serviceProvider))
                    return ColorPalette.GetBrush(Key);

                return new DynamicResourceExtension(resKey).ProvideValue(serviceProvider);
            }

            // 2) Static 모드: 우선 리소스 찾기
            // StaticResource는 파싱 시점에 없으면 예외가 날 수 있으니 TryFind로 먼저 확인
            var found = TryFindResource(resKey, serviceProvider) as SolidColorBrush;
            if (found != null)
                return found;

            // 3) 리소스가 없으면 ColorPalette로 폴백
            return ColorPalette.GetBrush(Key);
        }

        /// <summary>
        /// serviceProvider를 활용해, 타겟 요소→리소스 찾기 → 없으면 Application에서 찾기
        /// </summary>
        private static object TryFindResource(object key, IServiceProvider sp)
        {
            try
            {
                var pvt = sp?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                var targetObject = pvt?.TargetObject;

                // 템플릿 내부 등에서 SharedDp sentinel이 넘어오는 경우가 있어 DependencyObject 캐스팅 전 확인 필요
                if (targetObject is DependencyObject d)
                {
                    if (d is FrameworkElement fe)
                        return fe.TryFindResource(key);
                    if (d is FrameworkContentElement fce)
                        return fce.TryFindResource(key);
                }

                // 최후: Application 전역
                return Application.Current?.TryFindResource(key);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>리소스 존재 여부만 확인(예외 방지)</summary>
        private static bool ResourceExists(object key, IServiceProvider sp)
            => TryFindResource(key, sp) != null;
    }

    public static class ColorKeys
    {
    }

    public enum EColorKeys
    {
        // 브랜드 정체성
        BrandPrimary,
        BrandSecondary,
        BrandTertiary,
        BrandQuaternary,

        // 상태 (피드백성)
        StateInfo,
        StateSuccess,
        StateWarning,
        StateError,

        // 인터랙션 상태
        StateHover,
        StateActive,
        StateDisabled,

        // 텍스트
        TextPrimary,
        TextSecondary,
        TextTertiary,
        TextQuaternary,

        // 배경
        SurfacePrimary,
        SurfaceSecondary,
        SurfaceTertiary,
        SurfaceQuaternary,

        // 보더
        BorderPrimary,
        BorderSecondary,
        BorderTertiary,
        BorderQuaternary,

        // 아이콘
        IconPrimary,
        IconSecondary,
        IconTertiary,
        IconQuaternary,

        // 레이어
        LayerBase,
        Layer1,
        Layer2,
        Layer3,
    }
}
