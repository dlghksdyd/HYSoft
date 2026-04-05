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
