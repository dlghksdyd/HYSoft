# Drag and Drop System

네임스페이스: `HYSoft.Presentation.DragDrop`

## IDragDataContext (Interface)

```csharp
public interface IDragDataContext
{
    UIElement DragScope { get; set; }       // 드롭 대상 영역
    object DraggedItem { get; set; }        // 드래그 데이터
    string ItemAlias { get; set; }          // DataObject 포맷 식별자
    UIElement AdornerElement { get; set; }  // 고스트 이미지 요소
    double AdornerOpacity { get; set; }     // 고스트 투명도 (0~1)
    Point AdornerOffset { get; set; }       // 마우스로부터 오프셋
}
```

DragDataContext: 기본 구현 (AdornerOpacity=0.3, AdornerOffset=(0,0))

## DragDrop API

```csharp
void AddDragOverEventHandler(DragEventHandler handler);
void AddDragLeaveEventHandler(DragEventHandler handler);
void AddDragEnterEventHandler(DragEventHandler handler);
void PreviewMouseLeftButtonDown_InitData(MouseEventArgs e, IDragDataContext ctx);
void PreviewMouseMove_StartDragDrop(object sender, MouseEventArgs e);
T? Drop_GetDropData<T>(DragEventArgs e);
```

## 동작 흐름

1. **InitData**: 마우스 위치 + IDragDataContext 저장
2. **StartDragDrop**: 최소 드래그 거리 초과 시 GhostAdorner 생성, DragScope에 이벤트 구독, `DoDragDrop()` 호출
3. **드래그 중**: DragOver→위치 갱신, QueryContinueDrag→ESC/드롭 감지, DragLeave/Enter→Adorner 제거/추가
4. **종료**: 이벤트 해제, Adorner 제거, 상태 초기화

## 데이터 추출

```csharp
// Drop 이벤트 핸들러에서
var data = dragDrop.Drop_GetDropData<MyItemType>(e);
```

하위 문서: [Adorners](01-Adorners.md)
