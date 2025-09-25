using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HYSoft.Presentation.Converters
{
    /// <summary>
    /// inputs:
    ///   [0] double width
    ///   [1] double height
    ///   [2] CornerRadius cr (TopLeft만 사용)
    ///   [3] double expand (옵션, 기본 0)  => 사각형을 상하좌우로 expand 만큼 확장
    /// output: RectangleGeometry (r=r=cr.TopLeft)
    /// </summary>
    public class CornerRadiusToClipExpandConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = (values.Length > 0 && values[0] is double d0 && !double.IsNaN(d0)) ? d0 : 0.0;
            double h = (values.Length > 1 && values[1] is double d1 && !double.IsNaN(d1)) ? d1 : 0.0;
            var cr = (values.Length > 2 && values[2] is CornerRadius c) ? c : new CornerRadius(0);
            double ex = (values.Length > 3 && values[3] is double e && !double.IsNaN(e)) ? e : 0.0;

            // 확장: 원점 보정 포함
            double x = -ex, y = -ex, width = Math.Max(0, w + 2 * ex), height = Math.Max(0, h + 2 * ex);
            if (width <= 0 || height <= 0)
                return Geometry.Empty;

            // 각 변 길이
            double horiz = width;
            double vert = height;

            // 반경 가져오기
            double rtl = Math.Max(0, cr.TopLeft + 1);
            double rtr = Math.Max(0, cr.TopRight + 1);
            double rbr = Math.Max(0, cr.BottomRight + 1);
            double rbl = Math.Max(0, cr.BottomLeft + 1);

            // 반경 과대 시 클램프: 좌우 합/상하 합이 변 길이를 넘지 않도록 조정
            // 가로(상/하 변)
            double topSum = rtl + rtr;
            double bottomSum = rbl + rbr;
            if (topSum > horiz)
            {
                double scale = horiz / topSum;
                rtl *= scale; rtr *= scale;
            }
            if (bottomSum > horiz)
            {
                double scale = horiz / bottomSum;
                rbl *= scale; rbr *= scale;
            }

            // 세로(좌/우 변)
            double leftSum = rtl + rbl;
            double rightSum = rtr + rbr;
            if (leftSum > vert)
            {
                double scale = vert / leftSum;
                rtl *= scale; rbl *= scale;
            }
            if (rightSum > vert)
            {
                double scale = vert / rightSum;
                rtr *= scale; rbr *= scale;
            }

            // 직선/호 그리기
            var geo = new StreamGeometry { FillRule = FillRule.Nonzero };
            using (var ctx = geo.Open())
            {
                // 시작: 왼쪽 위 모서리의 아래쪽 끝점 (x + rtl, y)
                ctx.BeginFigure(new Point(x + rtl, y), isFilled: true, isClosed: true);

                // 상변: 좌상 → 우상-반경 앞까지
                ctx.LineTo(new Point(x + width - rtr, y), isStroked: true, isSmoothJoin: false);
                // 우상 모서리: 시계 방향 90도
                if (rtr > 0)
                    ArcTo(ctx, new Point(x + width, y + rtr), rtr);

                // 우변: 우상 → 우하-반경 앞까지
                ctx.LineTo(new Point(x + width, y + height - rbr), isStroked: true, isSmoothJoin: false);
                // 우하 모서리
                if (rbr > 0)
                    ArcTo(ctx, new Point(x + width - rbr, y + height), rbr);

                // 하변: 우하 → 좌하-반경 앞까지
                ctx.LineTo(new Point(x + rbl, y + height), isStroked: true, isSmoothJoin: false);
                // 좌하 모서리
                if (rbl > 0)
                    ArcTo(ctx, new Point(x, y + height - rbl), rbl);

                // 좌변: 좌하 → 좌상-반경 앞까지
                ctx.LineTo(new Point(x, y + rtl), isStroked: true, isSmoothJoin: false);
                // 좌상 모서리
                if (rtl > 0)
                    ArcTo(ctx, new Point(x + rtl, y), rtl);
            }
            geo.Freeze();
            return geo;
        }

        private static void ArcTo(StreamGeometryContext ctx, Point point, double radius)
        {
            // 90도 코너용 원호 (RadiusX=RadiusY=radius, IsLargeArc=false, SweepDirection=Clockwise)
            ctx.ArcTo(point, new Size(radius, radius), 0, false, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}