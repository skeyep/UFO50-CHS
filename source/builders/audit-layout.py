#!/usr/bin/env python3
"""按 UFO 50 原文本布局字段审计简体中文像素宽度与行数。"""

from __future__ import annotations

import argparse
import base64
import csv
import json
import math
import re
from pathlib import Path

from fontTools.ttLib import TTFont


LAYOUT_SUFFIX = re.compile(r"_(?:lim|wl|wc)$")
NO_LINE_START = set("，。！？；：、）》】」』…")
NO_LINE_END = set("（《【「『")


def decode_official_json(path: Path) -> dict[str, str]:
    encoded = path.read_text(encoding="ascii").strip()
    decoded = base64.b64decode(encoded).decode("utf-8")
    decoded = re.sub(r",\s*}\s*$", "\n}", decoded)
    return json.loads(decoded)


class FontMeasure:
    def __init__(self, path: Path, point_size: int, dpi: int) -> None:
        font = TTFont(path, lazy=True)
        self.upm = font["head"].unitsPerEm
        self.cmap = font.getBestCmap()
        self.metrics = font["hmtx"].metrics
        # GameMaker font_add() takes a point size, not a pixel size. On the
        # Windows runner, 8pt at 96 DPI rasterizes at roughly 10.67px.
        self.pixel_size = point_size * dpi / 72

    def char_width(self, char: str) -> int:
        glyph = self.cmap.get(ord(char))
        if glyph is None:
            return 8
        advance = self.metrics[glyph][0] * self.pixel_size / self.upm
        return max(1, math.floor(advance + 0.5))

    def width(self, text: str) -> int:
        return sum(self.char_width(char) for char in text)


def wrap_pixels(text: str, width: int, measure: FontMeasure) -> list[str]:
    lines: list[str] = []
    line = ""
    normalized = text.replace("\r\n", "\n").replace("\r", "\n")
    tokens: list[str] = []
    index = 0
    while index < len(normalized):
        char = normalized[index]
        if char == "*":
            end = index + 1
            while end < len(normalized) and normalized[end] == "*":
                end += 1
            tokens.append(normalized[index:end])
            index = end
            continue
        if char in "{[":
            closer = "}" if char == "{" else "]"
            end = normalized.find(closer, index + 1)
            if end >= 0:
                tokens.append(normalized[index : end + 1])
                index = end + 1
                continue
        tokens.append(char)
        index += 1

    for char in tokens:
        if char == "\n":
            lines.append(line)
            line = ""
            continue
        if char == " " and not line:
            continue
        candidate = line + char
        if line and measure.width(candidate) > width:
            next_line = "" if char == " " else char
            if len(char) == 1 and char != " " and len(line) > 1 and (char in NO_LINE_START or line[-1] in NO_LINE_END):
                next_line = line[-1] + char
                line = line[:-1]
            lines.append(line)
            line = next_line
        else:
            line = candidate
    if line or not lines:
        lines.append(line)
    return lines


def explicit_lines(text: str) -> int:
    return len(text.replace("\r\n", "\n").replace("\r", "\n").split("\n"))


def layout_value(data: dict[str, str], key: str, suffix: str) -> int | None:
    value = data.get(f"{key}_{suffix}", data.get(f"default_{suffix}"))
    if value is None:
        return None
    try:
        return int(float(value))
    except (TypeError, ValueError):
        return None


def extract_meta(lines: list[str]) -> dict[str, str]:
    assignment = re.compile(
        r'global\.TEXT_META(?:\.([A-Za-z0-9_]+)|\[\$\s*"([^"]+)"\])\s*=\s*("(?:\\.|[^"\\])*");'
    )
    result: dict[str, str] = {}
    for line in lines:
        match = assignment.search(line)
        if match:
            result[match.group(1) or match.group(2)] = json.loads(match.group(3))
    return result


