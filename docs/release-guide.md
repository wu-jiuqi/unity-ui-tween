# Release 使用与发布指南

版本 0.1.0。发布资产按用途分开：

| 资产 | 用途 |
|---|---|
| UnityTweenGallery-0.1.0-Windows-x64.zip | 解压后运行 UIMotionGallery.exe 查看展馆 |
| UnityTweenGallery-0.1.0-Source.zip | 完整 Unity 源工程 |
| UnityUITween-Templates-0.1.0.unitypackage | TweenGallery 模板与资源；目标项目需已有 DOTween 免费版 |
| unity-ui-tween-0.1.0.zip | Codex Skill、图鉴和模板资产 |

实际可用资产以本次 Release 或本地 dist 目录为准，不把未生成的包列为已交付。

## Windows 包

必须完整解压，保持 EXE、Data、UnityPlayer.dll 及其他生成依赖在原位置。运行展馆不需要安装 Unity 编辑器。基线为 Windows x64 Mono，推荐至少 1280×720 窗口；本机与独立干净环境的实测范围见 validation.md。

程序主要用于选择、比较和调参。效果应用到自己的游戏时需要导入模板或让 Codex 按 Skill 接入。模板并不改变原项目的库存、奖励发放、页面导航或按钮事件。

## 依赖与限制

- 基线是 UGUI，不承诺 UI Toolkit、TMP、WebGL 或移动平台无需适配。
- A08 仅模糊绑定 Image 的纹理；不是整屏或整棵 UI 的实时背景模糊。
- 共享元素和奖励目标需处于同一 Canvas；跨 Canvas 另行映射。
- 示例没有 DOTween Pro、外部粒子包、音效依赖。图形采用项目生成资源；中文字体和 DOTween 版权见根目录 ThirdPartyLicenses/README.md。
- 独立环境未验证的项目应在验收记录中明确保留，不以本机测试替代。

## 发布核对

1. 确认用户指定的目标仓库和可见范围，不把其他动效仓库默认当成本项目。
2. 源码版本、标签、PlayerSettings 版本、文档与包名保持一致。
3. 构建完整 Windows 包，附验证记录和第三方许可。
4. 为每个发布资产计算 SHA-256，记录到清单。
5. 上传后重新下载资产，核对校验和和解压结构，再启动 EXE。
6. Release 说明只写实际完成的编译、运行和环境验证，保留剩余限制。

当前发布状态以 PROGRESS.md 和实际 Release 链接为准。

