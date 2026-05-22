# CLAUDE.md

Claude Code가 HYSoft 프로젝트를 이해하는 데 필요한 컨텍스트.

## 문서 규칙

- 프로젝트 내 모든 .md 문서는 **60줄 이내**로 작성한다.
- 60줄 초과 시 계층 구조로 분할한다.
- 각 프로젝트 루트에 CLAUDE.md가 있으면 해당 프로젝트의 세부 컨텍스트를 참조한다.

## 프로젝트 개요

HYSoft는 WPF 데스크톱 앱용 .NET SDK. `HySoft.Bundle` NuGet으로 배포. MIT. 저자: Hwanyong.lee

## 기술 스택

- C# (latest, Nullable enable) / .NET Framework 4.8 (기본), .NET 8.0 Windows (일부)
- WPF / EF 6.5.1 / Microsoft.Xaml.Behaviors.Wpf 1.1.135
- XAML: `xmlns:hy="http://schemas.hysoft.com/wpf/2025/xaml"`

## 솔루션 구조

```
src/Communication/              비동기 TCP 클라이언트/서버
src/Communication.FileTransfer/ FT10 파일 전송
src/Data/                       MSSQL DbContext (EF6)
src/Presentation/               MVVM, 컨버터, 모달, 드래그앤드롭
src/Presentation.Styles/        34개 커스텀 컨트롤, 디자인 토큰
src/Docs/                       문서 뷰어 (WPF 앱)
src/TestApp/                    샘플 앱 (WPF 앱)
src/Bundle/                     NuGet 패키지 생성
```

## 빌드

- `Directory.Build.props`: 공통 속성, 버전은 `.to_publish_version`에서 읽음
- `Directory.Build.targets`: 컴파일 전 `GlobalXmlns.g.cs` 자동 생성
- Bundle: Release 시 `$(SolutionDir)nuget/`에 NuGet 자동 생성

## 의존 관계

```
Communication.FileTransfer --> Communication
Presentation.Styles --> Presentation
TestApp --> Communication, Communication.FileTransfer, Presentation, Presentation.Styles
Docs --> Presentation, Presentation.Styles
```

## 코딩 규칙

- XML 문서 주석 활성화, 한국어 주석, Nullable enable
- 커스텀 컨트롤: `Hy` 접두사 (HyButton, HyTextBox)
- 색상 토큰: 시맨틱 네이밍 (TextPrimary, SurfaceBase)
- 커밋 메시지: 한국어, 간결하게

## 상세 문서

AI 학습용 라이브러리 상세 문서: [INDEX](src/Docs/Documents/INDEX.md)
