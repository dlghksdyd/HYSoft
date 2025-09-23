using HYSoft.Presentation.Styles.Icons;
using HYSoft.Presentation.Styles.Lib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyIcon : Control
    {
        private Image? _image;

        // 상태별 캐시
        private ImageSource? _baseSource;
        private ImageSource? _tintedNormal;
        private ImageSource? _tintedHover;
        private ImageSource? _tintedPressed;

        static HyIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyIcon), new FrameworkPropertyMetadata(typeof(HyIcon)));

            SourceProperty = DependencyProperty.Register(
                nameof(Source),
                typeof(EIconKeys?),
                typeof(HyIcon),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnAnyChanged));

            ColorProperty = DependencyProperty.Register(
                nameof(Color),
                typeof(Brush),
                typeof(HyIcon),
                new PropertyMetadata(Brushes.Black, OnAnyChanged));

            ColorHoverProperty = DependencyProperty.Register(
                nameof(ColorHover),
                typeof(Brush),
                typeof(HyIcon),
                new PropertyMetadata(OnAnyChanged));

            ColorPressedProperty = DependencyProperty.Register(
                nameof(ColorPressed),
                typeof(Brush),
                typeof(HyIcon),
                new PropertyMetadata(OnAnyChanged));

            IsPressedProperty = DependencyProperty.Register(
                nameof(IsPressed),
                typeof(bool),
                typeof(HyIcon),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAnyChanged));

            CornerRadiusProperty = DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyIcon),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public static readonly DependencyProperty SourceProperty;
        public EIconKeys? Source
        {
            get => (EIconKeys?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty ColorProperty;
        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty ColorHoverProperty;
        public Brush? ColorHover
        {
            get => (Brush)GetValue(ColorHoverProperty);
            set => SetValue(ColorHoverProperty, value);
        }

        public static readonly DependencyProperty ColorPressedProperty;
        public Brush? ColorPressed
        {
            get => (Brush)GetValue(ColorPressedProperty);
            set => SetValue(ColorPressedProperty, value);
        }

        public static readonly DependencyProperty IsPressedProperty;
        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            set => SetValue(IsPressedProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty;
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public HyIcon()
        {
            
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UnsubscribeStateChanges();

            _image = GetTemplateChild("PART_Image") as System.Windows.Controls.Image;

            RebuildBaseAndTints(); // 소스/색을 반영해 틴트 생성
            ApplyCurrentStateSource(); // 현재 상태 반영

            SubscribeStateChanges();
        }
        
        private static void OnAnyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (HyIcon)d;

            if (e.Property == SourceProperty)
            {
                c.RebuildBaseAndTints();
                c.ApplyCurrentStateSource();
                return;
            }

            if (e.Property == ColorProperty || e.Property == ColorHoverProperty || e.Property == ColorPressedProperty)
            {
                c.RebuildTintsOnly();
                c.ApplyCurrentStateSource();
                return;
            }

            if (e.Property == IsPressedProperty)
            {
                c.ApplyCurrentStateSource();
                return;
            }
        }

        private void RebuildBaseAndTints()
        {
            // 원본 로드
            try
            {
                if (Source is not null)
                {
                    EIconKeys key = Source ?? EIconKeys.Add;
                    _baseSource = IconGenerator.GetIcon(key);
                }
                else
                {
                    _baseSource = null;
                }
            }
            catch
            {
                _baseSource = null;
            }

            RebuildTintsOnly();
        }

        private void RebuildTintsOnly()
        {
            if (_baseSource == null)
            {
                _tintedNormal = _tintedHover = _tintedPressed = null;
                return;
            }

            if (TryGetColor(Color, out var c0))
                _tintedNormal = IconTintHelper.TintImage(_baseSource, c0);
            else
                _tintedNormal = _baseSource;

            if (ColorHover != null && TryGetColor(ColorHover, out var c1))
                _tintedHover = IconTintHelper.TintImage(_baseSource, c1);
            else
                _tintedHover = _tintedNormal;

            if (ColorPressed != null && TryGetColor(ColorPressed, out var c2))
                _tintedPressed = IconTintHelper.TintImage(_baseSource, c2);
            else
                _tintedPressed = _tintedHover ?? _tintedNormal;
        }

        private static bool TryGetColor(Brush brush, out Color color)
        {
            if (brush is SolidColorBrush scb)
            {
                color = scb.Color;
                return true;
            }
            color = default;
            return false;
        }

        private void ApplyCurrentStateSource()
        {
            if (_image == null) return;

            var src = IsPressed ? _tintedPressed
                    : IsMouseOver ? _tintedHover
                    : _tintedNormal;

            _image.Source = src;

            // 품질 옵션
            _image.SnapsToDevicePixels = true;
            _image.UseLayoutRounding = true;
            _image.Stretch = Stretch.Uniform;
        }

        #region 상태 변화 구독
        private DependencyPropertyDescriptor? _sourceDesc;
        private DependencyPropertyDescriptor? _isMouseOverDesc;
        private DependencyPropertyDescriptor? _isPressedDesc;

        private void SubscribeStateChanges()
        {
            _sourceDesc = DependencyPropertyDescriptor.FromProperty(SourceProperty, typeof(HyIcon));
            _sourceDesc?.AddValueChanged(this, (s, e) =>
            {
                RebuildBaseAndTints();
                ApplyCurrentStateSource();
            });

            _isMouseOverDesc = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(HyIcon));
            _isMouseOverDesc?.AddValueChanged(this, OnStateChanged);

            _isPressedDesc = DependencyPropertyDescriptor.FromProperty(HyIcon.IsPressedProperty, typeof(HyIcon));
            _isPressedDesc?.AddValueChanged(this, OnStateChanged);
        }

        private void UnsubscribeStateChanges()
        {
            if (_sourceDesc != null)
            {
                _sourceDesc.RemoveValueChanged(this, (s, e) => { });
                _sourceDesc = null;
            }

            if (_isMouseOverDesc != null)
            {
                _isMouseOverDesc.RemoveValueChanged(this, OnStateChanged);
                _isMouseOverDesc = null;
            }
            if (_isPressedDesc != null)
            {
                _isPressedDesc.RemoveValueChanged(this, OnStateChanged);
                _isPressedDesc = null;
            }
        }

        private void OnStateChanged(object? sender, EventArgs e) => ApplyCurrentStateSource();
        #endregion

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!IsEnabled) return;

            CaptureMouse();
            SetCurrentValue(IsPressedProperty, true);
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!IsEnabled) return;

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            SetCurrentValue(IsPressedProperty, false);
        }
    }
}
