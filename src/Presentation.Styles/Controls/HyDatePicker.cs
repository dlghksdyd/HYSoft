using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System;
using System.Globalization;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyDatePicker : DatePicker
    {
        static HyDatePicker()
        {
            // Enable default style lookup from Themes/Generic.xaml for HyDatePicker
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(typeof(HyDatePicker)));
        }
        
        // Watermark content to show when no date is selected
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
                nameof(Watermark),
                typeof(object),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public object? Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        // Controls whether the inner textbox can be edited
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(
                nameof(IsEditable),
                typeof(bool),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }
        
        // Foreground brush for watermark content
        public static readonly DependencyProperty WatermarkForegroundProperty =
            DependencyProperty.Register(
                nameof(WatermarkForeground),
                typeof(Brush),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush? WatermarkForeground
        {
            get => (Brush?)GetValue(WatermarkForegroundProperty);
            set => SetValue(WatermarkForegroundProperty, value);
        }

        // Size (Width/Height) of the calendar toggle button
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register(
                nameof(ButtonSize),
                typeof(double),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(24d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double ButtonSize
        {
            get => (double)GetValue(ButtonSizeProperty);
            set => SetValue(ButtonSizeProperty, value);
        }

        // Scale factor applied to the popup calendar
        public static readonly DependencyProperty CalendarScaleProperty =
            DependencyProperty.Register(
                nameof(CalendarScale),
                typeof(double),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double CalendarScale
        {
            get => (double)GetValue(CalendarScaleProperty);
            set => SetValue(CalendarScaleProperty, value);
        }
        
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty PopupCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(PopupCornerRadius),
                typeof(CornerRadius),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius PopupCornerRadius
        {
            get => (CornerRadius)GetValue(PopupCornerRadiusProperty);
            set => SetValue(PopupCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty PopupBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(PopupBorderThickness),
                typeof(Thickness),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(new Thickness(1), FrameworkPropertyMetadataOptions.AffectsRender));

        public Thickness PopupBorderThickness
        {
            get => (Thickness)GetValue(PopupBorderThicknessProperty);
            set => SetValue(PopupBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty PopupHorizontalOffsetProperty =
            DependencyProperty.Register(
                nameof(PopupHorizontalOffset),
                typeof(double),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double PopupHorizontalOffset
        {
            get => (double)GetValue(PopupHorizontalOffsetProperty);
            set => SetValue(PopupHorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty PopupVerticalOffsetProperty =
            DependencyProperty.Register(
                nameof(PopupVerticalOffset),
                typeof(double),
                typeof(HyDatePicker),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double PopupVerticalOffset
        {
            get => (double)GetValue(PopupVerticalOffsetProperty);
            set => SetValue(PopupVerticalOffsetProperty, value);
        }
    }

    // Minimal custom Calendar for styling/aliasing usage in XAML
    public class HyCalendar : System.Windows.Controls.Calendar
    {
        static HyCalendar()
        {
            // Enable default style lookup from Themes/Generic.xaml for HyCalendar
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyCalendar),
                new FrameworkPropertyMetadata(typeof(HyCalendar)));
        }
    }

    // Minimal custom DatePickerTextBox-like control for aliasing usage in XAML
    public class HyDatePickerTextBox : TextBox
    {
        static HyDatePickerTextBox()
        {
            // Enable default style lookup from Themes/Generic.xaml for HyDatePickerTextBox
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyDatePickerTextBox),
                new FrameworkPropertyMetadata(typeof(HyDatePickerTextBox)));
        }

        // Watermark content for the textbox variant
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
                nameof(Watermark),
                typeof(object),
                typeof(HyDatePickerTextBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public object? Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        // Foreground brush for textbox watermark content
        public static readonly DependencyProperty WatermarkForegroundProperty =
            DependencyProperty.Register(
                nameof(WatermarkForeground),
                typeof(Brush),
                typeof(HyDatePickerTextBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush? WatermarkForeground
        {
            get => (Brush?)GetValue(WatermarkForegroundProperty);
            set => SetValue(WatermarkForegroundProperty, value);
        }

        // Controls whether this textbox is editable. When false, TextBox becomes read-only
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(
                nameof(IsEditable),
                typeof(bool),
                typeof(HyDatePickerTextBox),
                new FrameworkPropertyMetadata(true, OnIsEditableChanged));

        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyDatePickerTextBox tb && e.NewValue is bool b)
            {
                tb.IsReadOnly = !b;
            }
        }

        // SelectedDate DP for binding date value with the parent DatePicker
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(
                nameof(SelectedDate),
                typeof(DateTime?),
                typeof(HyDatePickerTextBox),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedDateChanged));

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not HyDatePickerTextBox tb) return;

            tb._isUpdatingText = true;
            try
            {
                if (e.NewValue is DateTime dt)
                {
                    // Use current culture short date pattern
                    tb.Text = dt.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
                }
                else
                {
                    tb.Text = string.Empty;
                }
            }
            finally
            {
                tb._isUpdatingText = false;
            }
        }

        private bool _isUpdatingText;

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (_isUpdatingText)
                return;

            if (!IsEditable)
                return;

            if (string.IsNullOrWhiteSpace(Text))
            {
                SetCurrentValue(SelectedDateProperty, null);
                return;
            }

            if (DateTime.TryParse(Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed))
            {
                SetCurrentValue(SelectedDateProperty, parsed);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // Ensure IsReadOnly reflects IsEditable on template application
            IsReadOnly = !IsEditable;
        }
    }
}
