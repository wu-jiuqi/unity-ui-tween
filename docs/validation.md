# 验证记录

## 0.6.0-preview2 整屏美术、文字与项目类型复查

2026-11-09，Unity6000.0.26f1 / Windows x64 Mono最终构建成功，0错误、0警告。按两张image_gen整图拆成8份PNG，固定艺术字与原生动态文字分开，实际Regular/Medium/Bold字重替代合成粗体；拒收中央透明的失败底板。主目录57项，原46项模板同步使用清晰字色与字体，Prefab GUID/既有fileID保留。

1600×900与1280×720各通过97项文字/Logo/图帧/装饰检查：T01–T07独立驱动与实际导出、布局占位固定、采样静默、原生暂停续播、完成一次、减弱动效、禁用清理；Logo13个30fps姿态与共同回弹屏障、独立网格绑定、末态收稳；显现数量与半透明基线、滚动提示端点及减弱动效静态可读；离散图帧和恢复、Falling/Burst确定性与清空、艺术标题与动态数值分层、真实字重、运动视口裁切。报告在validation-output/v060p2-{1600,1280}-typography。另运行--capture-motion导出T02/T03/T05的24fps采样帧，人工检查静止与掉落过程；发现过标题遮盖后增加独立裁切区并重跑。

两种分辨率的presentation46项、主展馆183项、原46案例及29项通用检查、展馆增强25项均通过。Editor模板138项、Runtime39项、按钮112项通过；Python打包9项与Skill结构校验通过，check_composition_art.py验证实际像素合约。源项目另扫描291个脚本、人工归类34个UI候选，记录与缺项映射在project-motion-audit.md；这是源代码类型审计，不是源游戏全流程实测。

运行截图与采样视频在docs/previews/v061。桌面离屏Canvas与程序输入不等同于人工手感、手机触摸、移动Profiler或独立干净机器验证。未改变源游戏业务工程，未发布新的GitHub Release。

## 0.6.0-preview1 三档规则与页面配方

2026-11-09，Unity 6000.0.26f1 / Windows x64 Mono 最终构建成功，0错误、0警告。主目录50项，原46项编号与模板GUID/既有fileID保留；三档规则覆盖已有时间轴和实时按钮，新增P01/P02在主展馆内运行。

1600×900和1280×720各通过46项新 presentation 检查：档位从基准推导且不修改源参数、不累乘，整页四阶段暂停采样不触发完成，主界面暂停键从采样时刻继续，视觉完成每轮一次、减弱动效末态、宿主禁用状态、完整基线恢复、原生按钮事件一次触发；响应式验证短屏滚动/长屏适配/横屏三列、安全预留边界、固定操作不重叠、原控件实例不变、选择与滚动在旋转及档位/减弱动效切换后保留、schemaVersion=3驱动及实际素材参数导出。新PNG在Player中真实加载，无SVG占位。报告与运行画面：validation-output/v060p1-{1600,1280}-presentation。

两个分辨率的原46项案例、29项通用检查、183项主展馆流程及25项增强检查通过，无错误。Editor模板138项、Runtime39项、按钮112项通过；Python打包9项和Skill结构校验通过。最终小修后重跑presentation 46项/分辨率；其余回归对应同轮此前构建。可复跑scripts/validate.ps1。

图像是实际Player Canvas离屏渲染，人工复查整页末态、三个尺寸和已有按钮。尺寸及安全预留是桌面模拟，不代表Android/iOS设备、真实触屏/系统旋转或移动Profiler验证。未运行独立干净设备，未发布新GitHub Release。

## 0.5.0-preview3 主展馆统一流程

2026-11-09，Unity 6000.0.26f1 / Windows x64 Mono 构建成功，0 错误、0 警告。正常界面撤掉实验室、工坊两个入口；主目录保留 46 项时间轴并加入 G01/G02，共 48 个选项。公共皮肤、自动播放/真实操作、原地调参、状态结束及导出在同一界面完成。

1600×900 与 1280×720 各 183 项主流程检查通过，包含六种时间轴造型、48 种造型/反馈组合、固定命中与一次原生点击、加载结束、频率 UI/导出一致、状态中途反向、拖动回位与切换取消、连续接续、减弱动效、返回自动模式，以及正常流程不创建旧诊断页。schemaVersion=2 校验实际素材、驱动与参数；G01/G02 不作为 MotionPlayer.Play 编号。

两个分辨率均通过原有 46 项案例、29 项通用检查和 25 项展馆增强检查。内部升级诊断 24 项、按钮夹具 24 项继续通过；这些夹具只在测试参数下创建。Editor 模板检查 138 项、Runtime 39 项、按钮 112 项通过。Python 打包测试 9 项与 Skill 结构校验通过。报告与运行画面在 validation-output/v050p3-final-*；PNG 为实际 Canvas 的离屏渲染。

