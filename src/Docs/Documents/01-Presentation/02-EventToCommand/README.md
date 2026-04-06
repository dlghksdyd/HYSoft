# Event, EventPayload, EventCollection

네임스페이스: `HYSoft.Presentation.Interactivity.CommandBehaviors`

## Event (Freezable)

단일 RoutedEvent-ICommand 매핑 정의. `FreezeCore()` → false (런타임 수정 허용).

### 속성

| 속성 | 타입 | 설명 |
|------|------|------|
| RoutedEvent | RoutedEvent | 바인딩할 이벤트 |
| HandledToo | bool | 처리된 이벤트도 수신 (기본 false) |
| Command | ICommand | 직접 커맨드 (값 기반) |
| CommandParameter | object | 직접 파라미터 (값 기반) |
| CommandBinding | BindingBase | 동적 커맨드 (바인딩 기반) |
| CommandParameterBinding | BindingBase | 동적 파라미터 (바인딩 기반) |

`ReceiveMarkupExtension()`이 `{Binding}` 마크업을 Command → CommandBinding으로 리다이렉트.

## EventPayload

이벤트 발생 시 ICommand.Execute()에 전달되는 읽기 전용 컨텍스트.

```csharp
public class EventPayload
{
    public object Sender { get; }        // 이벤트 발생 객체
    public RoutedEventArgs Args { get; } // 이벤트 인자
    public object Parameter { get; }     // 커맨드 파라미터
}
```

### ViewModel에서 수신

```csharp
public ICommand ClickCommand => new RelayCommand<EventPayload>(p =>
{
    var sender = p.Sender;
    var args = p.Args as MouseButtonEventArgs;
});
```

## EventCollection

`FreezableCollection<Event>` 상속. XAML에서 다중 Event 정의 컨테이너.

하위 문서: [EventToCommand Behavior](01-EventToCommand.md)
