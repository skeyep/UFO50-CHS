"""Inventory text limits, substitution fields, and Japanese call-site contracts."""
import argparse, base64, json, pathlib, re

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--code-dir',type=pathlib.Path)
parser.add_argument('--output',required=True,type=pathlib.Path)
args=parser.parse_args()
root=pathlib.Path(__file__).resolve().parents[1]
rows=[]; limits=[]; calls=[]; labels=[]
for p in sorted((root/'payload/ext/JAPANESE').glob('*_Text.json')):
    raw=p.read_text(encoding='utf-8') if p.name=='m_Text.json' else base64.b64decode(p.read_bytes()).decode('utf-8')
    data=json.loads(re.sub(r',\s*}\s*$','}',raw))
    for key,value in data.items():
        if re.search(r'_(lim|wl|wc)$',key):continue
        fields=re.findall(r'\*+|\{[^{}]+\}|\[[12UDLR]\]',value)
        if len(fields)>1: rows.append(dict(file=p.name,key=key,text=value,fields=fields))
        limit=int(data.get(key+'_lim','0'))
        if limit>0:limits.append(dict(file=p.name,key=key,text=value,limit=limit,truncated=len(value)>limit))
if args.code_dir:
    for p in sorted(args.code_dir.glob('*.gml')):
        lines=p.read_text(encoding='utf-8').splitlines()
        for i,line in enumerate(lines):
            if (re.search(r'draw_(?:text|text_ext|text_color)\(',line) and 'scrString(' in line) or re.search(r'scrStringDraw\w*\(',line):
                keys=re.findall(r'scrString\("([^"]+)"\)',line) or re.findall(r'scrStringDraw\w*\([^;]*?"([^"]+)"',line)
                labels.append(dict(file=p.name,line=i+1,keys=keys,
                                   context='\n'.join(lines[max(0,i-2):i+12])))
            if 'LANG_JAPANESE' in line and any('scrStringVal(' in l for l in lines[i:i+18]):
                calls.append(dict(file=p.name,line=i+1,context='\n'.join(lines[max(0,i-2):i+22])))
report=dict(multipleFields=rows,characterLimits=limits,japaneseSubstitutionBranches=calls,labelDrawSites=labels,
            fixedLabelSlots={'12/'+key:dict(width=40,minGap=4) for key in
                             ['stat_xp','stat_atk','stat_def','stat_spd','stat_evd','stat_vit','stat_res']},
            roleContracts={
                '12/battle_uses_on':['actor','target','skill_or_item'],
                '12/animal_extra_03a':['learner','teacher','skill'],
                '12/animal_extra_04':['learner','teacher','skill'],
                '37/offer_science':['price','science_count'],
            })
args.output.parent.mkdir(parents=True,exist_ok=True)
args.output.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
bad=[r for r in limits if r['truncated']]
print(json.dumps(dict(multipleFields=len(rows),characterLimits=len(limits),japaneseBranches=len(calls),labelDrawSites=len(labels),truncated=bad),ensure_ascii=False))
raise SystemExit(bool(bad))
