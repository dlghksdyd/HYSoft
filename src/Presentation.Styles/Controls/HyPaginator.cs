using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HYSoft.Presentation.Styles.Controls
{
    [TemplatePart(Name = PART_FirstButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_PreviousButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_NextButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_LastButton, Type = typeof(ButtonBase))]
    public class HyPaginator : Control
    {
        private const string PART_FirstButton = "PART_FirstButton";
        private const string PART_PreviousButton = "PART_PreviousButton";
        private const string PART_NextButton = "PART_NextButton";
        private const string PART_LastButton = "PART_LastButton";

        private ButtonBase? _firstButton;
        private ButtonBase? _previousButton;
        private ButtonBase? _nextButton;
        private ButtonBase? _lastButton;

        static HyPaginator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyPaginator), new FrameworkPropertyMetadata(typeof(HyPaginator)));
        }

        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(HyPaginator),
                new FrameworkPropertyMetadata(1,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnCurrentPageChanged, CoerceCurrentPage));

        public int TotalPages
        {
            get => (int)GetValue(TotalPagesProperty);
            set => SetValue(TotalPagesProperty, value);
        }
        public static readonly DependencyProperty TotalPagesProperty =
            DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(HyPaginator),
                new PropertyMetadata(1, OnTotalPagesChanged, CoerceTotalPages));

        public int PageSize
        {
            get => (int)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(HyPaginator),
                new PropertyMetadata(20));

        public int TotalItems
        {
            get => (int)GetValue(TotalItemsProperty);
            set => SetValue(TotalItemsProperty, value);
        }
        public static readonly DependencyProperty TotalItemsProperty =
            DependencyProperty.Register(nameof(TotalItems), typeof(int), typeof(HyPaginator),
                new PropertyMetadata(0));

        // ── 레이아웃 메트릭 (work-4002) ──────────────────────────────────────────────
        //
        // 🔴 왜 있는가: 이 값들은 예전에 ControlTemplate 안에 리터럴로 박혀 있었다. 그래서 paginator 를
        //    크게 쓰고 싶은 화면(이력·배터리 목록)은 컨트롤을 통째로 `LayoutTransform ScaleTransform 1.6`
        //    으로 확대하는 수밖에 없었고, 그것은 23-uiux-design §5 가 금지하는 구문이다.
        //    이 DP 들은 그 화면들이 «본체 크기»로 같은 크기를 낼 수 있게 한다.
        //
        // 🔴 근거 없는 인과를 적지 않는다: 원 지적(WM GBTP-302 #67)은 스케일 때문에 «흐릿하다» 였으나,
        //    오프스크린 렌더 대조에서는 전/후가 96~192 DPI 전 구간에서 사실상 같았다(최대 채널차 6~12/255).
        //    WPF 의 LayoutTransform 은 레이아웃을 다시 돌기 때문에 이 템플릿에서는 리샘플 흐림이 없었다.
        //    ⇒ 이 DP 들의 값어치는 «선명도»가 아니라 ⓐ §5 금지 구문 제거 ⓑ 크기가 매직 1.6 이 아니라
        //    값으로 드러나는 것 ⓒ 테두리·라운드가 있는 파생 템플릿에서 스냅 어긋남을 만들지 않는 것이다.
        //
        // 🔴 기본값은 종전 템플릿 리터럴과 «완전히 같다»(24 / 4 / 4,0,0,0 / 10 / 8 / 8,0,8,0).
        //    값을 주지 않는 화면의 렌더는 바뀌지 않는다. 이 불변이 work-4002 의 게이트이며
        //    tests/DiagnosisSystem.Tests/Design/PaginatorMetricsGuardTests.cs 가 기본값을 고정한다.

        /// <summary>내비게이션 버튼 한 변의 크기(px). 기본 24 — 종전 템플릿 리터럴과 같다.</summary>
        public double ButtonSize
        {
            get => (double)GetValue(ButtonSizeProperty);
            set => SetValue(ButtonSizeProperty, value);
        }
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register(nameof(ButtonSize), typeof(double), typeof(HyPaginator),
                new FrameworkPropertyMetadata(24d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>내비게이션 버튼 내부 여백. 기본 4 — 종전 템플릿 리터럴과 같다.</summary>
        public Thickness ButtonPadding
        {
            get => (Thickness)GetValue(ButtonPaddingProperty);
            set => SetValue(ButtonPaddingProperty, value);
        }
        public static readonly DependencyProperty ButtonPaddingProperty =
            DependencyProperty.Register(nameof(ButtonPadding), typeof(Thickness), typeof(HyPaginator),
                new FrameworkPropertyMetadata(new Thickness(4), FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 버튼 쌍(첫·이전 / 다음·끝) «안쪽» 간격. 각 쌍의 두 번째 버튼(이전·끝)의 여백으로 들어간다.
        /// 기본 <c>4,0,0,0</c> — 종전 템플릿 리터럴과 같다.
        /// </summary>
        public Thickness ButtonSpacing
        {
            get => (Thickness)GetValue(ButtonSpacingProperty);
            set => SetValue(ButtonSpacingProperty, value);
        }
        public static readonly DependencyProperty ButtonSpacingProperty =
            DependencyProperty.Register(nameof(ButtonSpacing), typeof(Thickness), typeof(HyPaginator),
                new FrameworkPropertyMetadata(new Thickness(4, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>처음·끝(◀◀ ▶▶ 글리프) 버튼의 글자 크기. 기본 10 — 종전 템플릿 리터럴과 같다.</summary>
        public double FirstLastGlyphFontSize
        {
            get => (double)GetValue(FirstLastGlyphFontSizeProperty);
            set => SetValue(FirstLastGlyphFontSizeProperty, value);
        }
        public static readonly DependencyProperty FirstLastGlyphFontSizeProperty =
            DependencyProperty.Register(nameof(FirstLastGlyphFontSize), typeof(double), typeof(HyPaginator),
                new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 이전·다음 버튼의 글자 크기. 기본 8 — 종전 템플릿 리터럴과 같다.
        /// 🔴 처음·끝과 «다른» 값인 것은 의도다(글리프 폭이 달라 같은 값이면 시각 크기가 어긋난다).
        /// </summary>
        public double PrevNextGlyphFontSize
        {
            get => (double)GetValue(PrevNextGlyphFontSizeProperty);
            set => SetValue(PrevNextGlyphFontSizeProperty, value);
        }
        public static readonly DependencyProperty PrevNextGlyphFontSizeProperty =
            DependencyProperty.Register(nameof(PrevNextGlyphFontSize), typeof(double), typeof(HyPaginator),
                new FrameworkPropertyMetadata(8d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>가운데 「Page x of y」 문구의 좌우 여백. 기본 <c>8,0,8,0</c> — 종전 템플릿 리터럴과 같다.</summary>
        public Thickness PageInfoMargin
        {
            get => (Thickness)GetValue(PageInfoMarginProperty);
            set => SetValue(PageInfoMarginProperty, value);
        }
        public static readonly DependencyProperty PageInfoMarginProperty =
            DependencyProperty.Register(nameof(PageInfoMargin), typeof(Thickness), typeof(HyPaginator),
                new FrameworkPropertyMetadata(new Thickness(8, 0, 8, 0), FrameworkPropertyMetadataOptions.AffectsMeasure));

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyPaginator ctrl)
                ctrl.UpdateButtonStates();
        }

        private static void OnTotalPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(CurrentPageProperty);
            if (d is HyPaginator ctrl)
                ctrl.UpdateButtonStates();
        }

        private static object CoerceCurrentPage(DependencyObject d, object baseValue)
        {
            if (d is HyPaginator ctrl && baseValue is int v)
            {
                int max = Math.Max(1, ctrl.TotalPages);
                if (v < 1) return 1;
                if (v > max) return max;
            }
            return baseValue;
        }

        private static object CoerceTotalPages(DependencyObject d, object baseValue)
        {
            if (baseValue is int v && v < 1) return 1;
            return baseValue;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_firstButton != null) _firstButton.Click -= OnFirstClick;
            if (_previousButton != null) _previousButton.Click -= OnPreviousClick;
            if (_nextButton != null) _nextButton.Click -= OnNextClick;
            if (_lastButton != null) _lastButton.Click -= OnLastClick;

            _firstButton = GetTemplateChild(PART_FirstButton) as ButtonBase;
            _previousButton = GetTemplateChild(PART_PreviousButton) as ButtonBase;
            _nextButton = GetTemplateChild(PART_NextButton) as ButtonBase;
            _lastButton = GetTemplateChild(PART_LastButton) as ButtonBase;

            if (_firstButton != null) _firstButton.Click += OnFirstClick;
            if (_previousButton != null) _previousButton.Click += OnPreviousClick;
            if (_nextButton != null) _nextButton.Click += OnNextClick;
            if (_lastButton != null) _lastButton.Click += OnLastClick;

            UpdateButtonStates();
        }

        private void OnFirstClick(object sender, RoutedEventArgs e) => SetCurrentValue(CurrentPageProperty, 1);
        private void OnPreviousClick(object sender, RoutedEventArgs e) => SetCurrentValue(CurrentPageProperty, Math.Max(1, CurrentPage - 1));
        private void OnNextClick(object sender, RoutedEventArgs e) => SetCurrentValue(CurrentPageProperty, Math.Min(TotalPages, CurrentPage + 1));
        private void OnLastClick(object sender, RoutedEventArgs e) => SetCurrentValue(CurrentPageProperty, TotalPages);

        private void UpdateButtonStates()
        {
            bool atFirst = CurrentPage <= 1;
            bool atLast = CurrentPage >= TotalPages;

            if (_firstButton != null) _firstButton.IsEnabled = !atFirst;
            if (_previousButton != null) _previousButton.IsEnabled = !atFirst;
            if (_nextButton != null) _nextButton.IsEnabled = !atLast;
            if (_lastButton != null) _lastButton.IsEnabled = !atLast;
        }
    }
}
