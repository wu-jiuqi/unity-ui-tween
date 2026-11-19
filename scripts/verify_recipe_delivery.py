"""Verify the portable ZIP against the tested player directory and bind it to reports/screenshots."""
import hashlib,json,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
def digest(path):
    with path.open('rb') as stream:return stream_digest(stream)
def stream_digest(stream):
    sha=hashlib.sha256()
    for block in iter(lambda:stream.read(1024*1024),b''):sha.update(block)
    return sha.hexdigest()
def record(path):return dict(path=path.relative_to(ROOT).as_posix(),sha256=digest(path),bytes=path.stat().st_size)
def main():
    version=(ROOT/'VERSION').read_text(encoding='utf-8-sig').strip()
    player=ROOT/'Builds/Recipes96';bundle=ROOT/f'dist/UIRecipeGallery-{version}-Windows.zip'
    reports=[ROOT/f'docs/validation/recipes96-{width}.json' for width in (1600,1280)]
    for path in reports:
        report=json.loads(path.read_text(encoding='utf-8-sig'))
        assert report['version']==version and report['passed'] and len(report['cases'])==96 and not report['errors'],path
    files={p.relative_to(player).as_posix():p for p in player.rglob('*') if p.is_file()}
    with zipfile.ZipFile(bundle) as archive:
        entries={name for name in archive.namelist() if not name.endswith('/')}
        assert entries==set(files),'ZIP entries differ from tested directory'
        for name,path in files.items():
            with archive.open(name) as stream:assert stream_digest(stream)==digest(path),name
        assert archive.testzip() is None,'ZIP CRC failure'
    screenshots=[ROOT/f'docs/previews/v080/representatives/{id}.png' for id in ('05.03','03.08','08.05','07.04','09.03','11.04')]
    manifest=dict(version=version,windowsZip=record(bundle),verifiedZipFiles=len(files),
        testedPlayer=[record(player/name) for name in ('UIRecipeGallery.exe','UnityPlayer.dll','UIRecipeGallery_Data/Managed/Assembly-CSharp.dll','UIRecipeGallery_Data/globalgamemanagers')],
        reports=[record(p) for p in reports],representativeScreenshots=[record(p) for p in screenshots],
        scope='Scripted Windows Player checks and screenshot provenance; not human visual, rhythm, rapid-input or three-second-comprehension approval')
    target=ROOT/'docs/validation/recipes96-delivery.json';target.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    bundle.with_name(bundle.name+'.sha256').write_text(digest(bundle)+'  '+bundle.name+'\n',encoding='ascii')
    print('Verified portable ZIP,',len(files),'identical files, reports and six representative screenshots')
if __name__=='__main__':main()
