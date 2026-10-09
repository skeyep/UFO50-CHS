import pathlib, re, json, csv, argparse
parser=argparse.ArgumentParser(description='清点 GML 参考中的数字及动态文字绘制点。')
parser.add_argument('--code-root',required=True,type=pathlib.Path)
parser.add_argument('--output-dir',required=True,type=pathlib.Path)
options=parser.parse_args()
source=options.code_root.resolve()
root=options.output_dir.resolve()
root.mkdir(parents=True,exist_ok=True)
if not source.is_dir(): raise SystemExit('GML 参考目录不存在。')
functions = {'draw_text':2,'draw_text_ext':2,'draw_text_color':2,'draw_text_bg':2,'draw_text_bg_centered':2,'draw_text_centered':2,'draw_text_ce':2,'scrDrawTextCentered':0,'scrDrawTextCenteredPoint':0}

# Follow single text arguments through local assignments in custom draw wrappers.
# This exposes callers such as scr04_DrawText and scrDrawTextInput as well as
# strings assembled into a local variable before the drawing call.
def symbols(expr):
    expr = re.sub(r'"(?:\\.|[^"\\])*"', '', expr)
    return set(re.findall(r'\b[A-Za-z_]\w*\b', expr))

def local_assignments(text):
    result = {}
    for m in re.finditer(r'(?m)\b(?:var\s+)?([A-Za-z_]\w*)(?:\[[^;\n]+?\])*\s*(?<![=!<>])=(?!=)\s*([^;\n]+)', text):
        result.setdefault(m.group(1), set()).update(symbols(m.group(2)))
    for call in re.finditer(r'\b(ds_grid_set|ds_grid_set_region|ds_map_add|ds_map_replace|ds_map_set|ds_list_add|ds_list_set|array_push)\s*\(', text):
        values = args_at(text, call.end())
        if len(values) < 2: continue
        for name in symbols(values[0]):
            result.setdefault(name, set()).update(symbols(values[-1]))
    return result

def expand_symbols(expr, assignments):
    pending = list(symbols(expr)); found = set()
    while pending:
        name = pending.pop()
        if name in found: continue
        found.add(name); pending.extend(assignments.get(name, set()) - found)
    return found

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

def object_owner(filename):
    match = re.match(r"gml_Object_(.+?)_(?:Create|Destroy|Step|Alarm|Draw|Other|CleanUp|PreCreate|KeyPress|KeyRelease|Keyboard|Mouse)_", filename)
    return match.group(1) if match else None

owner_assignments = {}
for file in sorted(source.glob('gml_Object_*.gml')):
    owner = object_owner(file.stem)
    if owner is None: continue
    assignments = owner_assignments.setdefault(owner, {})
    for name, dependencies in local_assignments(file.read_text(encoding='utf-8')).items():
        assignments.setdefault(name, set()).update(dependencies)

custom_wrappers = {}
for file in sorted(source.glob('gml_GlobalScript_*.gml')):
    body = file.read_text(encoding='utf-8')
    signature = re.search(r'\bfunction\s+(\w+)\s*\(([^)]*)\)', body)
    if not signature: continue
    indices = set()
    assignments = local_assignments(body)
    for call in re.finditer(r'(?<![\w])(draw_text|draw_text_ext|draw_text_color)\s*\(', body):
        args = args_at(body, call.end())
        if len(args) < 3: continue
        indices.update(int(n[3:]) for n in expand_symbols(args[2], assignments) if re.fullmatch(r'arg\d+', n))
    if len(indices) == 1:
        name = signature.group(1)
        if name not in functions:
            functions[name] = next(iter(indices)); custom_wrappers[name] = functions[name]
(root/'custom-text-wrapper-inventory.json').write_text(json.dumps(custom_wrappers, ensure_ascii=False, indent=2), encoding='utf-8')

for file in sorted(source.glob('*.gml')):
    text=file.read_text(encoding='utf-8')
    game=re.search(r'(?:o|scr)(\d\d)_',file.stem)
    fonts=list(re.finditer(r'(?:scrSetFont|draw_set_font)\(([^;\r\n]+)\)',text))
    for match in re.finditer(r'(?<![\w])('+ '|'.join(functions) +r')\s*\(',text):
        if re.search(r'\bfunction\s*$', text[max(0,match.start()-30):match.start()]): continue
        args=args_at(text,match.end()); index=functions[match.group(1)]
        if len(args)<=index: continue
        expr=args[index]
        literals=re.findall(r'"((?:\\.|[^"\\])*)"',expr)
        has_digit=any(re.search(r'\d',x) for x in literals)
        literal_numeric=bool(literals) and all(re.fullmatch(r'[0-9 +\-/:.,%()M]*',x) for x in literals)
        numeric_function=bool(re.search(r'\b(?:string|string_format|scrTimeFormat)\s*\(',expr))
        assignments = {name: set(values) for name, values in owner_assignments.get(object_owner(file.stem), {}).items()}
        for name, values in local_assignments(text[:match.start()]).items():
            assignments.setdefault(name, set()).update(values)
        indirect = expand_symbols(expr, assignments)
        indirect_numeric = bool(indirect & {'string', 'string_format', 'scrTimeFormat', 'scrStringVal', 'scrStringFormat'})
        numeric_function = numeric_function or indirect_numeric
        if 'scrString' in expr or any(name.startswith('scrString') for name in indirect): kind='mixed-translated-number' if numeric_function or has_digit else 'translated-text'
        elif literal_numeric or numeric_function: kind='numeric-candidate'
        elif literals: kind='literal-or-mixed'
        else: kind='dynamic-review-required'
        previous=[f for f in fonts if f.start()<match.start()]
        context = text[max(0, match.start()-700):min(len(text), match.end()+700)]
        flags = []
        for flag, pattern in {
            '容器取值': r'ds_grid_get|ds_map_find_value|ds_map_get|ds_list_find_value|\[[^\]]+\]',
            '补零或格式化': r'string_format|scrTimeFormat|string_replace[^\n]*"0"',
            '固定格或遮罩': r'draw_rectangle|draw_text_bg|\*\s*8|8\s*\*',
            '逐位或循环': r'\bfor\s*\(|string_char_at',
            '居中': r'center|Centered|draw_set_halign\(fa_center',
            '比分': r'(?i)score|points|winCount',
            '资源或价格': r'(?i)cash|gold|price|cost|stock|resource',
            '时间': r'(?i)timer|secs|msecs|minutes|hours|TimeFormat',
        }.items():
            if re.search(pattern, context): flags.append(flag)
        rows.append({'game':int(game.group(1)) if game else 0, 'file':file.name,'line':text.count('\n',0,match.start())+1,'function':match.group(1),'expression':expr,'category':kind,'nearestFont':previous[-1].group(1) if previous else 'inherited','reviewFlags':' / '.join(flags),'indirectSymbols':' / '.join(sorted(indirect)), 'customWrapper':match.group(1) in custom_wrappers, 'assignmentScope':object_owner(file.stem) or 'local-script', 'status':'待核对'})
(root/'numeric-draw-inventory.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
with (root/'numeric-draw-inventory.csv').open('w',encoding='utf-8-sig',newline='') as f:
    writer=csv.DictWriter(f,fieldnames=rows[0].keys()); writer.writeheader(); writer.writerows(rows)
summary={k:sum(r['category']==k for r in rows) for k in sorted(set(r['category'] for r in rows))}
print(json.dumps({'total':len(rows),'categories':summary},ensure_ascii=False))
for r in rows:
    if r['category']=='numeric-candidate': print(f"{r['game']:02} {r['file']}:{r['line']} [{r['nearestFont']}] {r['expression']}")
