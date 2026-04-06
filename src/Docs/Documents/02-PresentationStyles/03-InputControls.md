# Input Controls

## HyTextBox

워터마크 + 한글 입력 제어. HyTemplateControl 기반.

| 속성 | 기본값 |
|------|--------|
| Background | White |
| CornerRadius | 4 |
| Padding | 10 |
| Watermark | 설정 가능 |
| CanKorean | true |
| CanPaste | true |

- 텍스트 비어있으면 Watermark 표시 (TextWatermark 색상)

```xml
<hy:HyTextBox Watermark="이름" Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" />
```

## 기타 입력 컨트롤

| 컨트롤 | 설명 | XAML 예시 |
|--------|------|-----------|
| HyPasswordBox | 패스워드 입력 | `<hy:HyPasswordBox />` |
| HyRichTextBox | 리치 텍스트 편집 | `<hy:HyRichTextBox />` |
| HyComboBox | 드롭다운 선택 | `<hy:HyComboBox ItemsSource="{Binding Options}" />` |
| HyNumericUpDown | 숫자 스피너 | `<hy:HyNumericUpDown Value="{Binding Count}" />` |
| HyDatePicker | 날짜 선택 | `<hy:HyDatePicker SelectedDate="{Binding Date}" />` |
| HySlider | 슬라이더 | `<hy:HySlider Value="{Binding Volume}" />` |

## 공통 시각적 상태

| 상태 | 효과 |
|------|------|
| Normal | 기본 외형 |
| MouseOver | 테두리/배경 변경 |
| Focused | BorderFocus (#1282d6) |
| Disabled | 50% 불투명도 |
| Error | BorderError (#e4424d) |
