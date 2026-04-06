# Design Tokens - Core Colors

`ColorSemantics.xaml`에 118개 시맨틱 색상 브러시 정의. StaticResource + ComponentResourceKey.

## Text Colors

| Key | 값 | 용도 |
|-----|------|------|
| TextPrimary | #6e6e6e | 기본 텍스트 |
| TextSecondary | #1d2c3b | 보조 텍스트 |
| TextTertiary | #293b4d | 삼차 텍스트 |
| TextDisabled | #40586d | 비활성 |
| TextInverse | #f2f6fa | 다크 배경 위 |
| TextLink | #0672cb | 하이퍼링크 |
| TextError/Success/Warning/Info | - | 시맨틱 메시지 |
| TextWatermark | #d2d2d2 | 플레이스홀더 |

## Icon Colors

IconDefault(#1d2c3b), IconInverse(#ebf1f6), IconDisabled(#40586d), +Success/Warning/Error/Info

## Surface Colors

| Key | 값 | 용도 |
|-----|------|------|
| SurfaceBase | #ffffff | 기본 배경 |
| SurfaceSubtle | #ebf1f6 | 밝은 배경 |
| SurfaceElevated | #f2f6fa | 카드 배경 |
| SurfaceInverse | #1D2C3B | 다크 배경 |
| SurfaceOverlay | #0a0e14 | 오버레이 |

## Border Colors

| Key | 값 | 용도 |
|-----|------|------|
| BorderSubtle | #c5d4e3 | 밝은 테두리 |
| BorderDefault | #6e6e6e | 기본 |
| BorderStrong | #839db4 | 강조 |
| BorderFocus | #1282d6 | 포커스 |
| BorderError/Success/Warning | - | 시맨틱 |

## Font Size (FontSizeSemantics.xaml)

Xs=12, Sm=14, Md=16, Lg=20, Xl=24, 2Xl=32 (pt)

```xml
<TextBlock Foreground="{StaticResource TextPrimary}" FontSize="{StaticResource Md}" />
```

하위: [ComponentTokens](01-ComponentTokens.md) (Button, Input, Feedback, Badge, Table 등)
