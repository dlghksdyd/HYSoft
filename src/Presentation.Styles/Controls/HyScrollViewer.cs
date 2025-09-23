using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyScrollViewer : ScrollViewer
    {
        static HyScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyScrollViewer), new FrameworkPropertyMetadata(typeof(HyScrollViewer)));
        }

        public static readonly DependencyProperty ThumbColorProperty =
            DependencyProperty.Register(
                nameof(ThumbColor),
                typeof(Brush),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush ThumbColor
        {
            get => (Brush)GetValue(ThumbColorProperty);
            set => SetValue(ThumbColorProperty, value);
        }

        public static readonly DependencyProperty RepeatButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(RepeatButtonVisibility),
                typeof(Visibility),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Visibility RepeatButtonVisibility
        {
            get => (Visibility)GetValue(RepeatButtonVisibilityProperty);
            set => SetValue(RepeatButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty ScrollBarBackgroundProperty =
            DependencyProperty.Register(
                nameof(ScrollBarBackground),
                typeof(Brush),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush ScrollBarBackground
        {
            get => (Brush)GetValue(ScrollBarBackgroundProperty);
            set => SetValue(ScrollBarBackgroundProperty, value);
        }

        public static readonly DependencyProperty ScrollBarBorderBrushProperty =
            DependencyProperty.Register(
                nameof(ScrollBarBorderBrush),
                typeof(Brush),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush ScrollBarBorderBrush
        {
            get => (Brush)GetValue(ScrollBarBorderBrushProperty);
            set => SetValue(ScrollBarBorderBrushProperty, value);
        }

        public static readonly DependencyProperty ScrollBarBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(ScrollBarBorderThickness),
                typeof(Thickness),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Thickness ScrollBarBorderThickness
        {
            get => (Thickness)GetValue(ScrollBarBorderThicknessProperty);
            set => SetValue(ScrollBarBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ScrollBarSizeProperty =
            DependencyProperty.Register(
                nameof(ScrollBarSize),
                typeof(double),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 스크롤바 두께 (세로 스크롤바의 Width, 가로 스크롤바의 Height)
        /// </summary>
        public double ScrollBarSize
        {
            get => (double)GetValue(ScrollBarSizeProperty);
            set => SetValue(ScrollBarSizeProperty, value);
        }

        public static readonly DependencyProperty ScrollBarPaddingProperty =
            DependencyProperty.Register(
                nameof(ScrollBarPadding),
                typeof(Thickness),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public Thickness ScrollBarPadding
        {
            get => (Thickness)GetValue(ScrollBarPaddingProperty);
            set => SetValue(ScrollBarPaddingProperty, value);
        }

        public static readonly DependencyProperty ThumbCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ThumbCornerRadius),
                typeof(CornerRadius),
                typeof(HyScrollViewer),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius ThumbCornerRadius
        {
            get => (CornerRadius)GetValue(ThumbCornerRadiusProperty);
            set => SetValue(ThumbCornerRadiusProperty, value);
        }
    }

    public class HyScrollBar : ScrollBar
    {
        static HyScrollBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyScrollBar), new FrameworkPropertyMetadata(typeof(HyScrollBar)));
        }

        public static readonly DependencyProperty ThumbColorProperty =
            DependencyProperty.Register(
                nameof(ThumbColor),
                typeof(Brush),
                typeof(HyScrollBar),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush ThumbColor
        {
            get => (Brush)GetValue(ThumbColorProperty);
            set => SetValue(ThumbColorProperty, value);
        }

        public static readonly DependencyProperty RepeatButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(RepeatButtonVisibility),
                typeof(Visibility),
                typeof(HyScrollBar),
                new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Visibility RepeatButtonVisibility
        {
            get => (Visibility)GetValue(RepeatButtonVisibilityProperty);
            set => SetValue(RepeatButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty ThumbCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ThumbCornerRadius),
                typeof(CornerRadius),
                typeof(HyScrollBar),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius ThumbCornerRadius
        {
            get => (CornerRadius)GetValue(ThumbCornerRadiusProperty);
            set => SetValue(ThumbCornerRadiusProperty, value);
        }
    }

    public class HyThumb : Thumb
    {
        static HyThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyThumb), new FrameworkPropertyMetadata(typeof(HyThumb)));
        }

        public static readonly DependencyProperty ThumbColorProperty =
            DependencyProperty.Register(
                nameof(ThumbColor),
                typeof(Brush),
                typeof(HyThumb),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush ThumbColor
        {
            get => (Brush)GetValue(ThumbColorProperty);
            set => SetValue(ThumbColorProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyThumb),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }

    public class HyRepeatButton : RepeatButton
    {
        static HyRepeatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyRepeatButton), new FrameworkPropertyMetadata(typeof(HyRepeatButton)));
        }
    }
}
