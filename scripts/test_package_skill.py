"""Checks the Skill package validator using an isolated synthetic project."""
import importlib.util
import json
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
