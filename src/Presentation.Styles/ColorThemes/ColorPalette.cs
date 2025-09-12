using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.ColorThemes
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
        /// - 각 키에 대해 UI Dispatcher에 귀속된 브러시를 생성합니다.
        /// - Freeze하지 않으며, 이후 Override로 in-place 변경됩니다.
        /// </summary>
        private static Dictionary<EColorKeys, SolidColorBrush> BuildDefaults()
        {
            SolidColorBrush B(string hex, double? op = null)
            {
                // ToColor는 예외 버전이므로 초기값은 확실한 hex만 사용
                var b = MakeBrush(ToColor(hex), op);
                // (op는 MakeBrush에서 이미 적용되므로 아래 중복 적용은 무해하지만 명시적으로 남겨둡니다)
                if (op.HasValue) b.Opacity = Clamp01(op.Value);
                return b;
            }

            // 최신 키셋(EColorKeys) 기준 기본값
            return new Dictionary<EColorKeys, SolidColorBrush>
            {
                // Brand
                { EColorKeys.BrandPrimary,      B("#0B78BC") },
                { EColorKeys.BrandSecondary,    B("#E48900") },
                { EColorKeys.BrandTertiary,     B("#0A6DAA") },
                { EColorKeys.BrandQuaternary,   B("#095F95") },

                // State (Info/Success/Warning/Error + Hover/Active/Disabled 오버레이 감각)
                { EColorKeys.StateInfo,         B("#D1E8EF") },
                { EColorKeys.StateSuccess,      B("#00C763") },
                { EColorKeys.StateWarning,      B("#D97706") },
                { EColorKeys.StateError,        B("#FF3A3A") },

                { EColorKeys.StateHover,        B("#26FFFFFF") }, // ~15% white overlay
                { EColorKeys.StateActive,       B("#40FFFFFF") }, // ~25% white overlay
                { EColorKeys.StateDisabled,     B("#5CFFFFFF") }, // ~36% white overlay

                // Text
                { EColorKeys.TextPrimary,       B("#EAEAEA") },
                { EColorKeys.TextSecondary,     B("#BBBBBB") },
                { EColorKeys.TextTertiary,      B("#9CA3AF") },
                { EColorKeys.TextQuaternary,    B("#6B7280") },

                // Surface (다크 톤 예시)
                { EColorKeys.SurfacePrimary,    B("#1E2736") },
                { EColorKeys.SurfaceSecondary,  B("#2D3847") },
                { EColorKeys.SurfaceTertiary,   B("#4E5969") },
                { EColorKeys.SurfaceQuaternary, B("#34495E") },

                // Border
                { EColorKeys.BorderPrimary,     B("#999999") },
                { EColorKeys.BorderSecondary,   B("#7A8698") },
                { EColorKeys.BorderTertiary,    B("#5B6B7D") },
                { EColorKeys.BorderQuaternary,  B("#3E4C59") },

                // Icon (텍스트 톤과 동일 시작)
                { EColorKeys.IconPrimary,       B("#EAEAEA") },
                { EColorKeys.IconSecondary,     B("#BBBBBB") },
                { EColorKeys.IconTertiary,      B("#9CA3AF") },
                { EColorKeys.IconQuaternary,    B("#6B7280") },

                // Layer (초기에는 Surface 단계와 동일값)
                { EColorKeys.LayerBase,         B("#1E2736") },
                { EColorKeys.Layer1,            B("#2D3847") },
                { EColorKeys.Layer2,            B("#4E5969") },
                { EColorKeys.Layer3,            B("#34495E") },
            };
        }
    }
}