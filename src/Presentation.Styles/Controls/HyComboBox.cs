using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyComboBox : ComboBox
    {
        static HyComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyComboBox), new FrameworkPropertyMetadata(typeof(HyComboBox)));

            EventManager.RegisterClassHandler(
                typeof(HyComboBox),
                UIElement.LostFocusEvent,
                new RoutedEventHandler(OnLostFocusHandler),
                true);
        }

        private static void OnLostFocusHandler(object sender, RoutedEventArgs e)
        {
            if (sender is HyComboBox cb && cb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
            }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyComboBox),
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
                typeof(HyComboBox),
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
                typeof(HyComboBox),
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
                typeof(HyComboBox),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public double PopupHorizontalOffset
        {
            get => (double)GetValue(PopupHorizontalOffsetProperty);
            set => SetValue(PopupHorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty PopupVerticalOffsetProperty =
            DependencyProperty.Register(
                nameof(PopupVerticalOffset),
                typeof(double),
                typeof(HyComboBox),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public double PopupVerticalOffset
        {
            get => (double)GetValue(PopupVerticalOffsetProperty);
            set => SetValue(PopupVerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty Argument1Property =
            DependencyProperty.Register(
                nameof(Argument1),
                typeof(object),
                typeof(HyComboBox),
                new FrameworkPropertyMetadata(null));

        public object? Argument1
        {
            get => GetValue(Argument1Property);
            set => SetValue(Argument1Property, value);
        }

        public static readonly DependencyProperty Argument2Property =
            DependencyProperty.Register(
                nameof(Argument2),
                typeof(object),
                typeof(HyComboBox),
                new FrameworkPropertyMetadata(null));

        public object? Argument2
        {
            get => GetValue(Argument2Property);
            set => SetValue(Argument2Property, value);
        }

    }

    public class HyToggleButton : ToggleButton
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyToggleButton),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
