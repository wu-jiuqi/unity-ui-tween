using UnityEditor;
using UnityEngine;
namespace TweenGallery.Editor
{
    public sealed class CompositionArtImport : AssetPostprocessor
    {
        public override uint GetVersion()=>1;
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/TweenGallery/Resources/CompositionArt/"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;
            t.mipmapEnabled=false;t.isReadable=false;t.alphaIsTransparency=true;t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;
        }
    }
}
