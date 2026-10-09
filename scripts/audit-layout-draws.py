"""Index cursor, fixed-cell and adjacent-row layout candidates in original GML."""
import argparse
import csv
import json
import re
from pathlib import Path

parser = argparse.ArgumentParser(description='清点光标、8px布局和语义字体绘制位点。')
parser.add_argument('--code-root', required=True, type=Path)
parser.add_argument('--output-dir', required=True, type=Path)
args = parser.parse_args()
if not args.code_root.is_dir():
    raise SystemExit('GML 参考目录不存在。')
rows = []
patterns = {
    'cursor': re.compile(r'draw_sprite(?:_ext)?\([^,\n]*(?:[Hh]and|[Cc]ursor|[Ss]elect|[Pp]ointer|[Aa]rrow)|(?:[Hh]and|[Cc]ursor)\.(?:x|y)\s*=|\bcursor[XY]\s*=|draw_text\([^;\n]*,\s*"[><^]"\)'),
    'character-width': re.compile(r'string_length\([^;\n]*(?:\*\s*8\b|\b8\s*\*)|(?:\b8\s*\*\s*(?:i|j|k|sel|menuSel|subY)|\b(?:i|j|k|sel|menuSel|subY)\s*\*\s*8\b)'),
    'semantic-font': re.compile(r'scrSetFont\(global\.font(?:Avianos|Alien|Grimstone|FancyShort)\)'),
}
for path in sorted(args.code_root.glob('*.gml')):
    lines = path.read_text(encoding='utf-8-sig').splitlines()
    game = re.search(r'(?:scr|_o)(\d+)', path.stem)
    for index, line in enumerate(lines):
        reasons = [name for name, pattern in patterns.items() if pattern.search(line)]
        # Two nearby constant-Y text calls expose cases without a loop, such as warning labels.
        if 'draw_text' in line and re.search(r'\+\s*\d+\s*,', line):
            for next_line in lines[index + 1:index + 4]:
                if 'draw_text' in next_line:
                    first = re.search(r',\s*([^,]+?)\s*\+\s*(\d+)\s*,', line)
                    second = re.search(r',\s*([^,]+?)\s*\+\s*(\d+)\s*,', next_line)
                    if first and second and first[1].strip() == second[1].strip() and 0 < abs(int(first[2]) - int(second[2])) <= 8:
                        reasons.append('adjacent-rows'); break
        if reasons:
            context = '\n'.join(lines[max(0, index - 8):index + 9])
            rows.append({'siteId': f'{path.stem}:{index + 1}', 'game': game[1] if game else 'shared',
                         'code': path.stem, 'line': index + 1, 'reasons': reasons,
                         'source': line.strip(), 'context': context,
                         'resourceKeys': sorted(set(re.findall(r'scrString\w*\("([^"\n]+)"', context))),
                         'status': 'pending-original-scene'})
args.output_dir.mkdir(parents=True, exist_ok=True)
(args.output_dir / 'layout-source-sites.json').write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding='utf-8')
with (args.output_dir / 'layout-source-sites.csv').open('w', encoding='utf-8-sig', newline='') as stream:
    writer = csv.DictWriter(stream, fieldnames=['siteId', 'game', 'code', 'line', 'reasons', 'source', 'status'])
    writer.writeheader()
    for row in rows:
        writer.writerow({key: '|'.join(row[key]) if key == 'reasons' else row[key] for key in writer.fieldnames})
print(json.dumps({'sites': len(rows), 'reasons': {key: sum(key in row['reasons'] for row in rows) for key in [*patterns, 'adjacent-rows']}}))
