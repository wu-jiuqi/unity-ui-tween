# 编排、租约、播放条件与音频

## 播放与生命周期

按 `MotionPlayer` 当前 API 区分基线重播和当前姿态接续。常规 `Play` 停掉本实例的旧 Tween 并从稳定基线开始；`ContinueTo(MotionPose destination, float duration, Ease ease)` 从当前位置、缩放、旋转和透明度出发，不重采稳定基线，固定时长 Tween 不保证速度连续。可由 `MotionPose.Read(target, group)` 读取并设置目标字段；需要速度连续的变目标运动时使用独立 `MotionSpring1D` / `MotionSpring2D`，避免声称 Tween 插值保留速度。关闭或回收时显式调用 `StopAndRestore()`。

`MotionPlaybackLease` 将对象池借出与单次播放区分：每次借出调用 `Borrow()`，每次播放 `BeginPlayback()` 取得 token，取消则 `CancelPlayback()`。可在 `BeginPlayback(pageOwner)` 提供独立页面播放器，同时验证页面播放版本；无需页面归属时省略。异步等待后、每次写回前和回调前均用 `IsCurrent(token)` 或 `TryRun(token, action)` 检查；合法归还使用 `TryReturn(token, action)`，它先使租约失效再执行回池。`RunAfter` 只保证其自身等待后的校验，不能替代后续所有写回点的检查。不要把对象是否仍存在误当作同一次租约。

## 宿主条件与减弱模式

`MotionPlaybackGate` 的 `MotionPurpose` 分为 DecorativeLoop、InteractionFeedback、EssentialTransition；根据 `SetContext(pageVisible, viewportVisible, reduceMotion)`、`SetHostBlockers(modalVisible, navigationInProgress)` 和返回的 Play、Restore、CompleteImmediately 决定播放、恢复或落到终态。需要时由宿主将 `InteractionCancelled` 显式连接到当前 `MotionButtonInput` / `MotionDragInput.CancelInteraction()`，并在销毁或重绑定时解绑；不要取消无关页面的输入。宿主负责在页面隐藏、复用及重新显示时更新上下文和业务状态，并在停用或关闭对象之前调用 `SetContext`；组件不改变全局游戏暂停，也不自动替代业务转场。减弱动效要确保必要状态最终仍正确，装饰循环不应在不可见时持续占用。

## 声音

`MotionSoundTimeline` 与视觉采样分开，`Sample()` 不播放音效，也不会自动绑定音轨。宿主使用 `Begin(duration, looping, emitAtStart)` 建立播放，并在真实播放期间自行维护未回绕的秒数、调用 `Advance(unwrappedSeconds, paused, reverseOrSeek)`；该组件根据跨越时间节点触发，每轮一次。暂停不推进，反向或拖动定位静默，`Skip()` / `Stop()` 取消后续声音。停止时只作用于明确归该组件所有的 AudioSource；共享音频管理器和已有音效由项目管理。检查循环接缝、长帧跨越多个节点、取消后回调和对象回池。

以上是接入契约，不表示已在目标项目或手机上通过测试。实际校验应记录编译、运行条件、目标状态和观察到的行为。
