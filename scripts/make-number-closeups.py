"""生成数字 JPG 特写，记录单色、出屏及前景框线遮挡候选。"""
import argparse
import json
import math
import pathlib
import subprocess
import sys
from PIL import Image, ImageDraw


def border_occlusion_candidates(im, record, sx, sy, relative_x, relative_y):
    """检测贯穿数字核心且伸出两端的前景色横线，坐标为截图像素。"""
    x0 = math.floor(relative_x*sx)
    x1 = math.ceil((relative_x+record['width'])*sx)
    y0 = max(0, math.floor(relative_y*sy))
    y1 = min(im.height, math.ceil((relative_y+record['height'])*sy))
    margin = max(2, math.ceil(3*sx))
    left, right = x0-margin, x1+margin
    if left < 0 or right > im.width or right <= left:
        return []
    color = int(record.get('color', 16777215))
    expected = (color&255, (color>>8)&255, (color>>16)&255)
    rows = []
    for yy in range(y0, y1):
        if all(all(abs(channel-target)<=15 for channel,target in zip(im.getpixel((xx,yy)), expected)) for xx in range(left,right)):
            rows.append(yy)
    return [{'type':'continuous-horizontal-foreground-line-over-numeric-core', 'rows':rows, 'coreBox':[x0,y0,x1,y1], 'lineTestSpan':[left,right], 'assessment':'possible-overdraw-awaiting-context-review'}] if rows else []


def classify_numeric_region(uniform, partly_outside, occlusions):
    # 字体度量框包含留白；部分度量出屏单列几何提示，字面由特写复核。
    if uniform:
        return 'uniform-awaiting-visible-frame'
    if occlusions:
        return 'occlusion-candidate-awaiting-context-review'
    return 'eligible-for-visual-review'


