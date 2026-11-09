# 主展馆统一预览与导出

从 0.5.0-preview3 起，正常产品界面只有主展馆，不再提供“交互实验室”“按钮工坊”入口。公共皮肤、实时按钮状态、拖拽和连续接续都在原预览区域使用。

## 使用顺序

1. 左侧按用途或搜索选择效果。目录有 46 个 A–F 时间轴模板，以及 G01 拖拽回位、G02 连续接续，共 48 个选择项；G 不属于 MotionCatalog 的 46 个 Play 编号。
2. 右上选择“自动播放”或“真实操作”。普通时间轴效果的真实操作是点击画面触发一次；B07/B08 点击切换状态，播放中切换接续当前时间。G01/G02 只由输入驱动，自动播放按钮不可用。
3. 按钮预览中点击“造型”循环选择六种公共皮肤。“操作变体”选择原效果或八种实时反馈，选变体后直接进入真实操作。B05 礼物和 B08 原开关的原效果不是通用按钮底板，只有切到按钮反馈变体后才显示造型选择。
4. 在中间控件直接按住、松开、移出、点击；G01 按住拖动，G02 连续点击左/右。加载演示由“完成 / 切换状态”结束，真实异步任务仍由宿主处理。
5. 右侧调节实际生效的参数。时间轴使用秒；实时弹簧按钮使用 3–12 Hz 频率和强度，不把弹簧过程伪装成固定时长。拖动调位移/强度，连续接续调时长、位移和缓动。不适用的控制会禁用。
6. 复制提示词或参数 JSON。所选素材和驱动方式一起导出，无需转到另一页。切换效果/模式会取消旧操作、清理控件与补间；后台和失焦取消捕获。

六种皮肤来源统一为 `Resources/ButtonArt/cream-buttons.png`，由 GalleryTheme.Skin(index) 提供。skinIndex 0–5 依次是薄荷胶囊、淡紫织边、金色圆钮、珊瑚软糖、奶油描边、圆角方钮。圆形/方形的原时间轴按钮使用独立图标，避免把长文案挤进圆钮。Runtime 包不包含该演示图集，使用宿主图片；GalleryTheme 属于展馆代码，不是 Runtime 接入依赖。

## JSON 驱动契约

当前主目录共有50个选项：46个基础时间轴、G01/G02操作、P01/P02页面配方。P01/P02仍在主展馆“编排”分类。右侧克制/标准/庆祝应用到已有条目和页面配方，详见 motion-profiles.md；P01分层时间轴见 page-recipes.md，P02多尺寸交互见 responsive-layout.md。复制配置中的 styleProfile 保留档位来源，所有数值已生效，不重复乘档位。页面配方使用 schemaVersion=3、recipe 和明确 driver，effects 为空。

时间轴仍使用 `effects[]` 中的真实 MotionCatalog 编号、MotionSettings；新增 `previewMode="timeline"` 与 `skinIndex`，原消费者可以忽略可选字段。

真实操作使用 schemaVersion=2、previewMode="interaction"。不要只读取 effects[] 就当成时间轴：

- `interaction.driver="MotionButtonFeedback"`：variant 是 ButtonMotionStyle，使用导出的 intensity、frequency、damping、reducedMotion、unscaledTime。duration=0 表示弹簧不承诺固定结束时长；accentDuration 是图标/涟漪/扫光的一次装饰长度，标准档为 .55 秒。
- `interaction.driver="MotionDragInput + MotionJellyDrag"`：用独立固定命中层和 visual，导出 dragRange、axisRange、maxDisplacement、stretch、maxRotation、frequency、damping。G01 的 effects[] 为空，不调用 MotionPlayer.Play("G01")。
- driver=MotionPlayer、variant=ContinueTo：G02 的 effects[] 为空，按 duration、distance、leftScale、rightScale、tiltDegrees、ease 从稳定基线建立目标姿态，使用 ContinueTo 接续。减弱动效时直接应用目标姿态。
- driver=MotionPlayer、variant=trigger 或 toggle：使用 effects[] 中的合法编号；toggle 由宿主状态决定方向，并在打断时从相反时间点接续。

导出的是视觉接入配置，不保存点击次数、业务选中结果、正在执行的任务或奖励状态。宿主负责异步请求、禁用和完成；示例的状态按钮只是体验入口。

GalleryConfiguration 是完整展馆里的 DTO。只装 Runtime 的宿主应按以上数据契约自行读取所需字段，不为了读取 JSON 额外导入整个 Gallery。

## 内部验证

旧实验页仅保留为 `--upgrade-test` / `--button-test` 的内部诊断夹具，正常流程不会创建它们。旧 `--button-lab` 参数重定向至主展馆 B01 真实操作，不再打开单独页面。

`--workbench-test --test-output <folder>` 验证 48 种皮肤/反馈组合、原生点击、加载结束、拖动取消、连续接续、调参与导出、回到自动播放及不存在旧入口。旧 46 项时间轴和内部输入/生命周期回归继续保留。自动输入、离屏 Canvas 画面与真机手感评测应分别报告。
