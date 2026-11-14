using UnityEditor;
public sealed class Final3DTextureImporter:AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.Contains("/Final3D/Textures/"))return;var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;importer.wrapMode=UnityEngine.TextureWrapMode.Repeat;importer.filterMode=UnityEngine.FilterMode.Bilinear;importer.mipmapEnabled=true;importer.anisoLevel=4;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.CompressedHQ;}
}
