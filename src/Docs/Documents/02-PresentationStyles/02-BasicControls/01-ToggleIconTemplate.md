# ToggleSwitch, Icon, TemplateControl

## HyToggleSwitch

애니메이션 Thumb이 있는 토글 스위치.

| 속성 | 설명 |
|------|------|
| OffBackground | 꺼진 배경 (기본 BorderDefault) |
| OnBackground | 켜진 배경 (기본 ButtonPrimaryBg) |
| SwitchWidth/Height | 스위치 크기 |
| ThumbSize | Thumb 크기 |

- `TranslateTransform`으로 Thumb 이동 애니메이션
- IsChecked 변경 시 배경색 전환, KeyboardFocus 시 테두리 추가

```xml
<hy:HyToggleSwitch IsChecked="{Binding IsEnabled}" />
```

## HyIcon

87개 머티리얼 디자인 PNG 아이콘. Viewbox + Image (UniformToFill).

**구성 요소:**
- **IconExtension** (마크업 확장): Color(직접지정), ColorKey(EColorKeys enum)
- **IconGenerator**: `GetIcon(EIconKeys)` → ImageSource 로드
- **EIconKeys**: 자동 생성 아이콘 키 enum
- **IconKeys**: 키 → 리소스 경로 매핑
- **IconKeyToImageSourceConverter**: 런타임 아이콘 해석

```xml
<hy:HyIcon Width="24" Height="24"
    Source="{hy:Icon Key=Search, ColorKey=IconDefault}" />
```

## HyTemplateControl

입력 검증 기능의 베이스 템플릿 컨트롤.

| 속성 | 기본값 | 설명 |
|------|--------|------|
| CanKorean | true | 한글 입력 허용 |
| CanPaste | true | 붙여넣기 허용 |
| IsOnlyNumber | false | 숫자만 입력 |

```xml
<hy:HyTemplateControl IsOnlyNumber="True" CanPaste="False" />
```
