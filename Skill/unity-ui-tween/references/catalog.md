# 46 项 UI 动效图鉴

版本 0.5.0-preview2。编号、名称和默认参数以 `Resources/motion-catalog.json` 为单一数据源；展馆、模板预设与本文使用同一清单。每项对应 `Prefabs/<ID>.prefab` 和 `Presets/<ID>.asset`，实际播放统一使用 `MotionPlayer`。

全部 46 项已统一优化默认节奏、幅度、预设及示例美术。编号作为稳定调用键保留；按使用场景选型及实时按钮变体见 [统一选型](motion-system.md)。

## 如何区分相似效果

| 想看到的变化 | 选择 | 关键区别 |
|---|---|---|
| 单调变大到目标 | A03 缩放进入 | 默认不超过目标尺寸 |
| 放大超过目标再缩回 | A04 Pop | 缩放超调 |
| 落到目标后多次弹跳 | A05 Bounce | 位置弹跳 |
| 下落略微越界后停稳 | C01 Drop & Settle | 一次位置超调 |
| 快速左右平移 | B04 Shake | 平移错误轻抖 |
| 左右旋转 | B05 Wiggle | 角度提示摆动 |
| 一次冲击后衰减 | B06 Punch | 单次缩放冲击 |
| 新旧页沿同轴进出 | C05 Shared Axis | 有方向性位移 |
| 旧页消失后新页出现 | C06 Fade Through | 中间有空隙 |
| 新旧页同时淡入淡出 | C07 Crossfade | 两页可见时间重叠 |

OutBack、OutBounce、OutElastic 是缓动曲线，不是新增效果编号。A03 改用 OutElastic 会得到弹簧缩放，仍不是 A05 的位置弹跳。先用 Preset 对比定义，再比较曲线。

## 参数阅读方法

下列默认数值依次为 **完整时长秒 / 强度倍率 / Canvas 参考坐标位移 / 子项间隔秒**。默认 ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。并非每项都会使用全部参数，例如淡入不使用位移，旋转不使用间隔。多子项间隔会钳位，使编排在 duration 内完成。数值起止与填充比例可在 Inspector/代码设置，见 [接入手册](integration.md)。

## A · 出现与消失

### A01 淡入 / 淡出 · Fade In / Out

只改变透明度，让内容平滑出现；反向则逐渐消失。

