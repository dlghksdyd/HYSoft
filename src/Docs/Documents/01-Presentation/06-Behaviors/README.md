# Behaviors

네임스페이스: `HYSoft.Presentation.Interactivity`

## WindowDragBehavior

커스텀 윈도우 크롬 + 드래그 이동 + 시스템 메뉴.

**Attached Property**: `WindowDragBehavior.EnableDragMoveProperty : bool`

| 동작 | 설명 |
|------|------|
| 더블클릭 | 최대화/일반 토글 |
| 클릭 드래그 | 윈도우 이동 (DragMove) |
| 우클릭 | 시스템 메뉴 |
| ButtonBase/Thumb | 드래그 스킵 |

- WndProc 후킹 → WM_GETMINMAXINFO 처리 (모니터 작업 영역 제약)
- P/Invoke: MonitorFromWindow, GetMonitorInfo

```xml
<Window local:WindowDragBehavior.EnableDragMove="True" />
```

## FocusOnVisibleBehavior

**Attached Property**: `WhenTrueProperty : bool`

- Loaded + Visible + Enabled 시 포커스
- `Dispatcher.BeginInvoke(DispatcherPriority.Input)` 안전 타이밍
- TextBox: `CaretIndex = Text.Length`
- 첫 성공 후 핸들러 즉시 제거 (누수 방지)

```xml
<TextBox local:FocusOnVisibleBehavior.WhenTrue="{Binding IsEditing}" />
```

## FocusAction (TriggerAction)

Microsoft.Xaml.Behaviors 트리거 액션. `Focusable=true` 설정 후 Focus() 호출.

## ElementReferenceBinding

UIElement 참조를 ViewModel에 주입하는 Attached Behavior.

**Attached Property**: `TargetProperty : object`

- Loaded/DataContextChanged 감시 → BindingExpression으로 소스 해석 → 리플렉션 주입
- 지원 타입: UIElement, FrameworkElement, object, WeakReference\<UIElement\>

```xml
<Canvas local:ElementReferenceBinding.Target="{Binding DragScope}" />
```

하위 문서: [Utilities](01-Utilities.md)
