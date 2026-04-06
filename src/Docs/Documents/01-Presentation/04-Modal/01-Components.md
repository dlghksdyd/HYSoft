# Modal Components

## ModalBaseViewModel

팝업 생명주기 관리. `NotifyPropertyChangedBase` 상속.

```csharp
public class ModalBaseViewModel : NotifyPropertyChangedBase
{
    public ObservableCollection<ModalInfo> PopupList { get; }
    public void OpenPopup(object popupViewModel);
    public void ClosePopup(object popupViewModel);  // 참조 동일성으로 제거
}
```

## ModalInfo

단일 모달 인스턴스 데이터 클래스.

```csharp
public class ModalInfo : NotifyPropertyChangedBase
{
    public object Content { get; set; }   // ViewModel (DataTemplate으로 표시)
    public Brush Background { get; set; } // 오버레이 배경 (기본: 반투명 시안)
}
```

## ModalBaseView (XAML)

Grid 패널로 여러 ModalInfo를 오버레이 형태로 스택.

```xml
<ItemsControl ItemsSource="{Binding PopupList}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate><Grid /></ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Grid>
                <Rectangle Fill="{Binding Background}" />
                <ContentControl Content="{Binding Content}" />
            </Grid>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

## 사용 흐름

```csharp
// 1. 초기화 + 뷰 등록
ModalManager.Configure("#80000000");
ModalManager.RegisterView<ConfirmView, ConfirmVM>();
// 2. MainWindow에 ModalBaseView 배치
// 3. 모달 호출
var vm = new ConfirmVM { Message = "삭제하시겠습니까?" };
if (ModalManager.Open(vm) == ModalResult.Ok) { /* 확인 */ }
// 4. 다이얼로그 내부에서 닫기
ModalManager.Close(this, ModalResult.Ok);
```
