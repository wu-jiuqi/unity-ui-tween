# 参考图与拆分素材记录

0.6.0-preview2使用内置image_gen制作两张整屏参考，再引用同一主参考生成独立素材。原始PNG保留未重编码；没有SVG占位。两张中央alpha=0的失败底板已弃用，不在实际资源目录。最终实心底板与透明圆头组件分开验收，后者以九宫格复用为面板。

参考和精确提示词：ArtSource/Composition-v061，包含01-achievement-reference、02-responsive-reference、prompts.json、logo-glyph-prompt.json。asset-manifest.json记录8份PNG的实际尺寸、中央RGBA和SHA-256。font-manifest.json记录未修改的Noto Sans CJK SC Regular/Medium/Bold原始字体、官方来源、样式与哈希，许可随包提供。

| 资源 | 用途 | 文字载体 |
|---|---|---|
| ivory-panel | 安静实心文字舞台底板 | 无文字 |
| mint-action / cream-action / lavender-action | 主/次按钮、空白控件；奶油组件兼作九宫格面板 | 无文字，叠加原生Text |
| brand-title | 顶部品牌牌匾 | 固定“动效图鉴”艺术字 |
| achievement-title | P01成就标题缎带 | 固定“旅程达成”艺术字 |
| blank-ribbon | P02标题承载 | 无文字，叠加原生“选择奖励” |
| logo-glyphs | T03独立Logo字形四格图集 | 动/效/图/鉴分别一格 |

Sprite由GalleryCompositionArt读取实际图幅裁切留白，完整圆头/边缘保留在九宫格角区，避免中段拉伸成尖角。源图不被脚本补洞、改字或涂色。UGUI文字采用实际字重，关闭合成Bold；普通文字/动态数值仍可编辑。字体来自[官方Noto CJK源](https://github.com/notofonts/noto-cjk/tree/main/Sans/OTF/SimplifiedChinese)。

运行验证同时检查静止态与采样动态；Logo裁切在独立运动视口内，不能落到应用标题/控件上。docs/previews/v061中PNG是实际Player Canvas离屏渲染，MP4是同一驱动在24fps按时间轴采样后编码的视频，非人工输入或手机录屏。真实触屏、移动端安全区和手机Profiler仍未验证。
