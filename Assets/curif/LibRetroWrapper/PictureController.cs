using System;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[System.Serializable]
public class PictureAndFrameTextureInfo
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; }
}
[System.Serializable]
public class PictureAndFrameTexture
{
    public List<PictureAndFrameTextureInfo> textures { get; set; }
}


public class PictureController : MonoBehaviour
{
    public string yamlResourcePath = "Decoration/Pictures/contents"; // Path to the YAML file in Resources (without .yaml extension)
    private static PictureAndFrameTexture textureData = null;
    private System.Random random;

    void Start()
    {
        random = new System.Random(DateTime.Now.Second);
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

        // Load and parse the YAML file as a TextAsset
        if (textureData == null)
        {
            TextAsset yamlTextAsset = Resources.Load<TextAsset>(yamlResourcePath);
            textureData = LoadYamlFromTextAsset(yamlTextAsset);
        }

        if (textureData == null || textureData.textures.Count == 0)
        {
            ConfigManager.WriteConsoleWarning("[PictureController] No textures found in YAML file.");
            return;
        }

        // Shuffle the texture data
        List<PictureAndFrameTextureInfo> textures = ShuffleList(textureData.textures);
        int textureIndex = 0;
        for (int childIdx = 0; childIdx < gameObject.transform.childCount; childIdx++)
        {
            if (textureIndex >= textures.Count)
            {
                textureIndex = 0; // Reset index if we've gone through all textures
            }

            // Apply the texture to the child
            string texturePath = $"Decoration/Pictures/{textures[textureIndex].Name}";
            ApplyTextureToChild(texturePath, gameObject.transform.GetChild(childIdx));

            textureIndex++;
        }
    }

    private PictureAndFrameTexture LoadYamlFromTextAsset(TextAsset yamlTextAsset)
    {
        if (yamlTextAsset == null)
        {
            ConfigManager.WriteConsoleError($"[PictureController] YAML TextAsset not found at path: {yamlResourcePath}");
            return null;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<PictureAndFrameTexture>(yamlTextAsset.text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PictureController] Failed to load YAML file: {ex.Message}");
            return null;
        }
    }

    private void ApplyTextureToChild(string texturePath, Transform child)
    {
        Texture2D texture = Resources.Load<Texture2D>(texturePath);

        if (texture != null)
        {
            Renderer renderer;
            if (child.childCount > 0)
            {
                Transform picture = child.GetChild(0);
                renderer = picture.gameObject.GetComponent<Renderer>();
                renderer.material.SetTexture("_Albedo", texture);
            }
            else
            {
                renderer = child.gameObject.GetComponent<Renderer>();
                renderer.material.SetTexture("_MainTex", texture);
            }
        }
        else
        {
            ConfigManager.WriteConsoleWarning($"[PictureController] Texture not found at path: {texturePath}");
        }
    }
    /*
     * .OrderBy(name => random.Next()).ToArray():

        This approach leverages LINQ, which is both convenient and readable.
        However, it might be slower because it requires sorting the entire collection using a comparator that relies on a random number generator. Since sorting is generally O(n log n), this could be less efficient than a custom shuffle, especially with large datasets.
        It creates a new array rather than modifying the original collection in place, which may lead to additional memory usage.
     * Use .OrderBy(name => random.Next()).ToArray() when you prioritize readability and simplicity over performance, and when the collection size is small or performance is not a critical concern.
     * Use a custom shuffle method like ShuffleList<T>() when performance matters, especially with large collections, or when you need high-quality randomness.
     */
    private List<T> ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }
}
