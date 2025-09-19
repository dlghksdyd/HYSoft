#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
HYSoft Color Codegen (tokens.json -> ColorSemantics.xaml, ColorGenerator.cs)

- Input default: tokens.json  (override with --input)
- Output files: ColorSemantics.xaml, ColorGenerator.cs  (--outdir)
- Namespace default: HYSoft.Presentation.Styles.ColorTokens (--namespace)

Rules
- EColorKeys = 모든 "ColorSemantics/*" 트리의 leaf("value") 경로를 PascalCase로 결합한 키
  예) Button -> Primary -> fg  => ButtonPrimaryFg
      Navigation -> Sidebar -> item -> bg -> active => NavigationSidebarItemBgActive
- 색상 해석:
  {TokenFamily.Level}    -> ColorTokens/* .<TokenFamily>.<Level> -> (대개) {PrimitiveFamily.Level} -> ColorPrimitives/* .<PrimitiveFamily>.<Level> -> #hex
  {PrimitiveFamily.Level} -> ColorPrimitives/* .<PrimitiveFamily>.<Level> -> #hex
  {#aabbcc} / #aabbcc 허용
"""

from __future__ import annotations
import argparse
import json
import os
import re
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


def deep_merge(a: Dict[str, Any], b: Dict[str, Any]) -> Dict[str, Any]:
    """dict-deep merge (b overrides a)."""
    out = dict(a)
    for k, v in b.items():
        if k in out and isinstance(out[k], dict) and isinstance(v, dict):
            out[k] = deep_merge(out[k], v)
        else:
            out[k] = v
    return out


def collect_any_section(root: Dict[str, Any], base_key: str) -> Dict[str, Any]:
    """
    Accept top-level keys == base_key or base_key/* and deep-merge them.
    e.g., base_key="ColorPrimitives" will merge "ColorPrimitives", "ColorPrimitives/Default", ...
    """
    merged: Dict[str, Any] = {}
    found = False
    for k, v in root.items():
        if not isinstance(k, str):
            continue
        if k == base_key or k.startswith(base_key + "/"):
            if isinstance(v, dict):
                merged = deep_merge(merged, v)
                found = True
    if not found:
        return {}
    return merged


def to_pascal(segment: str) -> str:
    """Convert a key segment to PascalCase, preserving inner camelCase (just capitalize first char)."""
    if not segment:
        return ""
    # split on non-alnum just in case (rare)
    parts = re.split(r"[^0-9A-Za-z]+", segment)
    out = []
    for p in parts:
        if not p:
            continue
        out.append(p[0].upper() + p[1:])
    return "".join(out)


# ---------------- Token resolution ----------------

class TokenResolver:
    """
    Resolves values like:
      - {Primary.600} -> ColorTokens/* .Primary.600 -> (likely) {Blue.600} -> hex
      - {Blue.200}    -> ColorPrimitives/* .Blue.200 -> hex
      - {White}       -> ColorTokens/* .White.value  or  ColorPrimitives/* .White.value -> hex
      - {#aabbcc} or #aabbcc (direct)
    """

    def __init__(self, root: Dict[str, Any]) -> None:
        self.root = root
        self.primitives = collect_any_section(root, "ColorPrimitives")   # Berry/Blue/Gray/.../White/Black
        self.tokens = collect_any_section(root, "ColorTokens")           # Primary/Neutral/Secondary/... + White/Black

    def resolve_value_to_hex(self, val: str) -> str:
        v = val.strip()
        if v.startswith("#"):
            return to_lower_hex(v)
        if v.startswith("{") and v.endswith("}"):
            inner = v[1:-1].strip()
            if inner.startswith("#"):
                return to_lower_hex(inner)
            return self._resolve_ref(inner)
        raise ValueError(f"Unsupported value format: {val}")

    def _resolve_ref(self, ref: str) -> str:
        parts = ref.split(".")

        # --- Case A: single-part reference, e.g. {White}, {Black} ---
        if len(parts) == 1:
            family = parts[0]

            # 1) Try ColorTokens.<Family>.value
            tok_node = self.tokens.get(family)
            if isinstance(tok_node, dict) and ("value" in tok_node or "$value" in tok_node):
                v = tok_node.get("value") or tok_node.get("$value")
                return self.resolve_value_to_hex(v)

            # 2) Try ColorPrimitives.<Family>.value
            prim_node = self.primitives.get(family)
            if isinstance(prim_node, dict) and ("value" in prim_node or "$value" in prim_node):
                v = prim_node.get("value") or prim_node.get("$value")
                # 허용: direct hex 또는 다시 참조
                return self.resolve_value_to_hex(v)

            # 3) 팔레트형(50~900)만 있는 패밀리를 단일 참조로 부르면 에러
            raise KeyError(f"Unknown single-part reference: {ref} (no direct 'value' in ColorTokens/ColorPrimitives)")

        # --- Case B: two-part reference, e.g. {Primary.600}, {Blue.50} ---
        if len(parts) == 2:
            family, level = parts[0], parts[1]

            # 1) Try ColorTokens family
            tok_family = self.tokens.get(family)
            if isinstance(tok_family, dict):
                entry = tok_family.get(level)
                if isinstance(entry, dict):
                    value = entry.get("value") or entry.get("$value")
                    if not value:
                        raise KeyError(f"Missing ColorTokens.{family}.{level}.value")
                    return self.resolve_value_to_hex(value)

            # 2) Try ColorPrimitives family
            prim_family = self.primitives.get(family)
            if isinstance(prim_family, dict):
                # direct value (rare for families with level)
                if "value" in prim_family or "$value" in prim_family:
                    v = prim_family.get("value") or prim_family.get("$value")
                    return self.resolve_value_to_hex(v)
                entry = prim_family.get(level)
                if isinstance(entry, dict):
                    value = entry.get("value") or entry.get("$value")
                    if not value:
                        raise KeyError(f"Missing ColorPrimitives.{family}.{level}.value")
                    # primitives는 보통 hex 직값
                    return to_lower_hex(value)

            raise KeyError(f"Unknown color family/level in reference: {ref}")

        # --- Other formats are invalid ---
        raise ValueError(f"Color reference must be <Family> or <Family>.<Level>: {ref}")


# ---------------- Semantics collection & flattening ----------------

def collect_semantics(root: Dict[str, Any]) -> Dict[str, Any]:
    """Accept any top-level key 'ColorSemantics' or 'ColorSemantics/<suffix>' and deep-merge them."""
    return collect_any_section(root, "ColorSemantics") or {}


def flatten_semantics(sem_root: Dict[str, Any]) -> List[Tuple[str, str]]:
    """
    Walk the Semantics tree and collect leaves with 'value' (string).
    Key name is concatenation of path segments using PascalCase.
    Returns list of (KeyName, valueRef) in encounter order; later duplicates override earlier.
    """
    ordered: List[Tuple[str, str]] = []
    index_by_name: Dict[str, int] = {}

    def walk(node: Any, path: List[str]) -> None:
        # Leaf with "value"
        if isinstance(node, dict) and "value" in node and isinstance(node.get("value"), str):
            name = "".join(to_pascal(p) for p in path)
            val = node["value"]
            if name in index_by_name:
                ordered[index_by_name[name]] = (name, val)
            else:
                index_by_name[name] = len(ordered)
                ordered.append((name, val))
            return

        # Recurse into dict children (skip meta keys)
        if isinstance(node, dict):
            for k, v in node.items():
                if k.startswith("$"):
                    continue
                if k == "type" or k == "value":
                    continue
                walk(v, path + [k])

    for k, v in sem_root.items():
        walk(v, [k])

    return ordered


# ---------------- Generators ----------------

def gen_color_semantics_xaml(pairs: List[Tuple[str, str]], resolver: TokenResolver, ns: str) -> str:
    stamp = ts()
    lines: List[str] = []
    lines.append("<!-- Auto-generated " + stamp + " -->")
    lines.append('<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"')
    lines.append('                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"')
    lines.append('                    xmlns:c="clr-namespace:' + ns + '">')
    lines.append("")

    # Base SolidColorBrush keys
    for name, ref in pairs:
        hexv = resolver.resolve_value_to_hex(ref)
        lines.append(f'    <SolidColorBrush x:Key="{name}">{hexv}</SolidColorBrush>')

    lines.append("")

    # ComponentResourceKey mirrors
    for name, _ in pairs:
        lines.append(
            f'    <SolidColorBrush x:Key="{{ComponentResourceKey TypeInTargetAssembly={{x:Type c:ColorKeys}}, '
            f'ResourceId={{x:Static c:EColorKeys.{name}}}}}" '
            f'Color="{{Binding Color, Source={{StaticResource {name}}}}}" />'
        )

    lines.append("</ResourceDictionary>")
    return "\n".join(lines)


def gen_color_generator_cs(pairs: List[Tuple[str, str]], ns: str) -> str:
    enum_items = [name for name, _ in pairs]

    lines: List[str] = []
    lines.append("using System;")
    lines.append("using System.Collections.Generic;")
    lines.append("using System.Windows;")
    lines.append("using System.Windows.Media;")
    lines.append("")
    lines.append("namespace " + ns)
    lines.append("{")
    lines.append("    internal static class ColorGenerator")
    lines.append("    {")
    lines.append("        private static readonly Uri SemanticsUri =")
    lines.append('            new Uri("/Presentation.Styles;component/ColorTokens/ColorSemantics.xaml", UriKind.Relative);')
    lines.append("")
    lines.append("        // enum 이름과 XAML의 x:Key가 1:1로 동일하다는 전제")
    lines.append("        private static readonly string[] KeyNames = Enum.GetNames(typeof(EColorKeys));")
    lines.append("")
    lines.append("        public static Dictionary<EColorKeys, Brush> Generate()")
    lines.append("        {")
    lines.append("            var map = new Dictionary<EColorKeys, Brush>(capacity: KeyNames.Length);")
    lines.append("            ResourceDictionary? fallbackDict = null;")
    lines.append("")
    lines.append("            SolidColorBrush Resolve(string key)")
    lines.append("            {")
    lines.append("                // 1) 앱 리소스(병합 포함) 우선")
    lines.append("                if (Application.Current is not null)")
    lines.append("                {")
    lines.append("                    if (Application.Current.TryFindResource(key) is SolidColorBrush b1)")
    lines.append("                        return b1;")
    lines.append("                }")
    lines.append("")
    lines.append("                // 2) 없으면 패키지 XAML을 로드해서 조회")
    lines.append("                fallbackDict ??= (ResourceDictionary)Application.LoadComponent(SemanticsUri);")
    lines.append("                if (fallbackDict.Contains(key) && fallbackDict[key] is SolidColorBrush b2)")
    lines.append("                    return b2;")
    lines.append("")
    lines.append("                // 3) 모든 경로에서 못 찾으면 투명")
    lines.append("                return new SolidColorBrush(Colors.Transparent);")
    lines.append("            }")
    lines.append("")
    for name in enum_items:
        lines.append(f"            map[EColorKeys.{name}] = Resolve(nameof(EColorKeys.{name}));")
    lines.append("")
    lines.append("            return map;")
    lines.append("        }")
    lines.append("    }")
    lines.append("")
    lines.append("    public static class ColorKeys")
    lines.append("    {")
    lines.append("    }")
    lines.append("")
    lines.append("    public enum EColorKeys")
    lines.append("    {")
    for i, name in enumerate(enum_items):
        comma = "," if i < len(enum_items) - 1 else ""
        lines.append(f"        {name}{comma}")
    lines.append("    }")
    lines.append("}")
    lines.append("")
    return "\n".join(lines)


# ---------------- Main ----------------

def main() -> None:
    args = parse_args()
    data = read_json(args.input)

    # 1) Semantics 수집 (ColorSemantics 또는 ColorSemantics/* 병합)
    sem = collect_semantics(data)
    if not sem:
        raise SystemExit("No 'ColorSemantics' found (accepts 'ColorSemantics' or 'ColorSemantics/*').")

    # 2) 평탄화 → [(KeyName, "value")]
    pairs = flatten_semantics(sem)
    if not pairs:
        raise SystemExit("No semantic leaves with 'value' found under 'ColorSemantics'.")

    # 3) 생성
    resolver = TokenResolver(data)
    xaml = gen_color_semantics_xaml(pairs, resolver, ns=args.namespace)
    cs = gen_color_generator_cs(pairs, ns=args.namespace)

    # 4) 저장
    ensure_dir(args.outdir)
    xaml_path = os.path.join(args.outdir, "ColorSemantics.xaml")
    cs_path = os.path.join(args.outdir, "ColorGenerator.cs")

    with open(xaml_path, "w", encoding="utf-8") as f:
        f.write(xaml)
    with open(cs_path, "w", encoding="utf-8") as f:
        f.write(cs)

    print("[ok] Wrote:", xaml_path)
    print("[ok] Wrote:", cs_path)


if __name__ == "__main__":
    main()
