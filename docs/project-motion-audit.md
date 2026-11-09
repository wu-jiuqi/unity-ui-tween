# BounceGarden 动效类型复查

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
| LoadingPresentation、AlphabetIntroductionView.Boosters：按时钟切换角色/物件sprite | MotionSpriteSequence | **新增离散图帧时钟**，不误称缓动插帧 |
| AlphabetIntroductionView、TutorialMotionBoard：填充箭头、手势、移动/缩放/淡化、错峰教学 | D02、E05/E07、P01 + 编排文档 | 已有；Image.fillAmount需宿主指定Filled/方向，不能只改值不设显示模式 |
| PlayableTutorialGuide：指针指示、脉冲、引导轨迹 | E01/E03/E07 | 已有，输入/完成事件由宿主教学状态控制 |
| AppUI.Shop/StreakReward、ChainCollectionSession：奖励曲线飞行、吸收压扁、计数反馈 | D03/D04/D10 + P01 | 已有，世界坐标→Canvas映射要单独适配 |
| HomeFeedbackView、ChainCollectionHUD：主图/细节延迟跟随、入场后轻呼吸 | E01/E05、F04、P01 | 已有层级组合，不为每个装饰另造编号 |
| ChapterOpening70：双门展开、窗口扩张、新背景上移、文案阶段切换 | C01/C03/C06、F06 + 页面编排 | 补为组合配方说明；门图/遮罩/窗口分层，业务章节开通仍由宿主提交 |
| JourneyArt：旧卡沿弧离开、新卡淡入/缩放接棒 | C05/C07、F05 | 已有组合；中断时用G02/ContinueTo保留可见姿态 |
| PaintingCollectionPresentation：原画→像素画交叉淡化、框/标题延迟、星点运动 | C07、D04、E05、P01 | 已有组合；画册保存与Skip必须走幂等业务入口 |
| LeagueMatchPresentation：环状静态排布、错峰出现、进度/计数、轻浮动 | D01/D02、E01/E05、F04 | 已有组合；头像位于圆周不等于新增轨道旋转补间 |

## 组合接线补充

章节开门：建立leftDoor、rightDoor、windowMask、newBackground、caption和wholeGroup独立层；先进入，再门扇相反水平位移/横向缩至零与淡出，随后窗口扩大、新背景上移并交接文案，最后退场。宿主将阶段映射到C03/C07/F06或自己的确定性采样器；当前库没有声称提供专门ChapterOpening组件。

画作收藏：保留roughArt与pixelArt两层，C07交叉淡化，再由E05/P01思路错峰进入frame、caption与spark。吸收压扁用独立接收端视觉层，数据先由宿主结算，飞入和采样不再发奖励。

持续加载/首页Idle：保留实际任务/页面可见性条件，主程序只调度单轮T04/T06或已有E循环；切页/后台按项目要求取消或暂停，低频颤动留静置间隔。只播放loading动画不能代表任务完成。图帧替换与位置/透明度曲线可并行，但每个属性明确单一所有者。

## 不当作新增UI类型的内容

Core/World中的球体抛射、碰撞、链条重力、雪块消层、Boss移动、粒子/碎屑、拖尾和物件SpriteRenderer反馈均有候选数学公式。保留其玩法时钟与物理；只将可复用的可视模式映射为D03飞行、D09上浮、BX/G01受压回弹、MotionSpriteSequence图帧等。需要接世界物体时由宿主提供SpriteRenderer/Transform/坐标转换适配，不能直接把UGUI RectTransform组件挂到刚体或宣称已完成玩法移植。

本次是源码类型与可视模式复查，不是源游戏的全部运行流程实测。34个UI候选与本库新增驱动的Player回归是不同证据；未重构BounceGarden业务工程，未复制它的资源授权、存档、奖励、音效服务或章节导航到公共库。
