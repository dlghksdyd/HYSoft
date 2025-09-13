# -*- coding: utf-8 -*-
"""
generate_from_tokens_json.py

입력 JSON (Tokens Studio 스타일)에서 다음을 생성:
- Color.Primitives.xaml    (Colours.*, White/Black = Color / Scales = Int32; Scales 딕셔너리 x:Key="Scales")
- Color.Tokens.xaml        (Primary.* = Color; 값은 Primitives의 Color를 StaticResource로 참조)
- Color.Semantics.xaml     (시맨틱 = SolidColorBrush; Color는 Tokens/Primitives Color를 StaticResource로 참조)
- Colors.xaml              (외부노출 = ComponentResourceKey + EColorKeys)
- ColorKeys.cs             (EColorKeys = 시맨틱 키 PascalCase)

사용:
python generate_from_tokens_json.py --in tokens.json --outdir .\out --ns HYSoft.Presentation.Styles.ColorTokens
"""

import argparse
import json
import os
from datetime import datetime
from xml.sax.saxutils import escape as xml_escape

DEFAULT_NS = "HYSoft.Presentation.Styles.ColorTokens"

# 경로: 사용자가 예제로 준 pack URI
PRIMITIVES_RD_SOURCE = "/Presentation.Styles;component/ColorTokens/Color.Primitives.xaml"
TOKENS_RD_SOURCE     = "/Presentation.Styles;component/ColorTokens/Color.Tokens.xaml"

# ---------- Helpers ----------

def now_stamp():
    return datetime.now().strftime("%Y-%m-%d %H:%M:%S")

def wrap_dict(body: str, include_ns_c=False, ns=None, merged_sources=None):
    head = (
f"""<!-- Auto-generated {now_stamp()} -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml\""""
    )
    # 위 라인의 끝에 있는 \" 는 의도된 문자열 리터럴 이스케이프 (XAML 출력은 정상)
    if include_ns_c and ns:
        head += f'\n                    xmlns:c="clr-namespace:{ns}"'
    head += ">\n"
    if merged_sources:
        head += "    <ResourceDictionary.MergedDictionaries>\n"
        for src in merged_sources:
            head += f'        <ResourceDictionary Source="{src}" />\n'
        head += "    </ResourceDictionary.MergedDictionaries>\n\n"
    else:
        head += "\n"
    return head + body + "\n</ResourceDictionary>\n"

def pascal_join(parts):
    return "".join(p[:1].upper() + p[1:] for p in parts if p)

def flatten_colours(colours_node, prefix=("Colours",)):
    out = {}
    for family, scales in colours_node.items():
        for step, leaf in scales.items():
            if isinstance(leaf, dict) and "$value" in leaf:
                val = str(leaf["$value"]).strip()
                out[".".join(prefix + (family, step))] = val
    return out

def flatten_scales(scales_node):
    out = {}
    for group, items in scales_node.items():
        for name, leaf in items.items():
            if "$value" in leaf:
                out[".".join(("Scales", group, name))] = leaf["$value"]
    return out

def read_primitive_colors_and_scales(root):
    prim_default = root.get("Primitives/Default", {})
    colours = prim_default.get("Colours", {})
    colors_map = flatten_colours(colours)
    for mono in ("White", "Black"):
        if mono in prim_default and isinstance(prim_default[mono], dict) and "$value" in prim_default[mono]:
            colors_map[mono] = str(prim_default[mono]["$value"]).strip()
    scales = prim_default.get("Scales", {})
    scales_map = flatten_scales(scales) if scales else {}
    return colors_map, scales_map

def read_tokens(root):
    tokens_root = root.get("ColorTokens/DIMS", {})
    out = {}
    for name, node in tokens_root.items():
        for step, leaf in node.items():
            if isinstance(leaf, dict) and "$value" in leaf:
                ref = str(leaf["$value"]).strip().strip("{}").strip()
                out[f"{name}.{step}"] = ref  # e.g., "Primary.100" -> "Colours.Slate.100"
    return out

def read_semantics(root):
    sem_root = root.get("Semantics/DIMS", {})
    out = {}
    def walk(node, path):
        if isinstance(node, dict) and "$value" in node:
            ref = str(node["$value"]).strip().strip("{}").strip()
            out[".".join(path)] = ref     # "ButtonPrimary.Surface" -> "Primary.100" or "Colours.Slate.200"
            return
        if isinstance(node, dict):
            for k, v in node.items():
                walk(v, path + [k])
    walk(sem_root, [])
    return out

def semantics_key_to_pascal(name_with_dots):
    return pascal_join(name_with_dots.split("."))

# ---------- Scale key transform ----------

def transform_scale_key(key: str) -> str:
    """
    'Scales.2s.4XL' -> 'Scales.2S.4Xl'
    규칙:
      - 두번째 세그먼트가 '<숫자>s'면 마지막 's'를 'S'로
      - 세번째 세그먼트가 '...XL'로 끝나면 끝 'L'만 소문자화
    """
    parts = key.split(".")
    if len(parts) != 3 or parts[0] != "Scales":
        return key
    p2 = parts[1]
    if p2.lower().endswith("s") and p2[:-1].isdigit():
        p2 = p2[:-1] + "S"
    p3 = parts[2]
    if p3.endswith("XL"):
        p3 = p3[:-1] + "l"
    return ".".join([parts[0], p2, p3])

