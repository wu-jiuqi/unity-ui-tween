"""Cross-platform paths, Unicode, hierarchy and safety tests for portable_zip."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
import zipfile

spec = importlib.util.spec_from_file_location("portable_zip", Path(__file__).with_name("portable_zip.py"))
portable = importlib.util.module_from_spec(spec)
spec.loader.exec_module(portable)


class PortableZipTests(unittest.TestCase):
    def test_extract_with_forward_slashes_unicode_and_empty_folder(self):
        temp_root = Path(__file__).resolve().parent.parent / "validation-output"
        temp_root.mkdir(parents=True, exist_ok=True)
        with tempfile.TemporaryDirectory(prefix="portable-zip-test-", dir=temp_root) as tmp:
            root = Path(tmp)
            source = root / "展馆"
            nested = source / "资料" / "说明"
            nested.mkdir(parents=True)
            (source / "空目录").mkdir()
            (nested / "你好.txt").write_text("可验证", encoding="utf-8")
            output = root / "package.zip"
            portable.package(source, output, include_root=True)
            with zipfile.ZipFile(output) as archive:
                names = archive.namelist()
                self.assertIn("展馆/资料/说明/你好.txt", names)
                self.assertIn("展馆/空目录/", names)
                self.assertFalse(any("\\" in entry for entry in names))
                extract = root / "extract"
                archive.extractall(extract)
            self.assertEqual((extract / "展馆/资料/说明/你好.txt").read_text(encoding="utf-8"), "可验证")
            self.assertTrue((extract / "展馆/空目录").is_dir())
            with self.assertRaisesRegex(ValueError, "exists"):
                portable.package(source, output)
            with self.assertRaisesRegex(ValueError, "inside source"):
                portable.package(source, source / "invalid.zip")


if __name__ == "__main__":
    unittest.main()
