# Feedback Controls

## HyProgressBar

선형 진행 표시줄.

| 속성 | 기본값 |
|------|--------|
| Height | 6 |
| CornerRadius | 4 |
| Foreground | ButtonPrimaryBg |
| TrackBackground | BorderSubtle |

불확정 모드: ThicknessAnimation (Margin -80→300, 1.5초, Forever)

```xml
<hy:HyProgressBar Value="{Binding Progress}" Maximum="100" />
<hy:HyProgressBar IsIndeterminate="True" />
```

## HyProgressRing

원형 진행 표시줄.

| 속성 | 기본값 |
|------|--------|
| RingSize | 32 |
| StrokeThickness | 3 |
| RingColor | ButtonPrimaryBg |
| IsActive | true |

Viewbox + Canvas, Arc Path를 RotateTransform으로 회전.

```xml
<hy:HyProgressRing IsActive="{Binding IsLoading}" RingSize="48" />
```

## HyToast

토스트 알림. DropShadowEffect (BlurRadius 12, Depth 4, Opacity 0.25).

| 속성 | 기본값 |
|------|--------|
| Background | FeedbackInfoBg |
| Foreground | FeedbackInfoFg |
| Padding | 12,8 |
| MinWidth | 280 |

레이아웃: Grid 3컬럼 [아이콘(info) | 메시지 | 닫기(x)]

```xml
<hy:HyToast Content="저장되었습니다." />
```

## HyToolTip

툴팁 표시 컨트롤. 기본 WPF ToolTip에 디자인 토큰 적용.
