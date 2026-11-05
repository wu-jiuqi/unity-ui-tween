# Unity 模板与参数接入手册

版本 0.1.0。基线为 Unity 6000.0.26f1、UGUI 2.0.0、DOTween 免费版 1.3.045、Built-in 渲染管线。实际验证结果见随发布包提供的验收记录；版本信息不代表所有平台与管线均已验证。

## 三种接入方式

1. **示例预制体**：把 `Assets/TweenGallery/Prefabs/<ID>.prefab` 拖进 Canvas，观察其 MotionPlayer 绑定。
2. **现有 UI**：复用 Runtime、对应预设和必要 Shader；给现有 UI 的动画包装层添加 MotionPlayer，按图鉴设置 bindings。
3. **Codex 适配**：从展馆复制提示词，补上真实对象、触发事件与完成状态。Skill 与展馆使用同一份 Runtime。

Skill 的完整资产位于 `assets/TweenGallery/`，导入后对应 Unity 项目的 `Assets/TweenGallery/`；内部 Runtime、Shaders、Resources、Prefabs、Presets、GeneratedArt、Gallery、Scenes 等目录保持原相对结构。示例依赖字体和图形，复制时保留 `.meta` 文件；只为现有 UI 接入效果时可选择 Runtime、catalog、相应预设和必要 Shader。

先检查目标项目已有 DOTween 版本，避免导入第二份插件。模板 unitypackage 和 Skill 资源只包含 TweenGallery，目标项目需自行具备 DOTween 免费版；完整源工程包含已核对的原版依赖。包内模板不构成替换现有依赖的授权。若有同名脚本或不同版本的 TweenGallery，先在备份或分支中核对差异。数字和文字绑定使用 `UnityEngine.UI.Text`；TMP 需要增加适配层，不能直接把 TMP_Text 填入 Text 字段。

## 最小 A04 接入

受 LayoutGroup 控制的 UI 使用 `LayoutItem > MotionVisual` 两层：父层负责排版，子层负责动画。CanvasGroup 放在 MotionVisual。布局完成后捕获基线。

```csharp
using TweenGallery;
using UnityEngine;

public sealed class PopupMotion : MonoBehaviour
{
    [SerializeField] private MotionPlayer player;
    [SerializeField] private RectTransform visual;
    [SerializeField] private CanvasGroup opacity;

    public void Prepare()
    {
        Canvas.ForceUpdateCanvases();
        player.bindings = new MotionBindings { target = visual, group = opacity };
        player.CaptureBaseline();
    }

    public void Open()
    {
        visual.gameObject.SetActive(true);
        var options = MotionSettings.FromDefinition(MotionCatalog.Find("A04"));
        options.duration = 0.35f;
        options.strength = 0.7f;
        player.Play("A04", options);
    }

    public void Close()
    {
        player.StopAndRestore();
        visual.gameObject.SetActive(false);
    }
}
```

由项目在首次布局就绪后调用 Prepare。不要在动画中间状态调用 CaptureBaseline，否则该状态会成为新的基线。分辨率、布局或内容变化后执行：StopAndRestore → 更新布局 → Canvas.ForceUpdateCanvases → CaptureBaseline。

## Runtime API

| API | 用途 |
|---|---|
| MotionCatalog.All / Find(id) | 读取清单或按编号查默认值 |
| MotionSettings.FromDefinition(definition) | 创建可调参数 |
| MotionPlayer.Play(id, options) | 取代同 target 的前一个播放器，清理本实例、恢复基线后播放 |
| Pause() / Resume() | 暂停、恢复本实例 |
| StopAndRestore() | 取消本实例 Tween，恢复捕获值 |
| CaptureBaseline() | 记录绑定对象当前稳定状态 |
| Sample(normalizedTime) | 以相同求值器采样 0–1 进度，用于比较和验证 |
| IsPlaying / Progress | 读取播放状态和进度 |
| MotionPreset | 保存 effectId/settings 的 ScriptableObject，ApplyTo 应用到播放器 |
| MotionPointerFeedback | B01/B02 的真实按下/松开反馈组件 |

## 参数

| 字段 | 含义 |
|---|---|
| duration | 完整效果或单次循环的秒数，不是每个子项的秒数 |
| strength | 默认 1；缩放、振荡、模糊等幅度倍率 |
| distance | Canvas 参考坐标位移；没有位移的效果可能不使用 |
| stagger | 子项起动间隔秒数；自动限制总编排不超过 duration |
| ease | Preset 或 DOTween Ease 名称；Preset 保留每项固有差异 |
| reverse | 对完整效果反向采样，不自动回滚游戏状态 |
| loop | 重复完整周期，使用 StopAndRestore 终止 |
| speed | 本实例速度；0.25 为四分之一速，不改变 Time.timeScale |
| unscaledTime | 默认 true，游戏暂停时 UI 仍可播放 |
| startValue / endValue | 数字起止值，默认 100 / 1250；真实数值由业务系统保存 |
| fillFrom / fillTo | 填充比例，默认 0.08 / 0.90，范围 0–1 |

