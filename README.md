# HySoft.Bundle

WPF 데스크톱 앱용 .NET SDK. MIT. 저자: Hwanyong.lee

## 설치

```
dotnet add package HySoft.Bundle
```

## 포함 어셈블리

| Assembly | Description |
|----------|-------------|
| **Presentation.dll** | MVVM infrastructure, converters, DragDrop, Modal, attached behaviors |
| **Presentation.Styles.dll** | 33 custom WPF controls, color/font/icon design tokens, themes |
| **Communication.dll** | Async TCP client/server |
| **Communication.FileTransfer.dll** | File transfer protocol (chunked, CRC32, resume, progress) |
| **Data.dll** | MSSQL DbContext base class (Entity Framework 6) |
| **Docs.exe** | Interactive documentation viewer |
| **TestApp.exe** | Sample app with Modal, Icon, FileTransfer demos |

## 기술 스택

- C# (latest, Nullable enable) / .NET Framework 4.8, .NET 8.0 Windows
- WPF / EF 6.5.1 / Microsoft.Xaml.Behaviors.Wpf 1.1.135

## 사용

```xml
<!-- XAML 네임스페이스 -->
xmlns:hy="http://schemas.hysoft.com/wpf/2025/xaml"

<!-- 리소스 딕셔너리 병합 -->
<ResourceDictionary
    Source="/Presentation.Styles;component/Themes/Generic.xaml" />
```

## 링크

- Repository: https://github.com/dlghksdyd/HYSoft
