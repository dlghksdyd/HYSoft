# Docs CLI 채널 (AI 전용)

AI 가 Docs 앱을 마우스·포커스 없이 시험하는 통로. 사람이 그냥 실행한 Docs 에는 열리지 않는다.

## 쓰는 법

```powershell
dotnet build src/Docs/Docs.csproj -c Debug
pwsh -File scripts/docs-cli.ps1 launch            # --cli --background 로 기동, 준비까지 대기
pwsh -File scripts/docs-cli.ps1 area comm
pwsh -File scripts/docs-cli.ps1 open "Live Echo Demo"
pwsh -File scripts/docs-cli.ps1 echo.start 50505
pwsh -File scripts/docs-cli.ps1 shot C:\temp\docs.png
pwsh -File scripts/docs-cli.ps1 quit              # 프로세스 부재까지 확인
```

명령 목록은 `help`. 응답은 JSON 한 줄(`{"ok":…,"error":…,"data":…}`).
종료 코드: 0 ok · 1 앱이 거부 · 2 대상 없음/여러 개(`-TargetPid`) · 3 통신 실패.

## 설계

| 항목 | 내용 |
|------|------|
| 실행 인자 | `--cli` 채널 열기 · `--background` 화면 밖·작업 표시줄 없음·포커스 없음 |
| 파이프 | `HYSoft.Docs.Cli.<pid>` — 인스턴스끼리 섞이지 않음 |
| 접근 | 같은 사용자만(`CurrentUserOnly`), 로컬 경로만(SMB 원격은 거부) |
| 처리 | 연결 1회 = 요청 1줄 = 응답 1줄, UI 스레드에서 실행 |
| 준비 | 첫 화면이 그려지기 전에는 `ping`·`help` 만 답함 |
| 스크린샷 | `RenderTargetBitmap` 으로 창을 직접 그림 — 화면 캡처가 아니므로 창을 앞으로 가져오지 않음 |

## 판정 원칙

- `echo.*` 는 **실행 여부가 아니라 결과 상태**로 ok 를 정한다. 포트 충돌·형식 오류는 앱이 로그만
  남기므로, 실행만 보면 서버가 안 떴는데 ok 가 된다(2026-11-10 실측 후 고침).
- `click` 은 누르기만 하고 기다리지 않는다(모달이 뜨면 사람이 닫을 때까지 안 돌아오므로).
  결과는 `text`·`state` 로 다시 읽는다.
- `quit` 의 성공은 응답이 아니라 프로세스 부재로 판정한다.

## 소스

- 인자: [DocsCliOptions.cs](DocsCliOptions.cs) · 채널: [DocsCliServer.cs](DocsCliServer.cs)
- 명령: [DocsCliCommands.cs](DocsCliCommands.cs) · 클라이언트: [scripts/docs-cli.ps1](../../../scripts/docs-cli.ps1)
