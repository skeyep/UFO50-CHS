import pathlib, json, math, argparse
from PIL import Image, ImageDraw
parser=argparse.ArgumentParser(description='依据实机数字绘制区域记录生成数字特写 JPG。')
parser.add_argument('--evidence-dir',required=True,type=pathlib.Path)
evidence=parser.parse_args().evidence_dir.resolve()
if not evidence.is_dir(): raise SystemExit('实机证据目录不存在。')
report=[]
for file in sorted(evidence.glob('game-*/*-numbers.json')):
    screenshot=file.with_name(file.name.replace('-numbers.json','.png'))
    if not screenshot.exists(): continue
    im=Image.open(screenshot).convert('RGB')
    records=json.loads(file.read_text(encoding='utf-8'))
    seen={}; panels=[]; clipped=[]; regions=[]
    for item in records:
        gui=item['gui']
        basew=item['guiWidth'] if gui else item['viewWidth']
        baseh=item['guiHeight'] if gui else item['viewHeight']
        if not basew or not baseh: continue
        sx=im.width/basew; sy=im.height/baseh
        x=(item['x']-(0 if gui else item['viewX']))*sx
        y=(item['y']-(0 if gui else item['viewY']))*sy
        w=item['width']*sx; h=item['height']*sy
        box=(max(0,math.floor(x-6)),max(0,math.floor(y-6)),min(im.width,math.ceil(x+w+6)),min(im.height,math.ceil(y+h+6)))
        if box[2]<=box[0] or box[3]<=box[1]:
            clipped.append(item); continue
        if box in seen:
            index=seen[box]
            if item['text'] not in regions[index]['texts']: regions[index]['texts'].append(item['text'])
            draw=ImageDraw.Draw(panels[index])
            draw.rectangle((0,0,panels[index].width,23),fill=(24,24,24))
            draw.text((4,4),f"{screenshot.stem} | {item['text'][:50]}",fill='white')
            continue
        # 用户明确要求数字放大特写；完整截图另存原尺寸 .upload.jpg。
        crop=im.crop(box)
        factor=4 if crop.width<160 else 3
        zoom=crop.resize((crop.width*factor,crop.height*factor),Image.Resampling.NEAREST)
        panel=Image.new('RGB',(max(zoom.width,320),zoom.height+24),(24,24,24))
        ImageDraw.Draw(panel).text((4,4),f"{screenshot.stem} | {item['text'][:50]}",fill='white')
        panel.paste(zoom,(0,24))
        seen[box]=len(panels)
        regions.append({'box':box,'texts':[item['text']],'zoom':factor,'uniform':all(lo==hi for lo,hi in crop.getextrema())})
        panels.append(panel)
    outputs=[]
    for offset in range(0,len(panels),8):
        group=panels[offset:offset+8]
        canvas=Image.new('RGB',(max(p.width for p in group),sum(p.height for p in group)+8*(len(group)-1)),(24,24,24))
        yy=0
        for p in group: canvas.paste(p,(0,yy)); yy+=p.height+8
        out=screenshot.with_name(screenshot.stem+f'-numbers-zoom-{offset//8+1}.upload.jpg')
        canvas.save(out,quality=82,subsampling=0); outputs.append(out.name)
    report.append({'screenshot':str(screenshot.relative_to(evidence)), 'numericCalls':len(records),'distinctRegions':len(panels),'regions':regions,'uniformRegions':sum(r['uniform'] for r in regions),'outsideFrame':clipped,'closeups':outputs})
(evidence/'number-closeups.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'screenshots':len(report),'numericCalls':sum(r['numericCalls'] for r in report),'regions':sum(r['distinctRegions'] for r in report),'outsideFrame':sum(len(r['outsideFrame']) for r in report)},ensure_ascii=False))
