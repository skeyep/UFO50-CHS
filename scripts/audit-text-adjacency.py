"""Find label/number pairs in runtime draw records and render nearest-neighbor closeups."""
import argparse, json, pathlib, re
from PIL import Image, ImageDraw

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--evidence-dir', required=True, type=pathlib.Path)
args = parser.parse_args()
root = args.evidence_dir.resolve()
if not list(root.glob('game-*/*-texts.json')):
    raise SystemExit('缺少实际文字绘制记录：需要 game-*/*-texts.json。')
report = []
numeric = re.compile(r'^[0-9 +\-/:.,%$()MXPx×^]+(?:ATK|DEF|SPD|EVD)?$')
for file in sorted(root.glob('game-*/*-texts.json')):
    screenshot = file.with_name(file.name.replace('-texts.json', '.png'))
    if not screenshot.exists(): continue
    rows = json.loads(file.read_text(encoding='utf-8'))
    im = Image.open(screenshot).convert('RGB')
    pairs = []; panels = []; seen = set()
    for number in rows:
        if not re.search(r'\d', number['text']) or not numeric.fullmatch(number['text']): continue
        candidates = [label for label in rows if label['gui'] == number['gui']
                      and re.search(r'[^\x00-\x7f]', label['text'])
                      and '\n' not in label['text'] and label['x'] < number['x']
                      and abs(label['y'] - number['y']) <= 3]
        if not candidates: continue
        label = max(candidates, key=lambda r: r['x'])
        gap = number['x'] - label['x'] - label['width']
        if gap > 24: continue
        signature = (label['text'], number['text'], label['x'], label['y'], number['x'])
        if signature in seen: continue
        seen.add(signature)
        row = dict(label=label['text'], number=number['text'], gap=gap,
                   needsReview=gap < 4, labelBounds=[label[k] for k in ('x','y','width','height')],
                   numberBounds=[number[k] for k in ('x','y','width','height')])
        pairs.append(row)
        gui = number['gui']; sx = im.width / number['guiWidth' if gui else 'viewWidth']; sy = im.height / number['guiHeight' if gui else 'viewHeight']
        ox = 0 if gui else number['viewX']; oy = 0 if gui else number['viewY']
        box = (max(0, int((label['x']-ox-4)*sx)), max(0, int((min(label['y'],number['y'])-oy-3)*sy)),
               min(im.width, int((max(label['x']+label['width'],number['x']+number['width'])-ox+4)*sx)),
               min(im.height, int((max(label['y']+label['height'],number['y']+number['height'])-oy+3)*sy)))
        if box[2] <= box[0] or box[3] <= box[1]: row['outsideFrame'] = True; continue
        crop = im.crop(box); zoom = crop.resize((crop.width*3,crop.height*3),Image.Resampling.NEAREST)
        panel = Image.new('RGB',(max(320,zoom.width),zoom.height+24),(24,24,24))
        ImageDraw.Draw(panel).text((4,4),f'{screenshot.stem} | gap={gap:g}px',fill='white')
        panel.paste(zoom,(0,24)); panels.append(panel)
    closeups = []
    for offset in range(0,len(panels),6):
        group=panels[offset:offset+6]
        canvas=Image.new('RGB',(max(p.width for p in group),sum(p.height for p in group)+8*(len(group)-1)),(24,24,24))
        y=0
        for panel in group: canvas.paste(panel,(0,y)); y+=panel.height+8
        out=screenshot.with_name(screenshot.stem+f'-adjacency-{offset//6+1}.upload.jpg')
        canvas.save(out,quality=82,subsampling=0); closeups.append(out.name)
    report.append(dict(screenshot=str(screenshot.relative_to(root)),pairs=pairs,closeups=closeups))
(root/'text-adjacency.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(dict(screenshots=len(report),pairs=sum(len(r['pairs']) for r in report),
                     needsReview=[dict(screenshot=r['screenshot'],**p) for r in report for p in r['pairs'] if p['needsReview']]),ensure_ascii=False))
