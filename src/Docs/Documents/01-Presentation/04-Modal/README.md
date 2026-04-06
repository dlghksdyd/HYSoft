# Modal Dialog System - ModalManager

네임스페이스: `HYSoft.Presentation.Modal`

MVVM 기반 동기식 모달 다이얼로그의 정적 Facade.

## 초기화

```csharp
ModalManager.Configure(Brush background);
ModalManager.Configure(string backgroundHex); // "#80D1E8EF"
```

## 뷰 등록

```csharp
ModalManager.RegisterView<TView, TViewModel>();
// FrameworkElementFactory로 DataTemplate 동적 생성
```

## 모달 열기 (동기식)

```csharp
ModalResult result = ModalManager.Open(object viewmodel);
```

- `Dispatcher.CheckAccess()`로 UI 스레드 검증
- `TaskCompletionSource` + `DispatcherFrame`으로 동기 블로킹
- Close 호출 시까지 대기

## 모달 닫기

```csharp
ModalManager.Close(object viewmodel, ModalResult result = ModalResult.None);
ModalManager.CloseAll(ModalResult result = ModalResult.None);
```

## ModalResult

```csharp
public enum ModalResult { None, Ok, Cancel }
```

## 내부 상태

| 멤버 | 타입 | 설명 |
|------|------|------|
| View | ModalBaseView | UIElement 호스트 |
| ViewModel | ModalBaseViewModel | 팝업 목록 관리 |
| _pending | Dictionary\<object, TCS\> | 대기 중인 다이얼로그 |

하위 문서: [Components](01-Components.md)
