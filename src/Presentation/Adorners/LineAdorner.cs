using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HYSoft.Presentation.Adorners
{
    public sealed class LineAdorner : Adorner
    {
        private AdornerLayer? _lineAdornerLayer;

        public enum LinePosition { None, Top, Bottom }

        private LinePosition _pos = LinePosition.None;
        private double _x0, _x1, _y;
        private Brush _brush = Brushes.Transparent;
        private double _thickness;

        public LineAdorner(UIElement adornerRoot) : base(adornerRoot)
        {
            IsHitTestVisible = false;

            Enabled(adornerRoot);
        }

        public void Enabled(UIElement adornerScope)
        {
            Disabled();
            _lineAdornerLayer = AdornerLayer.GetAdornerLayer(adornerScope);
            _lineAdornerLayer?.Add(this);
        }

        public void Disabled()
        {
            _lineAdornerLayer?.Remove(this);
            _lineAdornerLayer = null;
        }

        public void ShowAtElement(FrameworkElement? row, LinePosition pos, Brush brush, double thickness)
        {
            if (row == null) { Hide(); return; }

            var topLeft = row.TranslatePoint(new Point(0, 0), AdornedElement);
            var w = row.ActualWidth;
            var h = row.ActualHeight;

            _x0 = topLeft.X;
            _x1 = topLeft.X + w;
            _y = pos == LinePosition.Top ? topLeft.Y : topLeft.Y + h;

            _pos = pos;
            _brush = brush;
            _thickness = thickness;

            InvalidateVisual(); // 즉시 다시 그리기
        }

        public void Hide()
        {
            _pos = LinePosition.None;

            InvalidateVisual(); // 즉시 다시 그리기
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_pos == LinePosition.None) return;

            var pen = new Pen(_brush, _thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            dc.DrawLine(pen, new Point(_x0, _y), new Point(_x1, _y));
        }
    }
}
