using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyButton : Button
    {
        public static readonly DependencyProperty CornerRadiusProperty;

        /// <summary>마우스를 올렸을 때 배경 위에 겹치는 색(기본 = 검정 6%). 짙은 바탕(제목줄)에서는 밝은 색을 준다.</summary>
        public static readonly DependencyProperty HoverOverlayProperty;

        /// <summary>눌렀을 때 배경 위에 겹치는 색(기본 = 검정 12.5%).</summary>
        public static readonly DependencyProperty PressedOverlayProperty;

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public Brush HoverOverlay
        {
            get => (Brush)GetValue(HoverOverlayProperty);
            set => SetValue(HoverOverlayProperty, value);
        }

        public Brush PressedOverlay
        {
            get => (Brush)GetValue(PressedOverlayProperty);
            set => SetValue(PressedOverlayProperty, value);
        }

        static HyButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyButton), new FrameworkPropertyMetadata(typeof(HyButton)));
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HyButton), new FrameworkPropertyMetadata());
            HoverOverlayProperty = DependencyProperty.Register(nameof(HoverOverlay), typeof(Brush), typeof(HyButton),
                new FrameworkPropertyMetadata(Frozen(Color.FromArgb(0x10, 0, 0, 0))));
            PressedOverlayProperty = DependencyProperty.Register(nameof(PressedOverlay), typeof(Brush), typeof(HyButton),
                new FrameworkPropertyMetadata(Frozen(Color.FromArgb(0x20, 0, 0, 0))));
        }

        private static Brush Frozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
