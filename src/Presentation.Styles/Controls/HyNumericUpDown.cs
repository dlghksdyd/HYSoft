using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HYSoft.Presentation.Styles.Controls
{
    [TemplatePart(Name = PART_IncreaseButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_DecreaseButton, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    public class HyNumericUpDown : Control
    {
        private const string PART_IncreaseButton = "PART_IncreaseButton";
        private const string PART_DecreaseButton = "PART_DecreaseButton";
        private const string PART_TextBox = "PART_TextBox";

        private ButtonBase? _increaseButton;
        private ButtonBase? _decreaseButton;
        private TextBox? _textBox;
        private bool _isUpdatingText;

        static HyNumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyNumericUpDown), new FrameworkPropertyMetadata(typeof(HyNumericUpDown)));
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(HyNumericUpDown),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged, CoerceValue));

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(HyNumericUpDown),
                new PropertyMetadata(0.0, OnRangeChanged));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(HyNumericUpDown),
                new PropertyMetadata(100.0, OnRangeChanged));

        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }
        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register(nameof(Increment), typeof(double), typeof(HyNumericUpDown),
                new PropertyMetadata(1.0));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyNumericUpDown),
                new PropertyMetadata(new CornerRadius(4)));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(HyNumericUpDown),
                new PropertyMetadata(null));

        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(HyNumericUpDown),
                new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyNumericUpDown ctrl)
                ctrl.UpdateTextFromValue();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ValueProperty);
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            if (d is HyNumericUpDown ctrl && baseValue is double v)
            {
                if (v < ctrl.Minimum) return ctrl.Minimum;
                if (v > ctrl.Maximum) return ctrl.Maximum;
            }
            return baseValue;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Detach old handlers
            if (_increaseButton != null)
                _increaseButton.Click -= OnIncreaseClick;
            if (_decreaseButton != null)
                _decreaseButton.Click -= OnDecreaseClick;
            if (_textBox != null)
                _textBox.LostFocus -= OnTextBoxLostFocus;

            _increaseButton = GetTemplateChild(PART_IncreaseButton) as ButtonBase;
            _decreaseButton = GetTemplateChild(PART_DecreaseButton) as ButtonBase;
            _textBox = GetTemplateChild(PART_TextBox) as TextBox;

            if (_increaseButton != null)
                _increaseButton.Click += OnIncreaseClick;
            if (_decreaseButton != null)
                _decreaseButton.Click += OnDecreaseClick;
            if (_textBox != null)
                _textBox.LostFocus += OnTextBoxLostFocus;

            UpdateTextFromValue();
        }

        private void OnIncreaseClick(object sender, RoutedEventArgs e) => Increase();
        private void OnDecreaseClick(object sender, RoutedEventArgs e) => Decrease();

        public void Increase()
        {
            SetCurrentValue(ValueProperty, Math.Min(Value + Increment, Maximum));
        }

        public void Decrease()
        {
            SetCurrentValue(ValueProperty, Math.Max(Value - Increment, Minimum));
        }

        private void UpdateTextFromValue()
        {
            if (_textBox == null || _isUpdatingText) return;

            _isUpdatingText = true;
            try
            {
                _textBox.Text = string.IsNullOrEmpty(StringFormat)
                    ? Value.ToString(CultureInfo.CurrentCulture)
                    : Value.ToString(StringFormat, CultureInfo.CurrentCulture);
            }
            finally
            {
                _isUpdatingText = false;
            }
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingText) return;

            if (_textBox != null && double.TryParse(_textBox.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out double parsed))
            {
                SetCurrentValue(ValueProperty, parsed);
            }
            else
            {
                // Revert to current value on invalid input
                UpdateTextFromValue();
            }
        }
    }
}