def append_layout_rows(
    rows: list[dict[str, object]],
    game_id: int,
    english: dict[str, str],
    japanese: dict[str, str],
    chinese: dict[str, str],
    measure: FontMeasure,
    key_prefix: str | None = None,
) -> None:
    for key, zh_value in chinese.items():
        if key_prefix is not None and not key.startswith(key_prefix):
            continue
        if LAYOUT_SUFFIX.search(key) or not isinstance(zh_value, str):
            continue
        if key not in english or not isinstance(english[key], str):
            continue
        wc = layout_value(english, key, "wc")
        wl = layout_value(english, key, "wl")
        if not wc or not wl or wc <= 0 or wl <= 0:
            continue
        box_width = wc * 8
        zh_lines = wrap_pixels(zh_value, box_width, measure)
        rows.append(
            {
                "game_id": game_id,
                "key": key,
                "box_px": box_width,
                "max_lines": wl,
                "english_explicit_lines": explicit_lines(english[key]),
                "japanese_explicit_lines": explicit_lines(str(japanese.get(key, ""))),
                "chinese_explicit_lines": explicit_lines(zh_value),
                "chinese_predicted_lines": len(zh_lines),
                "overflow_lines": max(0, len(zh_lines) - wl),
                "english": english[key].replace("\n", "\\n"),
                "japanese": str(japanese.get(key, "")).replace("\n", "\\n"),
                "chinese": zh_value.replace("\n", "\\n"),
                "chinese_wrapped": "\\n".join(zh_lines),
            }
        )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--font", type=Path)
    parser.add_argument("--font-size", type=int, default=8)
    parser.add_argument("--dpi", type=int, default=96)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()

    root = args.root.resolve()
    font_path = (
        args.font or root / "font-candidates" / "fonts" / "zpix-original-personal-only.ttf"
    ).resolve()
    output = (args.output or root / "chs-review" / "layout-audit.tsv").resolve()
    measure = FontMeasure(font_path, args.font_size, args.dpi)

    english_dir = root / "ext" / "ENGLISH"
    japanese_dir = root / "reference" / "JAPANESE-original"
    chinese_dir = root / "chs-tools" / "staging" / "JAPANESE"
    rows: list[dict[str, object]] = []

    for english_path in sorted(english_dir.glob("*_Text.json"), key=lambda item: int(item.stem.split("_")[0])):
        game_id = int(english_path.stem.split("_")[0])
        japanese_path = japanese_dir / english_path.name
        chinese_path = chinese_dir / english_path.name
        if not japanese_path.exists() or not chinese_path.exists():
            continue
        english = decode_official_json(english_path)
        japanese = decode_official_json(japanese_path)
        chinese = decode_official_json(chinese_path)
        append_layout_rows(rows, game_id, english, japanese, chinese, measure)

    meta_gml = root / "chs-tools" / "all-code" / "CodeEntries" / "gml_GlobalScript_scrLoadInternalText.gml"
    meta_lines = meta_gml.read_text(encoding="utf-8").splitlines()
    english_meta = extract_meta(meta_lines[2:5923])
    japanese_meta = extract_meta(meta_lines[35528:])
    chinese_meta = json.loads((chinese_dir / "m_Text.json").read_text(encoding="utf-8"))
    append_layout_rows(rows, 51, english_meta, japanese_meta, chinese_meta, measure, "game_51_")

    output.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = list(rows[0]) if rows else []
    with output.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames, delimiter="\t")
        writer.writeheader()
        writer.writerows(rows)

    overflow = [row for row in rows if row["overflow_lines"]]
    print(f"字体：{font_path}")
    print(f"GameMaker 字号：{args.font_size}pt @ {args.dpi} DPI（约 {measure.pixel_size:.2f}px/em）")
    print(f"布局字段文本：{len(rows)}")
    print(f"预测超过原版最大行数：{len(overflow)}")
    print(f"报告：{output}")
    for row in sorted(overflow, key=lambda item: (-int(item["overflow_lines"]), int(item["game_id"]), str(item["key"])))[:30]:
        print(
            f"ID {row['game_id']:02} {row['key']}: "
            f"{row['chinese_predicted_lines']}/{row['max_lines']} 行，{row['box_px']}px"
        )
    return 1 if overflow else 0


if __name__ == "__main__":
    raise SystemExit(main())

