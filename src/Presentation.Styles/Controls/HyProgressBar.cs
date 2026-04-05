using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyProgressBar : ProgressBar
    {
        static HyProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyProgressBar), new FrameworkPropertyMetadata(typeof(HyProgressBar)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyProgressBar),
                new PropertyMetadata(new CornerRadius(4)));

        public Brush TrackBackground
        {
            get => (Brush)GetValue(TrackBackgroundProperty);
            set => SetValue(TrackBackgroundProperty, value);
        }
        public static readonly DependencyProperty TrackBackgroundProperty =
            DependencyProperty.Register(nameof(TrackBackground), typeof(Brush), typeof(HyProgressBar),
                new PropertyMetadata(null));
    }

    public class HyProgressRing : Control
    {
        static HyProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyProgressRing), new FrameworkPropertyMetadata(typeof(HyProgressRing)));
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(HyProgressRing),
                new PropertyMetadata(true));

        public double RingSize
        {
            get => (double)GetValue(RingSizeProperty);
            set => SetValue(RingSizeProperty, value);
        }
        public static readonly DependencyProperty RingSizeProperty =
            DependencyProperty.Register(nameof(RingSize), typeof(double), typeof(HyProgressRing),
                new PropertyMetadata(32.0));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(HyProgressRing),
                new PropertyMetadata(3.0));

        public Brush RingColor
        {
            get => (Brush)GetValue(RingColorProperty);
            set => SetValue(RingColorProperty, value);
        }
        public static readonly DependencyProperty RingColorProperty =
            DependencyProperty.Register(nameof(RingColor), typeof(Brush), typeof(HyProgressRing),
                new PropertyMetadata(null));
    }
}