- **类型与用途**：基础变化；遮罩、提示、次要信息。
- **辨认差异**：没有位移或缩放；与 C07 的两页同时交替不同。
- **默认参数**：0.28s / 1 / 0 / 0s。
- **绑定**：target + group。
- **依赖与限制**：CanvasGroup；透明时交互必须同步关闭。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/A01.prefab`、`Presets/A01.asset`，调用 `MotionPlayer.Play("A01", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A01 淡入 / 淡出（Fade In / Out）。预期效果：只改变透明度，让内容平滑出现；反向则逐渐消失。 使用共享 MotionPlayer 模板，duration=0.28s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；CanvasGroup；透明时交互必须同步关闭。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### A02 滑入 / 滑出 · Slide In / Out

从偏移位置滑向原位，反向时离开。

- **类型与用途**：基础变化；侧边栏、通知、底部弹窗。
- **辨认差异**：以位置变化为主；C02 还包含背景遮罩。
- **默认参数**：0.38s / 0.7 / 72 / 0s。
- **绑定**：target；group 可用于伴随淡入。
- **依赖与限制**：使用布局节点下的独立动画包装层。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/A02.prefab`、`Presets/A02.asset`，调用 `MotionPlayer.Play("A02", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A02 滑入 / 滑出（Slide In / Out）。预期效果：从偏移位置滑向原位，反向时离开。 使用共享 MotionPlayer 模板，duration=0.38s、strength=0.7、distance=72、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；group 可用于伴随淡入；使用布局节点下的独立动画包装层。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### A03 缩放进入 / 退出 · Scale In / Out

从较小尺寸平滑放大到基线大小；反向缩小退出。

- **类型与用途**：基础变化；弹窗、图标、奖励。
- **辨认差异**：默认不超过目标大小；A04 超调，A05 发生位置弹跳。
- **默认参数**：0.32s / 0.35 / 0 / 0s。
- **绑定**：target + group。
- **依赖与限制**：target 的基线 localScale 应是项目最终尺寸。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/A03.prefab`、`Presets/A03.asset`，调用 `MotionPlayer.Play("A03", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A03 缩放进入 / 退出（Scale In / Out）。预期效果：从较小尺寸平滑放大到基线大小；反向缩小退出。 使用共享 MotionPlayer 模板，duration=0.32s、strength=0.35、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；target 的基线 localScale 应是项目最终尺寸。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### A04 弹出进入 · Pop In

先放大超过目标尺寸，再回到基线，配合淡入。

- **类型与用途**：组合效果；胜利弹窗、奖励、轻快的休闲游戏 UI。
- **辨认差异**：超调发生在缩放上；A05 的弹跳发生在位置上。
- **默认参数**：0.44s / 0.55 / 0 / 0s。
- **绑定**：target + group。
- **依赖与限制**：布局包装层；避免同时由 Animator 修改缩放。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/A04.prefab`、`Presets/A04.asset`，调用 `MotionPlayer.Play("A04", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A04 弹出进入（Pop In）。预期效果：先放大超过目标尺寸，再回到基线，配合淡入。 使用共享 MotionPlayer 模板，duration=0.44s、strength=0.55、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；布局包装层；避免同时由 Animator 修改缩放。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### A05 落位弹跳 · Bounce In

元素从上方落入，接近落点时多次弹跳并停稳。

- **类型与用途**：组合效果；奖励卡片、宝箱、落地提示。
- **辨认差异**：多次位置弹跳；C01 是一次越界后停稳，A04 是缩放回弹。
- **默认参数**：0.72s / 0.65 / 84 / 0s。
- **绑定**：target + group。
- **依赖与限制**：需预留垂直移动空间。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/A05.prefab`、`Presets/A05.asset`，调用 `MotionPlayer.Play("A05", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A05 落位弹跳（Bounce In）。预期效果：元素从上方落入，接近落点时多次弹跳并停稳。 使用共享 MotionPlayer 模板，duration=0.72s、strength=0.65、distance=84、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；需预留垂直移动空间。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### A06 翻转进入 · Flip In

目标从侧面翻转到正面并出现。

- **类型与用途**：组合效果；卡片首次展示、信息牌。
- **辨认差异**：只展示同一面；D07 在侧面时刻切换正反内容。
- **默认参数**：0.48s / 0.8 / 0 / 0s。
- **绑定**：target + group。
- **依赖与限制**：UGUI 平面旋转会产生透视近似；不是 3D 卡牌模型。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/A06.prefab`、`Presets/A06.asset`，调用 `MotionPlayer.Play("A06", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A06 翻转进入（Flip In）。预期效果：目标从侧面翻转到正面并出现。 使用共享 MotionPlayer 模板，duration=0.48s、strength=0.8、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；UGUI 平面旋转会产生透视近似；不是 3D 卡牌模型。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### A07 揭示 / 遮罩展开 · Reveal

裁剪窗口逐渐展开，露出内部内容，文字本身不拉伸。

- **类型与用途**：组合效果；标题、横幅、内容揭示。
- **辨认差异**：改变可见裁剪区；A03 会缩放整块内容。
- **默认参数**：0.42s / 1 / 0 / 0s。
- **绑定**：target 为内容；mask 为独立裁剪容器。
- **依赖与限制**：mask 上配置 RectMask2D，内容保持固定尺寸。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/A07.prefab`、`Presets/A07.asset`，调用 `MotionPlayer.Play("A07", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A07 揭示 / 遮罩展开（Reveal）。预期效果：裁剪窗口逐渐展开，露出内部内容，文字本身不拉伸。 使用共享 MotionPlayer 模板，duration=0.42s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为内容；mask 为独立裁剪容器；mask 上配置 RectMask2D，内容保持固定尺寸。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### A08 柔焦转清晰 · Blur In

绑定的 Image 纹理由模糊过渡到清晰，可伴随淡入。

- **类型与用途**：材质组合；插画卡片、奖励图标、静态 UI 图片。
- **辨认差异**：是真实纹理采样模糊；不会模糊同层文字或整个 UI 子树。
- **默认参数**：0.6s / 0.65 / 0 / 0s。
- **绑定**：target + image + group；image 需有可辨认纹理。
- **依赖与限制**：项目随附 UI 模糊 Shader；只作用于绑定 Image 的纹理。Player 构建需保留 TweenGallery/UITextureBlur，详见接入手册。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/A08.prefab`、`Presets/A08.asset`，调用 `MotionPlayer.Play("A08", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 A08 柔焦转清晰（Blur In）。预期效果：绑定的 Image 纹理由模糊过渡到清晰，可伴随淡入。 使用共享 MotionPlayer 模板，duration=0.6s、strength=0.65、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + image + group；image 需有可辨认纹理；项目随附 UI 模糊 Shader；只作用于绑定 Image 的纹理。Player 构建需保留 TweenGallery/UITextureBlur，详见接入手册。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

## B · 按钮与交互反馈

### B01 按压缩小 · Press Scale

按下缩小，松开回到原尺寸；自动演示展示完整按松周期。

- **类型与用途**：交互反馈；常规按钮。
- **辨认差异**：恢复时不超调；B02 松开后有明显弹性。
- **默认参数**：0.24s / 0.45 / 0 / 0s。
- **绑定**：target；可配置 group。
- **依赖与限制**：真实输入接入 MotionPointerFeedback；保留 Button 原 onClick。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B01.prefab`、`Presets/B01.asset`，调用 `MotionPlayer.Play("B01", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B01 按压缩小（Press Scale）。预期效果：按下缩小，松开回到原尺寸；自动演示展示完整按松周期。 使用共享 MotionPlayer 模板，duration=0.24s、strength=0.45、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；可配置 group；真实输入接入 MotionPointerFeedback；保留 Button 原 onClick。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### B02 柔和按压回弹 · Press & Bounce

按下蓄力缩小，松开短暂超过原尺寸，再恢复。

- **类型与用途**：交互反馈；主要操作按钮、领取按钮。
- **辨认差异**：先按压再回弹；A04 是首次出现，B06 是一次冲击。
- **默认参数**：0.38s / 0.55 / 0 / 0s。
- **绑定**：target；可配置 group。
- **依赖与限制**：真实输入接入 MotionPointerFeedback；同一缩放属性只有一个动画所有者。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B02.prefab`、`Presets/B02.asset`，调用 `MotionPlayer.Play("B02", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B02 柔和按压回弹（Press & Bounce）。预期效果：按下蓄力缩小，松开短暂超过原尺寸，再恢复。 使用共享 MotionPlayer 模板，duration=0.38s、strength=0.55、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；可配置 group；真实输入接入 MotionPointerFeedback；同一缩放属性只有一个动画所有者。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### B03 脉冲 / 呼吸 · Pulse / Breathing

目标柔和地放大再缩回，形成一次呼吸周期。

- **类型与用途**：循环反馈；可领取奖励、单个主按钮。
- **辨认差异**：改变大小；E01 上下移动，E04 改变可见亮度或透明度。
- **默认参数**：2.2s / 0.35 / 0 / 0s。
- **绑定**：target。
- **依赖与限制**：循环是可选参数；避免多个按钮同时强烈呼吸。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B03.prefab`、`Presets/B03.asset`，调用 `MotionPlayer.Play("B03", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B03 脉冲 / 呼吸（Pulse / Breathing）。预期效果：目标柔和地放大再缩回，形成一次呼吸周期。 使用共享 MotionPlayer 模板，duration=2.2s、strength=0.35、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；循环是可选参数；避免多个按钮同时强烈呼吸。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### B04 错误轻抖 · Shake

目标快速左右交替振动，幅度逐渐减小后归位。

- **类型与用途**：交互反馈；错误输入、资源不足、拒绝操作。
- **辨认差异**：强调快速平移；B05 是旋转，B06 是定向缩放冲击。
- **默认参数**：0.36s / 0.7 / 20 / 0s。
- **绑定**：target。
- **依赖与限制**：避免同时有其他位置动画；模板为确定性的错误轻抖便于比较。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B04.prefab`、`Presets/B04.asset`，调用 `MotionPlayer.Play("B04", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B04 错误轻抖（Shake）。预期效果：目标快速左右交替振动，幅度逐渐减小后归位。 使用共享 MotionPlayer 模板，duration=0.36s、strength=0.7、distance=20、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；避免同时有其他位置动画；模板为确定性的错误轻抖便于比较。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### B05 提示摆动 · Wiggle / Swing

围绕自身支点左右提示摆动，最后回到初始角度。

- **类型与用途**：交互反馈；礼物盒、活动入口、提醒图标。
- **辨认差异**：旋转而非位移；修改 pivot 会改变提示摆动支点。
- **默认参数**：0.55s / 0.45 / 0 / 0s。
- **绑定**：target。
- **依赖与限制**：pivot 由项目负责配置；模板不擅自改变布局锚点。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B05.prefab`、`Presets/B05.asset`，调用 `MotionPlayer.Play("B05", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B05 提示摆动（Wiggle / Swing）。预期效果：围绕自身支点左右提示摆动，最后回到初始角度。 使用共享 MotionPlayer 模板，duration=0.55s、strength=0.45、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；pivot 由项目负责配置；模板不擅自改变布局锚点。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### B06 触发冲击回弹 · Punch

受到一次缩放冲击后衰减回弹，最终恢复基线。

- **类型与用途**：交互反馈；命中反馈、获得资源、重要点击。
- **辨认差异**：一次定向冲击；B04 连续平移错误轻抖，B03 周期性呼吸。
- **默认参数**：0.42s / 0.48 / 0 / 0s。
- **绑定**：target。
- **依赖与限制**：同一属性避免叠加多个播放器。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B06.prefab`、`Presets/B06.asset`，调用 `MotionPlayer.Play("B06", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B06 触发冲击回弹（Punch）。预期效果：受到一次缩放冲击后衰减回弹，最终恢复基线。 使用共享 MotionPlayer 模板，duration=0.42s、strength=0.48、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；同一属性避免叠加多个播放器。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### B07 选中态切换 · Selection Transition

新选中项上移或放大并变色，旧选中项恢复。

- **类型与用途**：状态转换；标签页、导航、关卡选择。
- **辨认差异**：状态有明确新旧对象；不是只给一项做呼吸。
- **默认参数**：0.3s / 0.4 / 12 / 0s。
- **绑定**：target + image 为新选中；secondary + secondaryImage 为旧选中。
- **依赖与限制**：模板负责视觉转换；业务层仍决定哪个选项被选中。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B07.prefab`、`Presets/B07.asset`，调用 `MotionPlayer.Play("B07", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B07 选中态切换（Selection Transition）。预期效果：新选中项上移或放大并变色，旧选中项恢复。 使用共享 MotionPlayer 模板，duration=0.3s、strength=0.4、distance=12、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + image 为新选中；secondary + secondaryImage 为旧选中；模板负责视觉转换；业务层仍决定哪个选项被选中。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### B08 开关滑动 · Toggle Transition

滑块滑向另一端，轨道颜色同步改变。

- **类型与用途**：状态转换；设置开关、模式切换。
- **辨认差异**：位置和颜色共同表达开关状态；反向可示范关闭。
- **默认参数**：0.26s / 1 / 62 / 0s。
- **绑定**：target 为滑块；image 为轨道；destination 为开启端位置。
- **依赖与限制**：业务布尔值由项目维护；滑块终点按父级坐标配置。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/B08.prefab`、`Presets/B08.asset`，调用 `MotionPlayer.Play("B08", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 B08 开关滑动（Toggle Transition）。预期效果：滑块滑向另一端，轨道颜色同步改变。 使用共享 MotionPlayer 模板，duration=0.26s、strength=1、distance=62、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为滑块；image 为轨道；destination 为开启端位置；业务布尔值由项目维护；滑块终点按父级坐标配置。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

## C · 面板与页面切换

### C01 面板下落回弹 · Drop & Settle

面板由上方落下，轻微越过基线后回到原位。

- **类型与用途**：组合效果；胜利、失败、活动弹窗。
- **辨认差异**：一次位置越界后停稳；A05 多次落地弹跳。
- **默认参数**：0.52s / 0.65 / 82 / 0s。
- **绑定**：target + group。
- **依赖与限制**：预留移动空间；在布局节点内使用动画包装层。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/C01.prefab`、`Presets/C01.asset`，调用 `MotionPlayer.Play("C01", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 C01 面板下落回弹（Drop & Settle）。预期效果：面板由上方落下，轻微越过基线后回到原位。 使用共享 MotionPlayer 模板，duration=0.52s、strength=0.65、distance=82、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；预留移动空间；在布局节点内使用动画包装层。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### C02 抽屉展开 · Drawer Slide

侧边面板滑入，背景遮罩同时淡入；反向收起。

- **类型与用途**：组合效果；菜单、设置、背包抽屉。
- **辨认差异**：与 A02 相比额外管理背景遮罩。
- **默认参数**：0.38s / 1 / 224 / 0s。
- **绑定**：target + group；secondaryGroup 为背景遮罩。
- **依赖与限制**：遮罩 CanvasGroup 独立绑定；页面阻挡策略由业务层决定。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/C02.prefab`、`Presets/C02.asset`，调用 `MotionPlayer.Play("C02", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 C02 抽屉展开（Drawer Slide）。预期效果：侧边面板滑入，背景遮罩同时淡入；反向收起。 使用共享 MotionPlayer 模板，duration=0.38s、strength=1、distance=224、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；secondaryGroup 为背景遮罩；遮罩 CanvasGroup 独立绑定；页面阻挡策略由业务层决定。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### C03 折叠 / 展开 · Collapse / Expand

改变裁剪容器高度，展开详情内容，保持文字原比例。

- **类型与用途**：组合效果；任务详情、排行榜条目、折叠说明。
- **辨认差异**：只改变可见高度；不把文字整体压扁。
- **默认参数**：0.36s / 1 / 0 / 0s。
- **绑定**：target 为内容；mask 为独立裁剪容器。
- **依赖与限制**：mask 上需 RectMask2D；布局高度重排由项目 LayoutElement 单独衔接。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/C03.prefab`、`Presets/C03.asset`，调用 `MotionPlayer.Play("C03", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 C03 折叠 / 展开（Collapse / Expand）。预期效果：改变裁剪容器高度，展开详情内容，保持文字原比例。 使用共享 MotionPlayer 模板，duration=0.36s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为内容；mask 为独立裁剪容器；mask 上需 RectMask2D；布局高度重排由项目 LayoutElement 单独衔接。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### C04 共享元素变形 · Shared Element / Container Transform

同一个容器从缩略卡片位置和尺寸变为详情容器。

- **类型与用途**：页面转场；关卡详情、物品卡片展开。
- **辨认差异**：同一对象连续变形；C05 则是新旧两页沿同轴移动。
- **默认参数**：0.58s / 1 / 0 / 0s。
- **绑定**：target 为移动容器；destination 为目标矩形。
- **依赖与限制**：destination 定义目标矩形；支持同一 Canvas 内父空间转换、非居中 pivot 与缩放，要求轴方向一致；不支持跨 Canvas。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/C04.prefab`、`Presets/C04.asset`，调用 `MotionPlayer.Play("C04", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 C04 共享元素变形（Shared Element / Container Transform）。预期效果：同一个容器从缩略卡片位置和尺寸变为详情容器。 使用共享 MotionPlayer 模板，duration=0.58s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为移动容器；destination 为目标矩形；destination 定义目标矩形；支持同一 Canvas 内父空间转换、非居中 pivot 与缩放，要求轴方向一致；不支持跨 Canvas。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### C05 共享轴切换 · Shared Axis Transition

旧页面沿一条轴离开，新页面沿同轴进入。

- **类型与用途**：页面转场；步骤前进、返回、相邻页面。
- **辨认差异**：同时有方向性位移；C06/C07 仅通过透明度表达替换。
- **默认参数**：0.42s / 1 / 92 / 0s。
- **绑定**：target + group 为旧页；secondary + secondaryGroup 为新页。
- **依赖与限制**：新旧页要用独立的 RectTransform 和 CanvasGroup。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/C05.prefab`、`Presets/C05.asset`，调用 `MotionPlayer.Play("C05", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 C05 共享轴切换（Shared Axis Transition）。预期效果：旧页面沿一条轴离开，新页面沿同轴进入。 使用共享 MotionPlayer 模板，duration=0.42s、strength=1、distance=92、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group 为旧页；secondary + secondaryGroup 为新页；新旧页要用独立的 RectTransform 和 CanvasGroup。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### C06 淡出后淡入 · Fade Through

旧内容先淡出，中间短暂停留，再显示新内容。

- **类型与用途**：页面转场；关联较弱的页面切换。
- **辨认差异**：有空隙，两页不会同时强可见；C07 发生重叠。
- **默认参数**：0.44s / 1 / 0 / 0s。
- **绑定**：target + group 为旧页；secondary + secondaryGroup 为新页。
- **依赖与限制**：新旧页独立 CanvasGroup，隐藏页不应拦截输入。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/C06.prefab`、`Presets/C06.asset`，调用 `MotionPlayer.Play("C06", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 C06 淡出后淡入（Fade Through）。预期效果：旧内容先淡出，中间短暂停留，再显示新内容。 使用共享 MotionPlayer 模板，duration=0.44s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group 为旧页；secondary + secondaryGroup 为新页；新旧页独立 CanvasGroup，隐藏页不应拦截输入。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### C07 交叉淡化 · Crossfade

旧内容淡出的同时，新内容淡入。

- **类型与用途**：页面转场；图片、头像、标签内容切换。
- **辨认差异**：两页同时交叉，C06 则先消失后出现。
- **默认参数**：0.4s / 1 / 0 / 0s。
- **绑定**：target + group 为旧页；secondary + secondaryGroup 为新页。
- **依赖与限制**：两层叠放次序由项目决定；透明度不能替代业务激活状态。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/C07.prefab`、`Presets/C07.asset`，调用 `MotionPlayer.Play("C07", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 C07 交叉淡化（Crossfade）。预期效果：旧内容淡出的同时，新内容淡入。 使用共享 MotionPlayer 模板，duration=0.4s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group 为旧页；secondary + secondaryGroup 为新页；两层叠放次序由项目决定；透明度不能替代业务激活状态。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

## D · 数值、奖励与成就

### D01 数值滚动 · Count Up / Down

数字从起点连续更新到终点，不直接跳变。

- **类型与用途**：数值变化；分数、金币、经验结算。
- **辨认差异**：更新数字本身；D05 还编排多个条目的先后。
- **默认参数**：0.72s / 1 / 0 / 0s。
- **绑定**：target + label；settings.startValue / endValue 配置起止值。
- **依赖与限制**：当前模板使用 UGUI Text；数值权威值仍由业务系统保存。默认从 100 滚动到 1250，可配置。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/D01.prefab`、`Presets/D01.asset`，调用 `MotionPlayer.Play("D01", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D01 数值滚动（Count Up / Down）。预期效果：数字从起点连续更新到终点，不直接跳变。 使用共享 MotionPlayer 模板，duration=0.72s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true、startValue=100、endValue=1250。绑定 target + label；settings.startValue / endValue 配置起止值；当前模板使用 UGUI Text；数值权威值仍由业务系统保存。默认从 100 滚动到 1250，可配置。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D02 进度填充 · Progress Fill

填充比例平滑增长，达到目标后保持。

- **类型与用途**：数值变化；经验条、加载、任务进度。
- **辨认差异**：变化的是 Image.fillAmount；A07 是整个内容的裁剪揭示。
- **默认参数**：0.68s / 1 / 0 / 0s。
- **绑定**：target + fill；label 可显示百分比。
- **依赖与限制**：fill 使用 Image.Type.Filled；settings.fillFrom / fillTo 配置比例，默认 0.08 → 0.90。
- **反向展示**：展馆支持；反向采样同一效果。
- **模板**：`Prefabs/D02.prefab`、`Presets/D02.asset`，调用 `MotionPlayer.Play("D02", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D02 进度填充（Progress Fill）。预期效果：填充比例平滑增长，达到目标后保持。 使用共享 MotionPlayer 模板，duration=0.68s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true、fillFrom=0.08、fillTo=0.90。绑定 target + fill；label 可显示百分比；fill 使用 Image.Type.Filled；settings.fillFrom / fillTo 配置比例，默认 0.08 → 0.90。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D03 奖励飞入 · Fly to Target

奖励沿弧线飞向资源栏，落点在到达时短暂强调。

- **类型与用途**：组合效果；金币入账、星星收集、物品领取。
- **辨认差异**：单个奖励明确到达目标；D04 多个奖励先散开。
- **默认参数**：0.72s / 0.7 / 100 / 0s。
- **绑定**：target 为奖励；destination 为落点；secondary 为可选接收图标。
- **依赖与限制**：destination 必须处于同一 Canvas；支持父空间转换，不支持跨 Canvas；不依赖粒子插件。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D03.prefab`、`Presets/D03.asset`，调用 `MotionPlayer.Play("D03", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D03 奖励飞入（Fly to Target）。预期效果：奖励沿弧线飞向资源栏，落点在到达时短暂强调。 使用共享 MotionPlayer 模板，duration=0.72s、strength=0.7、distance=100、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为奖励；destination 为落点；secondary 为可选接收图标；destination 必须处于同一 Canvas；支持父空间转换，不支持跨 Canvas；不依赖粒子插件。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D04 奖励散开汇聚 · Burst / Collect

多个奖励先向外散开，再依次飞向接收位置。

- **类型与用途**：组合效果；宝箱、大额奖励、成就结算。
- **辨认差异**：先散后收；D03 只有一个奖励的飞行。
- **默认参数**：1.05s / 0.7 / 68 / 0.055s。
- **绑定**：target 为根；items 为奖励；destination 为接收位置。
- **依赖与限制**：使用有限个 UGUI 奖励图标；大量对象应使用池或另行适配粒子系统。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D04.prefab`、`Presets/D04.asset`，调用 `MotionPlayer.Play("D04", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D04 奖励散开汇聚（Burst / Collect）。预期效果：多个奖励先向外散开，再依次飞向接收位置。 使用共享 MotionPlayer 模板，duration=1.05s、strength=0.7、distance=68、stagger=0.055s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为根；items 为奖励；destination 为接收位置；使用有限个 UGUI 奖励图标；大量对象应使用池或另行适配粒子系统。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D05 逐项结算 · Sequential Count-up

条目按顺序出现并滚动数值，形成分段结算节奏。

- **类型与用途**：编排效果；关卡结算、得分明细。
- **辨认差异**：同时编排多个数字；D01 只有一项数值变化。
- **默认参数**：1.2s / 0.65 / 28 / 0.12s。
- **绑定**：target 为根；items + labels + itemGroups。
- **依赖与限制**：items、labels、itemGroups 按同一顺序对应；业务结算不在动画内计算。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D05.prefab`、`Presets/D05.asset`，调用 `MotionPlayer.Play("D05", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D05 逐项结算（Sequential Count-up）。预期效果：条目按顺序出现并滚动数值，形成分段结算节奏。 使用共享 MotionPlayer 模板，duration=1.2s、strength=0.65、distance=28、stagger=0.12s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true、startValue=100、endValue=1250。绑定 target 为根；items + labels + itemGroups；items、labels、itemGroups 按同一顺序对应；业务结算不在动画内计算。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D06 星星点亮 · Star Reveal

星星按顺序从暗到亮，点亮时短暂放大。

- **类型与用途**：编排效果；通关星级、评分、里程碑。
- **辨认差异**：对每颗星独立点亮；E05 是通用入场编排。
- **默认参数**：0.98s / 0.7 / 0 / 0.14s。
- **绑定**：target 为根；items；可配 itemGroups。
- **依赖与限制**：items 为星星图像；不代替评分计算。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D06.prefab`、`Presets/D06.asset`，调用 `MotionPlayer.Play("D06", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D06 星星点亮（Star Reveal）。预期效果：星星按顺序从暗到亮，点亮时短暂放大。 使用共享 MotionPlayer 模板，duration=0.98s、strength=0.7、distance=0、stagger=0.14s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为根；items；可配 itemGroups；items 为星星图像；不代替评分计算。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D07 卡牌翻面 · Card Flip

卡片转到侧面时切换正反面，再转回可读正面。

- **类型与用途**：组合效果；抽卡、奖励揭晓、身份牌。
- **辨认差异**：有内容切换；A06 始终展示同一面。
- **默认参数**：0.64s / 1 / 0 / 0s。
- **绑定**：target 为旋转根；group + image 为背面（初始）；secondaryGroup + secondaryImage 为正面（完成）。
- **依赖与限制**：正反面分别绑定；半程切换后禁止镜像文字持续可见。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D07.prefab`、`Presets/D07.asset`，调用 `MotionPlayer.Play("D07", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D07 卡牌翻面（Card Flip）。预期效果：卡片转到侧面时切换正反面，再转回可读正面。 使用共享 MotionPlayer 模板，duration=0.64s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为旋转根；group + image 为背面（初始）；secondaryGroup + secondaryImage 为正面（完成）；正反面分别绑定；半程切换后禁止镜像文字持续可见。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D08 宝箱开启 · Chest Open

箱体压缩蓄力、提示摆动，箱盖打开，奖励随后弹出。

- **类型与用途**：组合效果；宝箱奖励、里程碑揭晓。
- **辨认差异**：多个对象顺序组合；不是对整个宝箱只做一次缩放。
- **默认参数**：1.32s / 0.7 / 78 / 0.09s。
- **绑定**：target 为箱体；lid 为盖；items 为奖励。
- **依赖与限制**：UI 图形宝箱，无额外粒子或音频依赖；箱盖 pivot 决定铰链效果。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D08.prefab`、`Presets/D08.asset`，调用 `MotionPlayer.Play("D08", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D08 宝箱开启（Chest Open）。预期效果：箱体压缩蓄力、提示摆动，箱盖打开，奖励随后弹出。 使用共享 MotionPlayer 模板，duration=1.32s、strength=0.7、distance=78、stagger=0.09s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为箱体；lid 为盖；items 为奖励；UI 图形宝箱，无额外粒子或音频依赖；箱盖 pivot 决定铰链效果。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D09 飘字 · Floating Text

提示文字上浮并淡出，最终不再阻挡交互。

- **类型与用途**：组合效果；+100、暴击、经验变化。
- **辨认差异**：上浮后消失；E01 持续往返且保持可见。
- **默认参数**：0.82s / 0.55 / 72 / 0s。
- **绑定**：target + group；label 为文字。
- **依赖与限制**：对象池回收由项目决定；重播会恢复捕获的基线。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D09.prefab`、`Presets/D09.asset`，调用 `MotionPlayer.Play("D09", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D09 飘字（Floating Text）。预期效果：提示文字上浮并淡出，最终不再阻挡交互。 使用共享 MotionPlayer 模板，duration=0.82s、strength=0.55、distance=72、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；label 为文字；对象池回收由项目决定；重播会恢复捕获的基线。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### D10 到账轻强调 · Target Impact

接收图标短暂放大、变亮，再恢复原来的尺寸和颜色。

- **类型与用途**：组合效果；奖励抵达资源栏、目标命中确认。
- **辨认差异**：强调接收端；D03 负责飞行，可在项目中组合。
- **默认参数**：0.36s / 0.38 / 0 / 0s。
- **绑定**：target + image。
- **依赖与限制**：image 为要闪亮的图形；不要让多个动画同时修改它的颜色。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/D10.prefab`、`Presets/D10.asset`，调用 `MotionPlayer.Play("D10", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 D10 到账轻强调（Target Impact）。预期效果：接收图标短暂放大、变亮，再恢复原来的尺寸和颜色。 使用共享 MotionPlayer 模板，duration=0.36s、strength=0.38、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + image；image 为要闪亮的图形；不要让多个动画同时修改它的颜色。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

## E · 循环、引导与编排

### E01 悬浮 · Float

目标缓慢上浮再回落，形成一次上下周期。

- **类型与用途**：循环效果；头像、奖励、引导箭头。
- **辨认差异**：主要改变位置；B03 主要改变大小。
- **默认参数**：2.8s / 0.7 / 12 / 0s。
- **绑定**：target。
- **依赖与限制**：循环可选；停止恢复最初位置。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/E01.prefab`、`Presets/E01.asset`，调用 `MotionPlayer.Play("E01", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 E01 悬浮（Float）。预期效果：目标缓慢上浮再回落，形成一次上下周期。 使用共享 MotionPlayer 模板，duration=2.8s、strength=0.7、distance=12、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；循环可选；停止恢复最初位置。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### E02 循环旋转 · Rotate Loop

围绕自身中心转过一圈，可连续循环。

- **类型与用途**：循环效果；加载图标、光环。
- **辨认差异**：角度持续变化；B05 是角度往返提示摆动。
- **默认参数**：2.8s / 1 / 0 / 0s。
- **绑定**：target。
- **依赖与限制**：每周期固定旋转 360 度；strength 不影响此项，用 speed 或 duration 调速。图形应能辨认方向。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/E02.prefab`、`Presets/E02.asset`，调用 `MotionPlayer.Play("E02", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 E02 循环旋转（Rotate Loop）。预期效果：围绕自身中心转过一圈，可连续循环。 使用共享 MotionPlayer 模板，duration=2.8s、strength=1、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target；每周期固定旋转 360 度；strength 不影响此项，用 speed 或 duration 调速。图形应能辨认方向。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### E03 扫光 · Shine / Sweep

一条高光穿过卡片或按钮，仅在遮罩范围内可见。

- **类型与用途**：遮罩组合；稀有卡片、可领取按钮、活动入口。
- **辨认差异**：移动额外高光层；E04 则改变目标整体亮度或透明度。
- **默认参数**：1.05s / 0.65 / 180 / 0s。
- **绑定**：target 为被照亮容器；shine 为高光条；mask 为裁剪容器。
- **依赖与限制**：shine 是高光子物体；父级使用 Mask 或 RectMask2D。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/E03.prefab`、`Presets/E03.asset`，调用 `MotionPlayer.Play("E03", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 E03 扫光（Shine / Sweep）。预期效果：一条高光穿过卡片或按钮，仅在遮罩范围内可见。 使用共享 MotionPlayer 模板，duration=1.05s、strength=0.65、distance=180、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为被照亮容器；shine 为高光条；mask 为裁剪容器；shine 是高光子物体；父级使用 Mask 或 RectMask2D。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### E04 柔和呼吸亮度 · Blink / Flash

透明度缓慢降低再恢复，完成一次柔和呼吸亮度周期。

- **类型与用途**：循环效果；状态提醒、暂时强调。
- **辨认差异**：改变可见强度；B03 是缩放呼吸。
- **默认参数**：2.4s / 0.35 / 0 / 0s。
- **绑定**：target + group。
- **依赖与限制**：默认保持较慢节奏；没有自动高频闪光或音效。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/E04.prefab`、`Presets/E04.asset`，调用 `MotionPlayer.Play("E04", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 E04 柔和呼吸亮度（Blink / Flash）。预期效果：透明度缓慢降低再恢复，完成一次柔和呼吸亮度周期。 使用共享 MotionPlayer 模板，duration=2.4s、strength=0.35、distance=0、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target + group；默认保持较慢节奏；没有自动高频闪光或音效。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### E05 依次入场 · Stagger

多个元素按间隔依次滑入、淡入，形成阅读顺序。

- **类型与用途**：编排效果；菜单、结算按钮、任务列表。
- **辨认差异**：编排多个对象；A02 是单个对象的滑入。
- **默认参数**：0.76s / 0.6 / 32 / 0.075s。
- **绑定**：target 为根；items + itemGroups。
- **依赖与限制**：items 与 itemGroups 对应；布局负责位置，动画放在子包装层。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/E05.prefab`、`Presets/E05.asset`，调用 `MotionPlayer.Play("E05", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 E05 依次入场（Stagger）。预期效果：多个元素按间隔依次滑入、淡入，形成阅读顺序。 使用共享 MotionPlayer 模板，duration=0.76s、strength=0.6、distance=32、stagger=0.075s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为根；items + itemGroups；items 与 itemGroups 对应；布局负责位置，动画放在子包装层。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### E06 跟随 / 拖尾 · Follow / Trail

多个图标沿同一路线以时间差运动，产生拖尾层次。

- **类型与用途**：编排效果；连串奖励、引导装饰、路径强调。
- **辨认差异**：时间错开的队列；D04 是先爆散再汇聚。
- **默认参数**：2.5s / 0.6 / 68 / 0.1s。
- **绑定**：target 为根；items 为跟随图标。
- **依赖与限制**：示例使用有限 UGUI 图标，非实时粒子拖尾；不是全局鼠标跟随器。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/E06.prefab`、`Presets/E06.asset`，调用 `MotionPlayer.Play("E06", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 E06 跟随 / 拖尾（Follow / Trail）。预期效果：多个图标沿同一路线以时间差运动，产生拖尾层次。 使用共享 MotionPlayer 模板，duration=2.5s、strength=0.6、distance=68、stagger=0.1s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为根；items 为跟随图标；示例使用有限 UGUI 图标，非实时粒子拖尾；不是全局鼠标跟随器。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

### E07 引导手势 · Tutorial Gesture

指示手移动到目标、做按压动作，随后复位。

- **类型与用途**：编排效果；新手点击或滑动教学。
- **辨认差异**：包含明确目标和按压时刻；E01 只是悬浮。
- **默认参数**：1.65s / 0.75 / 72 / 0s。
- **绑定**：target 为手势图形；destination 为目标；secondary 为高亮目标。
- **依赖与限制**：UI 手势图形与高亮由绑定对象提供；不自动触发真实按钮点击。
- **反向展示**：展馆不开放；运行库反向采样不代表业务撤销。
- **模板**：`Prefabs/E07.prefab`、`Presets/E07.asset`，调用 `MotionPlayer.Play("E07", settings)`。

Codex 提示词：

> 为我的目标 UGUI 接入 E07 引导手势（Tutorial Gesture）。预期效果：指示手移动到目标、做按压动作，随后复位。 使用共享 MotionPlayer 模板，duration=1.65s、strength=0.75、distance=72、stagger=0s、ease=Preset、speed=1、reverse=false、loop=false、unscaledTime=true。绑定 target 为手势图形；destination 为目标；secondary 为高亮目标；UI 手势图形与高亮由绑定对象提供；不自动触发真实按钮点击。 保留现有业务交互；重复触发先取消本实例动画并恢复基线，关闭或销毁时清理，透明对象不阻挡点击。实际编译与运行后报告验证结果和未验证项。 视觉遵循目标项目已认可原型；分开命中、布局、文字与可变形层。默认 overshoot=1.1，按材质调整，避免全局套用大幅弹簧。

## F · 进阶设计组合

以下效果以公开资料中的运动设计原则为参考，使用原创形状、配色和编排进行转化。最终交付以实际构建和验收结果为准。

### F01 柔光扫描揭晓 · Holographic Scan Reveal

卡片分段展开，光带横扫，扫描细条依次点亮，最后露出完整内容。

- **用途**：稀有卡、科技奖励、档案解锁。
- **视觉差异**：分段几何展开与扫描线协同，而非单纯透明度或单次扫光。
- **默认参数**：1.15s / 0.65 / 140 / 0.075s。
- **绑定**：target=卡片；group=卡片组；shine=光带；items=扫描细条。
- **依赖**：UGUI 与 DOTween 免费版；各绑定对象需按模板建立，动画对象使用布局内包装层。
- **模板**：`Prefabs/F01.prefab`、`Presets/F01.asset`；调用 `MotionPlayer.Play("F01", settings)`。
- **参考来源**：[Animation in the League Client](https://www.riotgames.com/en/news/animation-league-legends-client)。借鉴客户端战利品场景的分层动画与焦点建立，原创改造为几何卡片、扫描带和信息显现；不使用原游戏素材。 只借鉴运动原则，不复制源站商标、插画或素材。

Codex 提示词：

> 为我的 Unity UGUI 界面接入 F01 柔光扫描揭晓（Holographic Scan Reveal）：卡片分段展开，光带横扫，扫描细条依次点亮，最后露出完整内容。 使用同源 MotionPlayer，duration=1.6s、strength=1、distance=140、stagger=0.1s、ease=Preset。绑定 target=卡片；group=卡片组；shine=光带；items=扫描细条。保留业务逻辑，处理重复播放、中断、禁用销毁和布局重算；实际预览并报告验证结果。

### F02 星轨徽章解锁 · Constellation Assemble

星点由外向中心逐颗汇聚，环旋转，徽章延后显现。

- **用途**：成就解锁、章节里程碑。
- **视觉差异**：多个相互关联星点先汇聚，再揭示徽章，区别于普通逐个点亮。
- **默认参数**：1.55s / 0.65 / 112 / 0.085s。
- **绑定**：target=徽章；group=徽章组；secondary=旋环；items/itemGroups=星点。
- **依赖**：UGUI 与 DOTween 免费版；各绑定对象需按模板建立，动画对象使用布局内包装层。
- **模板**：`Prefabs/F02.prefab`、`Presets/F02.asset`；调用 `MotionPlayer.Play("F02", settings)`。
- **参考来源**：[VALORANT 0.50 patch notes](https://playvalorant.com/en-us/news/game-updates/valorant-patch-notes-0-50/)。仅借鉴契约升级与解锁反馈的设计意图；星图、汇聚与徽章为本项目原创，并非源游戏所述视觉。 只借鉴运动原则，不复制源站商标、插画或素材。

Codex 提示词：

> 为我的 Unity UGUI 界面接入 F02 星轨徽章解锁（Constellation Assemble）：星点由外向中心逐颗汇聚，环旋转，徽章延后显现。 使用同源 MotionPlayer，duration=2.0s、strength=1、distance=150、stagger=0.12s、ease=Preset。绑定 target=徽章；group=徽章组；secondary=旋环；items/itemGroups=星点。保留业务逻辑，处理重复播放、中断、禁用销毁和布局重算；实际预览并报告验证结果。

### F03 分层标题入场 · Kinetic Editorial Title

标题字块错时从不同方向进入，下划线逐渐展开形成终态。

- **用途**：章节标题、活动开场、结算标题。
- **视觉差异**：强调字块节奏与方向对位，区别于相同元素统一位移的错列。
- **默认参数**：1.05s / 0.65 / 60 / 0.075s。
- **绑定**：target=稳定容器；items/itemGroups=字块；secondary=底线。
- **依赖**：UGUI 与 DOTween 免费版；各绑定对象需按模板建立，动画对象使用布局内包装层。
- **模板**：`Prefabs/F03.prefab`、`Presets/F03.asset`；调用 `MotionPlayer.Play("F03", settings)`。
- **参考来源**：[Making Stagger Reveal Animations for Text](https://tympanus.net/codrops/2020/06/17/making-stagger-reveal-animations-for-text/)。借鉴文字错时揭示原则，原创改造为鲜艳游戏标题、独立字块与底线编排，不复制源码和版式。 只借鉴运动原则，不复制源站商标、插画或素材。

Codex 提示词：

> 为我的 Unity UGUI 界面接入 F03 分层标题入场（Kinetic Editorial Title）：标题字块错时从不同方向进入，下划线逐渐展开形成终态。 使用同源 MotionPlayer，duration=1.7s、strength=1、distance=120、stagger=0.1s、ease=Preset。绑定 target=稳定容器；items/itemGroups=字块；secondary=底线。保留业务逻辑，处理重复播放、中断、禁用销毁和布局重算；实际预览并报告验证结果。

### F04 信息卡片编排 · Layered Dashboard Entrance

信息卡按层级和时间差进入，进度条尾随填充，形成有阅读顺序的界面。

- **用途**：数据面板、结算概览、任务仪表板。
- **视觉差异**：卡片与进度的先后关系有明确因果，不是整体面板缩放。
- **默认参数**：1.25s / 0.65 / 64 / 0.08s。
- **绑定**：target=稳定容器；items/itemGroups=卡片；secondary=进度条；fill=填充 Image。
- **依赖**：UGUI 与 DOTween 免费版；各绑定对象需按模板建立，动画对象使用布局内包装层。
- **模板**：`Prefabs/F04.prefab`、`Presets/F04.asset`；调用 `MotionPlayer.Play("F04", settings)`。
- **参考来源**：[Rovo motion](https://atlassian.design/rovo-ui/motion)。借鉴标题、内容和卡片的视觉先后，原创设计信息卡与进度条的因果编排。 只借鉴运动原则，不复制源站商标、插画或素材。

Codex 提示词：

> 为我的 Unity UGUI 界面接入 F04 信息卡片编排（Layered Dashboard Entrance）：信息卡按层级和时间差进入，进度条尾随填充，形成有阅读顺序的界面。 使用同源 MotionPlayer，duration=1.9s、strength=1、distance=120、stagger=0.12s、ease=Preset。绑定 target=稳定容器；items/itemGroups=卡片；secondary=进度条；fill=填充 Image。保留业务逻辑，处理重复播放、中断、禁用销毁和布局重算；实际预览并报告验证结果。

### F05 焦点卡片交接 · Magnetic Focus Carousel

新卡移到焦点并放大上移，旧卡退场缩小，周围候选错位；真实鼠标跟随由独立 MotionParallaxTilt 组件提供。

- **用途**：角色选择、商店推荐、关卡选择。
- **视觉差异**：表现新旧焦点交接与相邻卡的空间层级，不只是单对象选中缩放。
- **默认参数**：0.95s / 0.65 / 108 / 0.065s。
- **绑定**：target=新卡；secondary=旧卡；items/itemGroups=候选；鼠标倾斜另需 MotionParallaxTilt 的 surface、highlight、glint 和 EventSystem。
- **依赖**：UGUI 与 DOTween 免费版；各绑定对象需按模板建立，动画对象使用布局内包装层。
- **模板**：`Prefabs/F05.prefab`、`Presets/F05.asset`；调用 `MotionPlayer.Play("F05", settings)`。
- **参考来源**：[Material Design Awards 2019](https://design.google/library/material-design-awards-2019)。借鉴 Reflectly 案例轮播升层和选择状态，原创改造为焦点卡、新旧卡及候选的权重转换。 只借鉴运动原则，不复制源站商标、插画或素材。

Codex 提示词：

> 为我的 Unity UGUI 界面接入 F05 焦点卡片交接（Magnetic Focus Carousel）：新卡移到焦点并放大上移，旧卡退场缩小，周围候选错位；真实鼠标跟随由独立 MotionParallaxTilt 组件提供。 使用同源 MotionPlayer，duration=1.6s、strength=1、distance=160、stagger=0.08s、ease=Preset。绑定 target=新卡；secondary=旧卡；items/itemGroups=候选；鼠标倾斜另需 MotionParallaxTilt 的 surface、highlight、glint 和 EventSystem。保留业务逻辑，处理重复播放、中断、禁用销毁和布局重算；实际预览并报告验证结果。

### F06 秘境奖励揭晓 · Portal Prize Reveal

光环先展开旋转，碎片向外辐射，中心奖励延后显现。

- **用途**：高级奖励、成就揭晓、章节通关。
- **视觉差异**：多层景深与延迟揭示形成完整节奏，区别于单个 Pop 或爆散汇聚。
- **默认参数**：1.65s / 0.6 / 120 / 0.09s。
- **绑定**：target=核心；group=核心组；secondary=光环；secondaryGroup=光环组；items/itemGroups=碎片。
- **依赖**：UGUI 与 DOTween 免费版；各绑定对象需按模板建立，动画对象使用布局内包装层。
- **模板**：`Prefabs/F06.prefab`、`Presets/F06.asset`；调用 `MotionPlayer.Play("F06", settings)`。
- **参考来源**：[Animation in the League Client](https://www.riotgames.com/en/news/animation-league-legends-client)。借鉴奖励界面的分层视觉庆祝；传送环、碎片和奖励核心为本项目原创，原文未描述传送门。 只借鉴运动原则，不复制源站商标、插画或素材。

Codex 提示词：

> 为我的 Unity UGUI 界面接入 F06 秘境奖励揭晓（Portal Prize Reveal）：光环先展开旋转，碎片向外辐射，中心奖励延后显现。 使用同源 MotionPlayer，duration=2.2s、strength=1、distance=150、stagger=0.14s、ease=Preset。绑定 target=核心；group=核心组；secondary=光环；secondaryGroup=光环组；items/itemGroups=碎片。保留业务逻辑，处理重复播放、中断、禁用销毁和布局重算；实际预览并报告验证结果。


## 用户提供的补充参考

以下链接以作者描述、示例项目说明或用户提供的入口作为研究依据，未逐帧核验其演示；各项目的画面和脚本均未复制。

- F01 方块/遮罩揭示：[Codrops BlockRevealers](https://github.com/codrops/BlockRevealers)；抽象色块揭示的节奏，转为原创扫描卡片。
- F03 逐字符编排：[TypographyMotion](https://github.com/codrops/TypographyMotion/)，以及 [作者文章：错时文字揭示](https://tympanus.net/codrops/2020/06/17/making-stagger-reveal-animations-for-text/)。参考错时与阅读顺序，原创标题内容和版式。
- F05 轮播焦点层级：[Material Design Awards 中的 Reflectly](https://design.google/library/material-design-awards-2019)；另查阅 [Rapid Image Hover Menu](https://tympanus.net/codrops/2020/07/01/creating-a-menu-image-animation-on-hover/) 的悬停视觉跟随概念，示例图片标注 CC BY-NC-ND，本项目不分发或复制图片。
- F05 卡片层次另参考 [Balatro 卡片鼠标悬停研究](https://www.wavebeem.com/toybox/2025/balatro/) 的多层反光描述；卡图与几何层为原创。仅当实现经实际测试时，才能宣称支持鼠标悬停，自动播放演示不能代替真实输入验证。
