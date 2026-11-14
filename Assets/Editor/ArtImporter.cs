using UnityEditor;
public class ArtImporter:AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.Contains("/Resources/Art/"))return;var texture=(TextureImporter)assetImporter;texture.isReadable=assetPath.Contains("equipment-")||assetPath.Contains("bestiary-")||assetPath.EndsWith("relic-attacks-v23.png")||assetPath.Contains("hero-")||assetPath.EndsWith("ui-v2.png")||assetPath.EndsWith("-v7.png")||assetPath.Contains("armor-heroes-");texture.textureType=TextureImporterType.Default;texture.filterMode=UnityEngine.FilterMode.Point;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.mipmapEnabled=false;texture.alphaIsTransparency=true;texture.npotScale=TextureImporterNPOTScale.None;texture.wrapMode=UnityEngine.TextureWrapMode.Clamp;texture.maxTextureSize=2048;}
}
