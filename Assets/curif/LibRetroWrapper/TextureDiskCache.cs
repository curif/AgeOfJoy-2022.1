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
    public static Texture2D LoadFromDisk(string originalPath)
    {
        string cachePath = originalPath + EXTENSION;

        try
        {
            using (FileStream fs = File.Open(cachePath, FileMode.Open, FileAccess.Read))
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

                // 5. Apply and clear CPU copy immediately
                tex.Apply(false, true);

                return tex;
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleError($"[DiskCache] Corrupt file {cachePath}: {e.Message}");
            // Delete corrupt cache so we regenerate it next time
            File.Delete(cachePath);
            return null;
        }
    }

    /// <summary>
    /// Saves the compressed texture to disk.
    /// </summary>
    public static void SaveToDisk(string originalPath, Texture2D tex)
    {
        string cachePath = originalPath + EXTENSION;

        try
        {
            // Get the compressed ETC2 bytes
            byte[] rawData = tex.GetRawTextureData();

            using (FileStream fs = File.Open(cachePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // 1. Write Header
                writer.Write(tex.width);
                writer.Write(tex.height);
                writer.Write((int)tex.format);

                // 2. Write Data
                writer.Write(rawData);
            }
            // ConfigManager.WriteConsole($"[DiskCache] Cached: {Path.GetFileName(cachePath)}");
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleError($"[DiskCache] Write failed: {e.Message}");
        }
    }
}