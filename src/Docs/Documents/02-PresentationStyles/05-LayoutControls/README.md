# Layout Controls - Window, Dialog, TitleBar, Tab

## HyWindow

커스텀 윈도우. WindowStyle=None, AllowsTransparency=True.

| 속성 | 기본값 |
|------|--------|
| Background | SurfaceBase (#ffffff) |
| BorderBrush | BorderDefault (#6e6e6e) |
| BorderThickness | 1 |
| CornerRadius | 0 |

```xml
<hy:HyWindow>
    <Grid><hy:HyTitleBar /><ContentPresenter /></Grid>
</hy:HyWindow>
```

## HyDialog

그림자 효과 다이얼로그. SizeToContent=WidthAndHeight, CenterOwner.

| 속성 | 기본값 |
|------|--------|
| Background | SurfaceElevated (#f2f6fa) |
| CornerRadius | 8 |
| MinWidth/MinHeight | 320 / 160 |
| DialogTitle / DialogMessage | 설정 가능 |

DropShadowEffect (BlurRadius 20, Opacity 0.3, Depth 4). Grid 3행: 타이틀|메시지|버튼.

## HyTitleBar

윈도우 타이틀 바. Background=#293b4d, Foreground=#f2f6fa.

- WindowDragBehavior 연동 (드래그 이동)
- 최소화(MinimizeAppCommand) + 닫기(ExitAppCommand) 버튼
- HyIcon (Remove, Close), 버튼 호버: 33% 흰색 오버레이

```xml
<hy:HyTitleBar Content="내 애플리케이션" />
```

## HyTabControl / HyTabItem

| HyTabItem 속성 | 기본값 |
|----------------|--------|
| Padding | 12,8 |
| Background | Transparent |
| Cursor | Hand |

상태: Hover(SidebarItemBgHover), Selected(하단 2px 인디케이터 #0672cb), Disabled(50%)

```xml
<hy:HyTabControl>
    <hy:HyTabItem Header="일반"><!-- content --></hy:HyTabItem>
</hy:HyTabControl>
```
하위: [Navigation Controls](01-NavigationControls.md)
