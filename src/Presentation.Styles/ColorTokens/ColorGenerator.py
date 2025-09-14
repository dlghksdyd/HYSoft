#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
HYSoft Color Codegen

Reads a design-tokens JSON (default: tokens.json) and generates:
  - ColorSemantics.xaml
  - ColorGenerator.cs (includes ColorKeys class and EColorKeys enum)

Assumptions:
- EColorKeys is derived from Semantics (e.g., Semantics/DIMS).
- Semantics color values can reference:
    {Primary.600}  -> ColorTokens/DIMS.Primary.600 -> {Colours.Slate.600} -> hex
    {Colours.Slate.200} -> Primitives/Default.Colours.Slate.200 -> hex
- Generated XAML matches the requested structure:
    1) <SolidColorBrush x:Key="...">#hex</SolidColorBrush>
    2) <SolidColorBrush x:Key="{ComponentResourceKey ...}" Color="{Binding Color, Source={StaticResource ...}}"/>

Usage:
  python gen_colors.py
  python gen_colors.py --input design/tokens.json --outdir ./Presentation.Styles/ColorTokens
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
    ap.add_argument("--input", "-i", default="tokens.json", help="Input tokens JSON file (default: tokens.json)")
    ap.add_argument("--outdir", "-o", default=".", help="Output directory (default: current dir)")
    return ap.parse_args()

# ---------------- Utils ----------------

def ts() -> str:
    # e.g., 2025-09-14 00:33:17
    return datetime.now().strftime("%Y-%m-%d %H:%M:%S")

def ensure_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)

def read_json(path: str) -> Dict[str, Any]:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def to_lower_hex(s: str) -> str:
    s = s.strip()
    # normalize like "#AABBCC" or "#ffaabbcc" → lower
    return s.lower()

# ---------------- Token resolution ----------------

class TokenResolver:
    """
    Resolves:
      {Primary.600}         -> ColorTokens/DIMS -> nested reference -> hex
      {Colours.Slate.200}   -> Primitives/Default -> hex
    """

    def __init__(self, root: Dict[str, Any]) -> None:
        self.root = root

        # Shortcuts
        self.primitives = root.get("Primitives/Default", {})
        self.semantics = root.get("Semantics/DIMS", {})
        self.tokens = root.get("ColorTokens/DIMS", {})

    def resolve_ref(self, ref: str) -> str:
        """
        Resolve a reference like "{Primary.600}" or "{Colours.Slate.200}" to a hex color "#rrggbb" (lowercase).
        """
        ref = ref.strip()
        if ref.startswith("{") and ref.endswith("}"):
            ref = ref[1:-1].strip()

        parts = ref.split(".")
        if not parts:
            raise ValueError(f"Bad reference: {ref}")

        if parts[0] == "Primary":
            # Go through ColorTokens/DIMS -> Primary -> level -> value (which could itself be {Colours.Slate.n})
            level = parts[1] if len(parts) > 1 else None
            if not level:
                raise ValueError(f"Primary reference missing level: {ref}")
            primary_entry = self.tokens.get("Primary", {}).get(level, {})
            value = primary_entry.get("$value")
            if not value:
                raise KeyError(f"Missing ColorTokens/DIMS.Primary.{level} in tokens.")
            return self._resolve_value(value)

        elif parts[0] == "Colours":
            # Primitives/Default -> Colours -> Family -> Level
            if len(parts) != 3:
                raise ValueError(f"Colours reference must be Colours.<Family>.<Level>: {ref}")
            family, level = parts[1], parts[2]
            fam = self.primitives.get("Colours", {}).get(family, {})
            entry = fam.get(level, {})
            value = entry.get("$value")
            if not value:
                raise KeyError(f"Missing Primitives/Default.Colours.{family}.{level} in tokens.")
            return to_lower_hex(value)

        else:
            # Could be direct HEX "#xxxxxx" or unsupported path
            if ref.startswith("#"):
                return to_lower_hex(ref)
            raise ValueError(f"Unsupported reference root: {ref}")

    def _resolve_value(self, val: str) -> str:
        """
        val may be:
          - direct hex "#aabbcc"
          - nested reference "{Colours.Slate.600}"
        """
        val = val.strip()
        if val.startswith("{") and val.endswith("}"):
            return to_lower_hex(self.resolve_ref(val))
        if val.startswith("#"):
            return to_lower_hex(val)
        # Unexpected type
        raise ValueError(f"Unsupported value format: {val}")

