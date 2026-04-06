# MVVM Infrastructure

네임스페이스: `HYSoft.Presentation.Interactivity`

## NotifyPropertyChangedBase

모든 ViewModel의 추상 기본 클래스. `INotifyPropertyChanged` 구현.

```csharp
public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string? propertyName);
    protected void RaisePropertyChanged(string? propertyName);
    // 값 변경 시만 알림. 반환: 변경 여부
    protected bool SetProperty<T>(ref T storage, T value,
        [CallerMemberName] string? propertyName = null);
}
```

```csharp
public class MyViewModel : NotifyPropertyChangedBase
{
    private string _name;
    public string Name { get => _name; set => SetProperty(ref _name, value); }
}
```

## Arguments (Argument1 ~ Argument4)

XAML 바인딩 체인에서 여러 인자를 전달하는 Attached Property 홀더.

```csharp
public static class Argument1
{
    public static readonly DependencyProperty ValueProperty; // object 타입
    public static void SetValue(DependencyObject d, object v);
    public static object GetValue(DependencyObject d);
}
```

- `FrameworkPropertyMetadataOptions.Inherits`로 요소 트리 하위 상속
- EventToCommand의 EventPayload에 파라미터 전달 시 사용

```xml
<ListBoxItem local:Argument1.Value="{Binding ItemData}">
    <local:EventToCommand.Binding>
        <local:Event RoutedEvent="MouseDoubleClick"
            Command="{Binding DataContext.OpenCmd,
                RelativeSource={RelativeSource AncestorType=ListBox}}"
            CommandParameter="{Binding (local:Argument1.Value),
                RelativeSource={RelativeSource Self}}" />
    </local:EventToCommand.Binding>
</ListBoxItem>
```
하위: [RelayCommand](01-RelayCommand.md)
