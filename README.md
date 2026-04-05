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
| **Presentation.Styles.dll** | 33 custom WPF controls, color/font/icon design tokens, themes |
| **Communication.dll** | Async TCP client/server |
| **Communication.FileTransfer.dll** | File transfer protocol (chunked transfer, CRC32, resume, progress reporting) |
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
<hy:HyTextBox Watermark="Enter text" IsOnlyNumber="True" CanKorean="False" />

<!-- Icon with hover/press tinting -->
<hy:HyIcon Source="Save" Color="Black" ColorHover="Blue" ColorPressed="Gray" />

<!-- Design tokens -->
<TextBlock Foreground="{hy:Color TextPrimary}" FontSize="{hy:FontSize Md}" />
```

## Custom Controls

### Basic

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyButton` | Button | CornerRadius |
| `HyCheckBox` | CheckBox | BoxSize, CornerRadius, CheckBrush, BoxBorderBrush |
| `HyRadioButton` | RadioButton | Text, ButtonSize, EllipseBorderBrush, EllipseForeground |
| `HyToggleSwitch` | ToggleButton | SwitchWidth, SwitchHeight, ThumbSize, OnBackground, OffBackground |

### Text Input

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyTextBox` | TextBox | Watermark, IsOnlyNumber, CanKorean, CanPaste, CornerRadius |
| `HyPasswordBox` | TextBox | Watermark, CornerRadius |
| `HyTextBlock` | Control | Text, CornerRadius |
| `HyRichTextBox` | RichTextBox | Watermark, WatermarkForeground, CornerRadius |
| `HyNumericUpDown` | Control | Value, Minimum, Maximum, Increment, Watermark, StringFormat |

### Selection

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyComboBox` | ComboBox | CornerRadius, PopupCornerRadius, PopupHorizontalOffset |
| `HyListBox` | ListBox | CornerRadius |
| `HyDatePicker` | DatePicker | Watermark, IsEditable, CalendarScale, CornerRadius |
| `HySlider` | Slider | TrackHeight, ThumbSize, TrackCornerRadius |

### Data Display

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyDataGrid` | DataGrid | CornerRadius |
| `HyTreeView` | TreeView | CornerRadius |
| `HyBadge` | Control | Text, BadgeType (Neutral/Info/Success/Warning/Error/Highlight) |
| `HyProgressBar` | ProgressBar | CornerRadius, TrackBackground |
| `HyProgressRing` | Control | IsActive, RingSize, StrokeThickness, RingColor |

### Layout & Navigation

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyTabControl` | TabControl | CornerRadius |
| `HyExpander` | Expander | CornerRadius |
| `HyGroupBox` | GroupBox | CornerRadius |
| `HyScrollViewer` | ScrollViewer | ThumbColor, ScrollBarSize, ThumbCornerRadius |
| `HyGridSplitter` | GridSplitter | SplitterColor, SplitterThickness |
| `HyBreadcrumb` | ItemsControl | Separator, SeparatorForeground |
| `HyPaginator` | Control | CurrentPage, TotalPages, PageSize, TotalItems |

### Container & Chrome

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyWindow` | Window | CornerRadius |
| `HyTitleBar` | ContentControl | ExitAppCommand, IconSize, MinimizeAppCommand, MaximizeAppCommand |
| `HyDialog` | Window | DialogTitle, DialogMessage, ConfirmButtonText, CancelButtonText, ShowCancelButton |
| `HyToolBar` | ToolBar | CornerRadius |
| `HyStatusBar` | StatusBar | CornerRadius |

### Feedback

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyToast` | Control | Message, ToastType (Info/Success/Warning/Error), IsOpen, AutoCloseDelay |
| `HyToolTip` | ToolTip | CornerRadius |

### Menu

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyMenu` | Menu | CornerRadius |
| `HyContextMenu` | ContextMenu | CornerRadius |
| `HyMenuItem` | MenuItem | CornerRadius |

### Other

| Control | Base | Key Properties |
|---------|------|----------------|
| `HyIcon` | Control | Source (EIconKeys), Color, ColorHover, ColorPressed |
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

## File Transfer

```csharp
// Client - send file with progress
var client = new FileTransferClient(new TcpClientOptions(IPAddress.Parse("127.0.0.1"), 20000));
await client.SendFileAsync("data.zip",
    progress: new Progress<FileTransferProgress>(p =>
        Console.WriteLine($"{p.Percentage:P0} ({p.BytesTransferred}/{p.TotalBytes})")),
    cancellationToken: cts.Token);

// Server - receive files
var server = new FileTransferServer(new TcpServerOptions(IPAddress.Any, 20000), "./received");
await server.ReceiveFileAsync();
```

## Project Structure

```
HYSoft.sln
├── Communication              # Async TCP client/server transport layer
├── Communication.FileTransfer # File transfer protocol (depends on Communication)
├── Data                       # MSSQL DbContext base class
├── Presentation               # MVVM infrastructure, converters, behaviors
├── Presentation.Styles        # 33 WPF custom controls + design tokens
├── Docs                       # Interactive documentation viewer
├── TestApp                    # Sample application
└── Bundle                     # NuGet packaging aggregator
```

## Documentation

Run `Docs.exe` included in the package to browse interactive documentation for all 33 components.

## License

MIT
