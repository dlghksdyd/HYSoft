# HYSoft

Claude Code 가 HYSoft 저장소에서 작업할 때 먼저 읽는 문서. 패키지 소개는 [README.md](README.md).

## 문서 규칙

- 프로젝트 내 모든 .md 문서는 **60줄 이내**로 작성한다. 넘으면 계층 구조로 분할한다.
- 각 프로젝트 폴더의 `index.md` 가 그 프로젝트의 세부 컨텍스트다.

## 개요

WPF 데스크톱 앱용 .NET SDK. `HySoft.Bundle` NuGet 으로 배포. 모든 프로젝트 `net8.0-windows`
(커밋 `ae132ef` 에서 net48 에서 전환). 주요 의존: EF Core 9.0.8, Microsoft.Xaml.Behaviors.Wpf 1.1.135.

## 솔루션 구조 (HYSoft.sln)

```
src/Communication              비동기 TCP 클라이언트/서버
src/Communication.FileTransfer FT10 파일 전송 (→ Communication)
src/Data                       MSSQL DbContextBase (EF Core 9)
src/Presentation               MVVM 인프라, 컨버터, 비헤이비어
src/Presentation.Styles        Hy* 컨트롤 + 디자인 토큰 (→ Presentation)
src/Docs                       문서 뷰어 앱 (→ Presentation, Presentation.Styles, Communication)
src/TestApp                    샘플 앱 (→ Communication, FileTransfer, Presentation, Styles)
src/Bundle                     NuGet 패키징
tests/HYSoft.Communication.Tests  통신·파일 전송 시험 (xUnit, 실제 루프백 소켓)
```

## Branch Policy

- Do not push directly to `main`. Use a feature branch such as `hylee`.

## 빌드·시험

- `Directory.Build.props`: 공통 속성, 버전은 `.to_publish_version` 에서 읽음
- `Directory.Build.targets`: 컴파일 전 `GlobalXmlns.g.cs` 생성, Release 시 Bundle 스테이징 복사
  (시험 프로젝트는 `HysoftBundleStagingEnabled=false` 로 번들에서 뺀다)
- 시험: `dotnet test tests/HYSoft.Communication.Tests --filter "FullyQualifiedName~HYSoft.Communication.Tests"`
  — 순차 실행, 시험당 30초 상한(무기한 대기 회귀가 멈춤 대신 실패로 드러나게)

## Docs 앱을 AI 가 시험할 때

화면을 끌어오지 말고 CLI 를 쓴다: `pwsh -File scripts/docs-cli.ps1 launch` → [src/Docs/Cli/README.md](src/Docs/Cli/README.md)

## 코딩 규칙

- XML 문서 주석 활성화, 한국어 주석, Nullable enable
- 커스텀 컨트롤: `Hy` 접두사 (HyButton, HyTextBox)
- 색상 토큰: 시맨틱 네이밍 (TextPrimary, SurfaceBase)
- 커밋 메시지: 한국어, 간결하게

## 상세 문서

AI 학습용 라이브러리 상세 문서: [INDEX](src/Docs/Documents/INDEX.md)
