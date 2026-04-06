# HYSoft Solution Overview

WPF 데스크톱 앱 구축을 위한 .NET SDK. MVVM, 커스텀 컨트롤, TCP 통신, 파일 전송, DB 접근 제공.

## 프로젝트 구성 (8개)

| 프로젝트 | 유형 | 설명 |
|----------|------|------|
| Communication | 라이브러리 | 비동기 TCP 클라이언트/서버 |
| Communication.FileTransfer | 라이브러리 | FT10 파일 전송 (CRC32, Resume) |
| Data | 라이브러리 | MSSQL DbContext (EF6) |
| Presentation | 라이브러리 | MVVM, 컨버터, 모달, 드래그앤드롭 |
| Presentation.Styles | 라이브러리 | 34개 커스텀 컨트롤, 디자인 토큰 |
| TestApp | WPF 앱 | 파일전송/모달/아이콘 데모 |
| Docs | WPF 앱 | 컴포넌트 문서 뷰어 |
| Bundle | 패키징 | NuGet 패키지 생성 |

## 의존 관계

```
Bundle --> Communication, Communication.FileTransfer, Data, Presentation, Presentation.Styles
Communication.FileTransfer --> Communication
Presentation.Styles --> Presentation
TestApp --> Communication, Communication.FileTransfer, Presentation, Presentation.Styles
Docs --> Presentation, Presentation.Styles
```

## Target Frameworks

- .NET Framework 4.8 / .NET 8.0 Windows

## NuGet 의존성

| 패키지 | 버전 | 사용처 |
|--------|------|--------|
| Microsoft.Xaml.Behaviors.Wpf | 1.1.135 | Presentation |
| EntityFramework | 6.5.1 | Data |
| System.Memory / Buffers | 4.6.0 | Communication |

## XAML 네임스페이스

```xml
xmlns:hy="http://schemas.hysoft.com/wpf/2025/xaml"
```

## 핵심 설계 패턴

- **MVVM**: NotifyPropertyChangedBase, RelayCommand
- **Manager Pattern**: TcpClientManager, ModalManager 등 정적 생명주기 관리
- **Options Pattern**: TcpClientOptions, TcpServerOptions
- **Template Method**: DbContextBase.ConfigureModel
- **Attached Behavior**: EventToCommand, WindowDragBehavior
- **Design Token**: 118개 시맨틱 색상 토큰
