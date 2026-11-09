# 第三方依赖

- Unity 6000.0.26f1：仅交付其构建产物及本项目源文件，不交付 Unity Editor。使用者导入工程需自行安装并使用有效授权的 Unity。
- UGUI 2.0.0：Unity 内置包，工程通过 manifest 引用。
- DOTween 免费版 1.3.045：官方标准版本原样位于 `Assets/Plugins/DOTween`，并随工程保留全部内容和原始 `readme.txt`；没有 DOTween Pro。
  下载：https://dotween.demigiant.com/downloads/DOTween_1_3_045.zip
  许可：https://dotween.demigiant.com/license.php
- Noto Sans CJK SC Regular / Medium / Bold：官方未修改OTF，Resources引用名分别为GalleryFont.otf、GalleryMediumFont.otf、GalleryHeadingFont.otf；字体内部名称不变。使用实际字重避免小字合成粗体失真。完整版权和SIL OFL 1.1见 `NotoSansCJK-OFL.txt`。
  来源：https://github.com/notofonts/noto-cjk/tree/main/Sans/OTF/SimplifiedChinese

展馆图标、形状和基础纹理由本项目程序生成；交互实验室的天文台背景由 image gen 按本项目需求原创生成。模板包不携带 DOTween，请在目标项目保留已有版本，或按快速上手安装官方免费版。字体许可随模板一起提供。
