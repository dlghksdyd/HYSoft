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
| **Presentation.Styles.dll** | `Hy*` custom WPF controls, color/font/icon design tokens, themes |
| **Communication.dll** | Async TCP client/server (idle timeouts, per-connection disconnect event) |
| **Communication.FileTransfer.dll** | FT10 file transfer (chunked, CRC32, resume via `.ftpart`, progress) |
| **Data.dll** | MSSQL DbContext base class (Entity Framework Core 9) |
| **Docs.exe** | Interactive documentation viewer (UI Support · DB Support · Communication) |
| **TestApp.exe** | Sample app with Modal, Icon, FileTransfer demos |

## 기술 스택

- C# (latest, Nullable enable) / **.NET 8.0 Windows** (`net8.0-windows`, 모든 프로젝트)
- WPF / EF Core 9.0.8 (SqlServer) / Microsoft.Xaml.Behaviors.Wpf 1.1.135

## 사용

```xml
<!-- XAML 네임스페이스 -->
xmlns:hy="http://schemas.hysoft.com/wpf/2025/xaml"

<!-- 리소스 딕셔너리 병합 -->
<ResourceDictionary
    Source="/Presentation.Styles;component/Themes/Generic.xaml" />
```

## 문서

- 라이브러리 상세: [src/Docs/Documents/INDEX.md](src/Docs/Documents/INDEX.md)
- Docs.exe 를 실행하면 컴포넌트·API 를 화면으로 볼 수 있다

## 링크

- Repository: https://github.com/dlghksdyd/HYSoft
