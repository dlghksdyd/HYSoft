# EventToCommand (Attached Behavior)

네임스페이스: `HYSoft.Presentation.Interactivity.CommandBehaviors`

UIElement에 RoutedEvent→ICommand 바인딩을 부착하는 핵심 클래스.

## Attached Properties

```csharp
// 단일 이벤트
EventToCommand.BindingProperty : Event

// 다중 이벤트
EventToCommand.MultiBindingProperty : FreezableCollection<Event>
```

## XAML 사용

```xml
<!-- 단일 -->
<Button>
    <local:EventToCommand.Binding>
        <local:Event RoutedEvent="Button.Click"
                     Command="{Binding SaveCommand}" />
    </local:EventToCommand.Binding>
</Button>

<!-- 다중 -->
<Grid>
    <local:EventToCommand.MultiBinding>
        <local:EventCollection>
            <local:Event RoutedEvent="Mouse.MouseEnter"
                         Command="{Binding EnterCommand}" />
            <local:Event RoutedEvent="Mouse.MouseLeave"
                         Command="{Binding LeaveCommand}" />
        </local:EventCollection>
    </local:EventToCommand.MultiBinding>
</Grid>
```

## 내부 동작

1. Event 인스턴스를 **Clone**하여 바인딩 수명 보존
2. `BindingOperations.SetBinding()`으로 CommandBinding 적용
3. `BindingManager.SetBindingSource()`로 ElementName/RelativeSource 재배치
4. WeakReference 핸들러로 `UIElement.AddHandler()` 등록
5. Loaded/Unloaded에 WeakEventManager로 바인딩 재구성

## 메모리 관리

- `WeakReference<UIElement>`, `WeakReference<Event>`로 순환 참조 방지
- Detach 시 모든 DP 바인딩 + 복제 Event 해제
- `HashSet`으로 실행 중 Event 추적 → 재진입 방지
