# 文字、Logo 与离散帧配方

T01–T07 在主展馆“编排”分类中选取，使用独立驱动，不能传给 MotionPlayer.Play。已有F03是字块/底线分层入场，并不等同于逐字弹跳；A05是单目标落位弹跳，并不包含Logo逐字同步与网格形变。

| 编号 | 驱动与模式 | 适合用途 |
|---|---|---|
| T01 短文字整体弹跳 | MotionTextSequence / Bounce，单视觉层 | 奖励数值、短标题、确认反馈 |
| T02 逐字错峰弹跳 | MotionTextSequence / Bounce，多视觉层 | 独立字素按阅读顺序跳起 |
| T03 Logo掉落与果冻回弹 | MotionLogoDrop + MotionJellyGraphic | 分字下落、落地形变、共同衰减 |
| T04 逐字颤动强调 | MotionTextSequence / Shiver | 首页字标低频短促强调 |
| T05 打字机显现 | MotionTextSequence / Reveal | 固定排版后按字素显现 |
| T06 加载文字波浪 | MotionTextSequence / Wave | 等待时温和逐字轻跳 |
| T07 横向滚动提示 | MotionMarquee + RectMask2D | 提示文案完整进入并离开裁切区 |

另有 MotionSpriteSequence，将项目已有 Image 图帧按帧率离散采样；它是美术帧播放，不是缓动曲线，不插值生成中间帧。MotionConfettiSequence提供Falling彩纸与Burst星屑的确定性装饰运动，绑定预分配的items/groups并配置seed、duration、travel、spread、strength；独立种子不改全局随机数，末帧透明，Restore回基线。它不负责生成对象、对象池、粒子系统或真实物理。与骨骼和玩法模拟区分，不将它们全部称为补间。

## 固定布局与文字载体

外层 slot 负责排版、换行和命中，内层 visual 负责位置、缩放与旋转。整段普通文字可直接绑定一个 Text/TMP 的独立 RectTransform；分字动画需要宿主预先排好独立字素 visual，或使用已有TMP逐字顶点方案。Runtime 不拆改业务字符串，不能在每帧 substring 后重新布局。

UGUI演示使用StringInfo划分中文、组合字符等文本元素，不将UTF-16单个char当作所有语言的字。复杂连字、ZWJ emoji、从右向左语言和跨字距排版需要宿主保留其真实排版/顶点映射后适配；本例不承诺任意语言自动无损拆字。TMP宿主可在原生maxVisibleCharacters上接入显现时钟，但当前UGUI分组组件不直接替换TMP的网格/字距处理。

图形字需每字独立生成/导出；整块生成标题适合T01，不能任意切成T02/T03字块。按 art-composition-workflow.md 决定哪些需要image gen文字、哪些保留原生文字。字素可为Text、TMP独立包装层或Image；所有装饰关闭raycastTarget。

## 时间与生命周期

MotionVisualSequence 是文字、Logo、滚动提示和图帧的公共时钟。接好全部绑定且布局稳定后 CaptureBaseline，再 Seek(0) 准备首帧；Play/Pause/Resume/Seek/Sample/Restore 与 Step 可用。默认unscaledTime，speed只影响本实例；正常Update已自动推进，宿主不要再重复Step。

Seek/Sample暂停并静默采样，忽略非有限值；Seek到末帧不会补发完成。Play从头开始，每轮只发一次onVisualComplete；减弱动效直接达到必要末态。Restore和禁用恢复捕获的视觉属性，保留宿主文字和业务数据。失焦/后台暂停此时钟。PlayVersion可用于宿主拒绝过期回调；组件不触发奖励、导航、任务完成或音效。

文字TotalDuration为单字duration加`stagger*(字数-1)`；Reveal的duration为整段显现总时长。克制/标准/庆祝使用已换算参数；档位变化后不重复乘profile。T04/T06演示单轮，正式等待动画应由宿主按可见性和任务状态调度，轮次之间保留安静间隔；不要让低频首页字标持续抽动。长段说明默认静止。

## T03 保留项目开场节奏

源自 BounceGarden 的 JellyLogoIntro.cs / JellyGlyph。标准值：首延迟0.12秒，逐字间隔0.105秒，InCubic下落0.50秒；随后13个30fps手调落地姿态，压扁峰值约1.39×0.61、拉伸峰值约0.82×1.27。**所有字的落地形变结束后**，才统一进入持续果冻：横向/纵向相反变形、微跳、轻旋和Graphic网格剪切，共享衰减时钟。6.8秒内收稳，末0.65秒渐弱。

与源项目相比，默认下落高度改为宿主UI单位参数，页面跳转、全局Hold流程和SoundService未复制。原音调0.94+index%4×0.035及开场文案延迟2.05秒属于宿主编排，可用独立音频时间轴/显现组件接线；时间轴采样不能触发声音。新演示使用“动效图鉴”四个原创独立字，不复制游戏Logo美术或名称。timingScale统一调整整轮时长；字数增多时会自动确保最后落地之后有衰减收尾时间。

MotionJellyGraphic只作用于绑定的UI Graphic网格，是轻微剪切/弯曲近似，不是软体物理。CanvasGroup与glyph数量必须匹配，meshes可选。独立分字保持原始scale/rotation/pivot，不把布局容器或点击区一起变形。重排后先Restore再CaptureBaseline。

## 滚动、图帧与导出

MotionMarquee.visual须是viewport直接子层，居中锚点/支点，父级RectMask2D负责裁切；一次播放从右侧完全外部移到左侧完全外部，duration默认6秒，padding12。长帧仍由归一化时钟定位，提示队列、合并重复内容和循环间隔由宿主管理。减弱动效Play直接保留原位静态文字，不把必要信息移出视口；还须检查长文案是否需要静态换行。

MotionSpriteSequence绑定Image、非空frames及framesPerSecond，首尾帧稳定，Restore恢复原sprite；不抢占颜色、命中或游戏状态。SpriteRenderer需宿主适配显示端，当前组件提供UGUI Image路径。

T配方导出schemaVersion=3、previewMode=typography、recipe.driver/textMode/textContent/totalDuration与实际settings、素材路径，effects为空。所有数值已经生效；T03图帧/手调曲线由驱动固定定义，导出包含其可调时长、强度、高度和间隔。只读预览的业务语义文本不得因图形字而丢失。
