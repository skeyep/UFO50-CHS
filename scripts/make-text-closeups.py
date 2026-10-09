"""Locate expected runtime text and emit nearest-neighbour JPG closeups."""
import argparse
import json
import math
import subprocess
from importlib.util import module_from_spec, spec_from_file_location
from pathlib import Path

from PIL import Image

spec = spec_from_file_location('target_validation', Path(__file__).with_name('validate-layout-targets.py'))
target_validation = module_from_spec(spec)
spec.loader.exec_module(target_validation)


def matching_rows(rows, parts):
    groups = {}
    for row, box in rows:
        groups.setdefault(row.get('caller', ''), []).append((row, box))
    selected, missing = [], []
    for part in parts:
        found = False
        for group in groups.values():
            spans, stream = [], ''
            for row, box in group:
                start = len(stream)
                stream += target_validation.normalize(row['text'])
                spans.append((start, len(stream), row, box))
            offset = stream.find(part)
            if offset < 0:
                continue
            found = True
            end = offset + len(part)
            selected.extend((row, box) for start, stop, row, box in spans if start < end and stop > offset)
        if not found:
            missing.append(part)
    return selected, missing


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--evidence-dir', type=Path, required=True)
    parser.add_argument('--expectations', type=Path, required=True)
    parser.add_argument('--text-root', type=Path, required=True)
    parser.add_argument('--mode', required=True)
    parser.add_argument('--game', type=int, required=True)
    parser.add_argument('--jpeg-tool', type=Path, required=True)
    parser.add_argument('--scale', type=int, default=2)
    parser.add_argument('--padding', type=int, default=12)
    args = parser.parse_args()
    if args.scale < 1 or args.padding < 0:
        parser.error('scale must be positive and padding nonnegative')
    case = json.loads(args.expectations.read_text('utf-8-sig'))['modes'].get(args.mode)
    if case is None:
        parser.error('mode has no expected text; register its targets first')
    results = []
    for shot in case.get('shots', []):
        stem = f"game-{args.game}-{shot['name']}"
        picture, trace = args.evidence_dir / f'{stem}.png', args.evidence_dir / f'{stem}-texts.json'
        if not picture.exists() or not trace.exists():
            results.append({'shot': shot['name'], 'status': 'failed', 'reason': 'missing screenshot or trace'})
            continue
        with Image.open(picture) as source:
            im = source.convert('RGB')
        rows = []
        for row in json.loads(trace.read_text('utf-8-sig')):
            if row.get('alpha', 1) <= 0 or row.get('width', 0) <= 0 or row.get('height', 0) <= 0:
                continue
            if shot.get('caller') and row.get('caller') != shot['caller']:
                continue
            box = target_validation.layout.screen_box(row, im)
            if box and target_validation.layout.intersection(box, [0, 0, *im.size]):
                rows.append((row, box))
        all_targets = shot.get('allOf', [])
        alternatives = shot.get('anyOf', [])
        for index, target in enumerate(all_targets + alternatives):
            parts = target_validation.segments(target, args.text_root)
            selected, missing = matching_rows(rows, parts)
            result = {'shot': shot['name'], 'target': target, 'alternative': index >= len(all_targets),
                      'status': 'failed' if missing or not selected else 'captured-awaiting-visual-review', 'missing': missing}
            if selected:
                boxes = [box for row, box in selected]
                clipped = any(b[0] < 0 or b[1] < 0 or b[2] > im.width or b[3] > im.height for b in boxes)
                uniform = []
                for box in boxes:
                    core = im.crop((max(0, math.floor(box[0])), max(0, math.floor(box[1])),
                                    min(im.width, math.ceil(box[2])), min(im.height, math.ceil(box[3]))))
                    uniform.append(all(lo == hi for lo, hi in core.getextrema()))
                result.update(partiallyOutsideFrame=clipped, allTargetRegionsUniform=all(uniform))
                if clipped or all(uniform):
                    result.update(status='failed', reason='target clipped by frame' if clipped else 'target regions are uniform')
                pad = args.padding
                crop = [max(0, math.floor(min(b[0] for b in boxes)) - pad),
                        max(0, math.floor(min(b[1] for b in boxes)) - pad),
                        min(im.width, math.ceil(max(b[2] for b in boxes)) + pad),
                        min(im.height, math.ceil(max(b[3] for b in boxes)) + pad)]
                if crop[2] > crop[0] and crop[3] > crop[1]:
                    out = args.evidence_dir / f'{stem}-text-zoom-{index + 1}.png'
                    panel = im.crop(tuple(crop))
                    panel = panel.resize((panel.width * args.scale, panel.height * args.scale), Image.Resampling.NEAREST)
                    panel.save(out)
                    subprocess.run(['py', '-3.14', str(args.jpeg_tool), str(out)], check=True, capture_output=True)
                    result.update(crop=crop, scale=args.scale, closeup=out.with_suffix('.upload.jpg').name,
                                  calls=[{'text': row['text'], 'caller': row.get('caller'), 'box': box,
                                          'currentIsCHS': row.get('currentIsCHS')} for row, box in selected])
                else:
                    result.update(status='failed', reason='empty visible crop')
            results.append(result)
    failed = any(r['status'] == 'failed' and not r.get('alternative', False) for r in results)
    for shot in case.get('shots', []):
        if shot.get('anyOf') and not any(r.get('alternative') and r['shot'] == shot['name'] and r['status'] != 'failed' for r in results):
            failed = True
    report = {'mode': args.mode, 'status': 'failed' if failed or not results else 'captured-awaiting-visual-review', 'targets': results}
    (args.evidence_dir / 'text-closeups.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), 'utf-8')
    print(json.dumps({'mode': args.mode, 'status': report['status'], 'targets': len(results),
                      'closeups': sum('closeup' in r for r in results)}, ensure_ascii=False))
    raise SystemExit(1 if failed or not results else 0)


if __name__ == '__main__':
    main()
