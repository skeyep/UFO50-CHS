"""List high-stroke Han characters with whole-string context for manual review.

Reads a local official Unihan archive. It never substitutes characters: names,
puzzles and idioms require review of the whole expression and game context.
"""
import argparse
import collections
import json
from pathlib import Path
import re
import zipfile


def inventory(source_dir, strokes, threshold):
    rows = []
    counts = collections.Counter()
    for path in sorted(source_dir.glob('*.json')):
        if path.name == 'menu-reviewed-keys.json':
            continue
        data = json.loads(path.read_text(encoding='utf-8-sig'))
        for key, value in data.items():
            if not isinstance(value, str):
                continue
            found = sorted(set(value) & strokes.keys(), key=lambda c: (-strokes[c], c))
            found = [c for c in found if strokes[c] >= threshold]
            if not found:
                continue
            chars = [{'character': c, 'strokes': strokes[c], 'count': value.count(c)} for c in found]
            counts.update({c: value.count(c) for c in found})
            rows.append({'file': path.name, 'key': key, 'text': value, 'characters': chars,
                         'status': 'pending-context-review'})
    return rows, counts


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--unihan-zip', required=True, type=Path)
    parser.add_argument('--source-dir', type=Path, default=Path(__file__).resolve().parents[1] / 'source/translations')
    parser.add_argument('--threshold', type=int, default=13)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    if args.threshold < 1:
        parser.error('--threshold must be positive')
    strokes = {}
    with zipfile.ZipFile(args.unihan_zip) as archive:
        for line in archive.read('Unihan_IRGSources.txt').decode('utf-8').splitlines():
            match = re.fullmatch(r'U\+([0-9A-F]+)\tkTotalStrokes\t(\d+(?: \d+)*)', line)
            if match:
                strokes[chr(int(match[1], 16))] = max(map(int, match[2].split()))
    if not strokes:
        raise ValueError('No kTotalStrokes records in Unihan archive')
    rows, counts = inventory(args.source_dir, strokes, args.threshold)
    report = {'threshold': args.threshold, 'strokeSource': args.unihan_zip.name,
              'rows': rows, 'characters': [{'character': c, 'strokes': strokes[c], 'occurrences': n}
                                         for c, n in counts.most_common()]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'strings': len(rows), 'characters': len(counts), 'output': str(args.output)}))


if __name__ == '__main__':
    main()
