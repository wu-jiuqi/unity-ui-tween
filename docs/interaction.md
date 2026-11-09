# 指针、拖动与弹性反馈

重排中的列表与教学引导还需处理稳定业务身份、滚动归属和输入让位，见 [生产配方](production-recipes.md)。视觉变形不能改变真实点击对象或代替用户输入。

保留项目现有 Button 与业务回调。将固定且可射线命中的 `ButtonHitArea` 留给布局和交互，在其下单独建立 `MotionVisual` 作为动画目标；不要在按压中缩放命中区，也不要与 Animator、LayoutGroup 或另一脚本并写同一属性。

`MotionButtonInput` 挂在已有 Button 的固定命中区上；`visualPlayer.bindings.target` 必须指向命中区的子对象而非本体。组件保留 Button.onClick，不实现拖拽处理器，允许父级 ScrollRect 接收滚动；依照 EventSystem 拖拽阈值撤销视觉按压。它仅跟踪当前活动指针；停用、失焦或应用暂停时取消状态。`releaseOnExit` 只控制移出时的按压视觉，区域外松开仍取消点击。用 `RefreshBindings()` 更新引用，用 `CancelInteraction()` 主动取消。不要与旧 `MotionPointerFeedback` 同时控制相同目标；旧 Prefab 并不会自动迁移。

`MotionDragInput` 是明确需要消费拖动的控件，不应直接放到普通 ScrollRect 按钮上。绑定固定 `hitArea` 和可选 `MotionJellyDrag`；`onDragStarted`、`onDisplacementChanged`、`onReleased`、`onCancelled` 分别通知宿主。位移是命中区本地坐标相对起点，过阈值才开始拖动；捕获的指针移出后仍继续追踪。停用、失焦与应用暂停会取消交互；销毁或重用前也由宿主取消。

物理弹性与固定时长 Tween 是不同模型。`MotionJellyDrag` 使用饱和位移及弹簧回位：调节幅度前检查父级缩放与 Canvas 坐标。阻尼弹簧保留速度，低帧率和长帧不能简单截断时间；Tween 的从当前姿态接续仅保证位置连续，不自动保证速度连续。宿主仍需确定结束状态和业务点击/拖动结果。

接入时检查多指竞争、按下后拖出与松开、列表滚动、滚动中取消、触摸取消、对象池复用和失焦/后台。编辑器鼠标、合成 PointerEvent、真机触摸是不同证据；未运行的项须明示，不凭组件存在判断通过。
