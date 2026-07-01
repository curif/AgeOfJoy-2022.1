using UnityEngine;
using System.IO;
using System.Collections;
using System;
using UnityEngine.Rendering;

public class TextureCache : MonoBehaviour
{
    // Read-only: always reflects what is in the LRU cache.
    public Texture2D CachedTexture => CabinetTextureCache.GetCachedTexture(texturePath);

    public string texturePath;
    public float SavingDelayTime = 1f;

    // Invoked when the thumbnail texture becomes available (initial load or after a new frame is saved).
    public Action<Texture2D> OnTextureLoaded;

    private bool isSaving = false;
    private bool isLoadingExisting = false;
    private Texture pendingSourceTexture;

    public void Init(string path)
    {
        texturePath = path + ".png";
        StartCoroutine(CabinetTextureCache.LoadAndCacheAsync(
            texturePath,
            tex => OnTextureLoaded?.Invoke(tex),
            forceCompress: true
        ));
    }

    public void Load(Texture newSourceTexture)
    {
        if (isSaving || isLoadingExisting || AlreadyCached())
            return;

        // A PNG may already exist on disk from a previous session (or a race with
        // Init's own async load) even though it hasn't landed in the in-memory
        // cache yet. Load it instead of overwriting it with a fresh encode.
        if (File.Exists(texturePath))
        {
            isLoadingExisting = true;
            StartCoroutine(LoadExistingFromDiskCoroutine());
            return;
        }

        pendingSourceTexture = newSourceTexture;
        isSaving = true;
        StartCoroutine(SaveTextureCoroutine());
    }

    private IEnumerator LoadExistingFromDiskCoroutine()
    {
        yield return CabinetTextureCache.LoadAndCacheAsync(
            texturePath,
            tex => OnTextureLoaded?.Invoke(tex),
            forceCompress: true
        );
        isLoadingExisting = false;
    }

    public bool AlreadyCached()
    {
        return CabinetTextureCache.IsTextureCached(texturePath);
    }

    private IEnumerator SaveTextureCoroutine()
    {
        if (string.IsNullOrEmpty(texturePath))
        {
            ConfigManager.WriteConsoleError($"[TextureCache] SaveTextureCoroutine: texturePath is empty, skipping save.");
            isSaving = false;
            yield break;
        }

        yield return new WaitForSeconds(SavingDelayTime);

        Texture src = pendingSourceTexture;
        pendingSourceTexture = null;

        if (src == null)
        {
            ConfigManager.WriteConsoleError($"[TextureCache] SaveTextureCoroutine: source texture is null, skipping save.");
            isSaving = false;
            yield break;
        }

        // Blit into a temporary RenderTexture so we can read back any Texture type.
        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        UnityEngine.Rendering.AsyncGPUReadbackRequest request;
        try
        {
            Graphics.Blit(src, rt);

            // Async readback — no main-thread stall, no dropped frame.
            request = AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32);
            while (!request.done)
                yield return null;
        }
        finally
        {
            // Runs even if the coroutine is stopped mid-yield (e.g. player leaves the room),
            // so the temporary RenderTexture is never leaked.
            RenderTexture.ReleaseTemporary(rt);
        }

        if (request.hasError)
        {
            ConfigManager.WriteConsoleError($"[TextureCache] AsyncGPUReadback failed for {texturePath}");
            isSaving = false;
            yield break;
        }

        // Build a Texture2D from the readback data and encode it to PNG.
        Texture2D snap = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        snap.LoadRawTextureData(request.GetData<byte>());
        snap.Apply(false, false);

        byte[] imageData = snap.EncodeToPNG();
        Destroy(snap);

        try
        {
            File.WriteAllBytes(texturePath, imageData);
            ConfigManager.WriteConsole($"[TextureCache] {texturePath} saved first texture");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[TextureCache] {texturePath} can't save texture to disk", e);
            isSaving = false;
            yield break;
        }

        // Evict any stale in-memory entry so LoadAndCacheAsync picks up the fresh PNG.
        CabinetTextureCache.InvalidateCachedTexture(texturePath);

        // Reload through the LRU — this handles compression, .aojv1 disk cache, and budget tracking.
        yield return CabinetTextureCache.LoadAndCacheAsync(
            texturePath,
            tex => OnTextureLoaded?.Invoke(tex),
            forceCompress: true
        );

        isSaving = false;
    }
}
