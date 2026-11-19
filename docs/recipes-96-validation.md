# 0.8 按案例聚焦的验证记录

当前版本 **0.8.0-dev**。本轮先检查六个代表案例，再对完整 96 条执行回归；没有缩减范围。所有新截图来自当前 Windows Player，旧 v070 画面保留作历史参考。

## 六个代表案例

| 案例 | 实际检查 | 当前运行截图 |
|---|---|---|
| 05.03 按钮按压 | 原生命中、按下状态、固定命中区与视觉缩放分离 | [截图](previews/v080/representatives/05.03.png) |
| 03.08 卡牌翻面 | 一张主卡、点击翻面、曲线差异、重复采样不漂移 | [截图](previews/v080/representatives/03.08.png) |
| 08.05 拖放成功 | 无无关卡牌、大插槽、抓取偏移、接受/类型拒绝/取消 | [截图](previews/v080/representatives/08.05.png) |
| 07.04 列表重排 | 背包图标、同一行 ID 与实例身份保持 | [截图](previews/v080/representatives/07.04.png) |
| 09.03 血条变化 | 真实生命先扣减、主条即时变化、拖尾延后 | [截图](previews/v080/representatives/09.03.png) |
| 11.04 奖励飞行 | 奖励来源、飞行金币、实际钱包 HUD，重播不重复发奖 | [截图](previews/v080/representatives/11.04.png) |

[六例舞台总览](previews/v080/representatives.jpg)。这些是运行截图，不是新绘制的原型或整页贴图。

## 自动检查与范围

| 检查 | 结果 / 证据 |
|---|---|
| Unity 6000.0.26f1 Windows x64 构建 | 零错误、零警告；[报告](validation/recipes96-build.json) |
| 1600×1000 可见 Player | 96/96；[报告](validation/recipes96-1600.json) |
| 1280×720 可见 Player | 96/96；[报告](validation/recipes96-1280.json) |
| 目录和标签 | 默认 12 类/96 条；禁用、校验失败、对象、类型和交集筛选精确断言 |
| 工作台 | 只有一个播放区；继续/循环/反向；首次和重复时间轴采样稳定 |
| 语义状态 | 默认/按下/禁用、编辑与校验、拖放接受/拒绝/取消分别判断 |
| 显示回归 | 主数字文本高度、排序 ID 随对象、循环无人物遮挡、卡牌无无关对象 |
| 原始美术与规格 | 13 PNG 哈希、透明度、117 裁切矩形、96 唯一编号与三维标签 |
| 兼容打包 | 原 Skill 的 7 项打包契约测试 |

每条配方保留 normal、boundary、sampling、reduced、cleanup 五项检查。部分边界是中断/关闭，不代表所有连击、触摸、滚动手感均已穷尽。代表案例用 EventSystem 射线确认目标，再派发指针事件；未把所有案例都当作按钮调用。

代理检查 12 张分类联系表及重点完整画面的文字、遮挡、裁切、主体占比。修正了任务容器收起裁切、分隔条/强调框定位、共享元素按钮遮脸、焦点框、主数字截断、物品 ID 标签与通知反馈主体大小等问题。截图检查仍可能漏掉节奏和手感问题。

## 截图、报告与附件的一致性

打包后运行 `scripts/verify_recipe_delivery.py`：逐文件比较 ZIP 与被测 Player 目录，记录 EXE、UnityPlayer、业务程序集、场景数据、两份报告及六张代表截图的 SHA-256。[本轮交付清单](validation/recipes96-delivery.json)包含 ZIP 哈希与验证文件数。

截图来自本轮 Player，打包后未另行替换演示美术或业务程序集。这证明交付文件与被测目录一致，不把哈希记录当作真人体验认可。

## 未通过自动测试代替的验收

用户视觉、动画节奏、连击中断、拖放容错、真人三秒理解目标仍待实际体验；没有标记为人工通过。Android、微信、触屏、第二台干净 Windows 电脑与移动 Profiler 未验证。目标平台列中的“本机 Windows 通过”仅指本次环境。

## 复现

1. `scripts/recipes96.ps1 -Target Build`
2. `scripts/recipes96.ps1 -Target Validate`（窗口须可见，隐藏窗口可能黑屏）
3. `python scripts/collect_recipe_evidence.py`
4. `python scripts/document_recipes96.py`
5. `python scripts/test_recipes96.py`
6. 用 `scripts/portable_zip.py` 打包 `Builds/Recipes96` 后，运行 `scripts/verify_recipe_delivery.py`。

报告保留真实 UTC、版本、平台与分辨率。完整索引见 [96 条规格](recipes-96.md)，人工状态见 [验收矩阵](recipes-96-acceptance.md)。
