"""Validate the source raster contract; reads pixels but never modifies generated art."""
from pathlib import Path
import hashlib
import json
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
ART = ROOT / 'Assets/TweenGallery/Resources/CompositionArt'
EXPECTED = {'ivory-panel', 'mint-action', 'cream-action', 'lavender-action',
            'brand-title', 'achievement-title', 'blank-ribbon', 'logo-glyphs'}


def main():
    assert {p.stem for p in ART.glob('*.png')} == EXPECTED
    records = []
    for path in sorted(ART.glob('*.png')):
        im = Image.open(path).convert('RGBA')
        if path.stem == 'ivory-panel':
            # A hollow center is a rejected asset, even if its border looks attractive.
            sample = im.crop((im.width//4, im.height//4, im.width*3//4, im.height*3//4))
            assert sample.getchannel('A').getextrema()[0] == 255, 'Panel interior is not solid'
            for channel in sample.split()[:3]:
                low, high = channel.getextrema()
                assert low > 200 and high-low < 30, 'Panel has dirty or competing center shading'
        else:
            assert im.getpixel((0, 0))[3] == 0, path.name + ' has no transparent outer corner'
        if path.stem.endswith('-action'):
            center = im.getpixel((im.width//2, im.height//2))
            assert center[3] >= 240 and min(center[:3]) > 120, path.name + ' has a missing or dark label surface'
        if path.stem == 'logo-glyphs':
            for x in (0, 1):
                for y in (0, 1):
                    cell = im.crop((x*im.width//2, y*im.height//2, (x+1)*im.width//2, (y+1)*im.height//2))
                    assert cell.getchannel('A').getbbox(), 'Missing independent glyph'
        records.append({'path': path.relative_to(ROOT).as_posix(), 'size': list(im.size),
                        'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                        'center_rgba': list(im.getpixel((im.width//2, im.height//2)))})
    print('PASS 8 reference-derived PNGs: solid panel, quiet button centers, transparent contours, four glyph cells')
    output = ROOT / 'evidence/composition-art-validation.json'
    output.parent.mkdir(exist_ok=True)
    output.write_text(json.dumps({'passed': True, 'assets': records}, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')


if __name__ == '__main__':
    main()
