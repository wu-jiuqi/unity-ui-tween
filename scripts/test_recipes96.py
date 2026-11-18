import hashlib,json,unittest
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
def read_image(path):
    with Image.open(path) as image:return image.copy()

class RecipeContracts(unittest.TestCase):
    def test_complete_and_unique_specifications(self):
        rows=json.loads((ROOT/'Assets/TweenGallery/Resources/recipe-catalog.json').read_text(encoding='utf-8'))['recipes']
        self.assertEqual({r['id'] for r in rows},{f'{g:02}.{n:02}' for g in range(1,13) for n in range(1,9)})
        self.assertEqual(len(rows),96)
        for r in rows:
            for field in ('operation','observe','initialState','finalState','boundary','reducedMotion','tags'):
                self.assertTrue(r[field].strip(),(r['id'],field))
    def test_generated_pngs_unchanged_and_alpha_present(self):
        sources=json.loads((ROOT/'ArtSource/Recipes-v070/atlas-provenance.json').read_text(encoding='utf-8'))
        self.assertEqual(len(sources),13)
        for source in sources:
            path=ROOT/'Assets/TweenGallery/Resources/RecipeArt'/(source['atlas']+'.png')
            self.assertEqual(hashlib.sha256(path.read_bytes()).hexdigest(),source['sha256'])
            im=read_image(path)
            if source.get('role')!='authority':
                self.assertEqual(im.mode,'RGBA')
                lo,hi=im.getchannel('A').getextrema();self.assertEqual(lo,0)
                if source['atlas'] in ('approved-pointer','approved-ghost'):self.assertGreaterEqual(hi,220)
                else:self.assertEqual(hi,255)
    def test_sprite_rects_and_solid_text_faces(self):
        parts=json.loads((ROOT/'Assets/TweenGallery/Resources/RecipeArt/manifest.json').read_text(encoding='utf-8'))['parts']
        self.assertEqual(len(parts),117);self.assertEqual(len({p['key'] for p in parts}),117)
        solid={'panel','drawer','tooltip','list-row','toast','tab','tab-selected','button-blue','button-orange','button-green','button-purple','button-disabled','input-normal','input-focus','input-error','card-front','card-back','wallet'}
        for part in parts:
            image=read_image(ROOT/'Assets/TweenGallery/Resources/RecipeArt'/(part['atlas']+'.png'))
            self.assertGreaterEqual(part['x'],0);self.assertGreaterEqual(part['y'],0)
            self.assertLessEqual(part['x']+part['width'],image.width);self.assertLessEqual(part['y']+part['height'],image.height)
            self.assertLess(part['left']+part['right'],part['width']);self.assertLess(part['top']+part['bottom'],part['height'])
            if part['key'] in solid:
                x=int(part['x']+part['width']/2);y=int(image.height-part['y']-part['height']/2)
                self.assertGreater(image.getpixel((x,y))[3],220,part['key'])
    def test_reference_prototypes_present(self):
        for name in ('controls','surfaces','cards','rewards'):
            image=read_image(ROOT/f'ArtSource/Recipes-v070/prototypes/{name}.png')
            self.assertGreater(image.width,1000)
    def test_player_evidence_when_available(self):
        folder=ROOT/'docs/validation'
        paths=[folder/'recipes96-1600.json',folder/'recipes96-1280.json']
        for path in paths:
            if not path.exists():continue
            report=json.loads(path.read_text(encoding='utf-8'));self.assertTrue(report['passed'],str(path));self.assertEqual(len(report['cases']),96);self.assertFalse(report['errors'])

if __name__=='__main__':unittest.main()
