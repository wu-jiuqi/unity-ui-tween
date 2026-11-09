"""Inventory source evidence without treating QA, geometry or physics as UI tween recipes."""
import argparse
import hashlib
import json
from pathlib import Path
import re

PATTERN = re.compile(r'DOTween|\.DO[A-Z]|Mathf\.(?:Sin|Cos|PingPong)|AnimationCurve|SmoothDamp|(?:Lerp|SmoothStep)\(|maxVisibleCharacters')
API = re.compile(r'\.(DO\w+|SetEase|SetLoops|Append|Insert|Join|SetUpdate|SetDelay|OnComplete|AppendInterval|AppendCallback|InsertCallback)\(')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--scripts', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    files = sorted(args.scripts.rglob('*.cs'))
    records = []
    for path in files:
        source = path.read_text(encoding='utf-8-sig')
        lines = [i for i, line in enumerate(source.splitlines(), 1) if PATTERN.search(line)]
        if not lines:
            continue
        relative = path.relative_to(args.scripts).as_posix()
        records.append({'path': relative, 'motion_candidate_lines': lines,
                        'apis': sorted(set(API.findall(source))),
                        'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                        'scope': 'UI candidate' if relative.startswith('UI/') else 'manual review: world/physics/test/research'})
    result = {'source_root': 'BounceGarden/BounceGarden/Assets/Scripts', 'source_files_scanned': len(files),
              'ui_motion_candidates': sum(r['scope'] == 'UI candidate' for r in records),
              'note': 'Regex finds evidence candidates, not semantic types. Manual mapping is in project-motion-audit.md. QA, physics and texture-generation math are not counted as UI tweens.', 'files': records}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(f'Scanned {len(files)} scripts; {result["ui_motion_candidates"]} UI motion candidates; manual classification required')


if __name__ == '__main__':
    main()
