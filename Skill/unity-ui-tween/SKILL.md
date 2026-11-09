---
name: unity-ui-tween
description: "为 Unity UGUI 按项目画风制作并接入按钮、文字、Logo与页面动效；提供46项基础模板、按钮交互、文字弹跳/显现、果冻Logo、整页编排和响应式布局，并支持先生成整屏参考再拆美术拼装。"
---

# Unity UI Tween

## 先定整图，再拆素材还原

画风重做、文字与背景不协调或要求image gen参考时，先按 [整图到素材的制作流程](references/art-composition-workflow.md) 生成含最终文字层级的完整参考，再列出底板、标题、按钮、图标和装饰清单，逐件生成并在Unity分层拼装。用实际运行画面对照文字大小、字重、颜色、内边距、材质与高光；不能只换字色或继续往旧底板贴字。一般小修改复用合适资源，不强制重新生成。

固定品牌/艺术标题可生成独立带字PNG，并核对中文及保留语义文本；动态数值、按钮、说明和需本地化文字优先原生Text/TMP。逐字艺术Logo从生成阶段就拆成独立字，不能任意切共享底板。实心承字面板中央必须不透明；拒收中央透明洞、脏灰晕和错字，保留提示词、尺寸、alpha、裁切与哈希。采用实际字体字重，避免全局换肤将全部文字变成同一浅色或小字合成粗体。美术按用户要求使用真实PNG，不用SVG占位。

## 统筹设计与选型

按 [统一动效系统](references/motion-system.md) 先识别用途、材质与操作状态，再选择时间轴模板或实时状态变体。已有内容与新增内容一起检查：默认节奏、幅度、文字稳定性、阴影、布局、输入与末态。不要把优化请求缩减为另加一个示例页。参考原型是视觉方向，不把其中已有素材或按钮种类当成上限。

原 46 项与 BX 配方使用同一视觉和选型体系。B01/B02 的时间轴与 BX01/BX02 的按住状态是同家族不同驱动方式；E03 与 BX06 同理。选择所需方式，避免叠加重复控制器。编号是调用键，优先按用途和可观察差异解释效果。

按 [三档动效规则](references/motion-profiles.md) 为已有和新增内容统一选择克制、标准或庆祝。每次从目录基准生成参数，保持素材选择，避免反复乘上档位；导出 settings 已生效，不重复换算。高频输入、通用操作和低频奖励应有不同强度，减弱动效独立控制。

整页请求优先读 [P01 整页编排](references/page-recipes.md)：背景、面板、主视觉、奖励、操作区有明确先后，共用可采样时间轴；先准备首帧再显示。适配短屏、长屏和横屏时读 [P02 响应式布局](references/responsive-layout.md)，保留已有控件、选择和滚动，正文可滚动而主要操作保持可见。P01/P02 位于同一主展馆，分别用 MotionPageSequence / MotionResponsiveLayout，不是 MotionPlayer.Play 编号。schemaVersion=3 使用 recipe 驱动与素材/布局字段。视觉完成与真实奖励、导航和持久化分离。

按稳定编号选择动效，把展馆同源模板接入项目现有 UI。第一版基准为 Unity 6000.0.26f1、UGUI 和 DOTween 免费版；其他 Unity 版本需实际编译验证。UI Toolkit、Animator 或其他引擎需要另行适配，不能直接承诺本模板兼容。

文字与Logo需求读 [T01–T07配方](references/text-logo-recipes.md)：整段弹跳、逐字错峰、Logo掉落果冻、逐字颤动、打字机、加载波浪和滚动提示。T编号使用独立驱动，不传给MotionPlayer.Play；图形字与原生文字都绑定内部visual，排版slot和命中不动。Logo须先全部落稳再进入共享果冻；采样不触发声音、奖励或跳转。离散图帧和装饰彩纸分别有MotionSpriteSequence、MotionConfettiSequence，不当成缓动曲线或业务粒子系统。

需要复查项目缺项时按 [项目动效复查](references/project-motion-audit.md) 同时检查DOTween调用、自定义逐帧公式、文字顶点、图片帧和生命周期，再与现有类型映射。分清已提供驱动、组合接线和需要宿主适配的内容；不只搜索DOScale，也不把物理轨迹/QA/静态三角函数全算成UI补间。

