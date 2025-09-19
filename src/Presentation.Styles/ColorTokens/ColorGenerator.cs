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

            map[EColorKeys.TextPrimary] = Resolve(nameof(EColorKeys.TextPrimary));
            map[EColorKeys.TextSecondary] = Resolve(nameof(EColorKeys.TextSecondary));
            map[EColorKeys.TextTertiary] = Resolve(nameof(EColorKeys.TextTertiary));
            map[EColorKeys.TextDisabled] = Resolve(nameof(EColorKeys.TextDisabled));
            map[EColorKeys.TextInverse] = Resolve(nameof(EColorKeys.TextInverse));
            map[EColorKeys.TextLink] = Resolve(nameof(EColorKeys.TextLink));
            map[EColorKeys.TextError] = Resolve(nameof(EColorKeys.TextError));
            map[EColorKeys.TextSuccess] = Resolve(nameof(EColorKeys.TextSuccess));
            map[EColorKeys.TextWarning] = Resolve(nameof(EColorKeys.TextWarning));
            map[EColorKeys.TextInfo] = Resolve(nameof(EColorKeys.TextInfo));
            map[EColorKeys.TextWatermark] = Resolve(nameof(EColorKeys.TextWatermark));
            map[EColorKeys.IconDefault] = Resolve(nameof(EColorKeys.IconDefault));
            map[EColorKeys.IconInverse] = Resolve(nameof(EColorKeys.IconInverse));
            map[EColorKeys.IconDisabled] = Resolve(nameof(EColorKeys.IconDisabled));
            map[EColorKeys.IconSuccess] = Resolve(nameof(EColorKeys.IconSuccess));
            map[EColorKeys.IconWarning] = Resolve(nameof(EColorKeys.IconWarning));
            map[EColorKeys.IconError] = Resolve(nameof(EColorKeys.IconError));
            map[EColorKeys.IconInfo] = Resolve(nameof(EColorKeys.IconInfo));
            map[EColorKeys.SurfaceBase] = Resolve(nameof(EColorKeys.SurfaceBase));
            map[EColorKeys.SurfaceSubtle] = Resolve(nameof(EColorKeys.SurfaceSubtle));
            map[EColorKeys.SurfaceElevated] = Resolve(nameof(EColorKeys.SurfaceElevated));
            map[EColorKeys.SurfaceInverse] = Resolve(nameof(EColorKeys.SurfaceInverse));
            map[EColorKeys.SurfaceOverlay] = Resolve(nameof(EColorKeys.SurfaceOverlay));
            map[EColorKeys.BorderSubtle] = Resolve(nameof(EColorKeys.BorderSubtle));
            map[EColorKeys.BorderDefault] = Resolve(nameof(EColorKeys.BorderDefault));
            map[EColorKeys.BorderStrong] = Resolve(nameof(EColorKeys.BorderStrong));
            map[EColorKeys.BorderDivider] = Resolve(nameof(EColorKeys.BorderDivider));
            map[EColorKeys.BorderFocus] = Resolve(nameof(EColorKeys.BorderFocus));
            map[EColorKeys.BorderError] = Resolve(nameof(EColorKeys.BorderError));
            map[EColorKeys.BorderSuccess] = Resolve(nameof(EColorKeys.BorderSuccess));
            map[EColorKeys.BorderWarning] = Resolve(nameof(EColorKeys.BorderWarning));
            map[EColorKeys.ButtonPrimaryFg] = Resolve(nameof(EColorKeys.ButtonPrimaryFg));
            map[EColorKeys.ButtonPrimaryBg] = Resolve(nameof(EColorKeys.ButtonPrimaryBg));
            map[EColorKeys.ButtonPrimaryBorder] = Resolve(nameof(EColorKeys.ButtonPrimaryBorder));
            map[EColorKeys.ButtonSecondaryFg] = Resolve(nameof(EColorKeys.ButtonSecondaryFg));
            map[EColorKeys.ButtonSecondaryBg] = Resolve(nameof(EColorKeys.ButtonSecondaryBg));
            map[EColorKeys.ButtonSecondaryBorder] = Resolve(nameof(EColorKeys.ButtonSecondaryBorder));
            map[EColorKeys.ButtonTertiaryFg] = Resolve(nameof(EColorKeys.ButtonTertiaryFg));
            map[EColorKeys.ButtonTertiaryBg] = Resolve(nameof(EColorKeys.ButtonTertiaryBg));
            map[EColorKeys.ButtonTertiaryBorder] = Resolve(nameof(EColorKeys.ButtonTertiaryBorder));
            map[EColorKeys.ButtonDestructiveFg] = Resolve(nameof(EColorKeys.ButtonDestructiveFg));
            map[EColorKeys.ButtonDestructiveBg] = Resolve(nameof(EColorKeys.ButtonDestructiveBg));
            map[EColorKeys.ButtonDestructiveBorder] = Resolve(nameof(EColorKeys.ButtonDestructiveBorder));
            map[EColorKeys.InputBg] = Resolve(nameof(EColorKeys.InputBg));
            map[EColorKeys.InputFg] = Resolve(nameof(EColorKeys.InputFg));
            map[EColorKeys.InputPlaceholder] = Resolve(nameof(EColorKeys.InputPlaceholder));
            map[EColorKeys.InputBorder] = Resolve(nameof(EColorKeys.InputBorder));
            map[EColorKeys.InputHelperText] = Resolve(nameof(EColorKeys.InputHelperText));
            map[EColorKeys.InputErrorText] = Resolve(nameof(EColorKeys.InputErrorText));
            map[EColorKeys.FeedbackInfoFg] = Resolve(nameof(EColorKeys.FeedbackInfoFg));
            map[EColorKeys.FeedbackInfoBg] = Resolve(nameof(EColorKeys.FeedbackInfoBg));
            map[EColorKeys.FeedbackInfoBorder] = Resolve(nameof(EColorKeys.FeedbackInfoBorder));
            map[EColorKeys.FeedbackSuccessFg] = Resolve(nameof(EColorKeys.FeedbackSuccessFg));
            map[EColorKeys.FeedbackSuccessBg] = Resolve(nameof(EColorKeys.FeedbackSuccessBg));
            map[EColorKeys.FeedbackSuccessBorder] = Resolve(nameof(EColorKeys.FeedbackSuccessBorder));
            map[EColorKeys.FeedbackWarningFg] = Resolve(nameof(EColorKeys.FeedbackWarningFg));
            map[EColorKeys.FeedbackWarningBg] = Resolve(nameof(EColorKeys.FeedbackWarningBg));
            map[EColorKeys.FeedbackWarningBorder] = Resolve(nameof(EColorKeys.FeedbackWarningBorder));
            map[EColorKeys.FeedbackErrorFg] = Resolve(nameof(EColorKeys.FeedbackErrorFg));
            map[EColorKeys.FeedbackErrorBg] = Resolve(nameof(EColorKeys.FeedbackErrorBg));
            map[EColorKeys.FeedbackErrorBorder] = Resolve(nameof(EColorKeys.FeedbackErrorBorder));
            map[EColorKeys.BadgeNeutralFg] = Resolve(nameof(EColorKeys.BadgeNeutralFg));
            map[EColorKeys.BadgeNeutralBg] = Resolve(nameof(EColorKeys.BadgeNeutralBg));
            map[EColorKeys.BadgeNeutralBorder] = Resolve(nameof(EColorKeys.BadgeNeutralBorder));
            map[EColorKeys.BadgeInfoFg] = Resolve(nameof(EColorKeys.BadgeInfoFg));
            map[EColorKeys.BadgeInfoBg] = Resolve(nameof(EColorKeys.BadgeInfoBg));
            map[EColorKeys.BadgeInfoBorder] = Resolve(nameof(EColorKeys.BadgeInfoBorder));
            map[EColorKeys.BadgeSuccessFg] = Resolve(nameof(EColorKeys.BadgeSuccessFg));
            map[EColorKeys.BadgeSuccessBg] = Resolve(nameof(EColorKeys.BadgeSuccessBg));
            map[EColorKeys.BadgeSuccessBorder] = Resolve(nameof(EColorKeys.BadgeSuccessBorder));
            map[EColorKeys.BadgeWarningFg] = Resolve(nameof(EColorKeys.BadgeWarningFg));
            map[EColorKeys.BadgeWarningBg] = Resolve(nameof(EColorKeys.BadgeWarningBg));
            map[EColorKeys.BadgeWarningBorder] = Resolve(nameof(EColorKeys.BadgeWarningBorder));
            map[EColorKeys.BadgeErrorFg] = Resolve(nameof(EColorKeys.BadgeErrorFg));
            map[EColorKeys.BadgeErrorBg] = Resolve(nameof(EColorKeys.BadgeErrorBg));
            map[EColorKeys.BadgeErrorBorder] = Resolve(nameof(EColorKeys.BadgeErrorBorder));
            map[EColorKeys.BadgeHighlightFg] = Resolve(nameof(EColorKeys.BadgeHighlightFg));
            map[EColorKeys.BadgeHighlightBg] = Resolve(nameof(EColorKeys.BadgeHighlightBg));
            map[EColorKeys.BadgeHighlightBorder] = Resolve(nameof(EColorKeys.BadgeHighlightBorder));
            map[EColorKeys.TableHeaderBg] = Resolve(nameof(EColorKeys.TableHeaderBg));
            map[EColorKeys.TableRowBg] = Resolve(nameof(EColorKeys.TableRowBg));
            map[EColorKeys.TableRowHover] = Resolve(nameof(EColorKeys.TableRowHover));
            map[EColorKeys.TableRowSelected] = Resolve(nameof(EColorKeys.TableRowSelected));
            map[EColorKeys.TableRowActive] = Resolve(nameof(EColorKeys.TableRowActive));
            map[EColorKeys.TableBorder] = Resolve(nameof(EColorKeys.TableBorder));
            map[EColorKeys.TableStripe] = Resolve(nameof(EColorKeys.TableStripe));
            map[EColorKeys.NavigationSidebarBg] = Resolve(nameof(EColorKeys.NavigationSidebarBg));
            map[EColorKeys.NavigationSidebarItemFg] = Resolve(nameof(EColorKeys.NavigationSidebarItemFg));
            map[EColorKeys.NavigationSidebarItemBgActive] = Resolve(nameof(EColorKeys.NavigationSidebarItemBgActive));
            map[EColorKeys.NavigationSidebarItemBgHover] = Resolve(nameof(EColorKeys.NavigationSidebarItemBgHover));
            map[EColorKeys.NavigationSidebarBorder] = Resolve(nameof(EColorKeys.NavigationSidebarBorder));
            map[EColorKeys.NavigationTabFg] = Resolve(nameof(EColorKeys.NavigationTabFg));
            map[EColorKeys.NavigationTabIndicator] = Resolve(nameof(EColorKeys.NavigationTabIndicator));
            map[EColorKeys.NavigationMenuBarBg] = Resolve(nameof(EColorKeys.NavigationMenuBarBg));
            map[EColorKeys.NavigationMenuBarFg] = Resolve(nameof(EColorKeys.NavigationMenuBarFg));
            map[EColorKeys.NavigationMenuBarBorder] = Resolve(nameof(EColorKeys.NavigationMenuBarBorder));
            map[EColorKeys.SelectionBg] = Resolve(nameof(EColorKeys.SelectionBg));
            map[EColorKeys.SelectionBorder] = Resolve(nameof(EColorKeys.SelectionBorder));
            map[EColorKeys.SelectionFg] = Resolve(nameof(EColorKeys.SelectionFg));
            map[EColorKeys.SelectionInverseFg] = Resolve(nameof(EColorKeys.SelectionInverseFg));
            map[EColorKeys.ScrollbarTrack] = Resolve(nameof(EColorKeys.ScrollbarTrack));
            map[EColorKeys.ScrollbarThumb] = Resolve(nameof(EColorKeys.ScrollbarThumb));
            map[EColorKeys.ScrollbarBorder] = Resolve(nameof(EColorKeys.ScrollbarBorder));
            map[EColorKeys.TitleBarBackground] = Resolve(nameof(EColorKeys.TitleBarBackground));
            map[EColorKeys.TitleBarText] = Resolve(nameof(EColorKeys.TitleBarText));
            map[EColorKeys.TitleBarBorder] = Resolve(nameof(EColorKeys.TitleBarBorder));
            map[EColorKeys.PopupTitleSurface] = Resolve(nameof(EColorKeys.PopupTitleSurface));
            map[EColorKeys.PopupTitleText] = Resolve(nameof(EColorKeys.PopupTitleText));
            map[EColorKeys.PopupBorder] = Resolve(nameof(EColorKeys.PopupBorder));
            map[EColorKeys.PopupContentSurface] = Resolve(nameof(EColorKeys.PopupContentSurface));
            map[EColorKeys.PopupContentText] = Resolve(nameof(EColorKeys.PopupContentText));

            return map;
        }
    }

    public static class ColorKeys
    {
    }

    public enum EColorKeys
    {
        TextPrimary,
        TextSecondary,
        TextTertiary,
        TextDisabled,
        TextInverse,
        TextLink,
        TextError,
        TextSuccess,
        TextWarning,
        TextInfo,
        TextWatermark,
        IconDefault,
        IconInverse,
        IconDisabled,
        IconSuccess,
        IconWarning,
        IconError,
        IconInfo,
        SurfaceBase,
        SurfaceSubtle,
        SurfaceElevated,
        SurfaceInverse,
        SurfaceOverlay,
        BorderSubtle,
        BorderDefault,
        BorderStrong,
        BorderDivider,
        BorderFocus,
        BorderError,
        BorderSuccess,
        BorderWarning,
        ButtonPrimaryFg,
        ButtonPrimaryBg,
        ButtonPrimaryBorder,
        ButtonSecondaryFg,
        ButtonSecondaryBg,
        ButtonSecondaryBorder,
        ButtonTertiaryFg,
        ButtonTertiaryBg,
        ButtonTertiaryBorder,
        ButtonDestructiveFg,
        ButtonDestructiveBg,
        ButtonDestructiveBorder,
        InputBg,
        InputFg,
        InputPlaceholder,
        InputBorder,
        InputHelperText,
        InputErrorText,
        FeedbackInfoFg,
        FeedbackInfoBg,
        FeedbackInfoBorder,
        FeedbackSuccessFg,
        FeedbackSuccessBg,
        FeedbackSuccessBorder,
        FeedbackWarningFg,
        FeedbackWarningBg,
        FeedbackWarningBorder,
        FeedbackErrorFg,
        FeedbackErrorBg,
        FeedbackErrorBorder,
        BadgeNeutralFg,
        BadgeNeutralBg,
        BadgeNeutralBorder,
        BadgeInfoFg,
        BadgeInfoBg,
        BadgeInfoBorder,
        BadgeSuccessFg,
        BadgeSuccessBg,
        BadgeSuccessBorder,
        BadgeWarningFg,
        BadgeWarningBg,
        BadgeWarningBorder,
        BadgeErrorFg,
        BadgeErrorBg,
        BadgeErrorBorder,
        BadgeHighlightFg,
        BadgeHighlightBg,
        BadgeHighlightBorder,
        TableHeaderBg,
        TableRowBg,
        TableRowHover,
        TableRowSelected,
        TableRowActive,
        TableBorder,
        TableStripe,
        NavigationSidebarBg,
        NavigationSidebarItemFg,
        NavigationSidebarItemBgActive,
        NavigationSidebarItemBgHover,
        NavigationSidebarBorder,
        NavigationTabFg,
        NavigationTabIndicator,
        NavigationMenuBarBg,
        NavigationMenuBarFg,
        NavigationMenuBarBorder,
        SelectionBg,
        SelectionBorder,
        SelectionFg,
        SelectionInverseFg,
        ScrollbarTrack,
        ScrollbarThumb,
        ScrollbarBorder,
        TitleBarBackground,
        TitleBarText,
        TitleBarBorder,
        PopupTitleSurface,
        PopupTitleText,
        PopupBorder,
        PopupContentSurface,
        PopupContentText
    }
}
