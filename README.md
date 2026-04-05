# HYSoft

WPF Software Development Kit for .NET Framework 4.8.

## Features

- **Custom Controls** - HyButton, HyTextBox, HyCheckBox, HyComboBox, HyRadioButton, HyPasswordBox, HyScrollViewer, HyDatePicker, HyIcon, HyTitleBar, HyTextBlock, HyTemplateControl
- **Design Token System** - 145 semantic color tokens, 6 font size tokens, 87 icon assets with tinting support
- **MVVM Infrastructure** - RelayCommand, NotifyPropertyChangedBase, ObservableDictionary, EventToCommand
- **Drag & Drop** - GhostAdorner, LineAdorner, DragDrop orchestrator
- **Modal Dialog** - ModalManager with synchronous blocking API
- **Attached Behaviors** - WindowDragBehavior, FocusOnVisibleBehavior, ElementReferenceBinding

## Quick Start

### 1. Add project references

```xml
<ItemGroup>
    <ProjectReference Include="..\Presentation\Presentation.csproj" />
    <ProjectReference Include="..\Presentation.Styles\Presentation.Styles.csproj" />
</ItemGroup>
```

### 2. Add the XAML namespace

```xml
<Window xmlns:hy="http://schemas.hysoft.com/wpf/2025/xaml">
```

### 3. Use controls and tokens

```xml
<!-- Button with corner radius -->
<hy:HyButton Content="Click Me" CornerRadius="8" />

<!-- TextBox with watermark and input filtering -->
<hy:HyTextBox WaterMark="Enter text" IsOnlyNumber="True" CanKorean="False" />

<!-- Icon with hover/press tinting -->
<hy:HyIcon Source="Save" Color="Black" ColorHover="Blue" ColorPressed="Gray" />

<!-- Color and font size tokens -->
<TextBlock Foreground="{hy:Color TextPrimary}" FontSize="{hy:FontSize Md}" />
```

## Solution Structure

| Project | Type | Description |
|---------|------|-------------|
| **Presentation** | Library | MVVM infrastructure, converters, DragDrop, Modal, behaviors |
| **Presentation.Styles** | Library | Custom WPF controls, color/font/icon tokens, themes |
| **Docs** | WinExe | Interactive documentation viewer |
| **TestApp** | WinExe | Test application |

## Custom Controls

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyButton` | Button | CornerRadius |
| `HyTextBox` | TextBox | WaterMark, IsOnlyNumber, CanKorean, CanPaste, CornerRadius |
| `HyCheckBox` | CheckBox | BoxSize, CornerRadius, CheckBrush, BoxBorderBrush |
| `HyComboBox` | ComboBox | CornerRadius, PopupCornerRadius, PopupHorizontalOffset |
| `HyRadioButton` | RadioButton | Text, ButtonSize, EllipseBorderBrush, EllipseForeground |
| `HyPasswordBox` | TextBox | WaterMark, CornerRadius |
| `HyScrollViewer` | ScrollViewer | ThumbColor, ScrollBarSize, ThumbCornerRadius, RepeatButtonVisibility |
| `HyDatePicker` | DatePicker | Watermark, IsEditable, CalendarScale, CornerRadius |
| `HyIcon` | Control | Source (EIconKeys), Color, ColorHover, ColorPressed |
| `HyTitleBar` | ContentControl | ExitAppCommand, IconSize, MinimizeAppCommand, MaximizeAppCommand |
| `HyTextBlock` | Control | Text, CornerRadius |
| `HyTemplateControl` | Control | ComponentType, Header, Value, IsEditable, IsRequired, Commands |

## Design Tokens

### Colors

```xml
<TextBlock Foreground="{hy:Color TextPrimary}" />
<Border Background="{hy:Color SurfaceBase}" BorderBrush="{hy:Color BorderDefault}" />
<Button Background="{hy:Color ButtonPrimaryBg}" Foreground="{hy:Color ButtonPrimaryFg}" />
```

145 semantic tokens across categories: Text, Icon, Surface, Border, Button, Input, Feedback, Badge, Table, Navigation, Selection, Scrollbar, TitleBar, Popup.

### Font Sizes

```xml
<TextBlock FontSize="{hy:FontSize Md}" />
```

| Token | Size |
|-------|------|
| Xs | 12 |
| Sm | 14 |
| Md | 16 |
| Lg | 20 |
| Xl | 24 |
| _2Xl | 32 |

### Icons

```xml
<!-- In XAML markup -->
<Image Source="{hy:Icon Add}" />
<Image Source="{hy:Icon Save, ColorKey=IconDefault}" />

<!-- As control -->
<hy:HyIcon Source="Settings" Color="Black" Width="24" Height="24" />
```

87 Material Design icons with runtime tinting support.

## MVVM

```csharp
// ViewModel base class
public class MyViewModel : NotifyPropertyChangedBase
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private ICommand? _saveCommand;
    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(() => { /* save */ });
}
```

### EventToCommand (XAML event to ICommand binding)

```xml
<hy:HyTextBox>
    <hy:EventToCommand.Binding>
        <hy:Event Command="{Binding SearchCommand}" RoutedEvent="TextBox.TextChanged" />
    </hy:EventToCommand.Binding>
</hy:HyTextBox>
```

### Modal

```csharp
// Setup (once in App.xaml.cs)
ModalManager.Configure("#33ffffff");
ModalManager.RegisterView<MyPopupView, MyPopupViewModel>();

// Open modal (synchronous, blocks UI thread)
var result = ModalManager.Open(new MyPopupViewModel());
if (result == ModalResult.Ok) { /* confirmed */ }
```

## Build

```bash
dotnet build HYSoft.sln
```

## License

MIT
