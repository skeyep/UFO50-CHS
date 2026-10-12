"""Precise name/caret and selected-grid JPG crops from recorded name-entry states.

Supply runtime state JSON, actual Draw text/sprite records, full screenshots and
sprite metadata; this tool has no project task-directory or installation dependency.
The default state/caller/grid geometry matches the Grimstone name-entry fixture.
"""
import argparse
import json
import math
import subprocess
import sys
from pathlib import Path
from PIL import Image, ImageDraw

def compress_jpg(png):
    jpeg_tool=Path.home()/'.codex/tools/screenshot_jpeg.py'
    if jpeg_tool.exists():
        subprocess.run([sys.executable,str(jpeg_tool),str(png)],check=True,capture_output=True)
    else:
        with Image.open(png) as source:
            source.convert('RGB').save(png.with_name(png.stem+'.upload.jpg'),quality=82,subsampling=0,optimize=True)

def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def crop_box(im, box):
    return im.crop((max(0,math.floor(box[0])),max(0,math.floor(box[1])),min(im.width,math.ceil(box[2])),min(im.height,math.ceil(box[3]))))
def display_box(row,sx,sy):
    vx=0 if row.get('gui') else row.get('viewX',0);vy=0 if row.get('gui') else row.get('viewY',0)
    return [(row['x']-vx)*sx,(row['y']-vy)*sy,(row['x']-vx+row['width'])*sx,(row['y']-vy+row['height'])*sy]
