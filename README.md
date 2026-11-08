# Unity UI Motion Gallery

一套以 **Unity UGUI + DOTween 免费版** 实现的 46 项 UI 动效：可运行 Windows 展馆、46 个预设与示例预制体、通用 Runtime、Codex Skill 和中文文档。预览版本 **0.4.0-preview1**，发布资产以 GitHub Release 为准。

展馆使用鲜艳、高饱和度的休闲游戏 UI，并以同一份 MotionPlayer 播放实际交付的模板。A01–F06 固定编号覆盖出现消失、交互反馈、页面转场、奖励数值、循环和编排。

0.4.0-preview1 新增可拖动时间轴、参数 JSON 导出和 image gen 美术，优化数值文本与共享元素采样，并提供可重复打包及轻量 Runtime 包。保留全部 46 个现有 Prefab 的 GUID 与内部 fileID。

0.3.0 增加交互实验室、姿态接续、阻尼弹簧、果冻拖拽、事件与对象池保护、减弱动效和可选 TMP 适配；旧的 46 项预制体保留。完整 Skill 包可独立生成，不需先构建 Windows 展馆。

- [展馆时间轴、JSON 与新美术](docs/gallery-preview.md)，[生成记录](docs/art-generation-v040.md)，[轻量 Runtime 包](docs/runtime-package.md)。
- [快速上手](docs/quickstart.md)：Windows 展馆、Unity 工程与模板、Skill 安装。
- [46 项图鉴](docs/catalog.md)：效果差异、参数、绑定、依赖和逐项提示词。
- [进阶参考与原创转化](docs/design-references.md)。
- [接入与参数](docs/integration.md)：真实项目 API、布局与生命周期规则。
- [交互组件](docs/interaction.md)、[编排与音频](docs/orchestration.md)、[移动与性能](docs/mobile-performance.md)及 [Skill 独立打包](docs/skill-packaging.md)。
- [提示词与验收](docs/acceptance.md)：组合请求与检查标准。
- [实际验证记录](docs/validation.md)：以日志和实测为准。
- [发布使用指南](docs/release-guide.md)：完整解压、资产核对和已知范围。
- [第三方许可](ThirdPartyLicenses/README.md)：DOTween 与中文字体说明。

## 项目结构

```text
Assets/TweenGallery/
  Runtime/           共用动画、绑定、设置、预设和按压组件
  Shaders/           Image 纹理模糊
  Gallery/           展馆 UI 与示例对象
  Resources/         46 项清单与中文字体
  Prefabs/           A01–F06 共 46 个可复用示例
  Presets/           A01–F06 共 46 个参数预设
  GeneratedArt/      预制体所需图形资源
  Scenes/Gallery.unity
  Editor/            场景/模板准备与 Windows 构建
Skill/unity-ui-tween/ 可打包的 Codex Skill
docs/                中文说明
evidence/            构建及运行证据
```

## 工程基线

Unity **6000.0.26f1**、UGUI **2.0.0**、DOTween 免费版 **1.3.045**、Built-in 管线，Windows x64 Mono。源工程保持第三方原始版权与许可。所有平台和环境是否已实测，请查验证记录。

打开 Gallery.unity 可在编辑器运行。需要重新生成展馆场景和模板时，在此独立工程使用菜单 **Tools → UI Motion Gallery → Prepare scene and templates**；Windows 构建使用 **Build Windows and export templates**。Prepare 会重建当前展馆场景，先保存手动改动。

当前工作区为 0.4.0-preview1 预览版本，发布状态与下载资产以 GitHub Release 为准。当前验证和限制见 PROGRESS.md 与验证记录。

