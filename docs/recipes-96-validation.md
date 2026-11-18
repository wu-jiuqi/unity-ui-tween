# 96 条展馆验证记录

当前交付为 0.7.0-dev 开发版。视觉基准是用户指定的[卡牌与拖放定稿](../ArtSource/Recipes-v070/approved-reference.png)，运行画面为[真实 Windows Player 截图](previews/v070/reference-default.png)。

## 实际通过的检查

| 检查 | 结果 | 证据 |
|---|---|---|
| Unity 6000.0.26f1 Windows x64 构建 | 成功，零错误、零警告 | [构建报告](validation/recipes96-build.json) |
| 1600×1000 可见 Player | 96/96 配方通过 | [报告](validation/recipes96-1600.json) |
| 1280×720 可见 Player | 96/96 配方通过 | [报告](validation/recipes96-1280.json) |
| 96 个唯一编号与规格必填项 | 通过 | scripts/test_recipes96.py |
| 13 个原始 PNG 的哈希、透明度与 117 个裁切矩形 | 通过 | [美术来源](../ArtSource/Recipes-v070/atlas-provenance.json) |
| 旧 Skill 打包契约 | 7 项测试通过 | scripts/test_package_skill.py |

每条 Player 检查分别记录 normal、boundary、sampling、reduced、cleanup，覆盖正常操作结果、边界条件、重播/采样不提交业务、减弱动态与关闭清理。另有目录完整性、对象/属性通道冲突和默认定稿构图/缩放预览检查。长按包含真实经过阈值后只触发一次的检查。全部使用实际场景操作入口，不以存在名称代替场景行为。

## 视觉检查

每条配方保存一张 [Player 运行截图](previews/v070/cases/01.01.png)，完整索引在 [96 条规格](recipes-96.md)。截图是本次运行实际绘制的 UI，不是原型贴图。定稿图只提供独立图层与局部 Sprite 裁切；示例支持真实点击、输入、滚动、拖放、参数变化和时间控制。

代理对全部场景截图检查了舞台占用、文字/图形重叠、裁切、分层和定稿比例，并在实际 Player 中修复了目录锚点、滑块尺寸、卡面占比、原型人像位置、卡牌稀有度、共享元素标题遮挡、容器强调框、成就层级和钱包文字区等问题。用户是否认可视觉与手感，仍在[验收矩阵](recipes-96-acceptance.md)独立保留为待验收，未用自动测试代替。

## 尚未验证

未执行 Android、微信、移动触摸、移动 Profiler、第二台干净 Windows 电脑或完整真人可用性验收。本机桌面通过不代表这些平台通过。原 46 项预制体、旧场景和旧构建保留，本轮未重建或覆盖旧发行包。

## 复现

在仓库根运行 `scripts/recipes96.ps1 -Target Build`，再运行 `-Target Validate`。Player 必须可见：隐藏/最小化窗口可能返回黑屏截图。报告保留实际运行的 UTC、分辨率、Unity 与平台字段；历史版本记录不计入本轮结果。
