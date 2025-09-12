using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Colors
{
    /// <summary>
    /// XAML ResourceDictionary(ComponentResourceKey) 기반 + Fallback 매핑을 제공하는 팔레트.
    /// </summary>
    public static class ColorPalette
    {
        // XAML 리소스 키 생성: {ComponentResourceKey TypeInTargetAssembly={x:Type c:ColorKeys}, ResourceId={x:Static c:EColorKeys.*}}
        private static ComponentResourceKey BuildResourceKey(EColorKeys key)
            => new(typeof(ColorKeys), key);

        private static readonly Dictionary<EColorKeys, SolidColorBrush> Fallback = BuildFallback();

        private static Dictionary<EColorKeys, SolidColorBrush> BuildFallback()
        {
            // 헥스 → Brush 변환 + Freeze
            SolidColorBrush B(string hex)
            {
                var c = (Color)ColorConverter.ConvertFromString(hex.Trim());
                var b = new SolidColorBrush(c);
                b.Freeze();
                return b;
            }

            return new Dictionary<EColorKeys, SolidColorBrush>
            {
                // 브랜드
                { EColorKeys.BrandPrimary, B("#0B78BC") },
                { EColorKeys.BrandSecondary, B("#E48900") },

                // 텍스트
                { EColorKeys.TextPrimary, B("#EAEAEA") },
                { EColorKeys.TextSecondary, B("#BBBBBB") },

                // 보더
                { EColorKeys.BorderPrimary, B("#999999") },

                // 상태
                { EColorKeys.StateSuccess, B("#00C763") },
                { EColorKeys.StateInfo, B("#D1E8EF") },
                { EColorKeys.StateError, B("#FF3A3A") },

                // 서페이스(레이어)
                { EColorKeys.SurfacePrimary, B("#1E2736") },
                { EColorKeys.SurfaceSecondary, B("#2D3847") },
                { EColorKeys.SurfaceTertiary, B("#4E5969") },
                { EColorKeys.SurfaceQuaternary, B("#34495E") },
            };
        }

        /// <summary>
        /// XAML 리소스(우선) → 없으면 Fallback에서 Brush 반환.
        /// </summary>
        public static SolidColorBrush GetBrush(EColorKeys key)
        {
            // 1) XAML ResourceDictionary에서 조회
            if (Application.Current != null)
            {
                var resKey = BuildResourceKey(key);
                var found = Application.Current.TryFindResource(resKey) as SolidColorBrush;
                if (found != null)
                    return found; // 리소스 인스턴스는 보통 이미 Freeze됨
            }   

            // 2) Fallback 반환 (CloneFreezable로 새 인스턴스 필요 없으면 그대로)
            if (Fallback.TryGetValue(key, out var fb))
                return fb;

            // 3) 안전장치
            return Brushes.Transparent;
        }

        /// <summary>
        /// Color만 필요한 경우
        /// </summary>
        public static Color GetColor(EColorKeys key) => GetBrush(key).Color;

        /// <summary>
        /// 런타임에서 특정 키의 Brush를 강제로 덮어씌우고 싶을 때 (예: 테마 핫스왑).
        /// </summary>
        public static void Override(EColorKeys key, SolidColorBrush brush)
        {
            if (brush != null && !brush.IsFrozen)
                brush.Freeze();

            Fallback[key] = brush ?? Brushes.Transparent;
        }

        /// <summary>
        /// 다수 키를 한 번에 덮어쓰기 (예: JSON/서버에서 받아온 팔레트).
        /// </summary>
        public static void OverrideMany(IDictionary<EColorKeys, string> hexMap)
        {
            if (hexMap == null) return;

            foreach (var kv in hexMap)
            {
                var hex = kv.Value?.Trim();
                if (string.IsNullOrEmpty(hex)) continue;
                var color = (Color)ColorConverter.ConvertFromString(hex);
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                Fallback[kv.Key] = brush;
            }
        }
    }
}
