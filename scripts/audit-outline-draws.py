"""清点原版文字样式和中文描边候选；报告供逐场景实机复核。"""
import argparse
import json
import pathlib
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--code-dir', required=True, type=pathlib.Path)
parser.add_argument('--patch-file', type=pathlib.Path, default=pathlib.Path(__file__).resolve().parents[1] / 'payload/patch-font.csx')
parser.add_argument('--evidence-dir', action='append', type=pathlib.Path, default=[])
parser.add_argument('--output', required=True, type=pathlib.Path)
args = parser.parse_args()

outlined = {'fontDefault', 'fontDefault_JP', 'fontThinOutline', 'fontTall', 'fontTallBG'}
solid = {'fontGrimstone', 'fontBlocky', 'fontAlien', 'fontAvianos', 'fontUFO', 'fontGradient', 'fontBigWestern'}
bare = {'fontDefaultNoShadow', 'fontDefaultNoShadow_JP', 'fontNoShadow', 'fontTerminal', 'fontFancyShort', 'fontHandwriting', 'fontBlockyTall'}

def style(font):
    if font in outlined:
        return 'shadow-or-outline'
    if font in solid:
        return 'solid-black-cell'
    if font in bare:
        return 'bare-glyph'
    return 'other-or-caller-font'

calls = []
font_creations = []
for path in sorted(args.code_dir.glob('*.gml')):
    source = path.read_text(encoding='utf-8')
    requests = sorted(set(re.findall(r'(?:scrSetFont|draw_set_font)\(\s*(?:global\.)?(font\w+)\s*\)', source)))
    for line_no, line in enumerate(source.splitlines(), 1):
        if re.match(r'\s*function\b', line):
            continue
        for match in re.finditer(r'\b(draw_text\w*|scrDrawText\w*|scrStringDraw\w*)\s*\(', line):
            calls.append({'code': path.stem, 'line': line_no, 'call': match[1],
                          'source': line.strip(), 'fontHints': requests,
                          'styles': sorted({style(f) for f in requests}),
                          'backgroundHelper': '_bg' in match[1].lower(),
                          'reviewState': 'awaiting-scene-review'})
        if re.search(r'\bfont_add(?:_sprite(?:_ext)?)?\s*\(', line):
            font_creations.append({'code': path.stem, 'line': line_no, 'source': line.strip()})

# Legacy QA font references follow creation order in this patch's scrInitFonts.
# Prefer a requestedFontName supplied by newer harnesses when available.
patch = args.patch_file.read_text(encoding='utf-8')
init = patch.split('function scrInitFonts()', 1)[1].split('global.currFont', 1)[0]
font_order = re.findall(r'global\.(font\w+)\s*=\s*font_add(?:_sprite(?:_ext)?)?\(', init)
runtime = []
seen = set()
for directory in args.evidence_dir:
    for path in sorted(directory.glob('game-*/*-texts.json')):
        for row in json.loads(path.read_text(encoding='utf-8')):
            if not row.get('currentIsCHS') or not re.search('[\u3400-\u9fff]', str(row['text'])):
                continue
            name = row.get('requestedFontName')
            ref = re.search(r'__newfont(\d+)\b', str(row.get('requestedFont')))
            if not name and ref and int(ref[1]) < len(font_order):
                name = font_order[int(ref[1])]
            key = (str(directory), path.parent.name, row['text'], name)
            if key in seen:
                continue
            seen.add(key)
            runtime.append({'evidence': str(path), 'text': row['text'], 'requestedFont': name,
                            'style': style(name), 'outlineRouteExpected': name in outlined,
                            'x': row['x'], 'y': row['y'], 'width': row['width'], 'height': row['height'],
                            'reviewState': 'awaiting-visible-crop-review'})

result = {'drawCalls': len(calls), 'codeFiles': len({r['code'] for r in calls}),
          'outlinedFonts': sorted(outlined), 'solidCellFonts': sorted(solid), 'bareFonts': sorted(bare),
          'fontCreations': font_creations, 'calls': calls, 'runtimeCandidates': runtime,
          'note': 'fontHints 是文件内字体线索；继承字体、手绘底框、遮挡和动画结合实际画面逐项确认。'}
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({k: result[k] for k in ['drawCalls', 'codeFiles', 'outlinedFonts']}, ensure_ascii=False))
print(f'实机中文样式候选：{len(runtime)}')
