"""Collect actual Player reports and screenshots for the 0.8 presentation series."""
import json,shutil
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1]
def main():
    version=(ROOT/'VERSION').read_text(encoding='utf-8-sig').strip()
    for width in (1600,1280):
        source=ROOT/f'validation-output/recipes96-{width}/report.json';report=json.loads(source.read_text(encoding='utf-8-sig'))
        assert report['version']==version and report['passed'] and len(report['cases'])==96,report['errors']
        (ROOT/f'docs/validation/recipes96-{width}.json').write_text(json.dumps(report,ensure_ascii=False,indent=4)+'\n',encoding='utf-8')
    shutil.copy2(ROOT/'evidence/recipes96/build.json',ROOT/'docs/validation/recipes96-build.json')
    source=ROOT/'validation-output/recipes96-1600';dest=ROOT/'docs/previews/v080'
    for folder in ('cases','sheets','representatives'):(dest/folder).mkdir(parents=True,exist_ok=True)
    shutil.copy2(source/'reference-default.png',dest/'default.png')
    for key in ('05.03','03.08','08.05','07.04','09.03','11.04'):shutil.copy2(source/f'focus-{key}.png',dest/f'representatives/{key}.png')
    for group in range(1,13):
        sheet=Image.new('RGB',(1600,744),'#121822');draw=ImageDraw.Draw(sheet)
        for i in range(8):
            key=f'{group:02d}.{i+1:02d}';path=source/f'{key}.png';shutil.copy2(path,dest/f'cases/{key}.png')
            with Image.open(path) as im:
                tile=im.crop((300,65,1290,895));tile.thumbnail((400,336));x=i%4*400;y=i//4*372;sheet.paste(tile,(x,y));draw.text((x+12,y+339),key,fill='white',font_size=20)
        sheet.save(dest/f'sheets/{group:02d}.jpg',quality=88)
    # Two-row overview of the six representative stage captures, without recreating game art.
    board=Image.new('RGB',(1500,900),'#121822');draw=ImageDraw.Draw(board)
    for i,key in enumerate(('05.03','03.08','08.05','07.04','09.03','11.04')):
        with Image.open(dest/f'representatives/{key}.png') as im:
            tile=im.crop((300,65,1290,895));tile.thumbnail((500,420));x=i%3*500;y=i//3*450;board.paste(tile,(x,y));draw.text((x+12,y+423),key,fill='white',font_size=20)
    board.save(dest/'representatives.jpg',quality=90)
    print('Collected current reports, 96 native screenshots, six representatives and category sheets')
if __name__=='__main__':main()
