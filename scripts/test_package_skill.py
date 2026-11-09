"""Checks the Skill package validator using an isolated synthetic project."""
import importlib.util
import json
import os
from pathlib import Path
import tempfile
import unittest
import zipfile
import uuid

spec = importlib.util.spec_from_file_location("package_skill", Path(__file__).with_name("package_skill.py"))
pkg = importlib.util.module_from_spec(spec)
spec.loader.exec_module(pkg)


class PackageTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="unity-skill-test-")
        root = Path(self.temp.name)
        self.old = pkg.PROJECT, pkg.SKILL, pkg.UNITY
        pkg.PROJECT, pkg.SKILL, pkg.UNITY = root, root / "Skill/unity-ui-tween", root / "Assets/TweenGallery"
        pkg.SKILL.mkdir(parents=True)
        (pkg.SKILL / "SKILL.md").write_text("---\nname: unity-ui-tween\n---\n", encoding="utf-8")
        self.write_asset("Resources/motion-catalog.json", json.dumps({"effects": [{"id": i} for i in sorted(pkg.EXPECTED)]}))
        for name in ("Runtime/MotionPlayer.cs", "Runtime/MotionBindings.cs", "Runtime/MotionCatalog.cs", "Shaders/UITextureBlur.shader", "Resources/GalleryFont.otf", "Licenses/NotoSansCJK-OFL.txt"):
            self.write_asset(name, "test")
        for name in sorted(pkg.EXPECTED):
            self.write_asset("Prefabs/" + name + ".prefab", name)
            self.write_asset("Presets/" + name + ".asset", name)
        self.meta(pkg.UNITY)
        (root / "docs").mkdir()
        for name in pkg.DOC_NAMES:
            (root / "docs" / name).write_text("canonical " + name, encoding="utf-8")

    def tearDown(self):
        pkg.PROJECT, pkg.SKILL, pkg.UNITY = self.old
        self.temp.cleanup()

    def write_asset(self, relative, content):
        path = pkg.UNITY / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        directory = path.parent
        while directory != pkg.UNITY:
            self.meta(directory)
            directory = directory.parent
        path.write_text(content, encoding="utf-8")
        self.meta(path)

    def meta(self, path):
        guid = uuid.uuid5(uuid.NAMESPACE_URL, str(path)).hex
        Path(str(path) + ".meta").write_text("guid: " + guid + "\n", encoding="utf-8")

    def test_portable_archive_and_tamper_detection(self):
        # No Builds/Windows, executable, or Unity invocation is needed.
        sources = pkg.collect_files()
        archive = pkg.PROJECT / "test.zip"
        with zipfile.ZipFile(archive, "w") as z:
            for name, source in sources.items():
                z.write(source, name)
        pkg.verify_archive(archive, sources)
        self.assertTrue(all("\\" not in name for name in sources))
        (pkg.UNITY / "Prefabs/A01.prefab").write_text("modified after package", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "content differs"):
            pkg.verify_archive(archive, sources)

    def test_runtime_profile_excludes_demo_assets(self):
        self.write_asset("Resources/MotionArt/background.png", "large gallery art")
        sources = pkg.collect_files("runtime")
        names = set(sources)
        self.assertEqual(sum(name.endswith(".asset") for name in names), 46)
        self.assertTrue(any(name.endswith("Resources/motion-catalog.json") for name in names))
        self.assertFalse(any("/Prefabs/" in name or "/MotionArt/" in name or "GalleryFont" in name for name in names))
        full = pkg.collect_files()
        self.assertEqual(sum(name.endswith(".prefab") for name in full), 46)
        self.assertTrue(any("/MotionArt/" in name for name in full))

    def test_runtime_dependency_cannot_reference_excluded_gallery_asset(self):
        meta = (pkg.UNITY / "Resources/GalleryFont.otf.meta").read_text()
        guid = meta.split()[1]
        self.write_asset("Presets/A01.asset", "%YAML 1.1\nfont: {guid: " + guid + "}\n")
        pkg.collect_files("full")
        with self.assertRaisesRegex(ValueError, "omits dependency"):
            pkg.collect_files("runtime")

    def test_all_canonical_documents_ship_without_tracked_copies(self):
        for profile in ("full", "runtime"):
            with self.subTest(profile=profile):
                sources = pkg.collect_files(profile)
                archive = pkg.PROJECT / (profile + ".zip")
                with zipfile.ZipFile(archive, "w") as z:
                    for name, source in sources.items():
                        pkg.write_entry(z, name, source)
                pkg.verify_archive(archive, sources)
                with zipfile.ZipFile(archive) as z:
                    for name in pkg.DOC_NAMES:
                        self.assertEqual(z.read("unity-ui-tween/references/" + name),
                                         (pkg.PROJECT / "docs" / name).read_bytes())

    def test_canonical_documents_override_stale_skill_copies(self):
        refs = pkg.SKILL / "references"
        refs.mkdir()
        for name in pkg.DOC_NAMES:
            (refs / name).write_text("stale [broken](missing.md)", encoding="utf-8")
        for profile in ("full", "runtime"):
            with self.subTest(profile=profile):
                sources = pkg.collect_files(profile)
                for name in pkg.DOC_NAMES:
                    self.assertEqual(sources["unity-ui-tween/references/" + name],
                                     pkg.PROJECT / "docs" / name)
        (pkg.PROJECT / "docs/integration.md").unlink()
        for profile in ("full", "runtime"):
            with self.subTest(profile=profile), self.assertRaisesRegex(ValueError, "Missing canonical"):
                pkg.collect_files(profile)

    def test_markdown_links_resolve_from_archive_paths(self):
        (pkg.SKILL / "guide (v2).md").write_text("guide", encoding="utf-8")
        (pkg.SKILL / "guide(v2).md").write_text("guide", encoding="utf-8")
        (pkg.SKILL / "SKILL.md").write_text(
            '[guide](references/integration.md#setup)\n'
            '[space](guide%20%28v2%29.md "optional title")\n'
            '[angle](<guide (v2).md>)\n'
            '[parentheses](guide(v2).md)\n'
            r'[escaped](guide\(v2\).md)' + '\n'
            '[external](https://example.org/missing.md)\n'
            '[relative protocol](//example.org/missing.md)\n'
            '[email](mailto:example@example.org) [anchor](#section)\n'
            '`[example](missing-inline.md)`\n'
            '```markdown\n[example](missing-fenced.md)\n```\n'
            '~~~markdown\n[example](missing-tilde.md)\n~~~\n', encoding="utf-8")
        (pkg.PROJECT / "docs/integration.md").write_text(
            '[home](../SKILL.md) [peer](./acceptance.md?view=1#test)\n'
            '[source](../assets/TweenGallery/Runtime/MotionPlayer.cs)\n', encoding="utf-8")
        for profile in ("full", "runtime"):
            with self.subTest(profile=profile):
                pkg.collect_files(profile)

    def test_markdown_links_reject_missing_and_escaping_targets(self):
        for target in ("references/missing.md", "../outside.md", "%2e%2e/outside.md",
                       "references/../../unity-ui-tween/SKILL.md", "/SKILL.md",
                       "%2FSKILL.md", "references%5Cacceptance.md", "file:///tmp/example.md",
                       "file://server/share/example.md", "C:/skills/example.md", "C%3A%5Cskills%5Cexample.md"):
            with self.subTest(target=target):
                (pkg.SKILL / "SKILL.md").write_text(f"[bad]({target})", encoding="utf-8")
                with self.assertRaisesRegex(ValueError, "Broken package Markdown link.*SKILL.md"):
                    pkg.collect_files()

    def test_markdown_image_links_are_checked(self):
        (pkg.SKILL / "SKILL.md").write_text("![missing image](absent.png)", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "missing or omitted"):
            pkg.collect_files()

    def test_markdown_links_reject_assets_omitted_by_profile(self):
        (pkg.PROJECT / "docs/integration.md").write_text(
            "[demo](../assets/TweenGallery/Prefabs/A01.prefab)", encoding="utf-8")
        pkg.collect_files("full")
        with self.assertRaisesRegex(ValueError, "Broken package Markdown link.*integration.md.*missing or omitted"):
            pkg.collect_files("runtime")

    def test_reproducible_skill_zip_ignores_mtime(self):
        sources = pkg.collect_files()
        archives = [pkg.PROJECT / "first.zip", pkg.PROJECT / "second.zip"]
        for index, archive in enumerate(archives):
            for source in sources.values():
                os.utime(source, (946684800 + index * 86400, 946684800 + index * 86400))
            with zipfile.ZipFile(archive, "w") as z:
                for name, source in sources.items():
                    pkg.write_entry(z, name, source)
            pkg.verify_archive(archive, sources)
        self.assertEqual(archives[0].read_bytes(), archives[1].read_bytes())
        with zipfile.ZipFile(archives[0]) as z:
            self.assertTrue(all(info.date_time == (1980, 1, 1, 0, 0, 0) for info in z.infolist()))
            self.assertEqual(z.namelist(), sorted(z.namelist()))

    def test_missing_meta_fails_closed(self):
        (pkg.UNITY / "Prefabs/A01.prefab.meta").unlink()
        with self.assertRaisesRegex(ValueError, "Missing Prefabs"):
            pkg.collect_files()

    def test_duplicate_effect_fails_closed(self):
        path = pkg.UNITY / "Resources/motion-catalog.json"
        catalog = json.loads(path.read_text(encoding="utf-8"))
        catalog["effects"][-1]["id"] = catalog["effects"][0]["id"]
        path.write_text(json.dumps(catalog), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "exactly once"):
            pkg.collect_files()


if __name__ == "__main__":
    unittest.main()
