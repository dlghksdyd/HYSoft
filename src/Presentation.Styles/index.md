# Presentation.Styles

디자인 시스템 및 34개 커스텀 WPF 컨트롤 라이브러리.

- **네임스페이스**: `HYSoft.Presentation.Styles`
- **의존성**: Presentation (프로젝트), PresentationFramework.Aero2
- **테마**: `Themes/Generic.xaml`

## 디자인 토큰

- **색상**: 118개 시맨틱 브러시 (ColorSemantics.xaml)
  - Text, Icon, Surface, Border, Button(4종), Input, Feedback(4종), Badge(6종), Table, Nav
- **폰트**: 6단계 (Xs=12, Sm=14, Md=16, Lg=20, Xl=24, 2Xl=32)
- **아이콘**: 87개 머티리얼 디자인 PNG (런타임 틴팅)

## 컨트롤 (34개)

**기본**: HyButton, HyCheckBox, HyRadioButton, HyToggleSwitch, HyIcon, HyTemplateControl
**입력**: HyTextBox, HyPasswordBox, HyRichTextBox, HyComboBox, HyNumericUpDown, HyDatePicker, HySlider
**데이터**: HyDataGrid, HyTreeView, HyListBox, HyPaginator
**피드백**: HyBadge(6변형), HyToast, HyProgressBar, HyProgressRing, HyToolTip
**레이아웃**: HyWindow, HyDialog, HyTitleBar, HyTabControl, HyExpander, HyScrollViewer, HyGridSplitter
**네비게이션**: HyMenu, HyContextMenu, HyToolBar, HyStatusBar, HyBreadcrumb, HyTextBlock

## 네이밍 규칙

- 컨트롤: `Hy` 접두사 + PascalCase
- 색상 토큰: 카테고리 + 용도 (TextPrimary, ButtonPrimaryBg, BorderFocus)
- Primary 색상: #0672cb, 모서리: 4px(버튼), 8px(다이얼로그)

## 파일 구조

```
Themes/Generic.xaml           마스터 리소스 딕셔너리
ColorTokens/ColorSemantics.xaml
FontSizeTokens/FontSizeSemantics.xaml
Controls/Hy*.xaml             34개 컨트롤 스타일
Icons/png/                    87개 아이콘
```

## 상세 문서

[PresentationStyles docs](../Docs/Documents/02-PresentationStyles/README.md)
