# RelayCommand

네임스페이스: `HYSoft.Presentation.Interactivity.CommandBehaviors`

ICommand 구현체. 3가지 변형이 존재한다.

## RelayCommand (비제네릭)

```csharp
public class RelayCommand : ICommand
{
    public RelayCommand(Action execute, Func<bool>? canExecute = null);
    public bool CanExecute(object? parameter);
    public void Execute(object? parameter);
    public void RaiseCanExecuteChanged(); // CommandManager로 무효화
}
```

## RelayCommand\<T\>

```csharp
public class RelayCommand<T> : ICommand
{
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null);
    // parameter를 T로 캐스트하여 전달
}
```

## RelayCommandWithResult\<T\>

```csharp
public class RelayCommandWithResult<T> : ICommand
{
    public RelayCommandWithResult(Func<T?, bool> execute,
        Func<T?, bool>? canExecute = null);
    public bool Result { get; } // 마지막 실행 결과 저장
}
```

## 사용 예시

```csharp
// 비제네릭
public ICommand SaveCommand => new RelayCommand(
    () => Save(), () => CanSave());

// 제네릭
public ICommand DeleteCommand => new RelayCommand<int>(
    id => Delete(id), id => id > 0);

// 결과 반환
public RelayCommandWithResult<string> ValidateCommand => new(
    input => IsValid(input), _ => true);
```
