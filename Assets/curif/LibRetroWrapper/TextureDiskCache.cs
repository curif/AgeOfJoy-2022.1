using System;
using System.IO;
using UnityEngine;

public static class TextureDiskCache
{
    private const string EXTENSION = ".aojv1";

    /// <summary>
    /// Checks if a valid .aoj file exists and is newer than the original image.
    /// </summary>
    public static bool HasValidCache(string originalPath)
    {
        string cachePath = originalPath + EXTENSION;

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
    public static Texture2D LoadFromDisk(string originalPath, bool makeNoLongerReadable = true)
    {
        string cachePath = originalPath + EXTENSION;

        try
        {
            using (FileStream fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // 1. Read Header (12 bytes)
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                TextureFormat format = (TextureFormat)reader.ReadInt32();

                // 2. Read Raw Compressed Data
                int dataSize = (int)(fs.Length - fs.Position);
                byte[] rawData = reader.ReadBytes(dataSize);

                // 3. Create Texture
                // 'false' = No mipmaps (Standard for UI/Cabinet art)
                Texture2D tex = new Texture2D(width, height, format, false);

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
    public static System.Collections.IEnumerator LoadFromDiskAsync(string originalPath, Action<Texture2D> onComplete, bool makeNoLongerReadable = true)
    {
        string cachePath = originalPath + EXTENSION;

        int width = 0;
        int height = 0;
        TextureFormat format = TextureFormat.RGBA32;
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
                    width = reader.ReadInt32();
                    height = reader.ReadInt32();
                    format = (TextureFormat)reader.ReadInt32();

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
        Texture2D tex = new Texture2D(width, height, format, false);
        tex.LoadRawTextureData(rawData);
        tex.Apply(false, makeNoLongerReadable); // Upload to GPU

        onComplete?.Invoke(tex);
    }
    
    /// <summary>
    /// Saves the compressed texture to disk asynchronously.
    /// </summary>
    public static void SaveToDisk(string originalPath, Texture2D tex)
    {
        string cachePath = originalPath + EXTENSION;

        try
        {
            // 1. Extract all Unity-specific data on the Main Thread
            int width = tex.width;
            int height = tex.height;
            int format = (int)tex.format;
            byte[] rawData = tex.GetRawTextureData();

            // 2. Fire and forget a background thread for the slow Disk I/O
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (FileStream fs = File.Open(cachePath, FileMode.Create, FileAccess.Write))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        // 1. Write Header
                        writer.Write(width);
                        writer.Write(height);
                        writer.Write(format);

                        // 2. Write Data
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
    public static void DeleteCache(string originalPath)
    {
        string cachePath = originalPath + EXTENSION;
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