"""Inventory natural-language literals that bypass external localization resources."""
import argparse, csv, json, re
from pathlib import Path

p=argparse.ArgumentParser(description='清点代码中的硬编码显示正文与默认对白。')
p.add_argument('--code-root',type=Path,required=True)
p.add_argument('--output-dir',type=Path,required=True)
a=p.parse_args()
if not a.code_root.is_dir():raise SystemExit('GML参考目录不存在。')
literal=re.compile(r'"(?:\\.|[^"\\])*"')
rows=[]
for path in sorted(a.code_root.glob('*.gml')):
 lines=path.read_text(encoding='utf-8-sig').splitlines()
 for n,line in enumerate(lines,1):
  for m in literal.finditer(line):
   value=m[0][1:-1];before=line[:m.start()]
   if not re.search(r'[A-Za-z]{2}',value):continue
   category=None
   if re.search(r'\b(?:str[123]?|message|text|msg|strScroll\[[^]]+\])\s*=\s*$',before):category='display-default-or-assignment'
   if re.search(r'\b(?:scrDrawTextBox\w*|draw_text\w*|draw_text_bg|show_message)\([^;]*$',before):category='display-call'
   if not category:continue
   if re.search(r'(?:scrString\w*|asset_get_index|variable_\w+)\([^)]*$',before):continue
   if re.search(r'(?:show_debug_message|debug|Debug|assert|throw)',line+path.name):category='debug'
   prose=bool(re.search(r'[A-Za-z]{2}[ ,.!?\x27:][A-Za-z ]|[ ,][A-Za-z]{2}|[A-Za-z]{3}[.!?]',value))
   if not prose:continue
   gid=re.search(r'(?:scr|_o)(\d+)',path.stem)
   rows.append({'siteId':f'{path.stem}:{n}','game':gid[1] if gid else 'shared','code':path.stem,'line':n,'text':value,'category':category,'source':line.strip(),'context':'\n'.join(lines[max(0,n-4):n+3]),'status':'debug' if category=='debug' else 'pending-original-scene'})
a.output_dir.mkdir(parents=True,exist_ok=True)
(a.output_dir/'hardcoded-text-sites.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
with (a.output_dir/'hardcoded-text-sites.csv').open('w',encoding='utf-8-sig',newline='') as f:
 writer=csv.DictWriter(f,fieldnames=['siteId','game','text','category','status','source']);writer.writeheader()
 for row in rows:writer.writerow({k:row[k] for k in writer.fieldnames})
print(json.dumps({'sites':len(rows),'displayCandidates':sum(r['category']!='debug' for r in rows)}))
