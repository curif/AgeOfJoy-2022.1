using UnityEngine;
using System.IO;
using System.Collections;
using System;

public class TextureCache : MonoBehaviour
{
    public Texture2D CachedTexture;
    private Texture originalTexture;
    public string texturePath;
    public float SavingDelayTime = 1f;

    enum Status
    {
        None,
        WaitingForCreation,
        Loaded,
        Error
    }
    private Status status = Status.None;

    public void Init(string path)
    {
        texturePath = path + ".png";
        LoadTextureFromDisk();
    }
    public void Load(Texture newSourceTexture)
    {
        // Check if texture is already loaded
        if (CachedTexture != null || status == Status.WaitingForCreation)
        {
            // Debug.LogWarning("Texture is already loaded.");
            return;
        }

        if (CachedTexture == null)
        {
            // Assign the source texture to the texture variable
            originalTexture = newSourceTexture;
            status = Status.WaitingForCreation;

            // Save the texture to disk after N seconds
            StartCoroutine(SaveTextureCoroutine());
        }
    }

    public bool AlreadyCached()
    {
        return CachedTexture != null;
    }

    public void LoadTextureFromDisk()
    {
        if (!File.Exists(texturePath))
            return;

        byte[] fileData;
        try
        {
            fileData = File.ReadAllBytes(texturePath);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[LoadTextureFromDisk] {texturePath} can't read texture from disk", e);
            return;
        }

        CachedTexture = new Texture2D(2, 2, TextureFormat.RGB565, true);
        CachedTexture.LoadImage(fileData);
        status = Status.Loaded;

        ConfigManager.WriteConsole($"[TextureCache] {texturePath} loaded.");
    }

    private IEnumerator SaveTextureCoroutine()
    {
        yield return new WaitForSeconds(SavingDelayTime);

        byte[] imageData;
        if (originalTexture is Texture2D)
        {
            Texture2D tex2D = originalTexture as Texture2D;
            imageData = tex2D.EncodeToPNG();
            Graphics.CopyTexture(tex2D, CachedTexture);
        }
        else
        {
            CachedTexture = ConvertToTexture2D(originalTexture);
            imageData = CachedTexture.EncodeToPNG();
        }
        status = Status.Loaded;

        /*else
        {
            ConfigManager.WriteConsoleError($"[TextureCache] {texturePath} Unsupported texture for saving!");
            yield break;
        }*/

        try
        {
            File.WriteAllBytes(texturePath, imageData);
            ConfigManager.WriteConsole($"[TextureCache] {texturePath} saved first texture");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[TextureCache] {texturePath} can't save texture to disk", e);
        }

    }

    private Texture2D ConvertToTexture2D(Texture texture)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(texture, renderTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(texture.width, texture.height);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return tex;
    }

}
