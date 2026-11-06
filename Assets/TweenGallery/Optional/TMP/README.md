# 可选 TextMeshPro 适配

本目录默认不参与编译，基础 UGUI `MotionPlayer` 不依赖 TMP。此适配器展示独立的数值、整数百分比和文字揭示；它不自动将旧 `MotionBindings.label` 转成 TMP，也不接管库存、奖励或其他业务状态。

1. 确认目标工程已经安装兼容的 TextMeshPro 包和 DOTween 免费版。Unity 6 的 UGUI 包通常含 `Unity.TextMeshPro`；较早版本请按对应 Unity 版本安装。不要重复导入或静默升级。
2. 在需要的目标平台的 **Player Settings → Scripting Define Symbols** 加入 `TWEEN_GALLERY_TMP`。如果项目使用自定义 asmdef，还要让包含本脚本的程序集引用 `Unity.TextMeshPro` 与 DOTween；不要把本脚本置入不允许访问预编译 DOTween 的程序集。
3. 添加 `TweenGallery.Optional.TmpMotionAdapter`，调用 `Bind(TMP_Text)`，再使用 `PlayInteger(from, businessValue, duration)`、`PlayPercent(fromPercent, businessPercent, duration)` 或 `Reveal(duration)`。`duration <= 0` 直接显示最终状态，可用于由宿主决定的减少动效模式。
4. 数字仅在取整结果改变时更新；百分比是整数百分点，例如 25→80 显示 `25%`→`80%`。暂停、恢复和停止只作用于此实例的 Tween。取消数字动画保留当前显示；业务值变化或关闭时由调用方用 `SetBusinessInteger` 提交最终值，避免旧结算结果覆盖新结果。取消逐字效果恢复原 `maxVisibleCharacters`，不改字符串。
5. 在动画期间外部更改同一 TMP 字段前，先 `Stop()` 并以当前业务值重新启动；原文被替换时需重启 `Reveal` 以重算字符数。逐字使用 TMP 渲染字符数而非严格 Unicode 字素簇分段，复杂表情/组合字符需要项目专门适配。文本包裹、布局组件、动态字体图集加载、RTL 和本地化需要在目标项目实机检查。组件本身不生成音效或修改游戏暂停状态。

示例：

```csharp
var number = GetComponent<TweenGallery.Optional.TmpMotionAdapter>();
number.Bind(myTmpLabel);
number.PlayInteger(100, wallet.Amount, 0.45f);
// 例如库存变化或界面被回收：
number.SetBusinessInteger(wallet.Amount);
```

此适配器对每个组件拥有自己的 Tween 和版本号；在重入、禁用或销毁后不触发被取消动画的完成回调。它与 `MotionPlayer` 分离，两个组件不要同时写同一文本/可见字符属性。