所有参数可在 Inspector 或代码配置。展馆主要公开时长、强度、位移、间隔、曲线和播放控制。不是每个参数对每项效果都有意义。OutBack、OutBounce、OutElastic 会改变进度曲线，不能代替效果编号。

## 绑定约定

| 字段 | 对象 |
|---|---|
| target | 主动画 RectTransform 或多子项稳定根节点 |
| group | 主对象/页面 CanvasGroup |
| image | 主 Image；用于颜色或模糊 |
| secondary / secondaryGroup / secondaryImage | 第二页面、旧选中项或接收端；含义由效果决定 |
| destination | 开关、共享元素、奖励或手势的目标 RectTransform |
| mask | 独立裁剪容器，其上使用 Mask/RectMask2D |
| shine | 裁剪容器内部的高光条 |
| lid | UI 宝箱盖；pivot 应放在合理铰链位置 |
| fill | Image.Type.Filled 的进度图形 |
| label | UGUI Text 数字、百分比或飘字 |
| items / labels / itemGroups | 子项、文字、透明度数组，顺序一致 |

`Play` 与 `SetPressed` 都按 `bindings.target` 登记所有权：同一个 target 的新请求会调用前一个播放器的 StopAndRestore，再接管目标；不会终止不同 target 的播放。同 target 上的多个播放器不能靠并行调用叠加不同属性。需要组合时在同一播放器内编排，或用不同父子包装层分别承载位移与缩放。仲裁只检查 target，不检查不同 target 共享的 secondary、items 等绑定；这些对象仍需避免同时写同一属性。不要用 DOTween.KillAll 清理一个弹窗。

## 真实按钮交互

B01/B02 自动演示完整按下和松开周期。真实按钮上添加 MotionPointerFeedback，将其 player 指向绑定该视觉目标的 MotionPlayer；rebound=false 对应 B01，true 对应 B02。保留现有 Button.onClick，不额外复制业务回调。验证按住、移出后松开、快速双击、禁用后重用。已有 Animator/ColorTint 时避免修改同一属性。

SetPressed 与 Play 使用相同的 target 接管规则。MotionPointerFeedback 可以绑定已有或专门的 MotionPlayer；同 target 的按压与其他播放请求会互相取代，不并行叠加。切换时会恢复前任拥有的动画属性，再以新播放器捕获的基线运行，因此各播放器应在稳定布局状态捕获一致基线。

## 特殊效果边界

- **A08**：附带 Shader 只模糊绑定 Image 纹理，不模糊子 Text、整棵 UI 或实时背景。使用独立 Clamp 纹理，避免图集邻图溢色。目标项目在 Project Settings → Graphics → Always Included Shaders 中加入 `TweenGallery/UITextureBlur`，或通过确实被构建引用的材质保留该 Shader；否则动态 Shader.Find 可能在 Player 中失败。其他管线需检查遮罩、裁剪和打包后效果。不要在已有游戏工程运行 GalleryBuild.Prepare，它用于独立展馆，会重建场景并修改构建与输入设置。
- **A07/C03**：改变裁剪窗口，不压缩文字。需要周围条目重排时由项目衔接 LayoutElement；避免 LayoutGroup 每帧覆盖动画值。
- **C04/D03/D04/B08/E07**：destination 与目标必须处于同一 Canvas，支持父级坐标转换，不支持跨 Canvas 自动映射。C04 支持非居中 pivot 与缩放，但目标轴方向必须一致。
- **D07**：target 为两面共享旋转根；group 与 image 为背面、初始可见；secondaryGroup 与 secondaryImage 为正面、完成可见。两面各自用 CanvasGroup 控制整个子层级，防止背面子图形泄露。侧面时交换内容，业务层决定揭晓结果。
- **D08**：UI 宝箱盖与奖励组合，无额外粒子/音效依赖。大量奖励应使用对象池。
- **E03**：高光条加 UGUI 遮罩，不要求 DOTween Pro。
- **E06**：有限图标沿固定轨迹错时运动，不是实时鼠标轨迹或粒子拖尾。

## 完成、取消与业务状态

Play 重入先取消本实例动画并恢复基线，避免累积偏移；停用和销毁清理本实例 Tween。恢复只覆盖该效果拥有的动画属性，不覆盖无关业务值。StopAndRestore 不恢复库存、页面历史或选中数据。alpha 按捕获的基线乘权重；隐藏时临时关闭 blocksRaycasts，不修改 interactable。业务禁用请使用 interactable 或外层 CanvasGroup，避免与播放器同时写同一个 blocksRaycasts。

循环、反向和动画结束都不代表发奖、扣费或页面导航已成功。调用方继续负责这些业务。验收建议见 [提示词与验收](acceptance.md)。

