"""检查实际绘制字体；视觉验收仍需逐张查看数字特写。"""
import argparse
import json
import pathlib

parser = argparse.ArgumentParser(description='检查实机纯数字字体，并列出缺失证据。')
parser.add_argument('--evidence-dir', required=True, type=pathlib.Path)
options = parser.parse_args()
root = options.evidence_dir.resolve()
files = sorted(root.glob('game-*/*-numbers.json'))
allowed = set('0123456789 +-/:.,%$()MXPx×^')
failures = []
numeric = 0
for file in files:
    for index, row in enumerate(json.loads(file.read_text(encoding='utf-8'))):
        value = str(row['text'])
        if not any(c in '0123456789' for c in value) or not set(value) <= allowed:
            continue
        numeric += 1
        reason = None
        if 'currentIsCHS' not in row:
            reason = '缺少实际绘制字体标志'
        elif row['currentIsCHS']:
            reason = '纯数字仍使用中文字体'
        if reason:
            failures.append({'file': str(file.relative_to(root)), 'row': index, 'text': value, 'reason': reason})
    screenshot = file.with_name(file.name.replace('-numbers.json', '.png'))
    if not screenshot.exists():
        failures.append({'file': str(file.relative_to(root)), 'reason': '缺少对应完整截图'})
if not files or not numeric:
    failures.append({'reason': '没有纯数字实机证据'})
result = {'records': len(files), 'numericCalls': numeric, 'failures': failures, 'passed': not failures}
(root / 'number-font-check.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(result, ensure_ascii=False, indent=2))
raise SystemExit(0 if result['passed'] else 1)
