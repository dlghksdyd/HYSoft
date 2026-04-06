# Navigation & Utility Controls

## HyExpander

확장 가능 콘텐츠 컨테이너.

```xml
<hy:HyExpander Header="상세 정보" IsExpanded="{Binding ShowDetails}">
    <!-- 콘텐츠 -->
</hy:HyExpander>
```

## HyMenu / HyContextMenu

메뉴 바 및 컨텍스트 메뉴.

```xml
<hy:HyMenu>
    <MenuItem Header="파일">
        <MenuItem Header="열기" Command="{Binding OpenCommand}" />
        <MenuItem Header="저장" Command="{Binding SaveCommand}" />
    </MenuItem>
</hy:HyMenu>
```

## HyBreadcrumb

브레드크럼 네비게이션.

```xml
<hy:HyBreadcrumb ItemsSource="{Binding BreadcrumbItems}" />
```

## HyToolBar / HyStatusBar

```xml
<hy:HyToolBar>
    <hy:HyButton Content="저장" />
</hy:HyToolBar>
<hy:HyStatusBar Content="준비" />
```

## HyScrollViewer

커스텀 스크롤바. ScrollBarSize 속성으로 두께 조절.
ScrollbarTrack/Thumb/Border 토큰 사용.

## HyGridSplitter

그리드 분할선 컨트롤.

## HyTextBlock

텍스트 블록. TextPrimary 기본 색상.
