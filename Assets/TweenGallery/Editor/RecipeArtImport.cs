using UnityEditor;
using UnityEngine;

namespace TweenGallery.Editor
{
    public sealed class RecipeArtImport : AssetPostprocessor
    {
        void OnPreprocessTexture(){
            if(!assetPath.StartsWith("Assets/TweenGallery/Resources/RecipeArt/")||!assetPath.EndsWith(".png"))return;
            var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
        }
    }
}
