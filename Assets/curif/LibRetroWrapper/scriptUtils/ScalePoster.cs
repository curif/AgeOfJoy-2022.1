#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class ScalePoster
{
    [MenuItem("Custom/Scale Poster")]
    public static void ScaleSelectedTextures()
    {
        // Get selected textures in the Project window
        Object[] selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);

        foreach (Object obj in selectedTextures)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            // Set the texture format to PNG for optimal space and performance
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            textureImporter.alphaIsTransparency = true;
            textureImporter.SaveAndReimport();

            // Load the texture and scale it
            Texture2D originalTexture = (Texture2D)obj;
            Texture2D scaledTexture = ScaleTexture(originalTexture, 420, originalTexture.height * 420 / originalTexture.width);

            // Save the scaled texture as PNG
            byte[] bytes = scaledTexture.EncodeToPNG();
            string newPath = assetPath.Substring(0, assetPath.LastIndexOf('.')) + ".png";
            System.IO.File.WriteAllBytes(newPath, bytes);

            // Replace the original texture with the scaled texture
            AssetDatabase.ImportAsset(newPath);
            Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
            EditorGUIUtility.PingObject(newTexture);
        }

        Debug.Log("Poster scaling complete!");
    }

    // Utility function to scale a texture
    private static Texture2D ScaleTexture(Texture2D texture, int targetWidth, int targetHeight)
    {
        RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 32);
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);

        Texture2D scaledTexture = new Texture2D(targetWidth, targetHeight);
        scaledTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        scaledTexture.Apply();

        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        return scaledTexture;
    }
}
#endif