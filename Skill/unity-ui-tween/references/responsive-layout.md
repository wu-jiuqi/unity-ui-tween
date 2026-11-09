# 响应式与交互：P02 奖励选择

P02 与其他效果一起位于主展馆“编排”分类，采用 `MotionResponsiveLayout` 和独立按钮反馈。它不是 MotionPlayer 编号。并排或单独查看三种参考尺寸：390×520 短屏、390×780 长屏、844×390 横屏；支持旋转当前视图，三个预览共享宿主选择。

短屏纵向排列三张奖励卡，正文由 ScrollRect 滚动，确认/返回固定在底部；长屏完整显示正文，横屏转为三列并排和底部双按钮。安全预留内的标题、正文与操作区互不覆盖。旋转复用已有控件并保留选择；回到短屏恢复先前滚动位置。切换档位或减弱动效也保留尺寸、选择与滚动位置。

## 宿主接线顺序

1. 在根 RectTransform 上添加 MotionResponsiveLayout，绑定 safeRoot、title、viewport、content、actions、cards 和 ScrollRect；viewport 使用 RectMask2D。
2. insets 按 `(左, 上, 右, 下)` 填入根 UI 参考单位。默认 `(18,34,18,24)` 是演示值；实际应用要由宿主把 Screen.safeArea 转换到当前 Canvas/根节点坐标，不能直接将屏幕像素填入缩放后的 UI。根若已经是安全区，不要重复扣除。
3. onBeforeLayout 停止并恢复旧反馈；onLayoutChanged 调整卡片内部图标、文本、指示器和按钮，再重新 CaptureBaseline。布局组件只排列外层，动效只写内部视觉层。
4. 全部绑定完成后 Refresh；尺寸或 insets 改变会刷新，相同输入不重复重排。外层稳定后再准备打开动画首帧，最后允许接收输入，避免初始满尺寸闪烁。
5. 选择值由宿主持有，布局不发确认、不改奖励数据。RestoreScroll(normalized) 可恢复保存的纵向位置；范围0–1，非有限值忽略。

组件内置当前配方的三卡排列和固定操作区尺寸，适用于本例而非任意复杂 UI 的自动布局引擎。根过小、字体放大、长译文、更多卡片或折叠屏等情况需要扩展策略并验证最小尺寸。Landscape 判定为根宽大于高；Insets 变化也参与缓存失效。

示例界面使用 PNG 九宫格、原生文字和真实 ScrollRect/Scrollbar。固定命中层与浮动视觉层分开，拖动正文不将整个命中区挤压。按钮与卡片共享档位 Hz/阻尼；卡片强度为档位强度的0.6倍。减弱动效作用于所有页面按钮和卡片。

## 配置与验证范围

schemaVersion=3、previewMode=responsive，recipe.driver=MotionResponsiveLayout；记录当前 referenceSizes、safeInsets、displayMode，以及已经生效的 cardIntensity/buttonIntensity、springFrequency、springDamping、accentDuration 和 reducedMotion。effects 为空，不导入业务选择或确认次数，不重复乘 profile。

桌面 Player 通过模拟根尺寸和预留验证重排、边界、滚动与状态保留；这不是 Android/iOS 真机安全区或触摸手感验证。接入后仍应按项目实际 Canvas、字体、设备旋转、真实触摸及系统切后台做验收。基础生命周期与性能要求仍见 mobile-performance.md、interaction.md。
