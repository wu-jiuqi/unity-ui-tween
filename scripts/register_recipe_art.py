"""Read-only atlas analysis: write Unity Sprite rects and provenance, never modify generated PNGs."""
import argparse,hashlib,json
from pathlib import Path
from PIL import Image
import numpy as np
from scipy import ndimage

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/TweenGallery/Resources/RecipeArt'
SHEETS={
 'surfaces': ['panel','drawer','tooltip','list-row','list-selected','toast','tab','tab-selected','header'],
 'buttons': ['button-blue','button-orange','button-green','button-purple','button-disabled','button-shadow','button-gloss','button-square','button-close'],
 'inputs': ['toggle-track','toggle-knob','checkbox-off','checkbox-on','radio-off','radio-on','input-normal','input-focus','input-error','slider-track','slider-handle','value-bubble','underline','badge','page-dot','page-dot-active'],
 'cards': ['card-front','card-back','card-art','card-rarity','card-gloss','slot-source','slot-valid','slot-invalid','archive-slot'],
 'rewards': ['chest-body','chest-lid','star-lit','star-unlit','coin','crystal','medal','level-badge','wallet'],
 'bars': ['bar-track','bar-health','bar-damage','bar-xp','bar-gold','step-off','step-on','rank-row','rank-selected'],
 'feedback': ['notice-info','notice-success','notice-error','notice-warning','check','prohibit','arrow','hand','spinner','halo','shine','focus-outline','selection-outline','ghost','avatar','unread'],
 'objects': ['swords','gift','home','lock','gear','heart','xp','bubble','direction-arrow'],
 'approved-cards': ['approved-frame','approved-back','approved-portrait','approved-banner','approved-rarity','approved-gem','approved-glow','approved-amber','approved-potion'],
 'approved-shell': ['wb-sidebar','wb-stage','wb-inspector','wb-gray','wb-title','wb-play','wb-inventory','wb-slider','wb-handle'],
}
ICONS={'chest-body','chest-lid','star-lit','star-unlit','coin','crystal','medal','level-badge','card-art','card-rarity','card-gloss','button-gloss','toggle-knob','slider-handle','badge','page-dot','page-dot-active'}|set(SHEETS['feedback'])|set(SHEETS['objects'])|set(SHEETS['approved-cards'])
def main():
    parts=[];sources=[]
    for atlas,keys in SHEETS.items():
        path=ART/(atlas+'.png')
        if not path.exists():raise SystemExit(f'Missing imagegen atlas: {path}')
        image=Image.open(path).convert('RGBA');w,h=image.size;cols=4 if len(keys)==16 else 3;rows=cols
        a=image.getchannel('A');alpha=a.getextrema()
        if alpha[0]!=0 or alpha[1]!=255:raise SystemExit(f'Actual transparent and opaque alpha required: {atlas} {alpha}')
        sources.append(dict(atlas=atlas,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),width=w,height=h,alpha=list(alpha),source='imagegen',prototype='approved-reference' if atlas.startswith('approved-') else 'controls' if atlas in ('buttons','inputs','objects') else 'cards' if atlas=='cards' else 'surfaces' if atlas in ('surfaces','bars') else 'rewards'))
        # Generated sheets have equal conceptual cells but may shift silhouettes slightly.
        # Group connected alpha components by their grid centroid so a beveled corner is never
        # cut at a mathematical cell boundary. This only computes Sprite rects; PNG stays intact.
        labels,total=ndimage.label(np.asarray(a)>=32)
        boxes=[[] for _ in keys]
        for index,slices in enumerate(ndimage.find_objects(labels),1):
            if slices is None:continue
            yy,xx=slices
            if np.count_nonzero(labels[yy,xx]==index)<48:continue
            col=min(cols-1,int((xx.start+xx.stop)*.5/w*cols));row=min(rows-1,int((yy.start+yy.stop)*.5/h*rows))
            boxes[row*cols+col].append((xx.start,yy.start,xx.stop,yy.stop))
        for i,key in enumerate(keys):
            if not boxes[i]:raise SystemExit(f'Empty sprite cell {atlas}:{key}')
            regions=boxes[i];x=max(0,min(b[0] for b in regions)-3);top=max(0,min(b[1] for b in regions)-3);right=min(w,max(b[2] for b in regions)+3);bottom=min(h,max(b[3] for b in regions)+3)
            sw=right-x;sh=bottom-top
            if sw<20 or sh<12:raise SystemExit(f'Unusable sprite {key}: {sw}x{sh}')
            border=0 if key in ICONS else min(sw,sh)*.22
            if key.startswith('wb-'):border=0 if key=='wb-handle' else min(sw,sh)*.08
            parts.append(dict(key=key,atlas=atlas,x=x,y=h-bottom,width=sw,height=sh,left=border,bottom=border,right=border,top=border))
    # Small fixed tool icons are exact Sprite crops from the user's approved imagegen reference.
    reference=Image.open(ART/'approved-reference.png');height=reference.height
    regions={'ref-logo':(23,7,43,45),'ref-folder':(25,83,35,30),'ref-gear':(1284,81,36,32),'ref-thumbnail':(1286,184,100,113),'approved-exact-portrait':(612,151,326,309),
             'ref-card-icon':(65,178,30,32),'ref-drag-icon':(66,414,33,34),
             'ref-curve0':(1287,466,66,59),'ref-curve1':(1355,466,63,59),'ref-curve2':(1421,466,64,59),'ref-curve3':(1488,466,65,59)}
    for key,(x,y,w,h) in regions.items():parts.append(dict(key=key,atlas='approved-reference',x=x,y=height-y-h,width=w,height=h,left=0,bottom=0,right=0,top=0))
    pointer_path=ART/'approved-pointer.png'
    if pointer_path.exists():
        pointer=Image.open(pointer_path);bounds=pointer.getchannel('A').point(lambda x:255 if x>=32 else 0).getbbox();l,t,r,b=bounds
        parts.append(dict(key='approved-pointer',atlas='approved-pointer',x=l,y=pointer.height-b,width=r-l,height=b-t,left=0,bottom=0,right=0,top=0))
        sources.append(dict(atlas='approved-pointer',sha256=hashlib.sha256(pointer_path.read_bytes()).hexdigest(),width=pointer.width,height=pointer.height,alpha=list(pointer.getchannel('A').getextrema()),source='imagegen',prototype='approved-reference'))
    ghost_path=ART/'approved-ghost.png'
    if ghost_path.exists():
        ghost=Image.open(ghost_path);l,t,r,b=ghost.getchannel('A').point(lambda x:255 if x>=24 else 0).getbbox()
        parts.append(dict(key='approved-ghost',atlas='approved-ghost',x=l,y=ghost.height-b,width=r-l,height=b-t,left=0,bottom=0,right=0,top=0))
        sources.append(dict(atlas='approved-ghost',sha256=hashlib.sha256(ghost_path.read_bytes()).hexdigest(),width=ghost.width,height=ghost.height,alpha=list(ghost.getchannel('A').getextrema()),source='imagegen',prototype='approved-reference'))
    sources.append(dict(atlas='approved-reference',sha256=hashlib.sha256((ART/'approved-reference.png').read_bytes()).hexdigest(),width=reference.width,height=reference.height,source='user-approved imagegen reference',role='authority'))
    ART.joinpath('manifest.json').write_text(json.dumps(dict(schemaVersion=1,parts=parts),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    out=ROOT/'ArtSource/Recipes-v070/atlas-provenance.json';out.write_text(json.dumps(sources,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f'Registered {len(parts)} imagegen sprite parts, unmodified original PNGs')
if __name__=='__main__':main()