## 从画风扩展按钮

需要按截图或原型设计更多按钮时，阅读 [按钮美术与适配](references/button-design.md)，从用途、形状、材质和动作选择组合。沿用配色、光照、体积和边缘语言，允许设计新按钮、新图标和新用途；不要把参考游戏的素材、名称或页面集合当成限制。奶油花园原型是可选风格，不强制应用到其他项目。

已确认的原型优先决定本次视觉方向；历史项目用于兼容和接线，不限制新变体。

基础效果可继续用 B01–B08；需要真实按住、图标弹起、中心涟漪、扫光、选中或加载时，阅读 [BX01–BX08 交互配方](references/button-recipes.md)。MotionButtonFeedback 与 MotionPlayer 分开绑定，BX 编号不是 MotionPlayer.Play 编号。六种皮肤和实时反馈都在主展馆对应效果里切换，Runtime 包使用宿主自己的美术。文字、图标、装饰与命中区域分层，避免将整个按钮连同业务文字一起大幅拉伸。

预览或读取复制出的配置时，按 [主展馆流程与导出契约](references/gallery-workflow.md) 区分 timeline / interaction。G01/G02 是主目录的拖动与接续示例，不能传给 MotionPlayer.Play。schemaVersion=2 的交互配置按 driver 接线；同时保留素材选择和实际生效的频率/强度/位移等参数，不把实时输入重新降成自动时间轴。内部诊断夹具不是用户流程。

## 先检查目标项目

查看 Unity 版本、UGUI、已安装 DOTween 及其模块、渲染管线和项目动画约定。读取目标 UI 层级、布局组件、按钮事件及已有 Tween，确认谁负责打开、关闭和最终交互状态。保留项目风格与业务逻辑；展馆的奶油花园是默认演示风格。

复用项目已有 DOTween。缺少依赖时说明所需免费版与官方安装方式，不静默安装、升级、替换或重复导入，不引入 DOTween Pro。用户明确授权安装后再执行。

## 选定编号与表现

阅读 [46 项动效图鉴](references/catalog.md)，按用户给出的编号、动作描述或用途定位条目。A01–A08 为出现消失，B01–B08 为按钮反馈，C01–C07 为页面切换，D01–D10 为奖励数值，E01–E07 为循环引导与编排，F01–F06 为进阶原创设计组合。图鉴中的每项都保留独立含义，不因为底层实现相似合并条目。

容易混淆时先用可观察的差异解释选择：

- A03 缩放进入只从小到正常；A04 Pop 会越过目标尺寸再收回；A05 Bounce 强调位置上的落地弹跳。Elastic 是可选缓动曲线，不能用它替代这三个编号。
- B04 Shake 是快速位移振动；B05 Wiggle 是往返旋转；B06 Punch 是一次冲击后的衰减回弹。
- C05 共享轴表现沿同一方向前进或返回；C06 先淡出旧页再淡入新页；C07 两页同步交叉淡化。

优先从条目默认参数起步，再结合触发时机和对象调整。若用户提供展馆复制出的配置，保留其编号、时长、强度、距离、间隔、缓动、速度和方向设置；仅修改用户要求调整的部分。

## 接入同源模板

按 [模板接入与参数](references/integration.md) 选择所需绑定和调用方式。发行 ZIP 中的完整模板资产位于 `assets/TweenGallery/`，对应目标项目的 `Assets/TweenGallery/`；包括 Runtime、Shader、46 个预制体、46 个预设、图鉴数据、图形和字体。Git 源目录的 `Skill/unity-ui-tween/assets/` 是被忽略的生成物，可能不存在或过时；源码开发时以项目根 `Assets/TweenGallery/` 为准，执行 `python -X utf8 scripts/package_skill.py` 独立生成并验证发行包，无需 Windows 构建。安装发行包后，相对路径以 Skill 文件夹为根。复制时保留 `.meta`；只接入已有 UI 时按依赖选择必要部分。

使用命名空间 `TweenGallery` 的 `MotionPlayer`、`MotionBindings`、`MotionSettings` 和 `MotionCatalog`。将目标元素绑定到对应字段，在布局稳定后捕获基线，再通过 `Play(id, options)` 播放。用 `Pause()` / `Resume()` 控制暂停，用 `StopAndRestore()` 取消并恢复。若复制模板资产，只复制目标项目实际需要的代码、着色器和目录数据，保留已有脚本和依赖。

