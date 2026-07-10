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
    private bool legacyPngUnreadable = false;
    private Texture pendingSourceTexture;

    public void Init(string path)
    {
        texturePath = path + ".png";

        // Old installations may have a legacy PNG thumbnail left on disk from before video
        // thumbnails were compressed into .aojv1. Once the compressed cache is confirmed valid
        // it's pure disk waste - clean it up.
        if (TextureDiskCache.HasValidCache(texturePath) && File.Exists(texturePath))
        {
            try
            {
                File.Delete(texturePath);
                ConfigManager.WriteConsole($"[TextureCache] deleted legacy PNG {texturePath} (replaced by .aojv1)");
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"[TextureCache] failed deleting legacy PNG {texturePath}", e);
            }
        }

        StartCoroutine(CabinetTextureCache.LoadAndCacheAsync(
            texturePath,
            HandleFailedLoad,
            forceCompress: true
        ));
    }

    public void Load(Texture newSourceTexture)
    {
        if (isSaving || isLoadingExisting || AlreadyCached())
            return;

        // A cached thumbnail may already exist on disk (compressed .aojv1, or a legacy PNG
        // from before thumbnails were compressed) from a previous session - or a race with
        // Init's own async load - even though it hasn't landed in the in-memory cache yet.
        // Load it instead of overwriting it with a fresh capture. Skip this if a legacy PNG
        // was already found unreadable so we fall through to a fresh capture instead of
        // looping forever on a file Unity can't decode.
        if (!legacyPngUnreadable &&
            (TextureDiskCache.HasValidCache(texturePath) || File.Exists(texturePath)))
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
            HandleFailedLoad,
            forceCompress: true
        );
        isLoadingExisting = false;
    }

    // Invoked by LoadAndCacheAsync. On success just forwards the texture. On failure, if the
    // failing file is a legacy PNG thumbnail (no .aojv1 exists) that Unity's runtime decoder
    // couldn't read (e.g. shipped in a cabinet zip from an old game version but in a format
    // DownloadHandlerTexture rejects), remove it so Load() regenerates a fresh .aojv1 from the
    // playing video instead of failing on the same unreadable file forever.
    private void HandleFailedLoad(Texture2D tex)
    {
        if (tex != null)
        {
            OnTextureLoaded?.Invoke(tex);
            return;
        }

        if (!TextureDiskCache.HasValidCache(texturePath) && File.Exists(texturePath))
        {
            legacyPngUnreadable = true;
            try
            {
                File.Delete(texturePath);
                ConfigManager.WriteConsole($"[TextureCache] deleted unreadable legacy PNG {texturePath}; thumbnail will be regenerated from video");
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"[TextureCache] failed deleting unreadable legacy PNG {texturePath}", e);
            }
        }
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

        // Build a Texture2D from the readback data and hand it straight to the compressed
        // disk cache - no intermediate PNG. This handles compression, .aojv1 disk cache, and
        // budget tracking.
        Texture2D snap = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        snap.LoadRawTextureData(request.GetData<byte>());
        snap.Apply(false, false);

        yield return CabinetTextureCache.CacheTextureAsync(
            texturePath,
            snap,
            tex => OnTextureLoaded?.Invoke(tex),
            makeNoLongerReadable: true
        );

        isSaving = false;
    }
}
