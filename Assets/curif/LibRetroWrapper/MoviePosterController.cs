/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class MoviePosterController : MonoBehaviour
{
    [Tooltip("The decade of the posters (70/80/90)")]
    [SerializeField]
    int decade = 80;

    private static PictureAndFrameTexture textureData = null;
    private System.Random random;

    void Start()
    {
        random = new System.Random(DateTime.Now.Second);
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

        // Load and parse the YAML file as a TextAsset
        if (textureData == null)
        {
            TextAsset yamlTextAsset = Resources.Load<TextAsset>($"Decoration/MoviePoster/Pictures/{decade}/contents");
            textureData = LoadYamlFromTextAsset(yamlTextAsset);
        }

        if (textureData == null || textureData.textures.Count == 0)
        {
            ConfigManager.WriteConsoleWarning($"[MoviePosterController] No textures found in YAML file for decade {decade}.");
            return;
        }

        StartCoroutine(RandomizePosters());
    }

    IEnumerator RandomizePosters()
    {
        // Yielding one frame allows Unity to finish initializing objects
        yield return new WaitForEndOfFrame();

        // Load and randomize
        if (gameObject.transform.childCount > 0)
        {
            // Shuffle the texture data
            List<PictureAndFrameTextureInfo> textures = ShuffleList(textureData.textures);
            int textureIndex = 0;

            for (int childIdx = 0; childIdx < gameObject.transform.childCount; childIdx++)
            {
                if (textureIndex >= textures.Count)
                    textureIndex = 0; // Reset index if we've gone through all textures

                // Apply the texture to the child
                string texturePath = $"Decoration/MoviePoster/Pictures/{decade}/{textures[textureIndex].Name}";
                ApplyTextureToChild(texturePath, gameObject.transform.GetChild(childIdx));

                textureIndex++;

                // Adding a small delay between setting textures in each iteration
                yield return null;

            }
        }
    }

    private PictureAndFrameTexture LoadYamlFromTextAsset(TextAsset yamlTextAsset)
    {
        if (yamlTextAsset == null)
        {
            ConfigManager.WriteConsoleError($"[MoviePosterController] YAML TextAsset not found at path: Decoration/MoviePoster/Pictures/{decade}/contents");
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
            Debug.LogError($"[MoviePosterController] Failed to load YAML file: {ex.Message}");
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
                Transform poster = child.GetChild(0);
                renderer = poster.gameObject.GetComponent<Renderer>();
                renderer.material.SetTexture("_Albedo", texture);

                if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
                    renderer.material.SetFloat("_EmissiveAmount", 0.6f);
            }
            else
            {
                renderer = child.gameObject.GetComponent<Renderer>();
                renderer.material.SetTexture("_MainTex", texture);
            }
        }
        else
        {
            ConfigManager.WriteConsoleWarning($"[MoviePosterController] Texture not found at path: {texturePath}");
        }
    }

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

    private static Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (!File.Exists(filePath))
            throw new Exception($"Loading picture, file doesn't exist: {filePath}");

        fileData = File.ReadAllBytes(filePath);
        tex = new Texture2D(2, 2, TextureFormat.RGB565, true);
        if (tex == null)
            throw new Exception($"Error creating texture for {filePath}");

        tex.LoadImage(fileData); // This will auto-resize the texture dimensions.

        return tex;
    }

    public bool Replace(int childIndex, string newTexturePath)
    {
        if (childIndex >= 0 && childIndex < gameObject.transform.childCount)
        {
            GameObject childObject = gameObject.transform.GetChild(childIndex).gameObject;

            Texture2D newTexture;
            try
            {
                newTexture = LoadTexture(newTexturePath);
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"[MoviePosterController.Replace] Loading texture #{childIndex} from disk: {newTexturePath} ", e);
                throw;
            }

            ApplyTextureToChild(newTexturePath, childObject.transform);
            return true;
        }
        else
            throw new Exception($"Poster #{childIndex} doesn't exist");
    }

    public int Count()
    {
        return gameObject.transform.childCount;
    }
}