# 整页编排：P01 旅程达成

P01 在主展馆“编排”分类中，把背景、弹窗、徽章、奖励和操作区放进同一条可采样时间轴。它由 `MotionPageSequence` 驱动，不属于 MotionCatalog 的 46 项编号；不要调用 `MotionPlayer.Play("P01")`。整页配方与基础效果在同一目录中选取，无需进入独立实验室。

默认标准时长 1.10 秒。归一化区间如下，切换档位后秒数随 duration 缩放：

| 层 | 时间轴区间 | 表现 |
|---|---|---|
| 背景遮罩 | 0–0.18 | 淡入并衬托弹窗 |
| 弹窗 | 0–0.28 | 小幅上移、缩放越界后稳定 |
| 徽章 | 0.16–0.56 | 独立横向进入与柔和回弹 |
| 奖励 | 0.45–0.82 | 卡片错峰进入，金币弧线归集 |
| 操作区 | 0.78–1 | 最后显现，到末态才接收输入 |

文字始终为原生 UGUI Text，底板、徽章、图标和命中层分别绑定；不要将标题烘焙到生成图里，也不要把固定命中区域随装饰一起缩放。视觉图来自实际透明 PNG：`Resources/StyleArt/cream-panel.png`（九宫格）、`achievement-medal.png`、`reward-icons.png`（四格图集）。图集左上星星、右上幼芽、左下宝石、右下金币；每格按真实纹理尺寸的一半切割。按钮复用公共 PNG 皮肤，背景复用同风格花园插画，没有 SVG 占位。

## Runtime 接线

先建立布局与独立动画包装层，再设置 panel、emblem；可选 rewardA/rewardB、walletTarget、coins。分别绑定 backdrop、panelGroup、emblemGroup、rewardsGroup、actionsGroup。coins 需要与 walletTarget 在同一 Canvas 内，父级为 RectTransform，坐标轴方向一致；跨 Canvas/异向父级需另行转换和测试。

```csharp
sequence.CaptureBaseline(); // 所有绑定和布局稳定后捕获
sequence.Seek(0);           // 打开前首帧准备，避免先闪出完整页面
sequence.Play();
sequence.Pause();
sequence.Seek(.5f);         // 暂停定位，不触发完成事件
sequence.Resume();
// 页面关闭、取消或回池时：
sequence.Restore();
```

Play 从头开始并递增 PlayVersion；Update 使用独立时钟推进，也可在确定性测试中调用 Step(seconds)，正常运行不要再手动重复 Step。默认使用非缩放时间。失焦或后台暂停推进；OnDisable 恢复基线。Seek/Sample 对归一化值夹紧，忽略非有限值，不触发完成事件；Seek 到末帧后 Resume 不会补发完成。Play 完成每轮仅发一次 onVisualComplete，减弱动效直接达到必要末态并发一次视觉完成。设置 reducedMotion 后先 Seek 不会产生事件。

Restore 恢复捕获的缩放、位置、透明度和操作区输入标志。宿主原本禁用的 actionsGroup 到末态也保持禁用。重排布局时先 Restore，完成新布局后再 CaptureBaseline，避免采到半程动画。组件按独立包装层使用，不与 MotionPlayer 或布局组件同时写同一属性。

onVisualComplete 只表示视觉结束。金币计数在 GalleryPagePreview 中演示 1000→1120，既不写入真实钱包，也不发奖励。结算、持久化、导航和防重复提交始终由宿主事务负责；不要把拖动时间轴、重播或截图当作奖励发放入口。

## 展馆操作与导出

顶部四个阶段按钮用于暂停定位；下方时间轴可以连续拖动，暂停后继续会从当前时刻推进。自动模式进入即播放，真实操作模式提供“点击播放整页”。不支持的循环、反向和对比控件会禁用。

schemaVersion=3、previewMode=page 的 recipe 包含 `driver=MotionPageSequence`、实际 settings、profile、milestones 和图片资源路径。effects 数组为空。GalleryConfiguration 是展馆 DTO；Runtime 宿主可自行解析需要的字段，无需为了 JSON 导入整个 Gallery。
