using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyRadioButton : RadioButton
    {
        public static readonly DependencyProperty ButtonSizeProperty;
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty TextPaddingProperty;

        public static readonly DependencyProperty EllipseBackgroundProperty;
        public static readonly DependencyProperty EllipseForegroundProperty;
        public static readonly DependencyProperty EllipseBorderThicknessProperty;
        public static readonly DependencyProperty EllipseBorderBrushProperty;
        public static readonly DependencyProperty EllipseHoverProperty;

        public double ButtonSize
        {
            get => (double)GetValue(ButtonSizeProperty);
            set => SetValue(ButtonSizeProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public GridLength TextPadding
        {
            get => (GridLength)GetValue(TextPaddingProperty);
            set => SetValue(TextPaddingProperty, value);
        }

        /// <summary>라디오 원 배경(채우기) 브러시</summary>
        public Brush EllipseBackground
        {
            get => (Brush)GetValue(EllipseBackgroundProperty);
            set => SetValue(EllipseBackgroundProperty, value);
        }

        /// <summary>라디오 원 전경(아이콘/체크 점 등) 브러시</summary>
        public Brush EllipseForeground
        {
            get => (Brush)GetValue(EllipseForegroundProperty);
            set => SetValue(EllipseForegroundProperty, value);
        }

        /// <summary>라디오 원 테두리 두께 (StrokeThickness)</summary>
        public double EllipseBorderThickness
        {
            get => (double)GetValue(EllipseBorderThicknessProperty);
            set => SetValue(EllipseBorderThicknessProperty, value);
        }

        /// <summary>라디오 원 테두리 브러시 (Stroke)</summary>
        public Brush EllipseBorderBrush
        {
            get => (Brush)GetValue(EllipseBorderBrushProperty);
            set => SetValue(EllipseBorderBrushProperty, value);
        }

        /// <summary>마우스 Hover 시 적용할 원의 색상</summary>
        public Brush EllipseHover
        {
            get => (Brush)GetValue(EllipseHoverProperty);
            set => SetValue(EllipseHoverProperty, value);
        }

        static HyRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyRadioButton), new FrameworkPropertyMetadata(typeof(HyRadioButton)));

            ButtonSizeProperty = DependencyProperty.Register("ButtonSize", typeof(double), typeof(HyRadioButton), new FrameworkPropertyMetadata());
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HyRadioButton), new FrameworkPropertyMetadata());
            TextPaddingProperty = DependencyProperty.Register("TextPadding", typeof(GridLength), typeof(HyRadioButton), new FrameworkPropertyMetadata());

            EllipseBackgroundProperty = DependencyProperty.Register(
                "EllipseBackground",
                typeof(Brush),
                typeof(HyRadioButton),
                new FrameworkPropertyMetadata());

            EllipseForegroundProperty = DependencyProperty.Register(
                "EllipseForeground",
                typeof(Brush),
                typeof(HyRadioButton),
                new FrameworkPropertyMetadata());

            EllipseBorderThicknessProperty = DependencyProperty.Register(
                "EllipseBorderThickness",
                typeof(double),
                typeof(HyRadioButton),
                new FrameworkPropertyMetadata());

            EllipseBorderBrushProperty = DependencyProperty.Register(
                "EllipseBorderBrush",
                typeof(Brush),
                typeof(HyRadioButton),
                new FrameworkPropertyMetadata());

            EllipseHoverProperty = DependencyProperty.Register(
                "EllipseHover",
                typeof(Brush),
                typeof(HyRadioButton),
                new FrameworkPropertyMetadata());
        }
    }
}
