# BounceGarden 动效类型复查

## 版本、范围与覆盖等级

本次复查基线为 unity-ui-tween `0.6.0-preview2`（`20931a5a8d0384efafe87bf36aade68a2d1e8301`），源仓库为 [wu-jiuqi/Bounce-Garden](https://github.com/wu-jiuqi/Bounce-Garden)，固定源版本为 [`78c069dac18a3af087b3d04ac0ff78460d641a8c`](https://github.com/wu-jiuqi/Bounce-Garden/tree/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts)。下文291/34是已有库存记录，不是此次补充复查重新扫描所得；声音等人工补充证据不混入该候选数。

覆盖分四级，不能把“能组合”写成“已有专用组件”：

- **直接驱动**：已有Runtime提供该视觉类型，如T03、T07、等时sprite、Falling/Burst装饰；不代表完整移植源页面及其美术、业务和节奏。
- **组合示例**：已有基础能力可以组合，但宿主仍需编排阶段、建立层级和转换参数，如画作收藏、章节开门、排行重排。
- **宿主适配**：真实任务状态、数据聚合、TMP顶点映射、非等时姿态、跨Canvas坐标、声音服务与Skip业务接口等，不应假定编号自动完成。
- **明确排除**：玩法物理、存档/奖励事务、素材授权及源游戏完整流程回归。排除不等于漏做UI驱动。

本次修改为待验收补充；源码对照和文档检查不构成Unity编译、目标游戏或设备运行通过。

2026-11-09，扫描主工程 BounceGarden/BounceGarden/Assets/Scripts 共291个C#文件，找到34个UI动效候选文件，再按调用和自定义采样公式人工归类。扫描脚本 scripts/audit_project_motion.py，文件哈希、行号与API证据在 ArtSource/Composition-v061/project-motion-inventory.json。正则候选数量不是动画类型数量；QA、研究代码、静态几何计算和玩法物理不据此算成UI补间。

| 源项目用法与证据文件 | Skill对应内容 | 本轮处理 |
|---|---|---|
| UICraft、AppUI：缩放弹入、淡入淡出、返回/抽屉 | A01–A07、C01–C07 | 已有，补全组合接线口径 |
| UIBounce：固定命中、hover/press、移出/拖动、弹性松开 | B01/B02、BX01–BX03、G01 | 已有独立视觉与输入组件；不复制业务按钮事件 |
| ActivityJellyMotion、JellyChapterOrb：悬浮、拉伸、受压回弹 | E01、BX02、G01 | 已有曲线/弹簧，按方向和低频空闲组合 |
| ActivityCarousel、Round6Nav、LeagueRankingView：页面/选中移动、滚动吸附 | B07/B08、C02/C05、G02 | 已有；宿主ScrollRect选择/吸附与动画分离 |
| JellyLogoIntro：逐字掉落、13帧落地形变、统一持续果冻和网格变形 | T03 MotionLogoDrop + MotionJellyGraphic | **新增可运行驱动**，保留全部落稳后再共同回弹的屏障 |
| JellyLogoIntro：延迟2.05秒后，按每字0.035秒显现开场文案 | T05 Reveal | **新增固定排版显现**；宿主管理延迟/声音/跳转 |
| LoadingPresentation：逐字hop、字顶点波浪 | T02/T06 | **新增文字错峰与波浪**；UGUI独立字素演示，TMP保持原网格排版后适配 |
| HomeWordmarkWiggle：5.5秒周期内短促逐字颤动 | T04 Shiver | **新增单轮驱动**；可见性、静置间隔由宿主调度 |
| VictoryPresentation、FailurePresentation、ChainRatingView：成就/胜败图形字缩放与颤动 | T01/T02/T04 + P01 | **新增文字类型并接入原有整页配方**；结果提交不绑视觉完成 |
| ScoreGainPresentation、CollectionSession：合并增量、上浮、淡出、轻弹 | D01/D05/D09 + T01 | 已有基础，记录0.07秒聚合等待属于宿主数据策略 |
| InGameFeedbackView：6秒横向公告穿过裁切区 | T07 MotionMarquee | **新增独立驱动**；单轮路径、队列和静态可读替代分离 |
| AppUI.Fireworks/Confetti：放射星屑、彩纸重力/漂移/旋转 | MotionConfettiSequence | **新增Falling/Burst装饰采样**；预分配对象，末帧清空，宿主负责池/生命周期 |
| LoadingPresentation、AlphabetIntroductionView.Boosters：按时钟切换角色/物件sprite | MotionSpriteSequence | **等时帧直接驱动；非等时姿态需适配**。Booster的12fps可直接映射；Loading不能只用等时fps替换，下文给出原节拍 |
| AlphabetIntroductionView、TutorialMotionBoard：填充箭头、手势、移动/缩放/淡化、错峰教学 | D02、E05/E07、P01 + 编排文档 | 已有；Image.fillAmount需宿主指定Filled/方向，不能只改值不设显示模式 |
| PlayableTutorialGuide：指针指示、脉冲、引导轨迹 | E01/E03/E07 | 已有，输入/完成事件由宿主教学状态控制 |
| AppUI.Shop/StreakReward、ChainCollectionSession：奖励曲线飞行、吸收压扁、计数反馈 | D03/D04/D10 + P01 | 已有，世界坐标→Canvas映射要单独适配 |
| HomeFeedbackView、ChainCollectionHUD：主图/细节延迟跟随、入场后轻呼吸 | E01/E05、F04、P01 | 已有层级组合，不为每个装饰另造编号 |
| ChapterOpening70：双门展开、窗口扩张、新背景上移、文案阶段切换 | C01/C03/C06、F06 + 页面编排 | 补为组合配方说明；门图/遮罩/窗口分层，业务章节开通仍由宿主提交 |
| JourneyArt：旧卡沿弧离开、新卡淡入/缩放接棒 | C05/C07、F05 | 已有组合；中断时用G02/ContinueTo保留可见姿态 |
| PaintingCollectionPresentation：原画→像素画交叉淡化、框/标题延迟、星点运动 | C07、D04、E05、P01 | 已有组合；画册保存与Skip必须走幂等业务入口 |
| LeagueMatchPresentation：环状静态排布、错峰出现、进度/计数、轻浮动 | D01/D02、E01/E05、F04 | 已有组合；头像位于圆周不等于新增轨道旋转补间 |
| ResultAudioTimeline：一次性时间节点、胜败分支、Skip/禁用取消 | MotionSoundTimeline + 宿主声音服务 | **声音节点直接驱动，服务与业务宿主适配**；人工补充证据，不在既有34个UI候选内 |

## Loading：等时帧与非等时姿态不能混用

证据：[LoadingPresentation.cs](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/LoadingPresentation.cs)、[AlphabetIntroductionView.Boosters.cs](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/AlphabetIntroductionView.Boosters.cs)。Booster使用四帧12fps循环，并在独立位置属性上横向摆动；MotionSpriteSequence的fps模式适合这一类型。

Loading的猫使用6.7秒源编排，再以`animationTime = Elapsed * (6.7 / 3.6)`缩成3.6秒。以下为源编排秒数，实际播放秒数乘`3.6 / 6.7`：

| 区间 | sprite姿态 | 同步视觉 |
|---|---|---|
| 0–1.8 | idle | 轻微呼吸 |
| 1.8–2.35 | anticipate | 蓄力压缩 |
| 2.35–3.05 | celebrate | 正弦起跳 |
| 3.05–4.2 | settle | 落稳 |
| 4.2–5.65 | wave | 小幅摇摆 |
| 5.65–6.7 | idle | 返回静置 |

复用应保留六个时间段，首尾可引用同一idle sprite；各段停留为1.8、0.55、0.70、1.15、1.45、1.05秒，再统一缩放。0.6.0-preview2只有等时fps；本次待验收扩展为可选`float[] frameDurations`：null/空数组保留fps路径，非空则数量必须与frames一致且每项为有限正数。时长表在CaptureBaseline缓存，修改后应先Restore再CaptureBaseline，不在半程重采；测试结果以本次验收记录为准。不要仅给五个sprite设同一fps后宣称还原原节奏。

sprite、位置、缩放、旋转由同一宿主编排时钟定位，但分别有明确属性所有者；不能让多个自动Update和宿主Step重复推进。源实现跳过首次Update并将单帧增量限制到0.1秒，避免准备卡顿吞掉姿态；这会延长实际墙钟时长，是源Loading的展示策略，不宜偷偷推广成所有动画的全局规则。资源准备完成、最短展示时间与离页条件仍由宿主决定。

## 画作收藏：有基础能力，不等于已有整页配方

证据：[PaintingCollectionPresentation.cs](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/PaintingCollectionPresentation.cs)。该页使用4.6秒非缩放时钟；P01是旅程达成的固定配方，不能直接把它当成收藏页时间轴。

| 源时间（秒） | 层与阶段 | 可复用方式 |
|---|---|---|
| 0–0.45 | 遮罩淡入、画作由0.88缩放进入 | A类视觉思路，独立遮罩与art层 |
| 1.35–2.70 | roughArt淡出、pixelArt淡入 | C07交叉淡化，保留两张独立Image |
| 1.65起，逐项延迟0–0.4 | 星点移动/旋转/淡入淡出，单项2.1秒 | 装饰采样或独立spark层；不等同奖励汇聚 |
| 2.05–2.85 | frame显现 | 独立透明度轨道 |
| 2.65–3.10 | caption显现 | 独立字幕组 |
| 4.10–4.60 | wholeGroup退出 | 页面组淡出 |

宿主以同一个elapsed求各阶段进度，或自建确定性采样器；不要并行让不同播放器写同一CanvasGroup。源Skip与自然完成共享一次性回调保护，OnDisable仅清理回调。复用时分别定义“跳过后完成业务”“关闭取消”“静默采样”，用播放版本/租约和幂等业务入口防止旧回调提交；不要把Seek(1)当成领取或保存操作。

## 声音编排的人工补充证据

证据：[ResultAudioTimeline.cs](https://github.com/wu-jiuqi/Bounce-Garden/blob/78c069dac18a3af087b3d04ac0ff78460d641a8c/BounceGarden/BounceGarden/Assets/Scripts/UI/ResultAudioTimeline.cs)。源脚本不含现有扫描正则里的Tween/三角函数等关键字，因此“34个候选”不能作为声音编排覆盖证明。

- 初始化时取消旧瞬态音、选择结果背景音乐并播放胜败stinger。
- 胜利节点为0.38、0.55、0.76、1.00、1.45秒；失败节点为0.55、0.72秒；2.1秒触发对应语音。bitmask保证每个节点只触发一次。
- 源Skip和OnDisable取消后续事件及瞬态音。公共组件只应停止自己明确拥有的AudioSource，不能照搬源项目对全局SoundService的取消操作。
- MotionSoundTimeline已提供节点跨越与取消能力，具体接线见[编排与音频](orchestration.md)。用未回绕的真实播放秒数驱动，Seek/反向/Skip静默；宿主协调视觉与声音的暂停、后台、速度和结束，不把视觉Sample当成音频触发入口。胜败选择、音乐/语音服务及优先级仍由宿主负责。

以后扩展库存时，应为声音、sprite切换和生命周期保留单独证据分类或人工纳入理由，记录固定commit、文件路径与内容哈希；重新运行扫描后才更新候选数量。

## 组合接线补充

章节开门：建立leftDoor、rightDoor、windowMask、newBackground、caption和wholeGroup独立层；先进入，再门扇相反水平位移/横向缩至零与淡出，随后窗口扩大、新背景上移并交接文案，最后退场。宿主将阶段映射到C03/C07/F06或自己的确定性采样器；当前库没有声称提供专门ChapterOpening组件。

画作收藏：保留roughArt与pixelArt两层，C07交叉淡化，再由E05/P01思路错峰进入frame、caption与spark。吸收压扁用独立接收端视觉层，数据先由宿主结算，飞入和采样不再发奖励。

持续加载/首页Idle：保留实际任务/页面可见性条件，主程序只调度单轮T04/T06或已有E循环；切页/后台按项目要求取消或暂停，低频颤动留静置间隔。只播放loading动画不能代表任务完成。图帧替换与位置/透明度曲线可并行，但每个属性明确单一所有者。

## 不当作新增UI类型的内容

Core/World中的球体抛射、碰撞、链条重力、雪块消层、Boss移动、粒子/碎屑、拖尾和物件SpriteRenderer反馈均有候选数学公式。保留其玩法时钟与物理；只将可复用的可视模式映射为D03飞行、D09上浮、BX/G01受压回弹、MotionSpriteSequence图帧等。需要接世界物体时由宿主提供SpriteRenderer/Transform/坐标转换适配，不能直接把UGUI RectTransform组件挂到刚体或宣称已完成玩法移植。

本次是源码类型与可视模式复查，不是源游戏的全部运行流程实测。34个UI候选与本库新增驱动的Player回归是不同证据；未重构BounceGarden业务工程，未复制它的资源授权、存档、奖励、音效服务或章节导航到公共库。
