#!/usr/bin/env python3
"""Sync tracked Skill reference copies from canonical docs (or check for drift)."""
import argparse
from package_skill import PROJECT, SKILL, DOC_NAMES


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    drift = []
    for name in DOC_NAMES:
        source, target = PROJECT / "docs" / name, SKILL / "references" / name
        data = source.read_bytes()
        if not target.exists() or target.read_bytes() != data:
            drift.append(name)
            if not args.check:
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(data)
    if args.check and drift:
        print("Stale Skill references: " + ", ".join(drift))
        return 1
    print("Skill references are synchronized with docs/")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
