using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.ColorTokens
{
    /// <summary>
    /// 전역 컬러 팔레트.
    /// - 각 키(EColorKeys)당 "하나의" SolidColorBrush 인스턴스를 유지합니다.
    /// - 색 변경은 브러시 교체가 아니라 "제자리(in-place)" 업데이트로 처리하여
    ///   이미 UI에 연결된 컨트롤에 즉시 반영됩니다.
    /// - Freeze 하지 않습니다(동적 변경을 위해).
    ///
    /// 사용 가이드:
    ///  1) XAML: Background="{hy:Color SurfacePrimary}" 처럼 팔레트 브러시 인스턴스를 꽂아둡니다.
    ///  2) 런타임/디자인타임에 색 교체: ColorPalette.Override(EColorKeys.SurfacePrimary, "#112233");
    ///     → 같은 브러시 인스턴스의 Color만 바뀌니 UI가 즉시 갱신됩니다.
    ///  3) 브러시 참조를 바꾸어야 하는 특별한 경우에만 Replace를 사용하세요.
    ///
    /// 스레드/디스패처:
    ///  - 브러시 생성 및 변경은 해당 브러시가 귀속된 Dispatcher에서 수행해야 안전합니다.
    ///  - 생성 시 UI Dispatcher에 귀속되도록 MakeBrush로 만들며,
    ///    Override에서는 기존 브러시의 Dispatcher를 통해 안전하게 변경합니다.
    /// </summary>
    public static class ColorPalette
    {
        // 단일 모드 팔레트 (각 키당 "하나의" Brush 인스턴스 유지)
        private static readonly Dictionary<EColorKeys, SolidColorBrush> _map = BuildDefaults();

        // 팔레트 전체 보호용 락. (브러시 생성/등록/링크 업데이트 등 원자적 보장)
        private static readonly object _gate = new();

        public static event Action<EColorKeys, Color, double?>? Changed;

        /// <summary>
        /// 키에 해당하는 브러시를 반환합니다.
        /// 존재하지 않으면 Transparent를 반환합니다. (null 반환 방지)
        /// </summary>
        public static SolidColorBrush GetBrush(EColorKeys key)
            => _map.TryGetValue(key, out var b) ? b : Brushes.Transparent;

        /// <summary>
        /// 키에 해당하는 브러시의 Color를 반환합니다. (편의용)
        /// </summary>
        public static Color GetColor(EColorKeys key) => GetBrush(key).Color;

        // hex → Color (예외 버전)
        private static Color ToColor(string hex)
            => (Color)ColorConverter.ConvertFromString((hex ?? "").Trim());

        // ========= Override API =========

        /// <summary>
        /// 안전한 hex 파싱 시도. 실패 시 Transparent 반환.
        /// 잘못된 입력이 들어와도 예외가 바깥으로 전파되지 않도록 합니다.
        /// </summary>
        private static bool TryToColor(string? hex, out Color color)
        {
            try
            {
                color = (Color)ColorConverter.ConvertFromString((hex ?? "").Trim());
                return true;
            }
            catch
            {
                color = Colors.Transparent;
                return false;
            }
        }

        /// <summary>
        /// 현재 키의 Brush 인스턴스를 교체하지 않고,
        /// "제자리(in-place)"로 색/불투명도를 변경합니다. (hex 입력)
        /// UI는 즉시 갱신됩니다.
        /// </summary>
        public static void Override(EColorKeys key, string hex, double? opacity = null)
        {
            if (TryToColor(hex, out var c)) Override(key, c, opacity);
        }

        /// <summary>
        /// 현재 키의 Brush 인스턴스를 교체하지 않고,
        /// "제자리(in-place)"로 색/불투명도를 변경합니다. (Color 입력)
        /// - 동일 브러시 인스턴스 유지 → UI 즉시 갱신
        /// - opacity가 지정되면 0~1 범위에서 적용
        /// </summary>
        public static void Override(EColorKeys key, Color color, double? opacity = null)
        {
            lock (_gate)
            {
                if (!_map.TryGetValue(key, out var brush))
                {
                    brush = new SolidColorBrush(color);
                    if (opacity.HasValue) brush.Opacity = Clamp01(opacity.Value);
                    _map[key] = brush;
                }
                else
                {
                    void Apply()
                    {
                        brush.Color = color;
                        if (opacity.HasValue) brush.Opacity = Clamp01(opacity.Value);
                    }
                    var disp = brush.Dispatcher ?? Application.Current?.Dispatcher;
                    if (disp?.CheckAccess() == true) Apply();
                    else disp?.Invoke(Apply);
                }
            }

            // 🔔 구독자(릴레이 브러시)에게 알림
            Changed?.Invoke(key, color, opacity);
        }

        /// <summary>
        /// Brush 인스턴스를 "교체"합니다.
        /// · 주의: 기존 컨트롤들이 참조 중이던 브러시는 더 이상 업데이트되지 않습니다.
        /// · 즉, 이미 UI에 꽂힌 곳엔 반영되지 않습니다.
        /// · 특별한 이유(성능 최적화, 다른 Dispatcher 귀속 등)가 아니면 Override 사용을 권장합니다.
        /// </summary>
        public static void Replace(EColorKeys key, SolidColorBrush newBrush)
        {
            if (newBrush == null) return;
            lock (_gate) _map[key] = newBrush;
        }

        /// <summary>
        /// 여러 키를 한 번에 덮어쓰기 (hex 맵).
        /// 내부적으로 Override를 호출하므로 UI는 즉시 갱신됩니다.
        /// </summary>
        public static void OverrideMany(IDictionary<EColorKeys, string> hexMap)
        {
            if (hexMap == null) return;
            foreach (var kv in hexMap) Override(kv.Key, kv.Value);
        }

        /// <summary>
        /// 지정한 Color/Opacity로 UI Dispatcher에 귀속된 브러시를 생성합니다.
        /// - 항상 Freeze하지 않습니다(동적 변경을 위해).
        /// - Application.Current가 없을 수 있는 디자인/테스트 환경을 고려해 안전 경로 포함.
        /// </summary>
        private static SolidColorBrush MakeBrush(Color c, double? op = null)
        {
            void Init(SolidColorBrush b)
            {
                b.Color = c;
                if (op.HasValue) b.Opacity = Clamp01(op.Value);
            }

            var disp = Application.Current?.Dispatcher;
            if (disp?.CheckAccess() == true)
            {
                var b = new SolidColorBrush();
                Init(b);
                return b;
            }
            else if (disp != null)
            {
                return disp.Invoke(() =>
                {
                    var b = new SolidColorBrush();
                    Init(b);
                    return b;
                });
            }
            else // 안전망: 디스패처가 없으면 그냥 생성(디자인/테스트 환경)
            {
                var b = new SolidColorBrush();
                Init(b);
                return b;
            }
        }

        /// <summary>
        /// 0~1 범위로 Clamping.
        /// </summary>
        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        // ========= Defaults =========

        /// <summary>
        /// 초기 팔레트 구성.
        /// - 각 키(EColorKeys)를 순회하며 Application 리소스(Colors.xaml → Color.Semantics.xaml)에서
        ///   기본 색을 가져와 UI Dispatcher에 귀속된 SolidColorBrush 인스턴스를 만듭니다.
        /// - 리소스가 없으면 Transparent로 초기화합니다.
        /// - 생성된 브러시는 교체하지 않고, 이후 Override()로 in-place 변경됩니다.
        /// </summary>
        private static Dictionary<EColorKeys, SolidColorBrush> BuildDefaults()
        {
            var dict = new Dictionary<EColorKeys, SolidColorBrush>();

            // Application 리소스 접근 (디자인/테스트 환경 대비)
            ResourceDictionary? appRes = null;
            try { appRes = Application.Current?.Resources; } catch { /* no-op */ }

            foreach (EColorKeys key in Enum.GetValues(typeof(EColorKeys)))
            {
                Color baseColor = Colors.Transparent;

                // Colors.xaml 외부 노출 리소스 키: {ComponentResourceKey TypeInTargetAssembly={x:Type ColorKeys}, ResourceId=EColorKeys.Key}
                // C#에서는 다음과 같이 구성됩니다.
                var componentKey = new ComponentResourceKey(typeof(ColorKeys), key);

                // 1) SolidColorBrush 리소스로 제공되는 경우 (Colors.xaml에서 노출된 시맨틱 브러시)
                if (appRes != null && appRes.Contains(componentKey))
                {
                    var val = appRes[componentKey];
                    if (val is SolidColorBrush b && b != null)
                    {
                        baseColor = b.Color;
                    }
                    else if (val is Color c)
                    {
                        baseColor = c;
                    }
                    // 다른 타입이면 무시 → Transparent 유지
                }
                else
                {
                    // 2) 혹시 직접 시맨틱 키 이름으로 Color가 들어있는 경우를 대비 (드물지만 안전망)
                    //    예: Color.Semantics.xaml에 Color로 들어있거나 별도 병합 순서 차이 등
                    var fallbackKey = key.ToString(); // e.g., ButtonPrimarySurface
                    if (appRes != null && appRes.Contains(fallbackKey))
                    {
                        var val = appRes[fallbackKey];
                        if (val is SolidColorBrush b2 && b2 != null)
                            baseColor = b2.Color;
                        else if (val is Color c2)
                            baseColor = c2;
                    }
                }

                // 최종 기본색으로 UI Dispatcher에 귀속된 브러시 생성
                var brush = MakeBrush(baseColor);
                dict[key] = brush;
            }

            return dict;
        }
    }
}