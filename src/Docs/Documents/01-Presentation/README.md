# HYSoft.Presentation Library

WPF MVVM 인프라 및 UI 유틸리티. 네임스페이스: `HYSoft.Presentation`

의존성: Microsoft.Xaml.Behaviors.Wpf 1.1.135

## 구성 요소

| 카테고리 | 문서 | 주요 클래스 |
|----------|------|-------------|
| MVVM | [01-MVVM/](01-MVVM/README.md) | NotifyPropertyChangedBase, RelayCommand |
| 이벤트-커맨드 | [02-EventToCommand/](02-EventToCommand/README.md) | EventToCommand, Event |
| 값 변환기 | [03-Converters.md](03-Converters.md) | BoolToVisibility 등 8개 |
| 모달 | [04-Modal/](04-Modal/README.md) | ModalManager |
| 드래그앤드롭 | [05-DragDrop/](05-DragDrop/README.md) | DragDrop, Adorner |
| 동작/유틸 | [06-Behaviors/](06-Behaviors/README.md) | WindowDragBehavior, TreeSearchHelper |

## 아키텍처 특징

- **WeakReference** 기반 메모리 관리 (EventToCommand 순환 참조 방지)
- **Freezable 패턴** - Event 클래스가 XAML 컴파일 + 런타임 바인딩 동시 지원
- **DispatcherFrame** 기반 동기식 모달 블로킹
- **DataTemplate 동적 생성** - FrameworkElementFactory로 뷰-뷰모델 매핑
- **네이티브 윈도우 통합** - WndProc 후킹으로 모니터 제약 처리

## 주요 파일 위치

| 컴포넌트 | 경로 |
|----------|------|
| MVVM 기반 | Interactivity/ |
| 커맨드 바인딩 | Interactivity/CommandBehaviors/ |
| 컨버터 | Converters/ |
| 모달 | Modal/ |
| 드래그앤드롭 | DragDrop/, Adorners/ |
| 트리 탐색 | ElementTreeHelper/ |
| 바인딩 관리 | Bindings/ |
