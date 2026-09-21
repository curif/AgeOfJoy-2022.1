using System;
using System.IO;
using UnityEngine;

public static class TextureDiskCache
{
    private const string EXTENSION = ".aojv1";

    // Sentinel prefixing the v2 header (which carries a mip-level count). It's negative, so it can
    // never collide with a legacy v1 header, which begins directly with the width (a small positive
    // int). This keeps old caches readable without a mass re-encode.
    private const int HEADER_MAGIC = unchecked((int)0xA0DEC0DE);

    // Reads either the v2 header (magic + w + h + format + mipCount) or the legacy v1 header
    // (w + h + format, no mips). Leaves the reader positioned at the raw pixel data.
    private static void ReadHeader(BinaryReader reader, out int width, out int height, out TextureFormat format, out int mipCount)
    {
        int first = reader.ReadInt32();
        if (first == HEADER_MAGIC)
        {
            width = reader.ReadInt32();
            height = reader.ReadInt32();
            format = (TextureFormat)reader.ReadInt32();
            mipCount = reader.ReadInt32();
        }
        else
        {
            // Legacy v1: `first` was the width; single mip level only.
            width = first;
            height = reader.ReadInt32();
            format = (TextureFormat)reader.ReadInt32();
            mipCount = 1;
        }
    }

    // Resolves where the .aojv1 lives for a given source image.
    //   cacheDir null/empty -> sidecar next to the source (default; cabinets/screens/video).
    //   cacheDir set         -> <cacheDir>/<sourcefilename>.aojv1 (deco drop-folders keep their
    //                           cache in a subfolder, out of the user's way). Assumes cacheDir is
    //                           1:1 with a flat source folder of unique filenames.
    private static string GetCachePath(string originalPath, string cacheDir)
    {
        if (string.IsNullOrEmpty(cacheDir))
            return originalPath + EXTENSION;
        return Path.Combine(cacheDir, Path.GetFileName(originalPath) + EXTENSION);
    }

    /// <summary>
    /// Checks if a valid .aoj file exists and is newer than the original image.
    /// </summary>
    public static bool HasValidCache(string originalPath, string cacheDir = null)
    {
        string cachePath = GetCachePath(originalPath, cacheDir);

        if (!File.Exists(cachePath)) return false;

        try
        {
            // STALE CHECK: If original image was modified after the cache, ignore cache.
            DateTime originalTime = File.GetLastWriteTimeUtc(originalPath);
            DateTime cacheTime = File.GetLastWriteTimeUtc(cachePath);

            return cacheTime >= originalTime;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Loads the texture directly from raw compressed bytes (Fast).
    /// </summary>
    public static Texture2D LoadFromDisk(string originalPath, bool makeNoLongerReadable = true, string cacheDir = null)
    {
        string cachePath = GetCachePath(originalPath, cacheDir);

        try
        {
            using (FileStream fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // 1. Read Header (v2 magic+mips, or legacy v1)
                ReadHeader(reader, out int width, out int height, out TextureFormat format, out int mipCount);

                // 2. Read Raw Compressed Data (all mip levels, when present)
                int dataSize = (int)(fs.Length - fs.Position);
                byte[] rawData = reader.ReadBytes(dataSize);

                // 3. Create Texture (mip chain only if the cache stored one)
                Texture2D tex = new Texture2D(width, height, format, mipCount > 1);

                // 4. Load into Native GPU Memory
                tex.LoadRawTextureData(rawData);

                // 5. Apply and optionally clear CPU copy immediately
                tex.Apply(false, makeNoLongerReadable);

                return tex;
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleError($"[DiskCache] Corrupt file {cachePath}: {e.Message}");
            // Delete corrupt cache so we regenerate it next time
            try { File.Delete(cachePath); } catch {}
            return null;
        }
    }

    /// <summary>
    /// Asynchronously loads the texture from disk using a background thread for file I/O to prevent VR stutter.
    /// </summary>
    public static System.Collections.IEnumerator LoadFromDiskAsync(string originalPath, Action<Texture2D> onComplete, bool makeNoLongerReadable = true, string cacheDir = null)
    {
        string cachePath = GetCachePath(originalPath, cacheDir);

        int width = 0;
        int height = 0;
        TextureFormat format = TextureFormat.RGBA32;
        int mipCount = 1;
        byte[] rawData = null;
        bool isDone = false;
        bool hasError = false;

        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                using (FileStream fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    ReadHeader(reader, out width, out height, out format, out mipCount);

                    int dataSize = (int)(fs.Length - fs.Position);
                    rawData = reader.ReadBytes(dataSize);
                }
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleError($"[DiskCache] Corrupt file {cachePath}: {e.Message}");
                hasError = true;
            }
            finally
            {
                isDone = true;
            }
        });

        // Yield until the background thread finishes reading the file
        while (!isDone)
        {
            yield return null;
        }

        if (hasError || rawData == null)
        {
            try { File.Delete(cachePath); } catch { }
            onComplete?.Invoke(null);
            yield break;
        }

        // Texture creation MUST happen on the main thread
        Texture2D tex = new Texture2D(width, height, format, mipCount > 1);
        tex.LoadRawTextureData(rawData);
        tex.Apply(false, makeNoLongerReadable); // Upload to GPU

        onComplete?.Invoke(tex);
    }
    
    /// <summary>
    /// Saves the compressed texture to disk asynchronously.
    /// </summary>
    public static void SaveToDisk(string originalPath, Texture2D tex, string cacheDir = null)
    {
        string cachePath = GetCachePath(originalPath, cacheDir);

        try
        {
            // Make sure the alternate cache dir exists (self-heals if the user deleted it).
            if (!string.IsNullOrEmpty(cacheDir))
                Directory.CreateDirectory(cacheDir);

            // 1. Extract all Unity-specific data on the Main Thread
            int width = tex.width;
            int height = tex.height;
            int format = (int)tex.format;
            int mipCount = tex.mipmapCount;
            byte[] rawData = tex.GetRawTextureData(); // includes every mip level

            // 2. Fire and forget a background thread for the slow Disk I/O
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (FileStream fs = File.Open(cachePath, FileMode.Create, FileAccess.Write))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        // 1. Write v2 Header (magic + dims + format + mip count)
                        writer.Write(HEADER_MAGIC);
                        writer.Write(width);
                        writer.Write(height);
                        writer.Write(format);
                        writer.Write(mipCount);

                        // 2. Write Data (all mip levels)
                        writer.Write(rawData);
                    }
                    // ConfigManager.WriteConsole($"[DiskCache] Cached Async: {Path.GetFileName(cachePath)}");
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleError($"[DiskCache] Async Write failed: {e.Message}");
                }
            });
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleError($"[DiskCache] Setup for Async Write failed: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes the cached file from disk if it exists.
    /// </summary>
    public static void DeleteCache(string originalPath, string cacheDir = null)
    {
        string cachePath = GetCachePath(originalPath, cacheDir);
        try
        {
            if (File.Exists(cachePath))
            {
                ConfigManager.WriteConsole($"[DiskCache] Delete {cachePath}");
                File.Delete(cachePath);
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleError($"[DiskCache] Delete failed for {cachePath}: {e.Message}");
        }
    }
}