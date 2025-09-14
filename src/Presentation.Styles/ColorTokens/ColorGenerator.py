#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
HYSoft Color Codegen

- Input default: tokens.json  (override with --input)
- Output files: ColorSemantics.xaml, ColorGenerator.cs  (--outdir)
- Namespace default: HYSoft.Presentation.Styles.ColorTokens (--namespace)

Rules
- EColorKeys = 모든 "Semantics/*" 트리의 leaf("$value")를 경로 결합(PascalCase)한 키
  예) Button.Primary.Surface => ButtonPrimarySurface
      Table.Primary.SurfaceTitle => TablePrimarySurfaceTitle
- 색상 해석:
  {Primary.N} -> ColorTokens/* .Primary.N -> (보통) {Colours.Slate.N} -> Primitives/* .Colours.Slate.N -> #hex
  {Colours.Family.Level} -> Primitives/* .Colours.Family.Level -> #hex
  #hex 직접값도 허용
- ColorSemantics.xaml:
    1) <SolidColorBrush x:Key="KeyName">#hex</SolidColorBrush>
    2) <SolidColorBrush x:Key="{ComponentResourceKey ... ResourceId={x:Static c:EColorKeys.KeyName}}"
         Color="{Binding Color, Source={StaticResource KeyName}}"/>
- ColorGenerator.cs:
    앱 리소스 우선(TryFindResource) → 없으면 LoadComponent
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
    e.g., base_key="Primitives" will merge "Primitives", "Primitives/Default", "Primitives/Alt", ...
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


# ---------------- Token resolution ----------------

class TokenResolver:
    """
    Resolves values like:
      - {Primary.600} -> ColorTokens/* .Primary.600 -> (likely) {Colours.Slate.600} -> hex
      - {Colours.Slate.200} -> Primitives/* .Colours.Slate.200 -> hex
      - #rrggbb (direct)
    """

    def __init__(self, root: Dict[str, Any]) -> None:
        self.root = root
        self.primitives = collect_any_section(root, "Primitives")     # expects Colours/White/Black/Scales...
        self.tokens = collect_any_section(root, "ColorTokens")        # expects Primary/...

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
                raise KeyError(f"Missing ColorTokens/* .Primary.{level}")
            return self.resolve_value_to_hex(value)

        if head == "Colours":
            if len(parts) != 3:
                raise ValueError(f"Colours reference must be Colours.<Family>.<Level>: {ref}")
            family, level = parts[1], parts[2]
            fam = self.primitives.get("Colours", {}).get(family, {})
            entry = fam.get(level, {})
            value = entry.get("$value")
            if not value:
                raise KeyError(f"Missing Primitives/* .Colours.{family}.{level}")
            return to_lower_hex(value)

        # allow direct hex in braces e.g. {#aabbcc}
        if head.startswith("#"):
            return to_lower_hex(head)

        raise ValueError(f"Unsupported reference root: {ref}")


# ---------------- Semantics collection & flattening ----------------

def collect_semantics(root: Dict[str, Any]) -> Dict[str, Any]:
    """
    Accept any top-level key "Semantics" or "Semantics/<suffix>".
    If multiple present, deep-merge them (later ones override earlier).
    """
    return collect_any_section(root, "Semantics") or {}


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
    lines.append("<!-- Auto-generated " + stamp + " -->")
    lines.append('<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"')
    lines.append('                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"')
    lines.append('                    xmlns:c="clr-namespace:' + ns + '">')
    lines.append("")

    # Base SolidColorBrush keys
    for name, ref in pairs:
        hexv = resolver.resolve_value_to_hex(ref)
        lines.append('    <SolidColorBrush x:Key="' + name + '">' + hexv + '</SolidColorBrush>')

    lines.append("")

    # ComponentResourceKey mirrors (문자열 연결로 중괄호 이스케이프 이슈 회피)
    for name, _ in pairs:
        lines.append(
            '    <SolidColorBrush x:Key="{ComponentResourceKey TypeInTargetAssembly={x:Type c:ColorKeys}, '
            'ResourceId={x:Static c:EColorKeys.' + name + '}}" '
            'Color="{Binding Color, Source={StaticResource ' + name + '}}" />'
        )

    lines.append("</ResourceDictionary>")
    return "\n".join(lines)


def gen_color_generator_cs(pairs: List[Tuple[str, str]], ns: str) -> str:
    # enum items
    enum_items = [name for name, _ in pairs]

    # build C# file without format/f-strings to avoid brace escaping
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
    lines.append('        private static readonly Uri SemanticsUri =')
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
        lines.append("            map[EColorKeys." + name + "] = Resolve(nameof(EColorKeys." + name + "));")
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
        lines.append("        " + name + comma)
    lines.append("    }")
    lines.append("}")
    lines.append("")
    return "\n".join(lines)


# ---------------- Main ----------------

def main() -> None:
    args = parse_args()
    data = read_json(args.input)

    # 1) Semantics 수집 (Semantics 또는 Semantics/* 모두 허용, 다수면 병합)
    sem = collect_semantics(data)
    if not sem:
        raise SystemExit("No 'Semantics' found (accepts 'Semantics' or 'Semantics/*').")

    # 2) 평탄화 → [(KeyName, "$value")]
    pairs = flatten_semantics(sem)

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
