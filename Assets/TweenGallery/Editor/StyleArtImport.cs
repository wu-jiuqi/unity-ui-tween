using UnityEditor;
using UnityEngine;
namespace TweenGallery.Editor
{
    public sealed class StyleArtImport : AssetPostprocessor
    {
        public override uint GetVersion(){return 1;}
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/TweenGallery/Resources/StyleArt/"))return;
            var t=(TextureImporter)assetImporter;t.npotScale=TextureImporterNPOTScale.None;t.mipmapEnabled=false;t.isReadable=false;t.maxTextureSize=2048;
            t.alphaIsTransparency=true;t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;
            t.textureType=assetPath.EndsWith("reward-icons.png")?TextureImporterType.Default:TextureImporterType.Sprite;
            if(t.textureType==TextureImporterType.Sprite){t.spriteImportMode=SpriteImportMode.Single;t.spriteBorder=assetPath.EndsWith("cream-panel.png")?new Vector4(112,112,112,112):Vector4.zero;}
        }
    }
}