def main():
    ap=argparse.ArgumentParser(description='从实际姓名输入状态、绘制记录和精灵元数据生成最近邻JPG特写。')
    ap.add_argument('--evidence-dir',type=Path,required=True)
    ap.add_argument('--sprite-metadata',type=Path,required=True)
    ap.add_argument('--game-id',type=int,default=12)
    ap.add_argument('--state-pattern',default='qa-name12-*.json')
    ap.add_argument('--name-caller',default='o12__Game')
    ap.add_argument('--cursor-caller',default='o12_NameChar')
    ap.add_argument('--hand-sprite',default='s12_Hand')
    ap.add_argument('--name-y',type=float,default=80)
    args=ap.parse_args()
    args.evidence_dir=args.evidence_dir.resolve()
    folder=args.evidence_dir/f'game-{args.game_id}';meta={x['sprite']:x for x in read(args.sprite_metadata.resolve())}
    result=[];panels=[]
    selected={'name1-cursor0','name3-cursor0','name5-cursor0','name3-insert1','name5-insert4','delete-icon-used','kana-toggled','grid-row0-first','grid-row0-last','grid-row5-first','grid-row5-last'}
    outdir=args.evidence_dir/'name-closeups';outdir.mkdir(exist_ok=True)
    for diagpath in sorted(folder.glob(args.state_pattern)):
        diag=read(diagpath)
        if 'shot' not in diag:continue
        shot=diag['shot'];stem=f'game-{args.game_id}-{shot}';imagepath=folder/f'{stem}.png';textpath=folder/f'{stem}-texts.json';spritepath=folder/f'{stem}-sprites.json'
        if not imagepath.exists() or not textpath.exists():continue
        with Image.open(imagepath) as source:
            im=source.convert('RGB')
        rows=read(textpath);sprites=read(spritepath) if spritepath.exists() else []
        name_rows=[r for r in rows if r.get('caller')==args.name_caller and r.get('text')==diag['name'] and abs(r.get('y',0)-r.get('viewY',0)-args.name_y)<=3]
        if not name_rows:continue
        name=name_rows[0];sx=im.width/name['viewWidth'];sy=im.height/name['viewHeight'];box=display_box(name,sx,sy)
        cursorrows=[r for r in sprites if r.get('caller')==args.cursor_caller]
        cursor_records=[]
        for cursor in cursorrows:
            sm=meta.get(cursor['sprite']);frame=int(math.floor(cursor['frame']))%(sm['frames'] if sm else 1)
            opaque=next((f for f in sm['opaque'] if f['frame']==frame),None) if sm else None
            cursor_records.append({'runtime':cursor,'frame':frame,'opaque':opaque,'rendered':bool(cursor.get('visible') and opaque and opaque['bbox'])})
            if sm:
                scale_x=cursor.get('scaleX',1);scale_y=cursor.get('scaleY',1)
                cx=(cursor['x']-cursor['viewX']-sm['originX']*scale_x)*sx;cy=(cursor['y']-cursor['viewY']-sm['originY']*scale_y)*sy
                box=[min(box[0],cx),min(box[1],cy),max(box[2],cx+sm['width']*abs(scale_x)*sx),max(box[3],cy+sm['height']*abs(scale_y)*sy)]
        box=[box[0]-4*sx,box[1]-4*sy,box[2]+8*sx,box[3]+8*sy]
        crop=crop_box(im,box).resize((max(1,round((min(im.width,box[2])-max(0,box[0]))*2)),max(1,round((min(im.height,box[3])-max(0,box[1]))*2))),Image.Resampling.NEAREST)
        panel=Image.new('RGB',(max(320,crop.width),crop.height+24),(24,24,24));panel.paste(crop,(0,24))
        ImageDraw.Draw(panel).text((4,4),f'{shot} | index={diag["currNameChar"]} nameW={name["width"]} nameH={name["height"]}',fill='white')
        png=outdir/f'{shot}-name.png';panel.save(png)
        compress_jpg(png)
        rec={'shot':shot,'diag':str(diagpath),'nameDraw':name,'cursorRecords':cursor_records,'nameCrop':str(png.with_name(png.stem+'.upload.jpg')),
             'assessment':'visual review required; original overwrite block may cover the selected glyph'}
        result.append(rec)
        if shot in selected:panels.append(panel)
        # Character rows use their actual trace geometry; the original selected hand is separate.
        kana=any('☐' in str(r.get('text','')) for r in rows)
        target_y=(96 if kana else 112)+16*diag['my']
        grid_rows=[r for r in rows if r.get('caller')==args.name_caller and abs(r.get('y',0)-r.get('viewY',0)-target_y)<=3
                   and (('☐' in str(r.get('text',''))) if kana else r.get('width',999)<=8)]
        hands=[r for r in sprites if r.get('sprite')==args.hand_sprite and abs(r.get('y',0)-r.get('viewY',0)-(target_y-2))<=2]
        rec['selectedGridRows']=grid_rows;rec['selectedGridHands']=hands
        if grid_rows:
            boxes=[display_box(r,sx,sy) for r in grid_rows]
            for hand in hands:
                sm=meta[args.hand_sprite];frame=int(math.floor(hand.get('frame',0)))%sm['frames']
                opaque=next(x for x in sm['opaque'] if x['frame']==frame)
                if opaque['bbox']:
                    b=opaque['bbox'];ix=hand.get('scaleX',1);iy=hand.get('scaleY',1)
                    boxes.append([(hand['x']-hand['viewX']+(b[0]-sm['originX'])*ix)*sx,
                                  (hand['y']-hand['viewY']+(b[1]-sm['originY'])*iy)*sy,
                                  (hand['x']-hand['viewX']+(b[2]-sm['originX'])*ix)*sx,
                                  (hand['y']-hand['viewY']+(b[3]-sm['originY'])*iy)*sy])
            grid_box=[min(b[0] for b in boxes)-4*sx,min(b[1] for b in boxes)-4*sy,max(b[2] for b in boxes)+4*sx,max(b[3] for b in boxes)+4*sy]
            raw=crop_box(im,grid_box);zoom=raw.resize((raw.width*2,raw.height*2),Image.Resampling.NEAREST)
            grid_panel=Image.new('RGB',(max(320,zoom.width),zoom.height+24),(24,24,24));grid_panel.paste(zoom,(0,24))
            ImageDraw.Draw(grid_panel).text((4,4),f'{shot} | grid({diag["mx"]},{diag["my"]}) rowW={[r["width"] for r in grid_rows]}',fill='white')
            grid_png=outdir/f'{shot}-grid.png';grid_panel.save(grid_png)
            compress_jpg(grid_png)
            rec['gridCrop']=str(grid_png.with_name(grid_png.stem+'.upload.jpg'))
            if shot in selected:panels.append(grid_panel)
    boards=[]
    for start in range(0,len(panels),5):
        group=panels[start:start+5];board=Image.new('RGB',(max(x.width for x in group),sum(x.height for x in group)+8*(len(group)-1)),(24,24,24));y=0
        for panel in group:board.paste(panel,(0,y));y+=panel.height+8
        png=outdir/f'name-cursor-board-{start//5+1}.png';board.save(png)
        compress_jpg(png)
        boards.append(str(png.with_name(png.stem+'.upload.jpg')))
    (args.evidence_dir/'name-closeups.json').write_text(json.dumps({'rows':result,'boards':boards,'visualReview':'pending'},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'nameRegions':len(result),'boards':boards},ensure_ascii=False))
if __name__=='__main__':main()
