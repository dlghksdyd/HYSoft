# Value Converters

네임스페이스: `HYSoft.Presentation.Converters`

## Bool → Visibility

| Converter | true → | false → |
|-----------|--------|---------|
| BoolToVisibilityConverter | Visible | Collapsed |
| BoolToVisibilityReverseConverter | Collapsed | Visible |
| BoolToVisibilityMultiConverter | 모두 true→Visible | 하나라도 false→Collapsed |

모두 양방향. Multi는 ConvertBack 미지원.

## Geometry 변환기

| Converter | 입력 | 출력 |
|-----------|------|------|
| CornerRadiusToClipConverter | width, height, CornerRadius | RectangleGeometry |
| CornerRadiusToClipExpandConverter | width, height, CornerRadius, expand | StreamGeometry (ArcTo) |

- CornerRadiusToClipExpandConverter는 각 모서리 독립 반지름 + expand 확장/축소 지원
- 반대편 모서리 비례 스케일링으로 오버슈팅 방지

```xml
<Border.Clip>
    <MultiBinding Converter="{StaticResource CornerClipExpand}">
        <Binding Path="ActualWidth" RelativeSource="{RelativeSource Self}" />
        <Binding Path="ActualHeight" RelativeSource="{RelativeSource Self}" />
        <Binding Path="CornerRadius" RelativeSource="{RelativeSource Self}" />
        <Binding Path="Expand" />
    </MultiBinding>
</Border.Clip>
```

## 기타 변환기

| Converter | 입력 | 출력 |
|-----------|------|------|
| WidthHeightToRectConverter | width, height | Rect(0,0,w,h) NaN방어 |
| DoublePlusMarginConverter | v0, v1 | v0 + (v1 * 2) |
| StringToNullableIntConverter | string | int? (빈→null, 잘못된값→null) |
