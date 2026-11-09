# 0.5.0 按钮皮肤生成记录

使用内置 image_gen 生成一张 1536×1024 RGBA 图集。输入为用户认可的奶油花园美术参考板，作为风格参考，不作为编辑目标。没有复制原游戏按钮，也没有把参考游戏的名称、猫咪、图标或业务文案烘焙进新皮肤。

图集：Assets/TweenGallery/Resources/ButtonArt/cream-buttons.png。六个空白表面：薄荷胶囊、淡紫织边矩形、金色圆钮、珊瑚软糖矩形、奶油描边胶囊、圆角方钮。文字和图标使用原生 UGUI；生成资源只用于可选演示，Runtime 包不依赖它。

最终提示词：

```text
Use case: product-mockup / game UI asset atlas. Generate a production-ready transparent PNG sprite sheet of SIX NEW BLANK button surfaces in the supplied cream garden toy UI style reference. Reference is style only; create new varied shapes. Landscape canvas 1536x1024 with EXACTLY 3 columns and 2 rows of equal 512x512 cells. Each button centered at cell center, all contained in their own cell with at least 44px empty padding, NO labels, writing, letters, icons, characters, flowers, scene, grid lines or watermark. Top row left: wide mint capsule with delicate cream-gold bevel, width420 height170. Top middle: wide lavender soft rounded rectangle, width410 height180, stitched-looking soft edge, elegant subtle highlight. Top right: honey gold circular medallion button, diameter230, raised cream rim, blank center. Bottom row left: coral-peach jelly rounded rectangle, width400 height180, soft translucent candy highlight. Bottom middle: ivory outlined pill, width420 height170, narrow warm gold raised border and cream interior. Bottom right: mint cream rounded-square icon button, width220 height220, toy ceramic raised bevel. Orthographic front-on, consistent top-left warm light, restrained small drop shadow contained inside cells, tactile soft painted 3D casual UI, clean alpha silhouettes. No glow extending into neighbors, no tilt or perspective. Buttons must have blank clean interiors for separate native text and icons. Transparent background.
```

模型输出有柔和半透明边缘与阴影，实际形状范围以生成图和 ButtonStyleLab 中的裁切 Rect 为准，不把提示词期望尺寸当成像素测量结果。Unity 导入显式设置 NPOTScale.None、2048 上限、无 mipmap、无压缩；Player 检查实际纹理为 1536×1024，防止自动缩放后错用裁切坐标。该资源的烘焙边缘不适合直接任意拉伸或当成九宫格。
