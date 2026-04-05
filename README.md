# HYSoft

WPF Software Development Kit for .NET Framework 4.8.

## Installation

```
Install-Package HySoft.Bundle
```

or .NET CLI:

```
dotnet add package HySoft.Bundle
```

## What's Included

The `HySoft.Bundle` package contains the following assemblies:

| Assembly | Description |
|----------|-------------|
| **Presentation.dll** | MVVM infrastructure, converters, DragDrop, Modal, attached behaviors |
| **Presentation.Styles.dll** | Custom WPF controls, color/font/icon design tokens, themes |
| **Communication.dll** | Async TCP client/server, file transfer protocol |
| **Data.dll** | MSSQL DbContext base class (Entity Framework 6) |
| **Docs.exe** | Interactive documentation viewer (run to browse all components) |
| **TestApp.exe** | Sample application with Modal, Icon, FileTransfer demos |

## Quick Start

### 1. Add the XAML namespace

```xml
<Window xmlns:hy="http://schemas.hysoft.com/wpf/2025/xaml">
```

### 2. Use controls and tokens

```xml
<!-- Button -->
<hy:HyButton Content="Click Me" CornerRadius="8" />

<!-- TextBox with watermark and input filtering -->
<hy:HyTextBox WaterMark="Enter text" IsOnlyNumber="True" CanKorean="False" />

<!-- Icon with hover/press tinting -->
<hy:HyIcon Source="Save" Color="Black" ColorHover="Blue" ColorPressed="Gray" />

<!-- Design tokens -->
<TextBlock Foreground="{hy:Color TextPrimary}" FontSize="{hy:FontSize Md}" />
```

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

145 semantic tokens: Text, Icon, Surface, Border, Button, Input, Feedback, Badge, Table, Navigation, Selection, Scrollbar, TitleBar, Popup.

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
<Image Source="{hy:Icon Add}" />
<hy:HyIcon Source="Settings" Color="Black" Width="24" Height="24" />
```

87 Material Design icons with runtime tinting support.

## MVVM

```csharp
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

### EventToCommand

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

// Open (synchronous, blocks UI thread)
var result = ModalManager.Open(new MyPopupViewModel());
if (result == ModalResult.Ok) { /* confirmed */ }
```

## Documentation

Run `Docs.exe` included in the package to browse interactive documentation for all components.

## License

MIT