注意几个会改变实现选择的约束：

- 布局组件管理的对象优先使用内部动画容器，避免同时争用位置或尺寸；布局更新后按接入文档重新捕获基线。
- 每个实例只清理自己拥有的 Tween；连续触发应从可预测状态重播。不要使用全局 `KillAll`，不要通过修改全局 `Time.timeScale` 实现预览慢放。
- `Play` 与真实按压入口 `SetPressed` 都按 `bindings.target` 仲裁：同一 target 的新请求会停止并恢复前一个播放器，再接管目标；不同 target 的播放不会因此被停止。同 target 不用多个播放器叠加不同属性，组合由同一播放器实现或分配到不同包装层。该规则不仲裁两个不同 target 共享的 secondary/items 等绑定，也不能代替避免属性并写。
- MotionPointerFeedback 可绑定已有或专门的 MotionPlayer；同 target 发生播放与按压切换时遵循最新请求接管规则，不会并行动画叠加。保持隐藏状态的射线穿透、关闭清理和销毁清理一致；保留游戏暂停时是否继续播放的明确设置。
- A08 的示例模糊作用于单个 `Image` 的纹理，不会自动模糊文字、子层级或背后场景。整页实时模糊需要额外渲染方案。目标项目构建需保留 `TweenGallery/UITextureBlur` Shader，按接入文档配置，避免仅编辑器正常而 Player 被剥离。
- C04 需要明确起点和终点；模板支持同一 Canvas 内的父级坐标转换，但要求目标轴方向一致。跨 Canvas 必须另写映射适配，不能直接绑定或照搬 `anchoredPosition`。
- E03 需要高光层与遮罩；D07 需要正反面内容；D08 需要独立箱盖与奖励元素。依照条目绑定，不把组合效果描述成单一缓动。
- `reverse` 是模板时间方向设置，`loop` 是重复播放设置；它们不自动替代页面导航、数据结算、奖励发放或开关状态。逐项检查末帧与循环接缝，业务结果由项目自己的代码管理。

## 验证并交付

按 [验收与提示词](references/acceptance.md) 检查本次接入涉及的行为。至少覆盖实际编译、可运行时的视觉预览、连续重播、中途关闭/禁用、销毁、布局缩放及结束后的可点击状态。按钮按压还要验证按住、松开和移出；奖励动画要确认视觉到达与业务结算不会重复触发。

按需查阅 [指针交互](references/interaction.md)、[编排与生命周期](references/orchestration.md) 和 [移动端与性能](references/mobile-performance.md)。它们区分已提供的组件、项目需接线的逻辑和必须在实际设备测试的行为。

说明选用的编号、绑定位置、最终参数、调用入口，以及实际通过和未验证的检查。静态检查、`Sample()` 采样、编辑器运行和独立 Windows 运行属于不同证据，不互相替代。不能运行 Unity 时明确这一限制，不将代码生成描述为运行通过。

## 进阶设计组合

F01 是扫描揭晓，F02 是星图汇聚后解锁，F03 是字块与底线的排版编排，F04 是卡片和进度的分层入场，F05 是新旧焦点卡交接，F06 是多层门户奖励揭晓。按图鉴读取各自绑定、来源及原创转化；不要直接复制参考站的素材或商标。先核对目标项目素材、布局与交互语义，再适配同源模板。

F05 的焦点交接是自动演示；真实指针倾斜/高光需要 `MotionParallaxTilt`、可射线命中 Graphic 和 EventSystem，surface 绑定 target 内部独立包装层。具体绑定、默认值和动态赋值后重新采集基线见接入文档；需要实测指针 enter/move/exit。


## Package profile check

Before promising demo prefabs, inspect the installed assets. The default full
package contains all 46 demo prefabs. An optional runtime-only package omits
them and the gallery's fonts/art; bind the user's own UI instead. See
[profile boundaries](references/runtime-package.md). For repository work,
`docs/` is canonical and `scripts/sync_skill_docs.py` updates reference copies;
packaging embeds the canonical documents directly.