未进行真实触屏、独立干净设备或人工手感评测。新候选只作本地交付，没有发布新的 GitHub Release。


## 0.5.0-preview2 全量优化验证

2026-11-09，Unity 6000.0.26f1 / Windows x64 Mono 构建成功，0 错误、0 警告。本轮实际更新全部 46 个旧 Prefab、46 个预设、目录默认参数与提示词、完整展馆和交互实验室；另有三张重做的示范美术。不是只增加按钮工坊。

Editor LibraryValidation 共 138 项通过：每项检查真实 Prefab / 预设 / 目录参数一致、暖色深字和实际采样后恢复。原 RuntimeValidation 39 项及按钮 112 项通过。比对 preview1 确认全部旧 Prefab GUID 和既有对象/组件 fileID 保留，新增描边组件拥有新 fileID；默认表现有意调整。

1600×900 和 1280×720 均通过原有 46 项案例与 29 项通用检查、25 项展馆增强检查、24 项按钮工坊检查；另有 24 项升级检查通过，报告 errors 为空。Python 打包测试 9 项和 Skill 结构校验通过。六张家族总览覆盖全部 46 个动态示例，另有四张完整展馆画面；均为实际 Player 中同一 Canvas 的离屏渲染，不是系统截图。报告/画面位于 validation-output/v050p2-final-*，模板回归在 evidence/library-validation.txt。

新版本仍为本地候选，未修改已发布 Release。没有移动端、真实触屏、独立干净设备、人工手感或完整业务集成验证；默认参数改变后应检查宿主保存配置与 Prefab overrides。Runtime 包不包含展馆皮肤或美术。


## 0.5.0-preview1 按钮设计升级

2026-11-09，本机 Unity 6000.0.26f1 / Windows x64 Mono。新增 MotionButtonFeedback、六种原创新皮肤和八种可运行交互；原 A01–F06 共 46 项及其 Prefab 不变。Editor 按钮回归 112 项通过，覆盖固定命中区域、非单位基线、拖动/移出/多指竞争、Submit、禁用、busy、减弱动效和失焦取消；RuntimeValidation 原有 39 项通过。Python 打包测试 9 项通过，Skill 结构校验通过。

Player 的 1600×900 与 1280×720 按钮工坊各 24 项通过。原有 46 项案例 + 29 项通用检查、25 项展馆增强检查及 24 项升级检查在 1280×720 回归通过，errors 为空。PNG 来自同一实际 Canvas 的离屏渲染，非系统屏幕截图。报告在 validation-output/v050-final-*，Editor 记录在 evidence/button-feedback-validation.txt 和 runtime-validation.txt。

视觉复查发现并修正非二次幂图集被导入器缩放造成的裁切错位；Player 额外核对图集实际为 1536×1024。加载只旋转独立图标，文字保持稳定；涟漪/扫光使用图形遮罩。构建结果为 Succeeded，0 错误、0 警告。

本次没有移动端、真实触摸、真人手感评测或第二台干净设备验证；自动输入事件不等同于这些测试。完整包和 Runtime 包独立生成并校验，Runtime 不含新皮肤。候选版本仅本地交付，未修改已发布的 v0.4.0 Release。


## 0.4.0 正式版发布验证

2026-11-09，在仓库提交 ed39140 的代码基础上统一正式版版本号，使用 Unity 6000.0.26f1 重新构建 Windows x64 Mono，结果 Succeeded，0 错误、0 警告。Editor RuntimeValidation 39 项通过；9 项 Python 打包回归通过。

正式版 Player 在 1600×900、1280×720 下均通过 46 项案例、29 项通用检查和 25 项展馆增强检查；升级回归 24 项通过，所有报告 errors 为空。报告位于发布文档包的 release-evidence/，截图为同一实际 Canvas 的离屏渲染。全部 46 个原有 Prefab 的 GUID 与内部 fileID 保持不变。

0.4.0-preview1 的隔离工程验证为下方历史记录，本次正式发布未重新执行隔离工程导入。移动设备、真实触摸、系统后台、安全区、移动 Profiler、第二台干净 Windows 和断网环境仍未验证。下载资产校验与解压复测的最终结果以 GitHub Release 说明为准。


## 0.4.0-preview1 预览版本

2026-11-07，本机 Unity 6000.0.26f1 编译与 Prepare 通过，目录仍为 46 项。D02 原有表达式已经正确地先乘 100 再取整；本次统一填充与文字的钳制值、补上展馆可见的百分比绑定，并验证 8% / 49% / 90%、反向、恢复和范围钳制，避免把原有正确逻辑描述为新修复。

