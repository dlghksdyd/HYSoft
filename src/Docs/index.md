# Docs

HYSoft 라이브러리의 인터랙티브 문서 뷰어 WPF 애플리케이션.

- **Target**: WinExe
- **의존성**: Presentation, Presentation.Styles, Communication(Live Echo Demo)

## 기능

- 상단 영역 3개 — UI Support(컴포넌트·토큰) · DB Support(DbContextBase) · Communication(TCP·File Transfer)
- 영역을 바꾸면 그 영역의 첫 페이지가 열린다. 앱 시작 시 UI Support 첫 페이지
- Communication › Live Echo Demo: 127.0.0.1 에서 실제 TcpServer·TcpClient 왕복. 페이지를 떠나면 즉시 정리
- 페이지 문서 내용은 라이브러리 코드가 기준(`Mvvm/Communication/CommunicationContent.cs`, `Mvvm/Database/DatabaseContent.cs`)
- ModalManager + PopupInfoViewModel 활용, MVVM 패턴 기반

## AI 시험 채널

`--cli --background` 로 띄우면 Named Pipe 로 조종할 수 있다: [Cli/README.md](Cli/README.md)

## Documents 폴더

`Documents/` 하위에 AI 학습용 라이브러리 상세 문서 36개 (모두 60줄 이내).
진입점: [INDEX.md](Documents/INDEX.md)
