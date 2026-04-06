# HYSoft Library Documentation

WPF 데스크톱 애플리케이션 개발을 위한 .NET 라이브러리 모음.

## Documents

- [00-Overview](00-Overview/README.md) - 솔루션 구조
  - [QuickStart](00-Overview/01-QuickStart.md)
- [01-Presentation](01-Presentation/README.md) - MVVM 인프라, UI 유틸리티
  - [MVVM](01-Presentation/01-MVVM/README.md) | [RelayCommand](01-Presentation/01-MVVM/01-RelayCommand.md)
  - [Event](01-Presentation/02-EventToCommand/README.md) | [EventToCommand](01-Presentation/02-EventToCommand/01-EventToCommand.md)
  - [Converters](01-Presentation/03-Converters.md)
  - [Modal](01-Presentation/04-Modal/README.md) | [Components](01-Presentation/04-Modal/01-Components.md)
  - [DragDrop](01-Presentation/05-DragDrop/README.md) | [Adorners](01-Presentation/05-DragDrop/01-Adorners.md)
  - [Behaviors](01-Presentation/06-Behaviors/README.md) | [Utilities](01-Presentation/06-Behaviors/01-Utilities.md)
- [02-PresentationStyles](02-PresentationStyles/README.md) - 디자인 시스템, 컨트롤
  - [DesignTokens](02-PresentationStyles/01-DesignTokens/README.md) | [ComponentTokens](02-PresentationStyles/01-DesignTokens/01-ComponentTokens.md)
  - [BasicControls](02-PresentationStyles/02-BasicControls/README.md) | [ToggleIconTemplate](02-PresentationStyles/02-BasicControls/01-ToggleIconTemplate.md)
  - [InputControls](02-PresentationStyles/03-InputControls.md)
  - [DataControls](02-PresentationStyles/04-DataControls/README.md) | [Feedback](02-PresentationStyles/04-DataControls/01-FeedbackControls.md)
  - [LayoutControls](02-PresentationStyles/05-LayoutControls/README.md) | [Navigation](02-PresentationStyles/05-LayoutControls/01-NavigationControls.md)
- [03-Communication](03-Communication/README.md) - 비동기 TCP 통신
  - [TcpClient](03-Communication/01-TcpClient/README.md) | [Usage](03-Communication/01-TcpClient/01-ManagerUsage.md)
  - [TcpServer](03-Communication/02-TcpServer/README.md) | [Events](03-Communication/02-TcpServer/01-EventsUsage.md)
- [04-FileTransfer](04-FileTransfer/README.md) - FT10 파일 전송
  - [Client](04-FileTransfer/01-Client.md) | [Server](04-FileTransfer/02-Server.md)
- [05-Data](05-Data/README.md) - MSSQL (EF6)
  - [Usage](05-Data/01-Usage.md)
- [06-Bundle](06-Bundle.md) - NuGet 패키지 배포

## Namespace Hierarchy

```
HYSoft
  +-- Communication.Tcp.Client / .Server
  +-- Communication.FileTransfer
  +-- Data.Mssql
  +-- Presentation (.Interactivity, .Converters, .Modal, .DragDrop, .Adorners)
  +-- Presentation.Styles (.Controls, .Icons)
```
