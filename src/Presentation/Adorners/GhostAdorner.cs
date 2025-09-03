using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HYSoft.Presentation.Adorners
{
    public sealed class GhostAdorner : Adorner
    {
        private readonly VisualBrush _contentBrush;
        private readonly Size _contentSize;
        private Point _contentOffset;

        public GhostAdorner(UIElement adornerRoot, UIElement visual, double opacity = 0.3) : base(adornerRoot)
        {
            IsHitTestVisible = false;

            _contentSize = new Size(
                (visual as FrameworkElement)?.ActualWidth > 0 ? ((FrameworkElement)visual).ActualWidth : visual.RenderSize.Width,
                (visual as FrameworkElement)?.ActualHeight > 0 ? ((FrameworkElement)visual).ActualHeight : visual.RenderSize.Height);

            _contentBrush = new VisualBrush(visual)
            {
                Opacity = opacity,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };
        }

        public void SetPosition(double x, double y)
        {
            _contentOffset = new Point(x, y);

            InvalidateVisual(); // 즉시 다시 그리기
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(_contentBrush, null, new Rect(_contentOffset, _contentSize));
        }
    }
}