# ---------- Generators ----------

def gen_color_primitives_xaml(primitive_colors, primitive_scales):
    lines = []
    # Colours (Color)
    for key, hexval in sorted(primitive_colors.items()):
        lines.append(f'    <Color x:Key="{xml_escape(key)}">{xml_escape(hexval)}</Color>')

    # Scales (Int32) - 내부 ResourceDictionary + x:Key + 키 변환
    if primitive_scales:
        lines.append("")
        lines.append('    <!-- Scales (integers) -->')
        lines.append('    <ResourceDictionary x:Key="Scales" xmlns:sys="clr-namespace:System;assembly=mscorlib">')
        for key, num in sorted(primitive_scales.items(), key=lambda kv: kv[0]):
            tkey = transform_scale_key(key)
            lines.append(f'        <sys:Int32 x:Key="{xml_escape(tkey)}">{int(num)}</sys:Int32>')
        lines.append('    </ResourceDictionary>')

    return wrap_dict("\n".join(lines))

def gen_color_tokens_xaml(tokens_map):
    """
    Tokens = Color (값은 Primitives Color StaticResource 참조).
    + 상단에 Primitives 병합 추가.
    """
    lines = []
    for token_key, primitive_ref in sorted(tokens_map.items()):
        lines.append(f'    <Color x:Key="{xml_escape(token_key)}">{{StaticResource {xml_escape(primitive_ref)}}}</Color>')
    return wrap_dict("\n".join(lines), merged_sources=[PRIMITIVES_RD_SOURCE])

def gen_color_semantics_xaml(semantics_map):
    """
    Semantics = SolidColorBrush (Color는 Tokens 또는 Primitives의 Color StaticResource 참조).
    + 상단에 Tokens 병합 추가.
    """
    lines = []
    for sem_key, ref in sorted(semantics_map.items()):
        pascal = semantics_key_to_pascal(sem_key)
        lines.append(f'    <SolidColorBrush x:Key="{pascal}" Color="{{StaticResource {xml_escape(ref)}}}" />')
    return wrap_dict("\n".join(lines), merged_sources=[TOKENS_RD_SOURCE])

def gen_colors_exposed_xaml(semantics_map, ns):
    """
    외부 노출: ComponentResourceKey + EColorKeys
    (여기선 별도 병합이 필요 없도록 App.xaml에서 병합 순서 보장 권장)
    """
    lines = []
    for sem_key in sorted(semantics_map.keys()):
        pascal = semantics_key_to_pascal(sem_key)
        lines.append(
            f'    <SolidColorBrush x:Key="{{ComponentResourceKey TypeInTargetAssembly={{x:Type c:ColorKeys}}, '
            f'ResourceId={{x:Static c:EColorKeys.{pascal}}}}}" '
            f'Color="{{StaticResource {pascal}}}" />'
        )
    return wrap_dict("\n".join(lines), include_ns_c=True, ns=ns)

def gen_colorkeys_cs(semantics_map, ns):
    keys = [semantics_key_to_pascal(k) for k in sorted(semantics_map.keys())]
    members = ",\n        ".join(keys)
    return f"""// Auto-generated {now_stamp()}
namespace {ns}
{{
    public static class ColorKeys
    {{
    }}

    public enum EColorKeys
    {{
        {members}
    }}
}}
"""

# ---------- Main ----------

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--in", dest="infile", required=True, help="Tokens Studio style JSON file")
    ap.add_argument("--outdir", required=True, help="Output directory")
    ap.add_argument("--ns", default=DEFAULT_NS, help="C# namespace for ColorKeys.cs")
    args = ap.parse_args()

    with open(args.infile, "r", encoding="utf-8") as f:
        data = json.load(f)

    primitive_colors, primitive_scales = read_primitive_colors_and_scales(data)
    tokens_map     = read_tokens(data)       # Primary.* -> Colours.*.*
    semantics_map  = read_semantics(data)    # e.g. "ButtonPrimary.Surface" -> "Primary.100"

    os.makedirs(args.outdir, exist_ok=True)

    with open(os.path.join(args.outdir, "Color.Primitives.xaml"), "w", encoding="utf-8") as f:
        f.write(gen_color_primitives_xaml(primitive_colors, primitive_scales))

    with open(os.path.join(args.outdir, "Color.Tokens.xaml"), "w", encoding="utf-8") as f:
        f.write(gen_color_tokens_xaml(tokens_map))

    with open(os.path.join(args.outdir, "Color.Semantics.xaml"), "w", encoding="utf-8") as f:
        f.write(gen_color_semantics_xaml(semantics_map))

    with open(os.path.join(args.outdir, "Colors.xaml"), "w", encoding="utf-8") as f:
        f.write(gen_colors_exposed_xaml(semantics_map, args.ns))

    with open(os.path.join(args.outdir, "ColorKeys.cs"), "w", encoding="utf-8") as f:
        f.write(gen_colorkeys_cs(semantics_map, args.ns))

    print("✅ Generated files in", args.outdir)


if __name__ == "__main__":
    main()
