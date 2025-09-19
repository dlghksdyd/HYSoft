using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.FontSizeTokens
{
    internal static class FontSizeGenerator
    {
        private static readonly Uri SemanticsUri =
            new Uri("/Presentation.Styles;component/FontSizeTokens/FontSizeSemantics.xaml", UriKind.Relative);

        // enum 이름과 XAML의 x:Key가 1:1로 동일하다는 전제
        private static readonly string[] KeyNames = Enum.GetNames(typeof(EFontSizeKeys));

        public static Dictionary<EFontSizeKeys, double> Generate()
        {
            var map = new Dictionary<EFontSizeKeys, double>(capacity: KeyNames.Length);
            ResourceDictionary? fallbackDict = null;

            double Resolve(string key)
            {
                // 1) 앱 리소스(병합 포함) 우선
                if (Application.Current is not null)
                {
                    if (Application.Current.TryFindResource(key) is double b1)
                        return b1;
                }

                // 2) 없으면 패키지 XAML을 로드해서 조회
                fallbackDict ??= (ResourceDictionary)Application.LoadComponent(SemanticsUri);
                if (fallbackDict.Contains(key) && fallbackDict[key] is double b2)
                    return b2;

                // 3) 모든 경로에서 못 찾으면 기본 값으로
                return 16.0;
            }

            map[EFontSizeKeys.Xs] = Resolve(nameof(EFontSizeKeys.Xs));
            map[EFontSizeKeys.Sm] = Resolve(nameof(EFontSizeKeys.Sm));
            map[EFontSizeKeys.Md] = Resolve(nameof(EFontSizeKeys.Md));
            map[EFontSizeKeys.Lg] = Resolve(nameof(EFontSizeKeys.Lg));
            map[EFontSizeKeys.Xl] = Resolve(nameof(EFontSizeKeys.Xl));
            map[EFontSizeKeys._2Xl] = Resolve(nameof(EFontSizeKeys._2Xl));

            return map;
        }
    }

    public static class FontSizeKeys
    {
    }

    public enum EFontSizeKeys
    {
        Xs,
        Sm,
        Md,
        Lg,
        Xl,
        _2Xl,
    }
}
