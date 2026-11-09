# 验证记录

## 2026-11-09：本机 Unity 补测（源码 `5a1107c`）

在 Windows 上新建独立 clone，并使用已安装的 Unity 6000.0.26f1 补做前一轮缺少的实际运行验证。没有使用或改写现有业务工程。结果摘要另存于 [本轮机器可读记录](validation-local-20261109.json)。

- Editor：`TweenGallery.Editor.RuntimeValidation.Run` 39 项通过；`OrchestrationValidation.Run` 通过播放/对象池版本、声音跨越/循环/暂停/Seek/Skip、减弱动效决策检查。
- Runtime-only 导入：从本次源码生成 147 条目 ZIP，导入新建的空白工程，仅补齐已有 DOTween 和 UGUI 依赖，不带 Gallery 或 MotionArt。46 个预设身份全部核对通过，135 个导入资源与 ZIP 原文逐字节一致。真实 Editor Play Mode 190 项断言通过，覆盖 46 效果的往返采样一致性、有限数值、RectTransform/CanvasGroup/Image/Text 所测字段的停止与停用恢复，以及 D02 真实续播、宿主 Sample 时钟不自行推进、释放所有权后保留必要终态。此项不是独立 Player 或视觉测试。
- Windows x64 Mono：重新构建成功，BuildReport 为 0 错误、0 警告。1600×900 与 1280×720 各运行 46 个效果案例、29 项通用检查、25 项展馆增强检查，全部通过；另运行 24 项交互/编排升级检查，全部通过。五份 Player 报告的 errors 均为空，进程退出码均为 0。
- 图像：两种分辨率各生成 8 张同一运行中 Canvas 的 Camera/RenderTexture 离屏实渲染图，自动非空检查通过；查看并核对本轮 A04/D02（1600×900）、A08/F06（1280×720）。D02 在 50% 时间处显示 49%，符合 8%→90% 的配置。没有把隐藏窗口的系统截屏当作有效视觉证据，也未逐帧验收全部 46 种动画。
- 性能范围：Editor 的 C04/D02 各 1000 次热采样记录当前线程分配 0 字节；Windows Player 的 100 个 D03 对象 × 60 次 Sample，记录当前线程累计 CPU 15.5408 ms、托管分配 0 字节。只是本次受控采样路径结果，不代表整体帧率、GPU、完整 UI 重建或移动性能。
- Python：在 Windows 上重新运行打包 12 项、便携 ZIP 2 项测试及规范文档同步检查，全部通过。

本轮 Runtime ZIP SHA-256：`e38f1569f6d00fa541092ecaca3a541eede81960bc717a9d996f446911102a84`。这是 Windows checkout 的精确测试输入，不要求与其他操作系统的换行转换后打包哈希相同。原始本地记录位于验证 checkout 的 `evidence/runtime-current.log`、`evidence/runtime-validation.txt`、`evidence/orchestration-current.log`、`evidence/build-result.json` 和 `validation-output/current-*/`；相邻隔离工程的 `evidence/` 保存导入、Play Mode 和源文件比对报告。

未测试：Android/iOS（本机仅发现 Windows 构建支持）、实体触摸/真实系统切后台、安全区、移动 Profiler、第二台设备、其他 Unity 版本及可选 TMP define 路径。自动 EventSystem 输入与焦点/后台回调模拟不等同于真人触控或操作系统切换。没有将 Bounce 的业务聚合、列表重排、Sprite 帧、TMP 网格适配配方误写为已实现或已集成测试。

未发现需修改 Runtime 的回归问题。本次仅提交验证记录；构建过程产生的临时场景/设置重序列化已从独立 clone 还原，原有资产、预设、Prefab 与 GUID/fileID 没有提交变更。未创建或替换 Release，未修改 Bounce-Garden。

## 2026-11-09：生产配方与 Skill 打包改进（0.4.0 后续源码）

基于本库 `8ecfafa`，静态对照 Bounce-Garden `78c069dac18a3af087b3d04ac0ff78460d641a8c` 主工程实际 C# 实现，补生产配方及来源矩阵。新文档清楚区分现有 Runtime API 与需目标工程适配的聚合、列表、Sprite 帧和网格能力。独立前向使用审阅覆盖“同步加载后入场 + 帧动画 + 持续加分 + 滚动列表重排 + Skip”，并据此补清自管时钟必须逐帧 Sample、不能仍让 Play 自主推进。

本轮 Python 3 静态/打包验证：`test_package_skill.py` 12 项、`test_portable_zip.py` 2 项均通过；`sync_skill_docs.py --check`、Skill frontmatter 校验与 `git diff --check` 通过。新增测试覆盖两种 profile 的全部规范文档、缺失/过时副本、相对 Markdown 文件链接、被 runtime 排除的目标与路径越界。仅检查内联本地文件链接，不检查外链、引用式/HTML 链接或标题锚点。

完整 Skill 包 316 个条目，Runtime 包 147 个条目；两种 profile 各打两次，同环境固定输入逐字节一致，CRC 和源文件 SHA-256 校验通过。新生产配方与来源文档在两种 ZIP 中均与 docs 源文件逐字节一致。测试 ZIP 只保存在本地验证目录，没有作为新 Release 发布。

`Assets`、`Packages`、`ProjectSettings` 与基线无差异，因此保留全部 46 项运行模板、Prefab、预设与项目配置。本轮环境没有可用 Unity 编辑器或 C# 编译器，未重新执行 Unity 编译、Player、视觉/输入、独立工程导入或移动设备测试。下方历史运行记录不可当作本轮复测；本次结果只证明文档、Skill 结构和打包行为。

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
