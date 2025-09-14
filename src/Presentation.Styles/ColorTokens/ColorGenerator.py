#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
HYSoft Color Codegen (Semantics → ColorSemantics.xaml, ColorGenerator.cs)

- Input default: tokens.json  (override with --input)
- Output files: ColorSemantics.xaml, ColorGenerator.cs  (directory via --outdir)
- Namespace default: HYSoft.Presentation.Styles.ColorTokens (override with --namespace)

Rules
- EColorKeys = 모든 Semantics(DIMS) 트리의 leaf("$value")를 경로 결합(PascalCase)한 키들
  예) Button.Primary.Surface => ButtonPrimarySurface
      TablePrimary.Surface.Title => TablePrimarySurfaceTitle
- 색상 해석:
  {Primary.N} -> ColorTokens/DIMS.Primary.N -> (보통) {Colours.Slate.N} -> Primitives/Default.Colours.Slate.N -> #hex
  {Colours.Family.Level} -> Primitives/Default.Colours.Family.Level -> #hex
  #hex 직접값도 허용
- ColorSemantics.xaml 구조:
    1) <SolidColorBrush x:Key="KeyName">#hex</SolidColorBrush>
    2) <SolidColorBrush x:Key="{ComponentResourceKey ... ResourceId={x:Static c:EColorKeys.KeyName}}"
         Color="{Binding Color, Source={StaticResource KeyName}}"/>
