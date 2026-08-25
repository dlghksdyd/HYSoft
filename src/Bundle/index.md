# Bundle

모든 라이브러리를 `HySoft.Bundle` NuGet 패키지로 번들링하는 패키징 프로젝트.

- **패키지 ID**: HySoft.Bundle
- **Target**: net48, net8.0-windows7.0
- **출력**: `$(SolutionDir)nuget/`
- **자동 패키징**: Release 빌드 시

## 포함 어셈블리

Communication.dll, Communication.FileTransfer.dll, Data.dll,
Presentation.dll, Presentation.Styles.dll (각각 + XML 문서)

## 포함 앱

- **Docs.exe**: 33개 컴포넌트 문�� 뷰어
- **TestApp.exe**: 파일전송/모달/아이콘 데모

## 빌드 설정

소스 코드 없음. Staging/ 폴더에서 빌드된 DLL을 수집하여 nupkg 생성.
GenerateAssemblyInfo=false, NoWarn=CS2008;NU5128.
