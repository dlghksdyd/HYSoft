# Presentation

WPF MVVM 인프라 및 UI 유틸리티 라이브러리.

- **네임스페이스**: `HYSoft.Presentation`
- **의존성**: Microsoft.Xaml.Behaviors.Wpf 1.1.135

## 핵심 클래스

| 카테고리 | 클래스 |
|----------|--------|
| MVVM | NotifyPropertyChangedBase, RelayCommand, RelayCommand\<T\> |
| 이벤트-커맨드 | EventToCommand, Event, EventPayload, EventCollection |
| 컨버터 | BoolToVisibility(+Reverse/Multi), CornerRadiusToClip(+Expand) 등 8개 |
| 모달 | ModalManager, ModalBaseViewModel, ModalInfo, ModalResult |
| 드래그앤드롭 | DragDrop, IDragDataContext, GhostAdorner, LineAdorner |
| 동작 | WindowDragBehavior, FocusOnVisibleBehavior, ElementReferenceBinding |
| 유틸리티 | BindingManager, TreeSearchHelper, Arguments(1~4) |

## 설계 패턴

- **MVVM**: SetProperty<T>로 변경 감지, RelayCommand로 커맨드 바인딩
- **EventToCommand**: WeakReference 메모리 관리, Freezable 패턴, 바인딩 재배치
- **ModalManager**: DispatcherFrame 동기 블로킹, DataTemplate 동적 생성
- **WindowDragBehavior**: WndProc 후킹, P/Invoke 모니터 제약

## 파일 구조

```
Interactivity/                MVVM 기반, Arguments, Behaviors
Interactivity/CommandBehaviors/ RelayCommand, EventToCommand, Event
Converters/                   8개 값 변환기
Bindings/                     BindingManager
Modal/                        ModalManager, ModalBaseView
DragDrop/                     DragDrop, DragDataContext
Adorners/                     GhostAdorner, LineAdorner
ElementTreeHelper/            TreeSearchHelper
```

## 상세 문서

[Presentation docs](../Docs/Documents/01-Presentation/README.md)
