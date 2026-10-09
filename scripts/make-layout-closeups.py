"""Find geometric text/cursor candidates and save nearest-neighbour JPG evidence.

Text metrics and sprite opaque bounding boxes propose areas for visual review.
Missing traces, unresolved sprite frames and uniform screenshots remain explicit
evidence gaps even when no geometric intersection is found.
"""
import argparse
import json
import math
import re
import subprocess
import sys
from pathlib import Path
from PIL import Image, ImageDraw


def screen_box(row, image):
    gui = row.get('gui', False)
    width = row['guiWidth'] if gui else row['viewWidth']
    height = row['guiHeight'] if gui else row['viewHeight']
    if not width or not height:
        return None
    sx, sy = image.width / width, image.height / height
    x = row['x'] - (0 if gui else row['viewX'])
    y = row['y'] - (0 if gui else row['viewY'])
    return [x * sx, y * sy, (x + row['width']) * sx, (y + row['height']) * sy]


def intersection(a, b):
    box = [max(a[0], b[0]), max(a[1], b[1]), min(a[2], b[2]), min(a[3], b[3])]
    return box if box[2] > box[0] and box[3] > box[1] else None


def audit(trace, metadata):
    screenshot = trace.with_name(trace.name.replace('-texts.json', '.png'))
    if not screenshot.exists():
        return None
    im = Image.open(screenshot).convert('RGB')
    rows, seen, unresolved, clipped, font_mismatches, zero_metrics = [], set(), [], [], [], []
    for item in json.loads(trace.read_text(encoding='utf-8-sig')):
        if not item.get('text', '').strip() or item.get('alpha', 1) <= 0:
            continue
        mismatch = item.get('currentIsCHS') is False and re.search(r'[\u3400-\u9fff]', item['text'])
        if mismatch:
            font_mismatches.append({'record': item})
        if item['width'] <= 0 or item['height'] <= 0:
            zero_metrics.append(item)
            continue
        box = screen_box(item, im)
        if not box or not intersection(box, [0, 0, *im.size]):
            continue
        if box[0] < 0 or box[1] < 0 or box[2] > im.width or box[3] > im.height:
            clipped.append({'box': box, 'record': item})
        key = (item['text'], tuple(box))
        if key in seen:
            continue
        seen.add(key)
        rows.append({'kind': 'text', 'box': box, 'record': item})
    sprites = trace.with_name(trace.name.replace('-texts.json', '-sprites.json'))
    if sprites.exists():
        for item in json.loads(sprites.read_text(encoding='utf-8-sig')):
            if item.get('alpha', 1) <= 0:
                continue
            asset = metadata.get(item['sprite'])
            if not asset:
                unresolved.append(item)
                continue
            frame = math.floor(item['frame']) % int(asset['frames'])
            opaque_frame = next((r for r in asset['opaque'] if r['frame'] == frame), None)
            if opaque_frame is None:
                unresolved.append(item)
                continue
            opaque = opaque_frame['bbox']
            # Native blinking cursors include fully transparent, exported frames.
            # Such a frame has no visible pixels and therefore no collision region.
            if opaque is None and opaque_frame.get('pixels') == 0:
                continue
            if not opaque:
                unresolved.append(item)
                continue
            angle = math.radians(item.get('rotation', 0))
            cos, sin = math.cos(angle), math.sin(angle)
            corners = []
            for xx, yy in [(opaque[0], opaque[1]), (opaque[2], opaque[1]), (opaque[2], opaque[3]), (opaque[0], opaque[3])]:
                xx = (xx - int(asset['originX'])) * item.get('scaleX', 1)
                yy = (yy - int(asset['originY'])) * item.get('scaleY', 1)
                corners.append((item['x'] + xx * cos + yy * sin, item['y'] - xx * sin + yy * cos))
            left, top = min(p[0] for p in corners), min(p[1] for p in corners)
            right, bottom = max(p[0] for p in corners), max(p[1] for p in corners)
            box = screen_box(dict(item, x=left, y=top, width=right-left, height=bottom-top), im)
            if box and intersection(box, [0, 0, *im.size]):
                rows.append({'kind': 'cursor', 'box': box, 'record': item})
    collisions, overlays = [], []
    for i, a in enumerate(rows):
        for b in rows[i + 1:]:
            if a['kind'] == b['kind'] == 'cursor':
                continue
            hit = intersection(a['box'], b['box'])
            if not hit:
                continue
            result = {'a': a, 'b': b, 'intersection': hit}
            # Fixed-digit masks are reviewed separately with number closeups.
            labels = [r['record'].get('text', '') for r in (a, b)]
            same_numeric_mask = a['box'] == b['box'] and all(r['record'].get('numberFontNonnegative', False) for r in (a, b)) and any(re.fullmatch(r'0+(?::0+)*', value) for value in labels) and all(re.fullmatch(r'[ 0-9:]+', value) for value in labels)
            if a['kind'] == b['kind'] == 'text' and same_numeric_mask:
                overlays.append(result)
            else:
                collisions.append(result)
    focus = []
    choices = [r for r in rows if r['kind'] == 'text' and r['record']['text'].strip().upper() in {'是', '否', 'YES', 'NO', 'はい', 'いいえ'}]
    for choice in choices:
        candidates = [r for r in rows if r != choice and (r['kind']=='cursor' or r['record'].get('text') in {'>', '<', '▶', '→'})]
        if candidates:
            nearest = min(candidates, key=lambda r: abs(r['box'][0]-choice['box'][0])+abs(r['box'][1]-choice['box'][1]))
            if abs(nearest['box'][0]-choice['box'][0]) + abs(nearest['box'][1]-choice['box'][1]) < im.width/6:
                focus.append({'a': choice, 'b': nearest, 'intersection': intersection(choice['box'], nearest['box']), 'review': 'choice-and-cursor'})
    # Inspect every visible cursor even when it does not intersect YES/NO.
    # Glyph cursors and native sprites share the same nearest-label selection.
    cursor_focus = []
    for cursor in rows:
        if cursor['kind'] != 'cursor' and cursor['record'].get('text', '').strip() not in {'>', '<', '▶', '→'}:
            continue
        candidates = [r for r in rows if r['kind'] == 'text' and r != cursor]
        def gap(label):
            a, b = cursor['box'], label['box']
            return max(a[0]-b[2], b[0]-a[2], 0) + max(a[1]-b[3], b[1]-a[3], 0)
        nearest = min(candidates, key=gap) if candidates else cursor
        if nearest != cursor and gap(nearest) > im.width / 6:
            nearest = cursor
        cursor_focus.append({'a': cursor, 'b': nearest,
                             'intersection': intersection(cursor['box'], nearest['box']) if nearest != cursor else None,
                             'review': 'cursor-and-nearest-label',
                             'screenClearance': gap(nearest) if nearest != cursor else None})
    panels = []
    emitted = {}
    for hit in collisions + focus + cursor_focus:
        key = tuple(sorted((tuple(hit['a']['box']), tuple(hit['b']['box']))))
        if key in emitted:
            hit['closeup'], hit['crop'] = emitted[key]
            continue
        index = len(panels)
        a, b = hit['a']['box'], hit['b']['box']
        crop_box = [max(0, math.floor(min(a[0], b[0])-12)), max(0, math.floor(min(a[1], b[1])-12)), min(im.width, math.ceil(max(a[2], b[2])+12)), min(im.height, math.ceil(max(a[3], b[3])+12))]
        crop = im.crop(crop_box)
        zoom = crop.resize((crop.width * 4, crop.height * 4), Image.Resampling.NEAREST)
        panel = Image.new('RGB', (max(zoom.width, 320), zoom.height + 24), '#181818')
        panel.paste(zoom, (0, 24))
        draw = ImageDraw.Draw(panel)
        draw.text((4, 4), f'{screenshot.stem} {hit.get("review", "collision")} {index+1}', fill='white')
        for box, color in [(a, '#00ffff'), (b, '#ffff00')] + ([(hit['intersection'], '#ff4040')] if hit['intersection'] else []):
            draw.rectangle([(box[0]-crop_box[0])*4, (box[1]-crop_box[1])*4+24, (box[2]-crop_box[0])*4-1, (box[3]-crop_box[1])*4+23], outline=color, width=1)
        out = screenshot.with_name(screenshot.stem + f'-layout-zoom-{index+1}.upload.jpg')
        original = out.with_name(out.name.removesuffix('.upload.jpg') + '.png')
        panel.save(original)
        jpeg_tool = Path.home() / '.codex/tools/screenshot_jpeg.py'
        if jpeg_tool.exists():
            subprocess.run([sys.executable, str(jpeg_tool), str(original)], check=True, capture_output=True)
        else:
            panel.save(out, quality=82, subsampling=0, optimize=True)
        hit['closeup'] = out.name
        hit['crop'] = crop_box
        emitted[key] = (out.name, crop_box)
        panels.append(out.name)
    uniform = all(lo == hi for lo, hi in im.getextrema())
    warnings = []
    if uniform:
        warnings.append('uniform-frame')
    if not sprites.exists():
        warnings.append('missing-sprite-trace')
    if unresolved:
        warnings.append('unresolved-sprite-metadata-or-frame')
    if zero_metrics:
        warnings.append('zero-glyph-metrics')
    if font_mismatches:
        warnings.append('chinese-font-mismatch')
    if clipped:
        warnings.append('clipped-text-region')
    return {'screenshot': str(screenshot), 'uniformFrame': uniform, 'clippedText': clipped, 'zeroGlyphMetrics': zero_metrics, 'chineseFontMismatches': font_mismatches, 'textRegions': sum(r['kind']=='text' for r in rows), 'cursorRegions': sum(r['kind']=='cursor' for r in rows), 'spriteTracePresent': sprites.exists(), 'unresolvedSprites': unresolved, 'collisions': collisions, 'choiceFocus': focus, 'cursorFocus': cursor_focus, 'numericOverlays': overlays, 'closeups': panels,
            'auditLevel': 'text-metrics-and-sprite-opaque-bounding-boxes',
            'coverageWarnings': warnings,
            'assessment': 'incomplete-evidence' if warnings else 'requires-visual-review' if collisions else 'geometry-clear',
            'geometryClear': not collisions and not warnings}


