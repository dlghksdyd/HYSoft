using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.ColorTokens
{
    internal static class ColorGenerator
    {
        private static readonly Uri SemanticsUri =
            new Uri("/Presentation.Styles;component/ColorTokens/ColorSemantics.xaml", UriKind.Relative);

        // enum 이름과 XAML의 x:Key가 1:1로 동일하다는 전제
        private static readonly string[] KeyNames = Enum.GetNames(typeof(EColorKeys));

        public static Dictionary<EColorKeys, Brush> Generate()
        {
            var map = new Dictionary<EColorKeys, Brush>(capacity: KeyNames.Length);
            ResourceDictionary? fallbackDict = null;

            SolidColorBrush Resolve(string key)
            {
                // 1) 앱 리소스(병합 포함) 우선
                if (Application.Current is not null)
                {
                    if (Application.Current.TryFindResource(key) is SolidColorBrush b1)
                        return b1;
                }

                // 2) 없으면 패키지 XAML을 로드해서 조회
                fallbackDict ??= (ResourceDictionary)Application.LoadComponent(SemanticsUri);
                if (fallbackDict.Contains(key) && fallbackDict[key] is SolidColorBrush b2)
                    return b2;

                // 3) 모든 경로에서 못 찾으면 투명
                return new SolidColorBrush(Colors.Transparent);
            }

            map[EColorKeys.ButtonPrimarySurface] = Resolve(nameof(EColorKeys.ButtonPrimarySurface));
            map[EColorKeys.ButtonPrimaryText] = Resolve(nameof(EColorKeys.ButtonPrimaryText));
            map[EColorKeys.ButtonPrimaryBorder] = Resolve(nameof(EColorKeys.ButtonPrimaryBorder));
            map[EColorKeys.TablePrimarySurfaceTitle] = Resolve(nameof(EColorKeys.TablePrimarySurfaceTitle));
            map[EColorKeys.TablePrimarySurfaceContent] = Resolve(nameof(EColorKeys.TablePrimarySurfaceContent));
            map[EColorKeys.TablePrimaryBorderTitle] = Resolve(nameof(EColorKeys.TablePrimaryBorderTitle));
            map[EColorKeys.TablePrimaryBorderContent] = Resolve(nameof(EColorKeys.TablePrimaryBorderContent));
            map[EColorKeys.TablePrimaryTextTitle] = Resolve(nameof(EColorKeys.TablePrimaryTextTitle));
            map[EColorKeys.TablePrimaryTextContent] = Resolve(nameof(EColorKeys.TablePrimaryTextContent));
            map[EColorKeys.TablePrimarySurfaceContentHover] = Resolve(nameof(EColorKeys.TablePrimarySurfaceContentHover));
            map[EColorKeys.TextBlockPrimaryText] = Resolve(nameof(EColorKeys.TextBlockPrimaryText));
            map[EColorKeys.TextBlockPrimaryBorder] = Resolve(nameof(EColorKeys.TextBlockPrimaryBorder));
            map[EColorKeys.ScrollBarPrimaryBar] = Resolve(nameof(EColorKeys.ScrollBarPrimaryBar));
            map[EColorKeys.IconPrimaryFill] = Resolve(nameof(EColorKeys.IconPrimaryFill));
            map[EColorKeys.SectionPrimaryMenu] = Resolve(nameof(EColorKeys.SectionPrimaryMenu));
            map[EColorKeys.SectionPrimaryContent] = Resolve(nameof(EColorKeys.SectionPrimaryContent));

            return map;
        }
    }

    public static class ColorKeys
    {
        
    }

    public enum EColorKeys
    {
        ButtonPrimarySurface,
        ButtonPrimaryText,
        ButtonPrimaryBorder,
        TablePrimarySurfaceTitle,
        TablePrimarySurfaceContent,
        TablePrimaryBorderTitle,
        TablePrimaryBorderContent,
        TablePrimaryTextTitle,
        TablePrimaryTextContent,
        TablePrimarySurfaceContentHover,
        TextBlockPrimaryText,
        TextBlockPrimaryBorder,
        ScrollBarPrimaryBar,
        IconPrimaryFill,
        SectionPrimaryMenu,
        SectionPrimaryContent
    }
}