# ---------------- Semantics flattening ----------------

def flatten_semantics(sem: Dict[str, Any]) -> List[Tuple[str, str]]:
    """
    Flatten semantics tree into (KeyName, RefString) pairs in document order.
    Example:
      ButtonPrimary.Surface -> {Primary.100}  ==> ("ButtonPrimarySurface", "{Primary.100}")
      TablePrimary.Surface.Title -> {Primary.700} ==> ("TablePrimarySurfaceTitle", "{Primary.700}")
    Only leaves (objects that contain "$value") are collected.
    """
    out: List[Tuple[str, str]] = []

    def walk(node: Any, path: List[str]) -> None:
        if isinstance(node, dict) and "$value" in node:
            # Leaf
            key_name = "".join(path)
            out.append((key_name, node["$value"]))
            return
        if isinstance(node, dict):
            for k, v in node.items():
                # skip metadata-y keys that aren't nested colors
                if k.startswith("$"):
                    continue
                walk(v, path + [k])

    # the top structure (e.g., {"ButtonPrimary": {...}, "TablePrimary": {...}})
    for k, v in sem.items():
        walk(v, [k])

    return out

# ---------------- Generators ----------------

def gen_color_semantics_xaml(pairs: List[Tuple[str, str]], resolver: TokenResolver, ns: str = "HYSoft.Presentation.Styles.ColorTokens") -> str:
    """
    Generates:
      <!-- Auto-generated TIMESTAMP -->
      <ResourceDictionary ... xmlns:c="clr-namespace:HYSoft.Presentation.Styles.ColorTokens">
          <SolidColorBrush x:Key="Name">#hex</SolidColorBrush>
          ...
          <SolidColorBrush x:Key="{ComponentResourceKey ... ResourceId={x:Static c:EColorKeys.Name}}"
                           Color="{Binding Color, Source={StaticResource Name}}" />
      </ResourceDictionary>
    """
    stamp = ts()
    lines: List[str] = []
    lines.append(f"<!-- Auto-generated {stamp} -->")
    lines.append('<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"')
    lines.append('                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"')
    lines.append(f'                    xmlns:c="clr-namespace:{ns}">')
    lines.append("")

    # Base SolidColorBrush keys
    for name, ref in pairs:
        hexv = resolver._resolve_value(ref)
        lines.append(f'    <SolidColorBrush x:Key="{name}">{hexv}</SolidColorBrush>')

    lines.append("")

    # ComponentResourceKey mirrors
    for name, _ in pairs:
        lines.append(
            '    <SolidColorBrush x:Key="{ComponentResourceKey TypeInTargetAssembly={x:Type c:ColorKeys}, ResourceId={x:Static c:EColorKeys.' + name + '}}" '
            f'Color="{{Binding Color, Source={{StaticResource {name}}}}}" />'
        )

    lines.append("</ResourceDictionary>")
    return "\n".join(lines) + "\n"

def gen_color_generator_cs(pairs: List[Tuple[str, str]], ns: str = "HYSoft.Presentation.Styles.ColorTokens") -> str:
    """
    Generates ColorGenerator.cs in the requested shape.
    """
    stamp = ts()
    enum_items = [name for name, _ in pairs]

    # Build enum body
    enum_body = ",\n        ".join(enum_items)

    # Build mapping section
    map_lines: List[str] = []
    for name in enum_items:
        map_lines.append(f'            map[EColorKeys.{name}] = Resolve(nameof(EColorKeys.{name}));')
    map_body = "\n".join(map_lines)

    return f"""// Auto-generated {stamp}
using System;
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

# ---------------- Main flow ----------------

def main() -> None:
    args = parse_args()
    data = read_json(args.input)

    # Prepare resolver
    resolver = TokenResolver(data)

    # 1) Get semantics (EColorKeys source)
    sem = data.get("Semantics/DIMS", {})
    if not isinstance(sem, dict) or not sem:
        raise SystemExit("Semantics/DIMS not found or empty in input JSON.")

    pairs = flatten_semantics(sem)  # list of (KeyName, "$value")

    # 2) Generate outputs
    xaml = gen_color_semantics_xaml(pairs, resolver)
    cs = gen_color_generator_cs(pairs)

    # 3) Write files
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
