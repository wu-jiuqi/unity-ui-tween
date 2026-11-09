# 任务2：0.6 基线本地验收

日期：2026-11-09。验收完成后用户已授权推送并发布，修复纳入 0.6.0-preview3。本文保留 preview2 本地验收的范围与结果；发布包仅更新版本/发布元数据，按发布提交重新构建和验证。

## 正确基线
- unity-ui-tween 的 codex/button-style-skill-v050 与 v0.6.0-preview2 标签均指向 20931a5a8d0384efafe87bf36aade68a2d1e8301，两者无差异。
- 对照 Bounce-Garden 固定版本 78c069dac18a3af087b3d04ac0ff78460d641a8c。
- 本包不是之前的 0.4.0 包，也不是原 v0.6.0-preview2 Release 的原样重发。保留该基线已有的57个展馆选项和美术，只叠加以下修复。桌面验收包未升级版本；正式的本轮预发布包使用 0.6.0-preview3，避免覆盖已有标签与资产。

## 实际源码变化
1. MotionSpriteSequence 增加可选 frameDurations。空数组沿用固定fps；非空数组按每帧停留时长采样，覆盖 LoadingPresentation 的非等时姿态。校验长度、有限正数及总时长，捕获时建立快照，播放期间不重复分配。更改素材或节奏后先 Restore，再 CaptureBaseline。
2. MotionPageSequence 压缩大数组金币的错峰间隔，6枚以上也在总时长内结束并隐藏，保留原4枚演示节奏。
3. MotionVisualSequence / Page 的 Seek、Resume 不再为同一 PlayVersion 重复触发完成回调；Play 才开启新一轮。修复非有限速度/时间配置传播至播放进度，相关文字、Logo、纸屑、跑马灯同步使用有限时间。Jelly 拒绝非有限变形值。
4. 打包流程从本次构建的 evidence/build-result.json 取 BUILD_INFO，验证成功、零错误、版本一致和Unity版本，增加只验证模式及16个前置条件用例，不依赖忽略的根目录旧文件。

## Skill 的变化
- 将 Bounce 的加载姿态节拍、画作收藏阶段、音效节点取消和去重方式写入可追溯的源码映射。
- 区分直接组件、组合配方、宿主适配和明确排除。画作收藏整段流程仍是组合方案，未声称新增专用驱动；TMP顶点波浪仍需宿主适配。
- 补齐57项入口和独立组件验收；明确已有TMP数字、百分比及Reveal适配，避免重复实现。
- 三份 canonical docs 与 Skill references 已通过同步检查。原291/34库存是历史扫描，本次未谎称重新全量扫描。

## 本轮实际验证
- Unity 6000.0.26f1，Windows x64 Mono：构建成功，0错误、0警告。
- Editor：Library 138、Runtime 39、Button 112、SourcePattern 266、SpriteSequence 44项全部通过；Orchestration冒烟检查通过。
- Windows Player 1600×900和1280×720各通过：Typography 107、Presentation 47、Workbench 183、基础46案例及29通用检查、Enhancement 25。所有10份报告 passed=true、errors为空。
- Player新增检查包含回拖继续后完成不重发、非等时图帧、文字/Logo/图帧/纸屑实际中途禁用。Page中途禁用目前只在Editor显式调用回调验证。
- Python打包9项、PowerShell打包前置条件16项、8份美术像素检查通过。普通无测试参数启动保持运行30秒，由验证程序结束，不是自动测试专用包。
- P01与T03的Player实际Canvas截图已人工查看；不把Canvas截图或Unity事件注入写成操作系统鼠标实测。

## 如何验收
完整解压 Windows ZIP，双击 UIMotionGallery.exe；保留同目录 Data、MonoBleedingEdge、UnityPlayer.dll。无需Unity，无需联网。
重点看右上“自动播放/真实操作”、T01–T07、P01/P02、拖动/连续接续与参数导出。与原0.6外观基本相同，本轮核心价值在可复用源码和边界行为，不是换皮或新增编号。
非等时Sprite和6枚以上金币是组件复用回归，默认展馆不新增独立菜单项。查看 docs/project-motion-audit.md 的节拍表与 Skill 源码，不要仅凭展馆外观判断这两项。

## 未验证边界
未测Android/iOS/WebGL、IL2CPP、触屏硬件/操作系统鼠标、可选TMP适配编译、完成监听器重入、Bounce整场游戏流程。没有把这些写成已通过。
BUILD_INFO仅记录构建结果；验收包REVIEW_INFO及发布包RELEASE_INFO与源码指纹辅助追踪，不等于可复现构建证明。