Editor RuntimeValidation 最终 39 项通过。还复现并修复了创建当帧 Play → Sample(0.5) → Seek(0.2) → Resume 的延迟初始化问题：手动推进 0.1 秒后原为 65%，修复后为预期 30%；使用 ForceInit 固定 DOTween 的初始值。复现失败与修复通过日志分别为 `evidence/runtime-lazy-init-before.log`、`runtime-lazy-init-after.log`。

Windows x64 Mono 最终构建记录为 0 错误、0 警告。1600×900 与 1280×720 两种尺寸均运行实际 Player：原有 46 项案例及 29 项通用检查通过；新增展馆回归每种尺寸 25 项通过，覆盖三张 PNG Sprite、时间轴定位与继续、终点后倒拖、对比联动、B01/B02 按压接管后恢复时间轴，以及参数 JSON 中的真实设置与编号。升级回归 24 项通过。证据位于 `validation-output/v040-base-final-*/report.json`、`v040-gallery-final-*/gallery-enhancement-report.json`、`v040-upgrade-final/upgrade-report.json` 及 `evidence/build-v040-final.log`。

新美术已接入动态展馆。A04、A06、A08、B05、D02、E01、F01、F06 均保存了 1600×900 和 1280×720 的非空画面；逐张抽查确认宝箱透明边缘、晶石、场景插画与文字布局正常。图片是对同一个实际 Canvas 的离屏渲染，不是系统屏幕截图。隐藏窗口最初返回黑色交换链截图，已弃用该证据并改为专用 Camera/RenderTexture 捕获。自动化通过不等同于人工触屏或系统切后台测试。

Editor 受控采样中，C04 预热后 1000 次采样与 D02 1000 次不变数值采样记录当前线程托管分配 0 字节。Windows 升级检查另含 100 个 D03 对象 × 60 次 Sample 的受控采样，精确耗时见最终报告。以上仅适用于所测调用路径，不代表全部 UI 帧、GPU/过绘、移动端性能或帧率保证。

9 项 Python 打包测试与文档同步检查通过。Runtime 独立包剔除 Gallery、Editor、场景、Prefab、字体与美术，保留 46 项预设、图集和可选 TMP 适配器。相同输入与相同 Python/zlib 环境下，两次打包逐字节比较；这不意味着 Unity Player 构建本身可复现。完整包保留原 46 个 Prefab 及新增 PNG，原有 Prefab、预设、GUID 和内部 fileID 未改动。

隔离工程 `RuntimeOnlyCheckV040` 只导入 Runtime 包并加入已有 DOTween DLL 与 UGUI 2.0.0，实际 Unity Play Mode 验证全部 46 个编号、46 项预设和 D02 正反向 Sample / Seek / Resume / Reset / 禁用重启。精确次数、ZIP 校验值和日志保存在该工程 `evidence/`。这不是独立 Player 或移动端性能测试。

本机 `adb devices -l` 没有列出设备；Unity 只安装 Windows 构建支持。因此移动端构建、真实触摸、系统切后台、安全区/横竖屏和移动 Profiler 均未执行。

最终 Runtime ZIP 为 132,581 字节、145 个条目，A/B 两次构建逐字节一致，SHA-256 `f8679595b4d4f73abadc4bb3439f112b2a18589c23383dbd75e44771a1442d47`。该精确 ZIP 在隔离工程中通过 88 项断言（含创建当帧混合 Sample / Seek / Resume 回归），退出码 0；导入后的 135 个资源文件与包内容核对一致。

## 0.3.0 本地升级候选

本机 Windows 上使用 Unity 6000.0.26f1、Windows x64 Mono 构建成功；最终构建报告为 0 错误、0 警告。`validation-output/v030-upgrade-final/upgrade-report.json` 的 Player 回归结果为通过、无错误。实验室在 Player 中加载了 image gen 生成的原创背景，画面记录为 `validation-output/v030-upgrade-final/interaction-lab.png`。

该次运行覆盖：姿态中断与反向接续、稳定基线恢复、播放版本失效、物理弹簧的长帧和速度连续、果冻拖拽位移约束、对象池租约及过期回调、声音事件时间轴、减弱动效和必要终态。输入用例通过 EventSystem **程序模拟**了双指竞争、拖拽阈值、ScrollRect 路由、区域外释放、失焦和后台取消；这不等同于触摸设备或真实系统切后台测试。音效时间轴需要宿主维护播放时钟并调用 `Advance`，不是 `MotionPlayer` 的自动音频绑定。

桌面 Player 曾对 100 个奖励对象各执行 60 次 `Sample`：计时代码记录当前线程 CPU 合计 **6.5705 ms**、当前线程托管分配 **0 字节**。这是单次受控采样，仅覆盖该测试线程和采样路径，既不是连续帧真实并发或对象池完整开销，也不能作为移动端性能或优化幅度结论。

