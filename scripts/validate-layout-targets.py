"""Require the intended visible text in runtime layout evidence, including glyph draws."""
import argparse
import base64
import json
import re
from pathlib import Path

from PIL import Image

from importlib.util import module_from_spec, spec_from_file_location

spec = spec_from_file_location('layout_closeups', Path(__file__).with_name('make-layout-closeups.py'))
layout = module_from_spec(spec)
spec.loader.exec_module(layout)


def normalize(value):
    return re.sub(r'[\s*#]', '', value)


def read_resource(root, game, key):
    name = 'm_Text.json' if game in ('m', 51) else f'{game}_Text.json'
    raw = (root / name).read_bytes()
    if name != 'm_Text.json':
        raw = base64.b64decode(raw)
    text = raw.decode('utf-8-sig')
    # Official resources permit a trailing comma before the final object brace.
    data = json.loads(re.sub(r',\s*}\s*$', '}', text))
    return data[key]


def segments(target, root):
    if isinstance(target, str):
        return [normalize(target)]
    value = target.get('text')
    if value is None:
        value = read_resource(root, target['game'], target['key'])
    # Substituted values and inline icons are checked by their dedicated traces.
    return [part for part in (normalize(p) for p in re.split(r'\{\d+\}|\[\d+\]?|\*{2,}|[@^]', value)) if part]


def validate(evidence, game, case, root):
    results = []
    for shot in case.get('shots', []):
        stem = f"game-{game}-{shot['name']}"
        picture = evidence / f'{stem}.png'
        trace = evidence / f'{stem}-texts.json'
        result = {'shot': shot['name'], 'status': 'passed', 'missing': []}
        if not picture.exists() or not trace.exists():
            result.update(status='failed', reason='missing screenshot or text trace')
            results.append(result)
            continue
        with Image.open(picture) as image:
            size = image.size
            rows = json.loads(trace.read_text('utf-8-sig'))
            visible = []
            for row in rows:
                if row.get('alpha', 1) <= 0 or row.get('width', 0) <= 0 or row.get('height', 0) <= 0:
                    continue
                if shot.get('caller') and row.get('caller') != shot['caller']:
                    continue
                box = layout.screen_box(row, image)
                if box and layout.intersection(box, [0, 0, *size]):
                    visible.append(row)
        groups = {}
        for row in visible:
            groups.setdefault(row.get('caller', ''), []).append(row['text'])
        streams = [normalize(''.join(values)) for values in groups.values()]
        # Whole-string and per-glyph draws share the same normalized comparison.
        def present(target):
            parts = segments(target, root)
            if not parts:
                raise ValueError(f'Empty text expectation: {target}')
            return all(any(part in stream for stream in streams) for part in parts)
        for target in shot.get('allOf', []):
            if not present(target):
                result['missing'].append(target)
        alternatives = shot.get('anyOf', [])
        if alternatives and not any(present(target) for target in alternatives):
            result['missing'].append({'anyOf': alternatives})
        if result['missing']:
            result['status'] = 'failed'
        result['visibleCalls'] = len(visible)
        result['callers'] = list(groups)
        results.append(result)
    return results


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--evidence-dir', type=Path, required=True)
    parser.add_argument('--expectations', type=Path, required=True)
    parser.add_argument('--text-root', type=Path, required=True)
    parser.add_argument('--mode', required=True)
    parser.add_argument('--game', type=int, required=True)
    args = parser.parse_args()
    cases = json.loads(args.expectations.read_text('utf-8-sig'))['modes']
    case = cases.get(args.mode)
    if case is None:
        result = {'status': 'pending', 'reason': 'no target expectations for this mode'}
        exit_code = 0
    else:
        shots = validate(args.evidence_dir, args.game, case, args.text_root)
        result = {'status': 'passed' if shots and all(r['status'] == 'passed' for r in shots) else 'failed', 'shots': shots}
        exit_code = 0 if result['status'] == 'passed' else 1
    (args.evidence_dir / 'layout-target-validation.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), 'utf-8')
    print(json.dumps(result, ensure_ascii=False))
    raise SystemExit(exit_code)


if __name__ == '__main__':
    main()
