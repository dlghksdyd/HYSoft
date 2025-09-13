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

        private static Dictionary<EColorKeys, SolidColorBrush> BuildDefaults()
        {
            // Presentation.Styles 어셈블리 경로에 맞게 유지
            const string SemanticsPackUri = "/Presentation.Styles;component/ColorTokens/ColorSemantics.xaml";

            SolidColorBrush FromResource(string key)
            {
                // 1) Application 리소스 트리에서 우선 찾기 (Brush → Color 순서)
                object found = Application.Current?.TryFindResource(key);

                if (found is SolidColorBrush sb1)
                    return new SolidColorBrush(sb1.Color); // 공유 인스턴스 보호용 복제

                if (found is Color c1)
                    return new SolidColorBrush(c1);

                // 2) 아직 App 머지 전(초기화 타이밍)일 수 있으니, 세맨틱 사전을 직접 로드해 시도
                try
                {
                    var dict = new ResourceDictionary
                    {
                        Source = new Uri(SemanticsPackUri, UriKind.RelativeOrAbsolute)
                    };

                    if (dict[key] is SolidColorBrush sb2)
                        return new SolidColorBrush(sb2.Color);

                    if (dict[key] is Color c2)
                        return new SolidColorBrush(c2);
                }
                catch
                {
                    // pack URI 실패/키 없음 등은 무시하고 폴백으로
                }

                // 3) 최종 폴백: 투명(디버깅 원하면 Magenta 등으로 바꿔도 됨)
                return new SolidColorBrush(Colors.Transparent);
            }

            return new Dictionary<EColorKeys, SolidColorBrush>
            {
                { EColorKeys.ButtonPrimaryBorder,        FromResource("ButtonPrimaryBorder") },
                { EColorKeys.ButtonPrimarySurface,       FromResource("ButtonPrimarySurface") },
                { EColorKeys.ButtonPrimaryText,          FromResource("ButtonPrimaryText") },

                { EColorKeys.TablePrimaryBorderContent,  FromResource("TablePrimaryBorderContent") },
                { EColorKeys.TablePrimaryBorderTitle,    FromResource("TablePrimaryBorderTitle") },
                { EColorKeys.TablePrimarySurfaceContent, FromResource("TablePrimarySurfaceContent") },
                { EColorKeys.TablePrimarySurfaceTitle,   FromResource("TablePrimarySurfaceTitle") },
                { EColorKeys.TablePrimaryTextContent,    FromResource("TablePrimaryTextContent") },
                { EColorKeys.TablePrimaryTextTitle,      FromResource("TablePrimaryTextTitle") }, 
            };
        }
    }
}