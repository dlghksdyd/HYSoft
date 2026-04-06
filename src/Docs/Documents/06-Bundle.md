# HYSoft.Bundle

모든 라이브러리를 NuGet 패키지로 번들링. 패키지 ID: `HySoft.Bundle`.

출력: `$(SolutionDir)nuget/`. Release 빌드 시 자동 생성.

## 포함 어셈블리 (5개)

| 어셈블리 | 설명 |
|----------|------|
| Communication.dll | 비동기 TCP 통신 |
| Communication.FileTransfer.dll | FT10 파일 전송 |
| Data.dll | MSSQL DbContext |
| Presentation.dll | MVVM 인프라, 컨버터, 모달 |
| Presentation.Styles.dll | 커스텀 컨트롤, 디자인 토큰 |

## Target Frameworks

- net48 (.NET Framework 4.8)
- net8.0-windows7.0 (.NET 8.0 Windows)

## 포함 애플리케이션

- **TestApp**: 파일전송/모달/아이콘 데모 (3개 샘플)
- **Docs**: 33개 컴포넌트 인터랙티브 문서 뷰어

## 패키지 구조

```
lib/{TargetFramework}/
  +-- *.dll, *.xml (5개 어셈블리 + XML 문서)
  +-- Docs/ (문서 뷰어 앱)
  +-- TestApp/ (데모 앱)
resources/ (디자인 리소스)
```

## 사용

```xml
<!-- XAML 네임스페이스 -->
xmlns:hy="http://schemas.hysoft.com/wpf/2025/xaml"

<!-- 리소스 딕셔너리 병합 -->
<ResourceDictionary
    Source="/Presentation.Styles;component/Themes/Generic.xaml" />
```
