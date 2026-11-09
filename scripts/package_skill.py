#!/usr/bin/env python3
"""Build a portable, self-contained Skill zip without Unity or a Windows build.

The project Assets directory is the source of truth. The Git-ignored Skill/assets
directory is deliberately ignored: it may be missing or belong to an older build.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import sys
import zipfile

from portable_zip import write_entry

PROJECT = Path(__file__).resolve().parent.parent
SKILL = PROJECT / "Skill" / "unity-ui-tween"
UNITY = PROJECT / "Assets" / "TweenGallery"
EXPECTED = {f"{group}{n:02d}" for group, size in (("A", 8), ("B", 8), ("C", 7), ("D", 10), ("E", 7), ("F", 6)) for n in range(1, size + 1)}
# UGUI 2.0.0 built-in Image/Text/Button/Mask/RectMask2D/Outline/Shadow
# component script GUIDs, shipped by Unity rather than embedded in this ZIP.
UGUI_GUIDS = {"fe87c0e1cc204ed48ad3b37840f39efc", "5f7201a12d95ffc409449d95f23cf332", "cfabb0440166ab443bba8876756fdfa9", "3312d7739989d2b4e91e6319e9a96d76", "4e29b1a8efbd4b44bb3f3716e73f07ff", "e19747de3f5aca642ab2be37e372fb86", "31a19414c41e5ae4aae2af33fee712f6"}


def require(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


DOC_NAMES = ("acceptance.md", "catalog.md", "integration.md", "interaction.md",
             "mobile-performance.md", "orchestration.md", "skill-packaging.md", "runtime-package.md",
             "button-design.md", "button-recipes.md")


def runtime_asset(relative: str) -> bool:
    first = relative.split("/")[0]
    return (first in {"Runtime", "Optional", "Shaders", "Presets"}
            or relative in {"Runtime.meta", "Optional.meta", "Shaders.meta", "Presets.meta",
                            "Resources.meta", "Resources/motion-catalog.json",
                            "Resources/motion-catalog.json.meta"})


def collect_files(profile: str = "full") -> dict[str, Path]:
    require(profile in ("full", "runtime"), "Unknown package profile")
    require((SKILL / "SKILL.md").is_file(), "Missing Skill/SKILL.md")
    require(UNITY.is_dir(), "Missing Assets/TweenGallery")
    require((PROJECT / "Assets" / "TweenGallery.meta").is_file(), "Missing TweenGallery.meta")
    catalog_path = UNITY / "Resources" / "motion-catalog.json"
    catalog = json.loads(catalog_path.read_text(encoding="utf-8-sig"))
    effects = catalog.get("effects", [])
    ids = [effect.get("id") for effect in effects]
    require(len(ids) == len(EXPECTED) and set(ids) == EXPECTED, "Catalog must contain each original A01–F06 exactly once")
    for required in ("Runtime/MotionPlayer.cs", "Runtime/MotionBindings.cs", "Runtime/MotionCatalog.cs", "Shaders/UITextureBlur.shader", "Resources/GalleryFont.otf", "Licenses/NotoSansCJK-OFL.txt"):
        require((UNITY / required).is_file(), f"Missing template dependency: {required}")
    for effect_id in sorted(EXPECTED):
        for directory, extension in (("Prefabs", ".prefab"), ("Presets", ".asset")):
            path = UNITY / directory / (effect_id + extension)
            require(path.is_file() and Path(str(path) + ".meta").is_file(), f"Missing {directory} asset or .meta: {effect_id}")
    # All imported assets, including directories, need stable Unity GUIDs.
    for path in UNITY.rglob("*"):
        require(not path.is_symlink(), f"Symbolic links are not allowed: {path}")
        if path.name.endswith(".meta"):
            continue
        require(Path(str(path) + ".meta").is_file(), f"Missing Unity .meta: {path.relative_to(PROJECT)}")
    guid_paths: dict[str, Path] = {}
    for path in [PROJECT / "Assets" / "TweenGallery.meta", *UNITY.rglob("*.meta")]:
        content = path.read_text(encoding="utf-8-sig")
        match = re.search(r"^guid:\s*([0-9a-fA-F]{32})\s*$", content, re.MULTILINE)
        require(match is not None, f"Invalid Unity GUID: {path.relative_to(PROJECT)}")
        guid = match.group(1).lower()
        require(guid not in guid_paths, f"Duplicate Unity GUID in {path} and {guid_paths.get(guid)}")
        guid_paths[guid] = path
    for path in UNITY.rglob("*"):
        if path.suffix not in (".prefab", ".unity", ".asset") or not path.is_file():
            continue
        # Unity .asset may be binary (e.g. generated Texture2D); GUID references
        # in serialized YAML are checked, binary content is packaged verbatim.
        content = path.read_bytes()
        if not content.startswith(b"%YAML"):
            continue
        for match in re.finditer(rb"\bguid:\s*([0-9a-fA-F]{32})", content):
            guid = match.group(1).decode("ascii").lower()
            require(guid in guid_paths or guid in UGUI_GUIDS or guid.startswith("0000000000000000"), f"Unresolved asset GUID {guid} in {path.relative_to(PROJECT)}")
    out: dict[str, Path] = {}
    for path in sorted(SKILL.rglob("*")):
        if path.is_dir() or "assets" in path.relative_to(SKILL).parts:
            continue
        require(not path.is_symlink(), f"Symbolic links are not allowed: {path}")
        out[(PurePosixPath("unity-ui-tween") / PurePosixPath(path.relative_to(SKILL).as_posix())).as_posix()] = path
    for path in sorted(UNITY.rglob("*")):
        if path.is_file() and (profile == "full" or runtime_asset(path.relative_to(UNITY).as_posix())):
            out[(PurePosixPath("unity-ui-tween/assets/TweenGallery") / PurePosixPath(path.relative_to(UNITY).as_posix())).as_posix()] = path
    out["unity-ui-tween/assets/TweenGallery.meta"] = PROJECT / "Assets" / "TweenGallery.meta"
    # The docs directory is canonical; never ship stale Skill/reference copies.
    for name in DOC_NAMES:
        document = PROJECT / "docs" / name
        require(document.is_file(), f"Missing canonical document: docs/{name}")
        out[f"unity-ui-tween/references/{name}"] = document
    if profile == "runtime":
        # Check selected YAML against the selected package, not the full tree.
        selected_guids = {re.search(r"^guid:\s*([0-9a-fA-F]{32})", path.read_text(encoding="utf-8-sig"), re.MULTILINE).group(1).lower()
                          for name, path in out.items() if name.endswith(".meta")}
        for name, path in out.items():
            if path.suffix not in (".prefab", ".unity", ".asset"):
                continue
            data = path.read_bytes()
            if data.startswith(b"%YAML"):
                for guid_bytes in re.findall(rb"\bguid:\s*([0-9a-fA-F]{32})", data):
                    guid = guid_bytes.decode("ascii").lower()
                    require(guid in selected_guids or guid in UGUI_GUIDS or guid.startswith("0000000000000000"),
                            f"Runtime profile omits dependency {guid} in {name}")
    return dict(sorted(out.items()))


def digest_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def verify_archive(archive: Path, sources: dict[str, Path]) -> None:
    with zipfile.ZipFile(archive) as zf:
        entries = zf.namelist()
        require(len(entries) == len(set(entries)) and set(entries) == set(sources), "ZIP entries do not match source manifest")
        for name in entries:
            parts = PurePosixPath(name).parts
            require("\\" not in name and not name.startswith("/") and ".." not in parts, f"Non-portable or unsafe ZIP path: {name}")
            require(hashlib.sha256(zf.read(name)).hexdigest() == digest_file(sources[name]), f"ZIP content differs: {name}")
        require(zf.testzip() is None, "ZIP CRC check failed")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, help="Output ZIP (default: dist/unity-ui-tween-VERSION-standalone.zip)")
    parser.add_argument("--profile", choices=("full", "runtime"), default="full", help="Full gallery assets or lean own-UI runtime integration")
    parser.add_argument("--force", action="store_true", help="Replace an existing output ZIP")
    args = parser.parse_args()
    version = (PROJECT / "VERSION").read_text(encoding="utf-8-sig").strip()
    require(version and all(c.isalnum() or c in ".-_" for c in version), "Invalid VERSION")
    output = (args.output or PROJECT / "dist" / f"unity-ui-tween-{version}-{'standalone' if args.profile == 'full' else 'runtime'}.zip").resolve()
    require(args.force or not output.exists(), f"Output already exists (use --force to replace): {output}")
    require(not any(root.resolve() == output or root.resolve() in output.parents
                    for root in (UNITY, SKILL, PROJECT / "docs")),
            "ZIP destination cannot be inside source assets, Skill or docs")
    checksum = output.with_name(output.name + ".sha256")
    require(args.force or not checksum.exists(), f"Checksum already exists (use --force to replace): {checksum}")
    sources = collect_files(args.profile)
    output.parent.mkdir(parents=True, exist_ok=True)
    temp = output.with_name(output.name + ".tmp")
    require(not temp.exists(), f"Temporary output already exists: {temp}")
    try:
        with zipfile.ZipFile(temp, "x", compression=zipfile.ZIP_DEFLATED, compresslevel=6, allowZip64=True) as zf:
            for name, source in sources.items():
                write_entry(zf, name, source)
        verify_archive(temp, sources)
        os.replace(temp, output)
    finally:
        if temp.exists():
            temp.unlink()
    sha256 = digest_file(output)
    checksum.write_text(f"{sha256}  {output.name}\n", encoding="ascii")
    print(f"Verified profile={args.profile}; {len(sources)} entries; 46 effects; UGUI 2.0.0 and DOTween are external dependencies.")
    print(f"ZIP: {output}")
    print(f"SHA256: {sha256}")
    print(f"Checksum: {checksum}")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, KeyError, json.JSONDecodeError, zipfile.BadZipFile) as error:
        print(f"Skill packaging failed: {error}", file=sys.stderr)
        sys.exit(1)
