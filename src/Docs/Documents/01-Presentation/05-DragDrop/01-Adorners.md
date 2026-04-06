# Adorners

네임스페이스: `HYSoft.Presentation.Adorners`

## GhostAdorner

드래그 중 반투명 고스트 이미지를 렌더링한다.

- `IsHitTestVisible = false` (마우스 이벤트 무시)
- `VisualBrush`로 요소 스냅샷 캡처 (opacity 적용)
- 생성 시 콘텐츠 크기 캐시
- `SetPosition(double x, double y)` → 위치 갱신 + InvalidateVisual
- OnRender: _contentOffset에 VisualBrush 사각형 렌더링

## LineAdorner

재정렬 시 삽입 위치 표시선을 렌더링한다.

```csharp
public enum LinePosition { None, Top, Bottom }

void Enabled(UIElement adornerScope);   // AdornerLayer 등록
void Disabled();                         // 등록 해제
void ShowAtElement(FrameworkElement? row, LinePosition pos,
                   Brush brush, double thickness);
void Hide();
```

- OnRender: 수평선을 둥근 캡(round cap)으로 렌더링
- `TranslatePoint`로 AdornedElement 기준 상대 위치 계산

## 사용 예시

```csharp
var dragDrop = new DragDrop();
var ctx = new DragDataContext
{
    DragScope = targetPanel,
    DraggedItem = selectedItem,
    ItemAlias = "MyItem",
    AdornerElement = dragVisual,
    AdornerOpacity = 0.5
};

// PreviewMouseLeftButtonDown
dragDrop.PreviewMouseLeftButtonDown_InitData(e, ctx);

// PreviewMouseMove
dragDrop.PreviewMouseMove_StartDragDrop(sender, e);

// Drop
var data = dragDrop.Drop_GetDropData<MyItemType>(e);
```