- ColorGenerator.cs 구조: 사용자가 지정한 형태 그대로 (앱 리소스 우선, 없으면 LoadComponent)
"""

from __future__ import annotations
import argparse
import json
import os
from datetime import datetime
from typing import Any, Dict, List, Tuple


# ---------------- CLI ----------------

def parse_args() -> argparse.Namespace:
    ap = argparse.ArgumentParser(description="Generate ColorSemantics.xaml and ColorGenerator.cs from tokens JSON.")
    ap.add_argument("-i", "--input", default="tokens.json", help="Input tokens JSON file (default: tokens.json)")
    ap.add_argument("-o", "--outdir", default=".", help="Output directory (default: current dir)")
    ap.add_argument("-n", "--namespace", default="HYSoft.Presentation.Styles.ColorTokens",
                    help="C#/XAML namespace (default: HYSoft.Presentation.Styles.ColorTokens)")
    return ap.parse_args()


# ---------------- Utils ----------------

def ts() -> str:
    return datetime.now().strftime("%Y-%m-%d %H:%M:%S")


def ensure_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)


def read_json(path: str) -> Dict[str, Any]:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def to_lower_hex(s: str) -> str:
    return s.strip().lower()


# ---------------- Token resolution ----------------

class TokenResolver:
    """
    Resolves values like:
      - {Primary.600} -> ColorTokens/DIMS.Primary.600 -> (likely) {Colours.Slate.600} -> hex
      - {Colours.Slate.200} -> Primitives/Default.Colours.Slate.200 -> hex
      - #rrggbb (direct)
    """

    def __init__(self, root: Dict[str, Any]) -> None:
        self.root = root
        self.primitives = root.get("Primitives/Default", {})
        self.tokens = root.get("ColorTokens/DIMS", {})

    def resolve_value_to_hex(self, val: str) -> str:
        v = val.strip()
        if v.startswith("#"):
            return to_lower_hex(v)
        if v.startswith("{") and v.endswith("}"):
            return self._resolve_ref(v[1:-1].strip())
        raise ValueError(f"Unsupported value format: {val}")

    def _resolve_ref(self, ref: str) -> str:
        parts = ref.split(".")
        if not parts:
            raise ValueError(f"Bad reference: {ref}")

        head = parts[0]
        if head == "Primary":
            if len(parts) != 2:
                raise ValueError(f"Primary reference must be Primary.<Level>: {ref}")
            level = parts[1]
            entry = self.tokens.get("Primary", {}).get(level, {})
            value = entry.get("$value")
            if not value:
                raise KeyError(f"Missing ColorTokens/DIMS.Primary.{level}")
            return self.resolve_value_to_hex(value)

        if head == "Colours":
            if len(parts) != 3:
                raise ValueError(f"Colours reference must be Colours.<Family>.<Level>: {ref}")
            family, level = parts[1], parts[2]
            fam = self.primitives.get("Colours", {}).get(family, {})
            entry = fam.get(level, {})
            value = entry.get("$value")
            if not value:
                raise KeyError(f"Missing Primitives/Default.Colours.{family}.{level}")
            return to_lower_hex(value)

        # allow direct hex in braces e.g. {#aabbcc}
        if head.startswith("#"):
            return to_lower_hex(head)

        raise ValueError(f"Unsupported reference root: {ref}")


# ---------------- Semantics flattening ----------------

def flatten_semantics(sem_root: Dict[str, Any]) -> List[Tuple[str, str]]:
    """
    Walks the Semantics tree and collects leaves with "$value".
    Key name is concatenation of path segments (PascalCase-ish by JSON keys).
    Returns list of (KeyName, "$value") in encounter order.
    If duplicate KeyName appears later, it overrides earlier one.
    """
    ordered: List[Tuple[str, str]] = []
    index_by_name: Dict[str, int] = {}

    def walk(node: Any, path: List[str]) -> None:
        if isinstance(node, dict) and "$value" in node:
            name = "".join(path)
            val = node["$value"]
            if name in index_by_name:
                ordered[index_by_name[name]] = (name, val)
            else:
                index_by_name[name] = len(ordered)
                ordered.append((name, val))
            return

        if isinstance(node, dict):
            for k, v in node.items():
                if k.startswith("$"):
                    continue
                walk(v, path + [k])

    for k, v in sem_root.items():
        walk(v, [k])

    return ordered


# ---------------- Generators ----------------

def gen_color_semantics_xaml(pairs: List[Tuple[str, str]], resolver: TokenResolver, ns: str) -> str:
    stamp = ts()
    lines: List[str] = []
    lines.append(f"<!-- Auto-generated {stamp} -->")
    lines.append('<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"')
    lines.append('                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"')
    lines.append(f'                    xmlns:c="clr-namespace:{ns}">')
    lines.append("")

    # Base SolidColorBrush keys
    for name, ref in pairs:
        hexv = resolver.resolve_value_to_hex(ref)
        lines.append(f'    <SolidColorBrush x:Key="{name}">{hexv}</SolidColorBrush>')

    lines.append("")

    # ComponentResourceKey mirrors
    template = (
        '    <SolidColorBrush x:Key="{{ComponentResourceKey TypeInTargetAssembly={{x:Type c:ColorKeys}}, '
        'ResourceId={{x:Static c:EColorKeys.{key}}}}}" '
        'Color="{{Binding Color, Source={{StaticResource {key}}}}}" />'
    )
    for name, _ in pairs:
        lines.append(template.format(key=name))

    lines.append("</ResourceDictionary>")
    return "\n".join(lines) + "\n"


def gen_color_generator_cs(pairs: List[Tuple[str, str]], ns: str) -> str:
    enum_items = [name for name, _ in pairs]
    enum_body = ",\n        ".join(enum_items)

    map_lines: List[str] = []
    for name in enum_items:
        map_lines.append(f'            map[EColorKeys.{name}] = Resolve(nameof(EColorKeys.{name}));')
    map_body = "\n".join(map_lines)

    return f"""using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace {ns}
{{
    internal static class ColorGenerator
    {{
        private static readonly Uri SemanticsUri =
            new Uri("/Presentation.Styles;component/ColorTokens/ColorSemantics.xaml", UriKind.Relative);

        // enum 이름과 XAML의 x:Key가 1:1로 동일하다는 전제
        private static readonly string[] KeyNames = Enum.GetNames(typeof(EColorKeys));

        public static Dictionary<EColorKeys, Brush> Generate()
        {{
            var map = new Dictionary<EColorKeys, Brush>(capacity: KeyNames.Length);
            ResourceDictionary? fallbackDict = null;

            SolidColorBrush Resolve(string key)
            {{
                // 1) 앱 리소스(병합 포함) 우선
                if (Application.Current is not null)
                {{
                    if (Application.Current.TryFindResource(key) is SolidColorBrush b1)
                        return b1;
                }}

                // 2) 없으면 패키지 XAML을 로드해서 조회
                fallbackDict ??= (ResourceDictionary)Application.LoadComponent(SemanticsUri);
                if (fallbackDict.Contains(key) && fallbackDict[key] is SolidColorBrush b2)
                    return b2;

                // 3) 모든 경로에서 못 찾으면 투명 (디버깅용으로 바꾸고 싶으면 여기 색만 변경)
                return new SolidColorBrush(Colors.Transparent);
            }}

{map_body}

            return map;
        }}
    }}

    public static class ColorKeys
    {{
    }}

    public enum EColorKeys
    {{
        {enum_body}
    }}
}}
"""


# ---------------- Main ----------------

def main() -> None:
    args = parse_args()
    data = read_json(args.input)

    # Accept "Semantics/DIMS" (primary); if absent, try "Semantics"
    semantics = data.get("Semantics/DIMS") or data.get("Semantics")
    if not isinstance(semantics, dict) or not semantics:
        raise SystemExit("Semantics/DIMS not found or empty in input JSON.")

    resolver = TokenResolver(data)
    pairs = flatten_semantics(semantics)  # [(KeyName, "$value" or nested ref)]

    # Generate contents
    xaml = gen_color_semantics_xaml(pairs, resolver, ns=args.namespace)
    cs = gen_color_generator_cs(pairs, ns=args.namespace)

    # Write
    ensure_dir(args.outdir)
    xaml_path = os.path.join(args.outdir, "ColorSemantics.xaml")
    cs_path = os.path.join(args.outdir, "ColorGenerator.cs")

    with open(xaml_path, "w", encoding="utf-8") as f:
        f.write(xaml)
    with open(cs_path, "w", encoding="utf-8") as f:
        f.write(cs)

    print(f"[ok] Wrote: {xaml_path}")
    print(f"[ok] Wrote: {cs_path}")


if __name__ == "__main__":
    main()
