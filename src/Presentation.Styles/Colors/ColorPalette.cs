using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Colors
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    namespace HYSoft.Presentation.Styles.Colors
    {
        public static class ColorPalette
        {
            // 단일 모드 팔레트 (각 키당 "하나의" Brush 인스턴스 유지)
            private static readonly Dictionary<EColorKeys, SolidColorBrush> _map = BuildDefaults();
            private static readonly object _gate = new();

            public static SolidColorBrush GetBrush(EColorKeys key)
                => _map.TryGetValue(key, out var b) ? b : Brushes.Transparent;

            public static Color GetColor(EColorKeys key) => GetBrush(key).Color;

            // hex → Color
            private static Color ToColor(string hex)
                => (Color)ColorConverter.ConvertFromString((hex ?? "").Trim());

            // ========= Override API =========

            /// <summary>
            /// 현재 키의 Brush 인스턴스를 교체하지 않고, "제자리(in-place)"로 색/불투명도를 변경합니다.
            /// UI는 즉시 갱신됩니다.
            /// </summary>
            public static void Override(EColorKeys key, string hex, double? opacity = null)
                => Override(key, ToColor(hex), opacity);

            /// <summary>
            /// 현재 키의 Brush 인스턴스를 교체하지 않고, "제자리(in-place)"로 색/불투명도를 변경합니다.
            /// </summary>
            public static void Override(EColorKeys key, Color color, double? opacity = null)
            {
                lock (_gate)
                {
                    if (!_map.TryGetValue(key, out var brush))
                    {
                        brush = new SolidColorBrush(color);
                        if (opacity.HasValue) brush.Opacity = opacity.Value;
                        _map[key] = brush;
                        return;
                    }

                    void Apply()
                    {
                        brush.Color = color; // Freezable 변경 → UI 자동 갱신
                        if (opacity.HasValue) brush.Opacity = opacity.Value;
                    }

                    var disp = Application.Current?.Dispatcher;
                    if (disp?.CheckAccess() == true) Apply();
                    else disp?.Invoke(Apply);
                }

                // 팔레트 변경 알림
                PaletteSource.Instance.NotifyKey(key);
            }

            /// <summary>
            /// Brush 인스턴스를 "교체"합니다. 기존 참조는 더 이상 업데이트되지 않습니다.
            /// 특별한 이유가 없으면 in-place Override를 사용하세요.
            /// </summary>
            public static void Replace(EColorKeys key, SolidColorBrush newBrush)
            {
                if (newBrush == null) return;
                lock (_gate) _map[key] = newBrush;
            }

            /// <summary>
            /// 여러 키를 한 번에 덮어쓰기 (hex 맵)
            /// </summary>
            public static void OverrideMany(IDictionary<EColorKeys, string> hexMap)
            {
                if (hexMap == null) return;
                foreach (var kv in hexMap) Override(kv.Key, kv.Value);
            }

            // ========= Defaults =========

            private static Dictionary<EColorKeys, SolidColorBrush> BuildDefaults()
            {
                SolidColorBrush B(string hex, double? op = null)
                {
                    var b = new SolidColorBrush(ToColor(hex));
                    if (op.HasValue) b.Opacity = op.Value;
                    // ❗ Freeze하지 않습니다. (in-place 변경을 위해)
                    return b;
                }

                // 당신의 최신 키셋(EColorKeys) 기준으로 기본값을 채웁니다.
                return new Dictionary<EColorKeys, SolidColorBrush>
                {
                    // Brand
                    { EColorKeys.BrandPrimary, B("#0B78BC") },
                    { EColorKeys.BrandSecondary, B("#E48900") },
                    { EColorKeys.BrandTertiary, B("#0A6DAA") },
                    { EColorKeys.BrandQuaternary, B("#095F95") },

                    // State (Info/Success/Warning/Error + Hover/Active/Disabled 기본 오버레이 감각)
                    { EColorKeys.StateInfo, B("#D1E8EF") },
                    { EColorKeys.StateSuccess, B("#00C763") },
                    { EColorKeys.StateWarning, B("#D97706") },
                    { EColorKeys.StateError, B("#FF3A3A") },

                    { EColorKeys.StateHover, B("#26FFFFFF") }, // ~15% white overlay
                    { EColorKeys.StateActive, B("#40FFFFFF") }, // ~25% white overlay
                    { EColorKeys.StateDisabled, B("#5CFFFFFF") }, // ~36% white overlay

                    // Text
                    { EColorKeys.TextPrimary, B("#EAEAEA") },
                    { EColorKeys.TextSecondary, B("#BBBBBB") },
                    { EColorKeys.TextTertiary, B("#9CA3AF") },
                    { EColorKeys.TextQuaternary, B("#6B7280") },

                    // Surface (다크 톤 예시)
                    { EColorKeys.SurfacePrimary, B("#1E2736") },
                    { EColorKeys.SurfaceSecondary, B("#2D3847") },
                    { EColorKeys.SurfaceTertiary, B("#4E5969") },
                    { EColorKeys.SurfaceQuaternary, B("#34495E") },

                    // Border
                    { EColorKeys.BorderPrimary, B("#999999") },
                    { EColorKeys.BorderSecondary, B("#7A8698") },
                    { EColorKeys.BorderTertiary, B("#5B6B7D") },
                    { EColorKeys.BorderQuaternary, B("#3E4C59") },

                    // Icon (텍스트 톤과 동일 시작)
                    { EColorKeys.IconPrimary, B("#EAEAEA") },
                    { EColorKeys.IconSecondary, B("#BBBBBB") },
                    { EColorKeys.IconTertiary, B("#9CA3AF") },
                    { EColorKeys.IconQuaternary, B("#6B7280") },

                    // Layer (초기에는 Surface 단계와 동일값으로 시작)
                    { EColorKeys.LayerBase, B("#1E2736") },
                    { EColorKeys.Layer1, B("#2D3847") },
                    { EColorKeys.Layer2, B("#4E5969") },
                    { EColorKeys.Layer3, B("#34495E") },
                };
            }
        }
    }
}