def make_closeups(evidence):
    report = []
    for file in sorted(evidence.glob('game-*/*-numbers.json')):
        screenshot = file.with_name(file.name.replace('-numbers.json','.png'))
        if not screenshot.exists():
            continue
        im = Image.open(screenshot).convert('RGB')
        records = json.loads(file.read_text(encoding='utf-8'))
        seen, panels, clipped, regions, calls = {}, [], [], [], []
        for record_index, item in enumerate(records):
            gui = item['gui']
            basew = item['guiWidth'] if gui else item['viewWidth']
            baseh = item['guiHeight'] if gui else item['viewHeight']
            if not basew or not baseh:
                calls.append({'recordIndex':record_index, 'text':item['text'], 'assessment':'missing-view-size-awaiting-evidence'})
                continue
            sx, sy = im.width/basew, im.height/baseh
            rx = item['x']-(0 if gui else item['viewX'])
            ry = item['y']-(0 if gui else item['viewY'])
            x, y, w, h = rx*sx, ry*sy, item['width']*sx, item['height']*sy
            box = (max(0,math.floor(x-6)),max(0,math.floor(y-6)),min(im.width,math.ceil(x+w+6)),min(im.height,math.ceil(y+h+6)))
            coordinates = {'recordIndex':record_index, 'text':item['text'], 'numericBox':[x,y,x+w,y+h], 'screenshotScale':[sx,sy], 'gui':gui, 'viewOrigin':[0,0] if gui else [item['viewX'],item['viewY']], 'requestedFont':item.get('requestedFont'), 'actualFont':item.get('fontId')}
            if box[2]<=box[0] or box[3]<=box[1]:
                clipped.append(item)
                calls.append({**coordinates, 'assessment':'outside-frame-awaiting-visible-frame'})
                continue
            crop = im.crop(box)
            uniform = all(lo==hi for lo,hi in crop.getextrema())
            partly_outside = x<0 or y<0 or x+w>im.width or y+h>im.height
            occlusions = border_occlusion_candidates(im,item,sx,sy,rx,ry) if not uniform else []
            assessment = classify_numeric_region(uniform, partly_outside, occlusions)
            warnings = ['numeric-metric-box-partially-outside-frame'] if partly_outside else []
            calls.append({**coordinates, 'assessment':assessment, 'occlusionCandidates':occlusions, 'geometryWarnings':warnings})
            if box in seen:
                index = seen[box]
                region = regions[index]
                if item['text'] not in region['texts']:
                    region['texts'].append(item['text'])
                region['evidenceCoordinates'].append(coordinates)
                for candidate in occlusions:
                    if candidate not in region['occlusionCandidates']:
                        region['occlusionCandidates'].append(candidate)
                region['partlyOutsideFrame'] |= partly_outside
                for warning in warnings:
                    if warning not in region['geometryWarnings']:
                        region['geometryWarnings'].append(warning)
                region['assessment'] = classify_numeric_region(region['uniform'],region['partlyOutsideFrame'],region['occlusionCandidates'])
                region['representativeEligible'] = region['assessment']=='eligible-for-visual-review'
                draw = ImageDraw.Draw(panels[index])
                draw.rectangle((0,0,panels[index].width,23),fill=(24,24,24))
                draw.text((4,4),f"{screenshot.stem} | {item['text'][:50]}",fill='white')
                continue
            # 完整截图另存原尺寸 .upload.jpg；特写保持像素边缘。
            factor = 4 if crop.width<160 else 3
            zoom = crop.resize((crop.width*factor,crop.height*factor),Image.Resampling.NEAREST)
            panel = Image.new('RGB',(max(zoom.width,320),zoom.height+24),(24,24,24))
            ImageDraw.Draw(panel).text((4,4),f"{screenshot.stem} | {item['text'][:50]}",fill='white')
            panel.paste(zoom,(0,24))
            seen[box] = len(panels)
            regions.append({'box':box,'texts':[item['text']],'zoom':factor,'uniform':uniform,'partlyOutsideFrame':partly_outside,'geometryWarnings':warnings,'evidenceCoordinates':[coordinates],'occlusionCandidates':occlusions,'assessment':assessment,'representativeEligible':assessment=='eligible-for-visual-review'})
            panels.append(panel)
        outputs = []
        for offset in range(0,len(panels),8):
            group = panels[offset:offset+8]
            canvas = Image.new('RGB',(max(p.width for p in group),sum(p.height for p in group)+8*(len(group)-1)),(24,24,24))
            yy = 0
            for panel in group:
                canvas.paste(panel,(0,yy))
                yy += panel.height+8
            out = screenshot.with_name(screenshot.stem+f'-numbers-zoom-{offset//8+1}.upload.jpg')
            original = out.with_name(out.name.removesuffix('.upload.jpg')+'.png')
            canvas.save(original)
            compressor = pathlib.Path.home()/'.codex/tools/screenshot_jpeg.py'
            if compressor.exists():
                subprocess.run([sys.executable,str(compressor),str(original)],capture_output=True,check=True)
            else:
                canvas.save(out,quality=82,subsampling=0,optimize=True)
            outputs.append(out.name)
        report.append({'screenshot':str(screenshot.relative_to(evidence)), 'numericCalls':len(records),'distinctRegions':len(panels),'regions':regions,'uniformRegions':sum(r['uniform'] for r in regions),'outsideFrame':clipped,'closeups':outputs,'evidenceAssessments':calls,'occlusionCandidateRegions':sum(bool(r['occlusionCandidates']) for r in regions),'pendingVisibleRegions':sum(not r['representativeEligible'] for r in regions)})
    (evidence/'number-closeups.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    return report


def main():
    parser = argparse.ArgumentParser(description='依据实机数字绘制区域记录生成数字特写 JPG。')
    parser.add_argument('--evidence-dir',required=True,type=pathlib.Path)
    evidence = parser.parse_args().evidence_dir.resolve()
    if not evidence.is_dir():
        raise SystemExit('实机证据目录不存在。')
    report = make_closeups(evidence)
    print(json.dumps({'screenshots':len(report),'numericCalls':sum(r['numericCalls'] for r in report),'regions':sum(r['distinctRegions'] for r in report),'outsideFrame':sum(len(r['outsideFrame']) for r in report),'occlusionCandidateRegions':sum(r['occlusionCandidateRegions'] for r in report),'pendingVisibleRegions':sum(r['pendingVisibleRegions'] for r in report)},ensure_ascii=False))


if __name__ == '__main__':
    main()
