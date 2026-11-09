using UnityEditor;
using UnityEngine;
namespace TweenGallery.Editor
{
    public sealed class ButtonArtImport : AssetPostprocessor
    {
        public override uint GetVersion() { return 1; }
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/TweenGallery/Resources/ButtonArt/")) return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;
            t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;t.maxTextureSize=2048;
            t.npotScale=TextureImporterNPOTScale.None;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;
        }
    }
}
