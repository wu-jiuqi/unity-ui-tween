# 快速上手

## 只看效果：Windows 展馆

1. 解压整个 `UnityTweenGallery-0.3.0-Windows-x64.zip`。
2. 双击 `UIMotionGallery.exe`。EXE 需和 Data 文件夹、UnityPlayer.dll 等一起保留。
3. 左侧选分类或搜索编号/名称，点击效果。
4. 使用播放、暂停、重播、0.25x 慢放和循环观察动作；调整时长、强度、位移、间隔与曲线。
5. 在相似效果对比中查看 A03/A04/A05 等差异。恢复默认后复制提示词，交给 Codex 接入真实项目。
6. 顶部点击“交互实验室”可体验固定命中区按钮、果冻拖拽、当前姿态接续与减弱动效；点击“返回图鉴”退出。

“反向”只为适合反向展示的条目开放。数值和进度的具体起止值可在 Unity Inspector/代码设置。Escape 可退出。Windows 包是否已通过本机和独立环境验证，以 validation.md 为准。

## 打开独立 Unity 工程

1. 解压 `UnityTweenGallery-0.3.0-Source.zip`。
2. 使用 Unity Hub 添加包含 Assets、Packages、ProjectSettings 的目录。
3. 优先用 Unity 6000.0.26f1 打开，等待导入和编译完成。
4. 打开 `Assets/TweenGallery/Scenes/Gallery.unity`，按 Play。

若需重建生成资源，在本工程使用 **Tools → UI Motion Gallery → Prepare scene and templates**。此操作重新创建 Gallery 场景，保留已存在的预制体并按需生成缺失项；先保存正在编辑的场景。不要把它当成项目已有页面的迁移工具。

## 导入模板到现有项目

1. 备份项目或创建分支。检查 UGUI 与现有 DOTween，避免重复安装。模板包只导出 TweenGallery 资源，不内置 DOTween；缺少依赖时先从 [DOTween 官方下载页](https://dotween.demigiant.com/download.php) 获取免费版，按其说明完成导入与 Setup，再导入模板。已存在 DOTween 时先确认兼容，不覆盖升级。
2. 导入 `UnityUITween-Templates-0.3.0.unitypackage`。保留所选示例所依赖的 Runtime、预设、GeneratedArt、Shader 与字体资源。
3. 将 `Prefabs/A04.prefab` 等示例放进 Canvas，或给现有包装层添加 MotionPlayer。
4. 依据图鉴核对 target、group、items、destination 等绑定，布局稳定后 CaptureBaseline。
5. 调用 Play，并测试连续触发、关闭、销毁、分辨率改变。具体见 [接入手册](integration.md)。

只用自己的 UI 时通常只需要 Runtime、对应预设、catalog 和该效果必要的 Shader；示例预制体还依赖 GeneratedArt 与字体。保留所有导入资源的 .meta 文件，避免 GUID 引用断开。

## 安装 Codex Skill

将 `unity-ui-tween-0.3.0.zip` 解压得到的 `unity-ui-tween` 文件夹放入：

- 默认：`%USERPROFILE%\.codex\skills\unity-ui-tween`
- 如果设置了 CODEX_HOME：`%CODEX_HOME%\skills\unity-ui-tween`

结构应为 `unity-ui-tween/SKILL.md`，不要多套一层同名目录。包中 references 提供图鉴和接入规则，assets/TweenGallery 提供模板资源。重新加载 Codex 后，可输入：

> 使用 $unity-ui-tween，给我的领取按钮做 B02 按压回弹。保留原按钮事件，先检查项目 Unity、UGUI 和 DOTween，绑定已有视觉对象，验证快速点击与禁用后的恢复。

Skill 提供实施方法和模板，不会自动把依赖安装进所有项目。遇到项目版本差异，Codex 应核对现状并在目标项目中实际验证。

