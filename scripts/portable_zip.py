#!/usr/bin/env python3
"""Create and verify an interoperable ZIP without shell path conventions."""
from __future__ import annotations
import argparse
import hashlib
import os
from pathlib import Path, PurePosixPath
import sys
import zipfile


def digest(stream):
    h = hashlib.sha256()
    for chunk in iter(lambda: stream.read(1024 * 1024), b""):
        h.update(chunk)
    return h.digest()


def package(source: Path, target: Path, include_root: bool = False, force: bool = False):
    source = source.resolve(strict=True)
    target = target.resolve()
    if not source.is_dir(): raise ValueError("Source must be a directory")
    if source == target or source in target.parents: raise ValueError("ZIP destination cannot be inside source")
    if target.exists() and not force: raise ValueError("ZIP exists; pass --force to replace it")
    paths = sorted(source.rglob("*"))
    files = {}
    directories = set()
    for path in paths:
        if path.is_symlink(): raise ValueError(f"Symbolic links are not supported: {path}")
        rel = PurePosixPath(path.relative_to(source).as_posix())
        name = str(PurePosixPath(source.name) / rel) if include_root else str(rel)
        if "\\" in name or name.startswith("/") or ".." in PurePosixPath(name).parts:
            raise ValueError(f"Non-portable path: {name}")
        if path.is_dir(): directories.add(name + "/")
        elif path.is_file(): files[name] = path
        else: raise ValueError(f"Unsupported file type: {path}")
    if include_root: directories.add(source.name + "/")
    target.parent.mkdir(parents=True, exist_ok=True)
    temp = target.with_name(target.name + ".tmp")
    if temp.exists(): raise ValueError(f"Temporary output already exists: {temp}")
    try:
        with zipfile.ZipFile(temp, "x", zipfile.ZIP_DEFLATED, compresslevel=6, allowZip64=True) as z:
            for name in sorted(directories):
                z.writestr(name, b"")
            for name, path in files.items():
                z.write(path, name)
        with zipfile.ZipFile(temp) as z:
            entries = z.namelist()
            if len(entries) != len(set(entries)) or set(entries) != set(files) | directories:
                raise ValueError("ZIP entry set mismatch")
            for name in entries:
                if "\\" in name or name.startswith("/") or ".." in PurePosixPath(name).parts:
                    raise ValueError(f"Unsafe ZIP entry: {name}")
            for name, path in files.items():
                with path.open("rb") as a, z.open(name) as b:
                    if digest(a) != digest(b): raise ValueError(f"ZIP file mismatch: {name}")
            bad = z.testzip()
            if bad: raise ValueError(f"CRC error: {bad}")
        os.replace(temp, target)
    finally:
        if temp.exists(): temp.unlink()
    print(f"Verified {len(files)} files, {len(directories)} directories: {target}")


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("source", type=Path)
    p.add_argument("output", type=Path)
    p.add_argument("--include-root", action="store_true")
    p.add_argument("--force", action="store_true")
    a = p.parse_args()
    package(a.source, a.output, a.include_root, a.force)


if __name__ == "__main__":
    try: main()
    except (OSError, ValueError, zipfile.BadZipFile) as error:
        print(f"Portable ZIP failed: {error}", file=sys.stderr)
        sys.exit(1)
