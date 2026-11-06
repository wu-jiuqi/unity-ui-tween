# 移动端、文本与性能

## UGUI Text 和可选 TMP

基础 Runtime 继续使用 UGUI Text，未安装 TMP 的项目不需导入 TMP。可选 `Optional/TMP/TmpMotionAdapter.cs` 仅在项目已有 TMP、DOTween 且显式启用 `TWEEN_GALLERY_TMP` 后参与编译；详见同目录 README。不要安装或替换现有插件作为接入的隐含步骤。

`TmpMotionAdapter.Bind(TMP_Text)` 绑定后，可用 `PlayInteger(from, businessValue, duration, ...)`、`PlayPercent(fromPercent, businessPercent, duration, ...)` 或 `Reveal(duration, ...)`；数字只在整数变化时更新显示。业务变化后宿主需停止旧动画或通过 `SetBusinessInteger` 写入权威值，避免晚到的动画覆盖；`Pause`、`Resume`、`Stop` 的作用限于该组件。逐字显示基于 TMP `maxVisibleCharacters`，不承诺严格 Unicode 字素簇切分；动态富文本变化应重新启动并实测。

## 移动验收

1. 在竖屏和横屏、不同 CanvasScaler 与分辨率下检查安全区、可点击区域、文本裁剪及字号。系统安全区由宿主布局实际应用，不要假设模板自动完成。
2. 真机触摸 ScrollRect 子按钮：轻点保留点击，超过系统阈值拖动滚动并取消点击；检查多指、拖出继续跟踪和触摸取消。
3. 前后台切换、应用失焦、关闭界面、销毁和对象池回借时检查指针、音效和延迟任务无残留；必要状态在减弱模式到达业务终态。
4. 用人工或自动注入长帧/低帧率，确认弹簧不丢失经过时间且恢复稳定；确认奖励并发与回池不会写到旧租约。
5. 分别测试无 TMP 的基础工程、启用 TMP 的真实目标工程以及 TMP 数字终值和逐字排版；非真实平台的编辑器测试不能代替真机。

## 性能证据

缓存绑定，不在逐帧无差别调用 `GetComponent`、`GetComponentsInChildren` 或反复分配字符串；实际组件层级变化时显式刷新，不能依赖失效引用。用目标设备 Development Player 的 Profiler 或可复现运行数据记录机型、分辨率、构建、同时播放数量、CPU 时间、GC 分配、帧时间、批次和测试时长，再说明结论。没有 Profiler/真机数据时，只能描述设计意图和已运行验证，不能宣称性能达标。
