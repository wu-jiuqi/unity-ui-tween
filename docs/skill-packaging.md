# 独立 Skill 打包与验证

项目 Git 源目录中的 `Skill/unity-ui-tween/assets/` 为忽略的生成目录，可能不存在或包含旧版本。`Assets/TweenGallery/` 是模板、预设、Prefab、图鉴和字体的源；发行 Skill ZIP 必须包含这些资产与 Unity `.meta`，并将它们放在 `unity-ui-tween/assets/TweenGallery/`。

从工程根目录执行：

```powershell
python -X utf8 scripts/package_skill.py
python -X utf8 scripts/test_package_skill.py
```

打包不运行 Unity、不要求 Windows 展馆先构建。默认产出 `dist/unity-ui-tween-<VERSION>-standalone.zip`；已有同名文件时不会覆盖，需显式添加 `--force`。也可用 `--output <path>` 指定测试位置。注意：这是工作区内的本地文件操作，并不会自动发布或安装 Skill。

脚本检查 46 个固定编号、各编号的 Prefab/预设和 `.meta`、运行脚本、Shader、字体与许可证；验证 Unity GUID 唯一性及 YAML 资源引用，并把 UGUI 内置组件 GUID 作为外部依赖识别。ZIP 内路径使用跨平台 `/`；关闭 ZIP 后再次校验各文件的 SHA-256、CRC、条目集合和路径安全性。二进制资源本身逐字节校验，但脚本不能证明 Unity 运行时所有材质、布局或可视效果正确。结果输出 ZIP 的 SHA-256，并在旁边写入 `.sha256` 文件。

UGUI 与 DOTween 免费版由目标工程提供；本 ZIP 不包含或静默安装 DOTween Pro。`Optional/TMP/` 被打包，但未定义 `TWEEN_GALLERY_TMP` 时不依赖 TMP。不要因为工程有已有 DOTween 而复制第二份。

`test_package_skill.py` 在隔离临时目录构造合成工程，检查无 Windows 构建时可打包、缺 `.meta` 和重复编号会失败，以及打包后源被改动能被检测。实际工程还需检查 Unity 编译、导入和播放器运行；仅有脚本校验不能替代这些测试。