def main():
    parser = argparse.ArgumentParser(description='检测实机文字与光标交叠，自动生成最近邻放大 JPG。')
    parser.add_argument('--evidence-dir', type=Path, required=True)
    parser.add_argument('--sprite-metadata', type=Path)
    args = parser.parse_args()
    metadata = {r['sprite']: r for r in json.loads(args.sprite_metadata.read_text(encoding='utf-8-sig'))} if args.sprite_metadata else {}
    reports = [r for trace in sorted(args.evidence_dir.glob('game-*/*-texts.json')) if (r := audit(trace, metadata))]
    output = args.evidence_dir / 'layout-collisions.json'
    output.write_text(json.dumps(reports, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps({'screenshots': len(reports), 'collisions': sum(len(r['collisions']) for r in reports), 'zeroGlyphMetrics': sum(len(r['zeroGlyphMetrics']) for r in reports), 'chineseFontMismatches': sum(len(r['chineseFontMismatches']) for r in reports), 'missingSpriteTraces': sum(not r['spriteTracePresent'] for r in reports), 'unresolvedSprites': sum(len(r['unresolvedSprites']) for r in reports), 'geometryClearScreenshots': sum(r['geometryClear'] for r in reports), 'incompleteEvidenceScreenshots': sum(bool(r['coverageWarnings']) for r in reports)}, ensure_ascii=False))


if __name__ == '__main__':
    main()
