# 按钮交互配方 BX01–BX08

这八套新增交互由 `TweenGallery.MotionButtonFeedback` 提供，与原 A01–F06 共 46 项目录独立。原 B01–B08 和 Prefab 不变；不要把 BX 编号传给 MotionPlayer.Play。完整包提供按钮工坊，Runtime 包提供同一运行组件，可绑定宿主自己的按钮。

| 编号 | style | 可观察差异 | 必需装饰绑定 |
|---|---|---|---|
| BX01 | SoftPress | 按住等比轻压，释放衰减回弹 | 无 |
| BX02 | Jelly | 按住横向舒展、纵向挤压，释放回弹 | 无 |
| BX03 | Lift | 鼠标悬停轻抬，按下下沉 | 无；触屏只有按压，无悬停 |
| BX04 | IconPop | 按压底板，成功松开后图标上跃、轻转归位 | icon |
| BX05 | Ripple | 成功松开后从中心扩散并消隐 | ripple、rippleGroup |
| BX06 | Sheen | 成功松开后独立斜高光横向扫过 | sheen、sheenGroup |
| BX07 | Selection | 业务选中后轻抬并显示选中层，取消后归位 | selection、selectionGroup |
| BX08 | Loading | 按压反馈；业务 busy 时图标持续旋转，结束归位 | icon |

涟漪为中心扩散，不是跟随鼠标落点。BX07 是选中视觉适配器，不自动创建 ToggleGroup 或切页。BX08 只显示加载，不自动发请求、不决定超时、不禁用宿主 Button。组件会拒绝自己的 busy 按压；宿主仍需按业务阻止重复请求及管理 interactable。已有 OnClick 不会被替换或触发。

## 层级与最小接入

```text
ButtonRoot               // 布局、固定 Graphic、Button、MotionButtonFeedback
  Selection              // 可选兄弟层，装饰 Graphic 不拦截输入
  Surface                // 独立视觉包装层，不能就是 ButtonRoot
    Skin
    Ripple / Sheen       // 独立层；需要轮廓裁切时置于适当 Mask 内
    Icon / Label
```

```csharp
var feedback = button.gameObject.AddComponent<TweenGallery.MotionButtonFeedback>();
feedback.surface = animatedChild;
feedback.icon = iconChild; // BX04 / BX08 需要
feedback.style = TweenGallery.ButtonMotionStyle.IconPop;
feedback.CaptureBaseline(); // 绑定和布局稳定之后
// 现有 button.onClick 继续由业务自己管理。
feedback.SetSelected(selectedByHost);
feedback.SetBusy(requestInFlight);
```

适用于 UGUI Button 与普通 Selectable。文字用宿主的 Text 或 TMP；新组件不引用 TMP，亦不依赖 DOTween UGUI 扩展。它复用 MotionSpring1D 的解析弹簧处理连续打断，不占用 MotionPlayer 的 46 项采样时间轴。`Step(dt)` 可用于受控预览，正常运行由 Update 调用；不要在同一帧同时手动 Step 和自动 Update。

## 参数与时钟

默认 frequency=7 Hz、damping=.64、intensity=1。按下趋向压缩目标，松开趋向零，接续当前速度；这是弹簧过程，不承诺固定毫秒结束。单次 icon/ripple/sheen 装饰持续 .55 秒。项目可从 intensity .6–1、damping .65–.9 起调；圆形工具按钮宜减小强度，避免影响图标识别。

默认使用 unscaledTime；局内随暂停冻结时设置为 false。reducedMotion 关闭装饰运动、旋转和抬升，保留较小按压及必要选中状态。失焦/后台取消本次按压并暂停该组件的时间推进，不在恢复时补播点击。忙碌状态由业务维持；后台回来是否继续请求是宿主逻辑。

## 生命周期与并发

- 固定命中层接收输入；不实现 IDragHandler，父 ScrollRect 保留拖动入口。超过 EventSystem 拖动阈值或收到 dragging 后取消点击；只有主指针负责本次按压。
- pointer exit 后释放压缩；按住重新进入可恢复按压。外部松开、拖动、取消、失焦、禁用均不产生成功装饰。
- 键盘/手柄 Submit 播放视觉反馈；业务事件仍由原 Button 处理。不要为了补动画再次 Invoke onClick。
- 宿主禁用 Selectable 时要保持动画层和可点击状态一致。加载、选中、disabled 外观可以由宿主单独管理；本组件不修改 Button.interactable。
- 一套 feedback 独占 surface 及其可选层；不要与 MotionPlayer、Animator、另一个 feedback 或 Button 的 Animation/Scale 过渡同时写相同属性。外层入场和内层按压可用不同包装层组合。
- `CancelInteraction()` 取消临时输入和装饰；`Restore()` 恢复采集的 Transform/CanvasGroup 值。改变布局前先取消并 Restore，调整后再 CaptureBaseline。不要在变形中把当前姿态采成新基线；更换绑定前先恢复旧绑定。
- OnDisable 清理输入、busy 和临时运动并恢复原值，保留 IsSelected 语义；复用对象时宿主显式 SetSelected/SetBusy 同步新对象状态。装饰 CanvasGroup/Graphic 由宿主配置为不拦截输入。

## 预览与验证

Windows 展馆顶部“按钮工坊”，或启动参数 `--button-lab`。八种配方均可真实点击；工坊的“减弱动效”和“结束加载”控制用于对照。加载演示由右侧按钮结束。

Editor：`TweenGallery.Editor.ButtonFeedbackValidation.Run`。Player：`--button-test --test-output <folder>`，检查真实构建中的图集、八种按压、固定命中区域、原业务事件、选中及减弱动效，输出状态图和 button-report.json。事件自动化不等于真机触摸或人工手感评测。
