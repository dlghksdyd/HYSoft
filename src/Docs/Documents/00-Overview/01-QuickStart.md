# Quick Start

## MVVM ViewModel

```csharp
public class MyViewModel : NotifyPropertyChangedBase
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    public ICommand SaveCommand => new RelayCommand(() => Save());
}
```

## 모달 다이얼로그

```csharp
ModalManager.Configure("#80D1E8EF");
ModalManager.RegisterView<MyDialogView, MyDialogVM>();
ModalResult result = ModalManager.Open(new MyDialogVM());
```

## TCP 통신

```csharp
var options = new TcpClientOptions(IPAddress.Parse("127.0.0.1"), 9000);
var client = TcpClientManager.Create(options);
await client.ConnectAsync();
await client.SendAsync(data);
```

## 파일 전송

```csharp
var client = new FileTransferClient(tcpOptions);
var progress = new Progress<FileTransferProgress>(p =>
    Console.WriteLine($"{p.Percentage:P1}"));
await client.SendFileAsync("file.bin", progress);
```

## 리소스 딕셔너리 병합

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary
                Source="/Presentation.Styles;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```
