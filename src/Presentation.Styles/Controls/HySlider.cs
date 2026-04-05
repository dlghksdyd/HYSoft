using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HySlider : Slider
    {
        static HySlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HySlider), new FrameworkPropertyMetadata(typeof(HySlider)));
        }

        public double TrackHeight
        {
            get => (double)GetValue(TrackHeightProperty);
            set => SetValue(TrackHeightProperty, value);
        }
        public static readonly DependencyProperty TrackHeightProperty =
            DependencyProperty.Register(nameof(TrackHeight), typeof(double), typeof(HySlider),
                new PropertyMetadata(4.0));

        public double ThumbSize
        {
            get => (double)GetValue(ThumbSizeProperty);
            set => SetValue(ThumbSizeProperty, value);
        }
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(nameof(ThumbSize), typeof(double), typeof(HySlider),
                new PropertyMetadata(16.0));

        public CornerRadius TrackCornerRadius
        {
            get => (CornerRadius)GetValue(TrackCornerRadiusProperty);
            set => SetValue(TrackCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackCornerRadiusProperty =
            DependencyProperty.Register(nameof(TrackCornerRadius), typeof(CornerRadius), typeof(HySlider),
                new PropertyMetadata(new CornerRadius(2)));
    }
}
