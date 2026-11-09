# Unity UI Motion Gallery

一套以 **Unity UGUI + DOTween 免费版** 实现的 UI 动效库：46个基础预设/预制体、可运行Windows展馆、交互/页面/文字配方、通用Runtime、Codex Skill和中文文档。当前开发版 **0.6.0-preview2**；已发布稳定版为 **0.4.0**。

展馆统一使用暖奶油、薄荷、淡紫与暖金的柔和玩具质感 UI，并以同一份 MotionPlayer 播放实际交付的模板。A01–F06 固定编号覆盖出现消失、交互反馈、页面转场、奖励数值、循环和编排。

0.4.0 新增可拖动时间轴、参数 JSON 导出和 image gen 美术，优化数值文本与共享元素采样，并提供可重复打包及轻量 Runtime 包。保留全部 46 个现有 Prefab 的 GUID 与内部 fileID。

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

## 0.6.0 三档规则与页面配方

preview2先用image gen生成两张整屏参考，再拆出8份PNG并拼装，使用独立艺术标题、空白缎带、原生动态文字和实际字体字重，统一优化旧目录与新页面的文字/底板关系。拒收中央透明的失败底板。流程已强化到Skill：[整图到美术资源](docs/art-composition-workflow.md)。完整Skill同时打包参考图、提示词和资源清单，Runtime仍不带演示美术/字体。

主目录现为57项：原46项、G01/G02、P01/P02，以及T01–T07。新增短文字/逐字弹跳、逐字颤动、打字机、加载波浪、滚动提示和项目开场Logo掉落果冻；还提供离散Sprite帧与装饰彩纸/星屑的独立时钟。接入见 [文字与Logo配方](docs/text-logo-recipes.md)。扫描源项目291个脚本、复核34个UI动效候选，类型映射和宿主适配边界见 [项目动效复查](docs/project-motion-audit.md)。

preview2实际画面：[整页拼装](docs/previews/v061/page-complete.png)、[响应式](docs/previews/v061/responsive-compare.png)、[Logo果冻](docs/previews/v061/logo-jelly.png)；24fps采样视频：[文字弹跳](docs/previews/v061/T02-sampled.mp4)、[Logo掉落](docs/previews/v061/T03-sampled.mp4)、[打字机](docs/previews/v061/T05-sampled.mp4)。素材提示词、来源与失败验收见 [生成记录](docs/art-generation-v061.md)。

下文为preview1的页面配方基础：

按确认的三张原型，将克制/标准/庆祝接入原有46项与实时反馈；新增 P01“旅程达成”整页编排、P02“奖励选择”响应式交互，均在主展馆“编排”分类。主目录共50项，没有新增独立工坊入口。页面支持阶段采样、暂停续播与减弱动效；短屏正文滚动、底部操作固定，旋转保留选择与滚动位置。

新生成的透明 PNG 面板、徽章和奖励图集已用于实际页面，按钮与花园插画沿用公共美术，没有 SVG 占位。文字为原生 UGUI Text。Skill 同步加入 [三档规则](docs/motion-profiles.md)、[整页接线](docs/page-recipes.md)、[响应式与首帧](docs/responsive-layout.md)；schemaVersion=3 明确页面驱动和实际参数，视觉预览不发放业务奖励。完整包含美术，Runtime 包只带组件与接入文档。新候选推送开发分支，不更改既有 Release。

实际 Player 画面：[整页完成](docs/previews/v060/page-complete.png)、[三尺寸对照](docs/previews/v060/responsive-compare.png)、[横屏选择](docs/previews/v060/responsive-landscape.png)。PNG生成记录见 [美术来源](docs/art-generation-v060.md)，运行报告与限制见 [验证记录](docs/validation.md)。

## 0.5.0 全量动效优化

原有 46 项的推荐节奏、强度、位移、预设、实际 Prefab 和展馆视觉已全部更新，采用统一奶油花园风格；宝箱、晶石、场景插画同步重做。GUID 与既有对象 fileID 保留，默认表现有意调整。统一选型见 [动效系统](docs/motion-system.md)。

另有六种原创空白按钮皮肤和 BX01–BX08 八套可运行交互：柔软按压、果冻挤压、悬浮抬升、图标回弹、中心涟漪、扫光、选中和加载。它们独立于原有 46 项目录，原 Prefab 保持兼容。以认可的原型确定画风，允许设计新造型与用途，不限定为历史游戏素材。

在同一个主展馆里切换 **自动播放 / 真实操作**，直接调整按钮造型和操作变体。G01/G02 在左侧“操作”分类提供拖拽回位与连续接续；旧实验室和工坊只保留内部诊断，不再有产品入口。详见 [统一操作与导出](docs/gallery-workflow.md)。接入方式见 [按钮设计](docs/button-design.md)、[按钮交互配方](docs/button-recipes.md)。完整 Skill 包含皮肤；Runtime 包只带运行组件和接入参考，使用宿主美术。0.5.0-preview3 为本地候选，未发布到 GitHub Release。

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

当前工作区为 0.4.0 预览版本，发布状态与下载资产以 GitHub Release 为准。当前验证和限制见 PROGRESS.md 与验证记录。

