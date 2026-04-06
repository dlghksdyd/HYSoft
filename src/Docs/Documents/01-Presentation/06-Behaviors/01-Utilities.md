# Binding & Tree Utilities

## BindingManager (Static)

네임스페이스: `HYSoft.Presentation.Bindings`

WPF 바인딩을 재배치하여 ElementName/RelativeSource 참조를 재해석한다.

```csharp
static void SetBindingSource(
    DependencyObject attachedObject,
    DependencyObject target,
    DependencyProperty dp)
```

### 바인딩 타입: Binding, MultiBinding, PriorityBinding 모두 처리.

### ElementName/RelativeSource 해석

| 소스 | 해석 |
|------|------|
| ElementName | FindByName()으로 NameScope 계층 검색 |
| Self | target 자신 |
| FindAncestor | 상위 타입/레벨 검색 |
| TemplatedParent | 소스의 TemplatedParent |
| 순수 {Binding} | Source=attachedObject, Path=DataContext.[원본] |

보존: Mode, Converter, StringFormat, FallbackValue, ValidationRules 등 전부 복사.

## TreeSearchHelper (Static)

네임스페이스: `HYSoft.Presentation.ElementTreeHelper`

Visual/Logical 트리 탐색 유틸리티.

```csharp
// 상위 검색 (VisualTreeHelper.GetParent)
static T? FindAncestor<T>(DependencyObject? current);

// 하위 검색 (재귀 깊이우선)
static T? FindChild<T>(DependencyObject? current);

// 동일 DataContext 공유 최상위 요소 검색
static T? FindTopElementSharingDataContext<T>(
    FrameworkElement? current, Type? dataContextType);
```

### 트리 탐색 순서

1. Visual 트리 (VisualTreeHelper.GetParent)
2. FrameworkElement.Parent
3. LogicalTreeHelper.GetParent (폴백)

`FindTopElementSharingDataContext`: DataContext 참조 동일성으로 범위 판단.
템플릿 내부 → 외부 View 네비게이션, 템플릿 루트 식별에 사용.
