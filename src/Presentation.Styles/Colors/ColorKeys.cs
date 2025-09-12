using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSoft.Presentation.Styles.Colors
{
    public static class ColorKeys
    {
    }

    public enum EColorKeys
    {
        // 브랜드 정체성
        BrandPrimary,
        BrandSecondary,
        BrandTertiary,
        BrandQuaternary,

        // 상태 (피드백성)
        StateInfo,
        StateSuccess,
        StateWarning,
        StateError,

        // 인터랙션 상태
        StateHover,
        StateActive,
        StateDisabled,

        // 텍스트
        TextPrimary,
        TextSecondary,
        TextTertiary,
        TextQuaternary,

        // 배경
        SurfacePrimary,
        SurfaceSecondary,
        SurfaceTertiary,
        SurfaceQuaternary,

        // 보더
        BorderPrimary,
        BorderSecondary,
        BorderTertiary,
        BorderQuaternary,

        // 아이콘
        IconPrimary,
        IconSecondary,
        IconTertiary,
        IconQuaternary,

        // 레이어
        LayerBase,
        Layer1,
        Layer2,
        Layer3,
    }
}
