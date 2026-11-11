using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyMenu : Menu
    {
        static HyMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyMenu), new FrameworkPropertyMetadata(typeof(HyMenu)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyMenu),
                new FrameworkPropertyMetadata(new CornerRadius(0)));
    }

    public class HyContextMenu : ContextMenu
    {
        static HyContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyContextMenu), new FrameworkPropertyMetadata(typeof(HyContextMenu)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyContextMenu),
                new FrameworkPropertyMetadata(new CornerRadius(4)));
    }

    public class HyMenuItem : MenuItem
    {
        static HyMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyMenuItem), new FrameworkPropertyMetadata(typeof(HyMenuItem)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyMenuItem),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        /// <summary>마우스·키보드로 가리킬 때의 배경(기본 = NavigationItemBgHover). 짙은 메뉴줄(제목줄)에서는 TitleBarHover 를 준다.</summary>
        public Brush HighlightBackground
        {
            get => (Brush)GetValue(HighlightBackgroundProperty);
            set => SetValue(HighlightBackgroundProperty, value);
        }
        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.Register(nameof(HighlightBackground), typeof(Brush), typeof(HyMenuItem),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// 메뉴줄 항목이 «지금 화면»인가 — 참이면 아래 밑줄(<see cref="CurrentIndicatorBrush"/>)을 그린다.
        /// 굵기·크기는 바꾸지 않는다(바꾸면 화면을 옮길 때마다 옆 항목이 밀린다).
        /// </summary>
        public bool IsCurrent
        {
            get => (bool)GetValue(IsCurrentProperty);
            set => SetValue(IsCurrentProperty, value);
        }
        public static readonly DependencyProperty IsCurrentProperty =
            DependencyProperty.Register(nameof(IsCurrent), typeof(bool), typeof(HyMenuItem),
                new FrameworkPropertyMetadata(false));

        /// <summary>«지금 화면» 밑줄 색(기본 = NavigationTabIndicator).</summary>
        public Brush CurrentIndicatorBrush
        {
            get => (Brush)GetValue(CurrentIndicatorBrushProperty);
            set => SetValue(CurrentIndicatorBrushProperty, value);
        }
        public static readonly DependencyProperty CurrentIndicatorBrushProperty =
            DependencyProperty.Register(nameof(CurrentIndicatorBrush), typeof(Brush), typeof(HyMenuItem),
                new FrameworkPropertyMetadata(null));
    }
}
