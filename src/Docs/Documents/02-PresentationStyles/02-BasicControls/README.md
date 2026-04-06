# Basic Controls - Button, CheckBox, RadioButton

## HyButton

4가지 변형 (Primary, Secondary, Tertiary, Destructive).

| 속성 | 기본값 |
|------|--------|
| Foreground | ButtonPrimaryFg |
| Background | ButtonPrimaryBg |
| BorderBrush | ButtonPrimaryBorder |
| FontSize | 20 |
| CornerRadius | 4 |

상태: Hover(10% overlay), Pressed(20%), KeyboardFocus(10%), Disabled(50% opacity)

```xml
<hy:HyButton Content="확인" Background="{StaticResource ButtonPrimaryBg}" />
```

## HyCheckBox

커스텀 체크박스. 체크/미결정(Indeterminate) 상태.

| 속성 | 설명 |
|------|------|
| BoxSize | 체크박스 크기 |
| CornerRadius | 모서리 둥글기 |
| CheckBrush | 체크마크 색상 (기본 Black) |
| CheckThickness | 체크마크 두께 (기본 3) |

- 체크: `M 4 10 L 8 14 L 14 6` (V자)
- 미결정: `M 4 10 L 14 10` (수평선)

```xml
<hy:HyCheckBox Content="동의합니다" IsChecked="{Binding Agreed}" />
```

## HyRadioButton

커스텀 라디오 버튼. 타원 외형 커스터마이즈.

| 속성 | 설명 |
|------|------|
| EllipseBackground | 타원 배경 |
| EllipseForeground | 선택 시 내부 원 |
| EllipseBorderThickness/Brush | 타원 테두리 |
| EllipseHover | 호버 색상 |

```xml
<hy:HyRadioButton Content="옵션 A" GroupName="Group1" />
```

하위: [ToggleSwitch, Icon, TemplateControl](01-ToggleIconTemplate.md)
