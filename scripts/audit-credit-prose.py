"""清点制作名单正文，检测被整体保留名单规则跳过的英文句子。"""
import argparse,json,re
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument('--text',type=Path,required=True)
parser.add_argument('--output',type=Path,required=True)
parser.add_argument('--strict',action='store_true')
args=parser.parse_args()
policy=json.loads(Path(__file__).with_name('credit-prose-policy.json').read_text('utf-8-sig'))
patterns=[re.compile(p,re.I) for p in policy['patterns']]
data=json.loads(args.text.read_text('utf-8-sig'));rows=[]
for key,value in data.items():
    if not re.fullmatch(r'game_credits_\d+_\d+b?',key):continue
    if not value.strip():status='blank'
    elif any(p.search(value.strip(' *-')) for p in patterns):status='prose-needs-localization'
    elif re.search(r'[\u3400-\u9fff]',value):status='localized-prose'
    else:status='original-name-role-code-or-brand'
    rows.append(dict(key=key,value=value,status=status))
pending=[r for r in rows if r['status']=='prose-needs-localization']
report=dict(fields=len(rows),pending=len(pending),classification={s:sum(r['status']==s for r in rows) for s in sorted({r['status'] for r in rows})},rows=rows)
args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps(report,ensure_ascii=False,indent=2),'utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='rows'},ensure_ascii=False))
raise SystemExit(1 if args.strict and pending else 0)
