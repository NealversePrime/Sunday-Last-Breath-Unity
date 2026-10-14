#if UNITY_EDITOR
using UnityEditor;

public class TextureToSpritePostprocessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath.Contains("Assets/Sprites/"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
        }
    }
}
#endif

