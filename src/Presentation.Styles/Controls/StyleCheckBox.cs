using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class StyleCheckBox : CheckBox
    {
        static StyleCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StyleCheckBox), new FrameworkPropertyMetadata(typeof(StyleCheckBox)));
        }

        public Thickness TextPadding
        {
            get => (Thickness)GetValue(TextPaddingProperty);
            set => SetValue(TextPaddingProperty, value);
        }
        public static readonly DependencyProperty TextPaddingProperty =
            DependencyProperty.Register(nameof(TextPadding), typeof(Thickness), typeof(StyleCheckBox),
                new PropertyMetadata());

        // 체크 박스 박스(사각형) 크기
        public double BoxSize
        {
            get => (double)GetValue(BoxSizeProperty);
            set => SetValue(BoxSizeProperty, value);
        }
        public static readonly DependencyProperty BoxSizeProperty =
            DependencyProperty.Register(nameof(BoxSize), typeof(double), typeof(StyleCheckBox),
                new PropertyMetadata(18.0));

        // 모서리
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(StyleCheckBox),
                new PropertyMetadata(new CornerRadius(4)));

        // 박스 테두리 두께
        public Thickness BoxBorderThickness
        {
            get => (Thickness)GetValue(BoxBorderThicknessProperty);
            set => SetValue(BoxBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty BoxBorderThicknessProperty =
            DependencyProperty.Register(nameof(BoxBorderThickness), typeof(Thickness), typeof(StyleCheckBox),
                new PropertyMetadata(new Thickness(1)));

        // 체크 선 두께
        public double CheckThickness
        {
            get => (double)GetValue(CheckThicknessProperty);
            set => SetValue(CheckThicknessProperty, value);
        }
        public static readonly DependencyProperty CheckThicknessProperty =
            DependencyProperty.Register(nameof(CheckThickness), typeof(double), typeof(StyleCheckBox),
                new PropertyMetadata(2.0));

        // 색상류
        public Brush BoxBackground
        {
            get => (Brush)GetValue(BoxBackgroundProperty);
            set => SetValue(BoxBackgroundProperty, value);
        }
        public static readonly DependencyProperty BoxBackgroundProperty =
            DependencyProperty.Register(nameof(BoxBackground), typeof(Brush), typeof(StyleCheckBox),
                new PropertyMetadata(Brushes.White));

        public Brush BoxBorderBrush
        {
            get => (Brush)GetValue(BoxBorderBrushProperty);
            set => SetValue(BoxBorderBrushProperty, value);
        }
        public static readonly DependencyProperty BoxBorderBrushProperty =
            DependencyProperty.Register(nameof(BoxBorderBrush), typeof(Brush), typeof(StyleCheckBox),
                new PropertyMetadata(Brushes.Gray));

        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register(nameof(HoverBackground), typeof(Brush), typeof(StyleCheckBox),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(20, 0, 0, 0))));

        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }
        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register(nameof(PressedBackground), typeof(Brush), typeof(StyleCheckBox),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))));

        public Brush CheckBrush
        {
            get => (Brush)GetValue(CheckBrushProperty);
            set => SetValue(CheckBrushProperty, value);
        }
        public static readonly DependencyProperty CheckBrushProperty =
            DependencyProperty.Register(nameof(CheckBrush), typeof(Brush), typeof(StyleCheckBox),
                new PropertyMetadata(Brushes.Black));

        // 비활성화 시 컨트롤 전체 투명도
        public double DisabledOpacity
        {
            get => (double)GetValue(DisabledOpacityProperty);
            set => SetValue(DisabledOpacityProperty, value);
        }
        public static readonly DependencyProperty DisabledOpacityProperty =
            DependencyProperty.Register(nameof(DisabledOpacity), typeof(double), typeof(StyleCheckBox),
                new PropertyMetadata(0.4));
    }
}
