# 来源对照与提取边界

此次对照使用 `wu-jiuqi/Bounce-Garden` 的提交 `78c069dac18a3af087b3d04ac0ff78460d641a8c`，主工程为 `BounceGarden/BounceGarden`，以及本库 `8ecfafa` 的现有 Skill/Runtime。下方链接固定到该提交；源仓库是私有仓库，需要其访问权限。安装本 Skill 不需要访问源仓库，复用契约完整收录在 [生产配方](production-recipes.md) 中。

方法是读取真实 C# 实现与相关 QA 源码，区分已提取能力、缺失说明和应改进而非照搬的做法。此次没有修改 Bounce-Garden，没有复制其游戏素材、文案、存档或业务代码，也没有据此新增动效编号。没有在本轮执行 Bounce 或 Unity Player；历史 QA 的存在不代表新环境已验证。

## 已有能力：保留，不重复造轮子

| 源实现 | 本库已有能力 | 复用时注意 |
| --- | --- | --- |
| [UIBounce](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/UIBounce.cs)、[EncouragementDrag](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/EncouragementDrag.cs) | 固定命中区、MotionButtonInput / MotionDragInput / MotionJellyDrag | 不是所有 Button 都应该消费拖动；保留 ScrollRect；已有解析弹簧不照搬 dt 截断 |
| [ActivityJellyMotion](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/ActivityJellyMotion.cs) | MotionSpring、播放条件门控、视觉包装层 | 此源实现还含逐帧程序化运动，不能只搜 DOTween |
| [ResultAudioTimeline](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/ResultAudioTimeline.cs) | MotionSoundTimeline | 声音跟所选表现时钟；不能清理共享音频管理器的全部声音 |
| [InGameFeedbackView](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/InGameFeedbackView.cs) | 播放版本、MotionPlaybackLease、D09 / D10 | 页面/会话版本和池借用版本分开；视觉事件不重算真实结算 |

## 本轮补齐的生产契约

| 实际来源与符号 | 抽取内容 | 本轮处理 |
| --- | --- | --- |
| [VictoryPresentation.Render / Finish](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/VictoryPresentation.cs)、[PaintingCollectionPresentation.Skip / OnDisable](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/PaintingCollectionPresentation.cs) | 阶段表、输入就绪、自然完成 / 跳过 / 取消 | 配方 2：把表现完成与业务提交拆开；原例中的对象创建不能无条件放进可重入采样 |
| [LoadingPresentation.Update / Render](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/LoadingPresentation.cs)、[FailurePresentation.Update](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/FailurePresentation.cs) | 首次可见后起计、准备长帧与可读性 | 配方 3：表现限幅可选且延长墙钟时间，不用于权威倒计时/弹簧；明确音画同钟 |
| [ScoreGainPresentation.Changed / Update](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/ScoreGainPresentation.cs) | 短静默窗聚合加分飘字 | 配方 4：在原静默窗思路上补最大等待，避免持续事件一直不显示；本库没有新增聚合运行组件 |
| [LeagueRankingView.Refresh / Create](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/LeagueRankingView.cs) | 稳定 ID 行复用、当前姿态重排、独立果冻内层 | 配方 5：补布局槽位、滚动锚点、重绑和对象池身份约束 |
| [VictoryCatAnimation.FrameAt](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/VictoryCatAnimation.cs)、[FailureCatAnimation.FrameAt](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/FailureCatAnimation.cs) | 非等时长 Sprite 帧 + 外层 UI 编排 | 配方 6：统一时间定义、注册点与素材缓存；不是 Runtime 已有帧播放器 |
| [JellyLogoIntro / JellyGlyph.Impact / ModifyMesh](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/JellyLogoIntro.cs)、LoadingPresentation.Render | 字块落地、手工姿态、UGUI/TMP 顶点形变 | 配方 7：区分揭示与变形，避免顶点累加漂移和每帧无条件重建；单帧协程等待不等于精确可采样时间轴 |
| [HomeWordmarkWiggle.Update](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/HomeWordmarkWiggle.cs)、[HomeFeedbackView.Present](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/HomeFeedbackView.cs) | 带静默段的错峰微动、只在有新内容时入场 | 配方 8：明确空闲门控与恢复策略，避免重新可见后堆积补播 |
| [TutorialMotionBoard.Configure / Sample](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/TutorialMotionBoard.cs) | 独立教学对象、可采样演示 | 配方 8：不操作真实会话；进一步要求完整视觉状态可重建，不能把存在 Sample 方法当作所有模式都无历史依赖的证据 |
| [PlayableTutorialGuide.Local](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/PlayableTutorialGuide.cs)、[ChainCollectionHUD.Target](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/ChainCollectionHUD.cs) | 世界与 UI 相互映射、引导让位、奖励落点 | 配方 9：显式选择相机/平面/anchor，区分实测源实现与通用透视场景适配；没有把跨 Canvas 能力添加进 D03/C04 |

## 优化取舍

本轮更新 Skill 路由、生产配方与验收，并让完整包和 Runtime 包都校验参考文档链接是否随包存在。保持全部 46 项模板、Prefab、预设和运行 API 不变。需要新增通用帧播放器、TMP 顶点组件或聚合器时，再根据目标工程实际需求实现和运行验证；不要因本页列出方法就声称它们已经可直接调用。

源项目有版本化 QA 脚本，如 LoadingQA、VictoryQA、BannerJellyQA；它们提示应测首帧长帧、按钮门槛、命中区域与弹性复位。本轮仅参考其测试意图，不沿用旧通过数字，也不把静态查看变成新的运行证据。