可选 TMP 适配在本机独立工程中启用并测试，Player 结果 `failures=0`；该测试工程另装入 TMP 所需资源。另在最终模板导入工程开启 `TWEEN_GALLERY_TMP` 后编译并验证即时数值通过。主展馆工程未启用该 define，基础运行时保留 UGUI Text 路径，并不要求项目安装 TMP。独立 Skill 包无需构建 Windows 即可生成；打包检查覆盖 46 项、Prefab/预设、GUID、依赖和 ZIP 内容。跨平台检查包括 Windows 生成包在 Ubuntu WSL 解压，以及 Ubuntu 创建包含中文路径的 ZIP 由 Windows 读取；Skill 测试 3 项和便携 ZIP 测试 1 项通过。最终源码稳定后应重新执行打包检查。

最终源码已重新生成并验证 Skill 包；Windows 展馆、源码及 Skill 三个 ZIP 在本机 Ubuntu WSL 中均通过 CRC 检查且条目路径不含反斜杠。Windows 包解压后启动的升级 Player 自测通过。

0.3.0 最终 Player 原有 46 项回归通过，报告 `validation-output/v030-base-final/report.json` 记录 46/46、无错误；构建日志 `evidence/build-v030-final2.log` 对应的构建报告为 0 错误、0 警告。

最终模板包已在本机独立 Unity 6000.0.26f1 工程中导入，46/46 Prefab、预设、依赖和采样检查通过；比较 v0.2.1 包确认原有 46 个 Prefab 的 GUID 与内部 fileID 一致。被测模板包 SHA-256 为 `f5fce9b83b810533ebac36e3ce60a883e13d67619a0c1809bcf66d732cc23315`，独立工程报告保存在 `IntegrationCheckV030/evidence/`。

竖屏及不同安全区、真实触摸与 ScrollRect、按住时真实切后台、移动设备低帧与单次长帧、移动端 Profiler/真实并发奖励及对象池性能均尚未实测；也未在第二台干净 Windows 设备或断网环境运行。

## 0.2.1 进阶版候选

Unity 6000.0.26f1，Windows x64 Mono；本机 1600×900、1280×720 的可见 Player 自动测试均为 46/46 通过，覆盖原 40 项和 F01–F06 的播放、重播、状态恢复与有限数值。F05 经 EventSystem 的实际射线命中后派发指针进入与退出，验证卡片倾斜、独立高光位移和归中。对每个新效果记录约 15%、50%、85% 三个阶段的画面，复查了星图、门户和标题等视觉区分。此次测试使用程序派发指针事件，尚不是人工鼠标手感评测。

独立 Unity 6000.0.26f1 工程已导入进阶模板包，46 个 Prefab 的绑定、图形、字体、Shader、预设及动画采样/复位检查通过，F02/F06 文字显隐与复位也通过。0.2.1 模板包 SHA-256：`88572ab006c70d9c9c4bf3213e8b5f7b07caa088bb72265a26361d80eaeac948`，该版本恢复并保留原 40 个 Prefab 的内部对象标识。独立工程仍在同一台电脑上，发布包下载验证以最终交付说明为准；尚未在第二台干净 Windows 设备或断网环境中验证。调研页面中的动态在当前文本抓取环境未逐帧观看，参考细节来自原作者说明与用户提供描述。

版本 0.1.0，Unity 6000.0.26f1，Windows x64 Mono，UGUI 2.0.0，DOTween 免费版 1.3.045，Built-in 渲染管线。Windows 构建成功，0 错误、0 警告。

本机 Windows 11 在 1600×900 和 1280×720 窗口下运行，40 项自动冒烟测试均通过：每项采样、正常播放、重复播放、停止和停用恢复及基本有限数值检查。测试还覆盖暂停、恢复、慢放、游戏暂停期间播放、分类与搜索、播放按钮、复制提示词和三组对比。

独立用例覆盖动画期间业务文字及可交互状态更改、重新采集布局、半透明基线、B02 按下松开、D03 到达目标中心、C04 非居中 pivot 与非单位缩放对齐、E02 旋转循环接缝以及同目标播放器和按压的接管。A04、B02、D03、E03 也在展馆控制之外创建、播放和销毁。实际截图确认 D07 翻面正面和 D08 箱盖开启可见。

在一个独立 Unity 6000.0.26f1 工程中导入最终模板包并安装同一 DOTween 免费版，编译成功；40 个 Prefab 的必要绑定、Sprite、纹理、字体、Shader 及动画采样检查通过。该模板包 SHA-256 为 `7f8163df3039125c1d7f153592bce4bacb271b8228215ad9a84379264ff7f5a6`。独立工程验证仍在本机完成。

上述为 0.1.0 构建时的历史记录；自动冒烟不能证明各效果适配所有项目、分辨率和操作节奏。目标项目应根据 Canvas、Layout 和实际输入继续测试。
