# 0.5.0 按钮皮肤生成记录

使用内置 image_gen 生成一张 1536×1024 RGBA 图集。输入为用户认可的奶油花园美术参考板，作为风格参考，不作为编辑目标。没有复制原游戏按钮，也没有把参考游戏的名称、猫咪、图标或业务文案烘焙进新皮肤。

图集：Assets/TweenGallery/Resources/ButtonArt/cream-buttons.png。六个空白表面：薄荷胶囊、淡紫织边矩形、金色圆钮、珊瑚软糖矩形、奶油描边胶囊、圆角方钮。文字和图标使用原生 UGUI；生成资源只用于可选演示，Runtime 包不依赖它。

最终提示词：

```text
Use case: product-mockup / game UI asset atlas. Generate a production-ready transparent PNG sprite sheet of SIX NEW BLANK button surfaces in the supplied cream garden toy UI style reference. Reference is style only; create new varied shapes. Landscape canvas 1536x1024 with EXACTLY 3 columns and 2 rows of equal 512x512 cells. Each button centered at cell center, all contained in their own cell with at least 44px empty padding, NO labels, writing, letters, icons, characters, flowers, scene, grid lines or watermark. Top row left: wide mint capsule with delicate cream-gold bevel, width420 height170. Top middle: wide lavender soft rounded rectangle, width410 height180, stitched-looking soft edge, elegant subtle highlight. Top right: honey gold circular medallion button, diameter230, raised cream rim, blank center. Bottom row left: coral-peach jelly rounded rectangle, width400 height180, soft translucent candy highlight. Bottom middle: ivory outlined pill, width420 height170, narrow warm gold raised border and cream interior. Bottom right: mint cream rounded-square icon button, width220 height220, toy ceramic raised bevel. Orthographic front-on, consistent top-left warm light, restrained small drop shadow contained inside cells, tactile soft painted 3D casual UI, clean alpha silhouettes. No glow extending into neighbors, no tilt or perspective. Buttons must have blank clean interiors for separate native text and icons. Transparent background.
```

模型输出有柔和半透明边缘与阴影，实际形状范围以生成图和 ButtonStyleLab 中的裁切 Rect 为准，不把提示词期望尺寸当成像素测量结果。Unity 导入显式设置 NPOTScale.None、2048 上限、无 mipmap、无压缩；Player 检查实际纹理为 1536×1024，防止自动缩放后错用裁切坐标。该资源的烘焙边缘不适合直接任意拉伸或当成九宫格。


## preview2 全库美术统一

2026-11-09 使用内置 image_gen，依据同一已认可原型，重新生成 reward-chest、energy-crystal、adventure-world 三张示范美术。替换现有 MotionArt PNG，保留 Unity .meta GUID。宝箱与晶石为真实 RGBA 透明图，场景为不透明图；同时用于旧 A/B/D/E/F 示例，不只用于新按钮工坊。

提示词方向分别为：奶油木质、蜂蜜金边、薄荷缎带的柔软玩具宝箱；淡紫到薄荷渐变、圆润切面、暖金底环的晶石；有留白、奶油小路、薄荷灌木和淡紫花朵的晴天花园。共同约束为柔和左上光、无文字/图标/商标、原生文字另叠，避免霓虹、尖锐写实金属和高饱和科幻质感。宝箱/晶石模型输出 1280×1280，场景输出 1536×1024，Unity 按现有导入器生成运行资源；提示词中的期望大小不当作实际输出尺寸。
