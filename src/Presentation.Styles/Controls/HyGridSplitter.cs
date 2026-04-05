using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyGridSplitter : GridSplitter
    {
        static HyGridSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyGridSplitter), new FrameworkPropertyMetadata(typeof(HyGridSplitter)));
        }

        public Brush SplitterColor
        {
            get => (Brush)GetValue(SplitterColorProperty);
            set => SetValue(SplitterColorProperty, value);
        }
        public static readonly DependencyProperty SplitterColorProperty =
            DependencyProperty.Register(nameof(SplitterColor), typeof(Brush), typeof(HyGridSplitter),
                new PropertyMetadata(null));

        public double SplitterThickness
        {
            get => (double)GetValue(SplitterThicknessProperty);
            set => SetValue(SplitterThicknessProperty, value);
        }
        public static readonly DependencyProperty SplitterThicknessProperty =
            DependencyProperty.Register(nameof(SplitterThickness), typeof(double), typeof(HyGridSplitter),
                new PropertyMetadata(4.0));
    }
}
