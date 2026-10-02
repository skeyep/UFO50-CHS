import pathlib, re, json, csv, argparse
parser=argparse.ArgumentParser(description='清点私有 GML 参考中的数字及动态文字绘制点。')
parser.add_argument('--code-root',required=True,type=pathlib.Path)
parser.add_argument('--output-dir',required=True,type=pathlib.Path)
options=parser.parse_args()
source=options.code_root.resolve()
root=options.output_dir.resolve()
root.mkdir(parents=True,exist_ok=True)
if not source.is_dir(): raise SystemExit('GML 参考目录不存在。')
functions = {'draw_text':2,'draw_text_ext':2,'draw_text_color':2,'draw_text_bg':2,'draw_text_bg_centered':2,'draw_text_centered':2,'draw_text_ce':2,'scrDrawTextCentered':0,'scrDrawTextCenteredPoint':0}
rows=[]
def args_at(text,start):
    args=[]; begin=start; depth=1; quoted=False; escaped=False
    for i in range(start,len(text)):
        ch=text[i]
        if quoted:
            if escaped: escaped=False
            elif ch=='\\': escaped=True
            elif ch=='"': quoted=False
            continue
        if ch=='"': quoted=True
        elif ch in '([{': depth+=1
        elif ch in ')]}':
            depth-=1
            if not depth: return args+[text[begin:i].strip()]
        elif ch==',' and depth==1: args.append(text[begin:i].strip()); begin=i+1
    return []
for file in sorted(source.glob('*.gml')):
    text=file.read_text(encoding='utf-8')
    game=re.search(r'(?:o|scr)(\d\d)_',file.stem)
    fonts=list(re.finditer(r'(?:scrSetFont|draw_set_font)\(([^;\r\n]+)\)',text))
    for match in re.finditer(r'(?<![\w])('+ '|'.join(functions) +r')\s*\(',text):
        args=args_at(text,match.end()); index=functions[match.group(1)]
        if len(args)<=index: continue
        expr=args[index]
        literals=re.findall(r'"((?:\\.|[^"\\])*)"',expr)
        has_digit=any(re.search(r'\d',x) for x in literals)
        literal_numeric=bool(literals) and all(re.fullmatch(r'[0-9 +\-/:.,%()M]*',x) for x in literals)
        numeric_function=bool(re.search(r'\b(?:string|string_format|scrTimeFormat)\s*\(',expr))
        if 'scrString' in expr: kind='mixed-translated-number' if numeric_function or has_digit else 'translated-text'
        elif literal_numeric or numeric_function: kind='numeric-candidate'
        elif literals: kind='literal-or-mixed'
        else: kind='dynamic-review-required'
        previous=[f for f in fonts if f.start()<match.start()]
        context = text[max(0, match.start()-700):min(len(text), match.end()+700)]
        flags = []
        for flag, pattern in {
            '补零或格式化': r'string_format|scrTimeFormat|string_replace[^\n]*"0"',
            '固定格或遮罩': r'draw_rectangle|draw_text_bg|\*\s*8|8\s*\*',
            '逐位或循环': r'\bfor\s*\(|string_char_at',
            '居中': r'center|Centered|draw_set_halign\(fa_center',
            '比分': r'(?i)score|points|winCount',
            '资源或价格': r'(?i)cash|gold|price|cost|stock|resource',
            '时间': r'(?i)timer|secs|msecs|minutes|hours|TimeFormat',
        }.items():
            if re.search(pattern, context): flags.append(flag)
        rows.append({'game':int(game.group(1)) if game else 0, 'file':file.name,'line':text.count('\n',0,match.start())+1,'function':match.group(1),'expression':expr,'category':kind,'nearestFont':previous[-1].group(1) if previous else 'inherited','reviewFlags':' / '.join(flags),'status':'待核对'})
(root/'numeric-draw-inventory.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
with (root/'numeric-draw-inventory.csv').open('w',encoding='utf-8-sig',newline='') as f:
    writer=csv.DictWriter(f,fieldnames=rows[0].keys()); writer.writeheader(); writer.writerows(rows)
summary={k:sum(r['category']==k for r in rows) for k in sorted(set(r['category'] for r in rows))}
print(json.dumps({'total':len(rows),'categories':summary},ensure_ascii=False))
for r in rows:
    if r['category']=='numeric-candidate': print(f"{r['game']:02} {r['file']}:{r['line']} [{r['nearestFont']}] {r['expression']}")
