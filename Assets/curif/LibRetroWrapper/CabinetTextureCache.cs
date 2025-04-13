//#define FORCE_565
//#define GAMMA_FIX
//#define TEXTURE_DEBUG
//#define TEXTURE_SAVE
//#define RESCALE
//#define RESCALED_TEXTURE_LOAD
//#define RESCALED_TEXTURE_SAVE


using System;
using System.IO;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Experimental.Rendering;
public static class CabinetTextureCache
{

    private static byte[] astcMagicNumber = new byte[] { 0x13, 0xAB, 0xA1, 0x5C };

    private static ResourceCache<string, Texture2D> CachedTextures = null;
    private static GpuRgb565Converter gpuRgb565Converter = null;

    // Method to load and cache a texture
    public static Texture2D LoadAndCacheTexture(string path)
    {
        Texture2D tex = null;
        float sizeBytes = 4f;

        if (CachedTextures == null)
        {
            // lazy creation:
           if (DeviceController.IsQ3)
                CachedTextures = ResourceCacheManager.Create<string, Texture2D>("texturesCache", 1536f); 
           else
                CachedTextures = ResourceCacheManager.Create<string, Texture2D>("texturesCache", 1024f);

        }

        if (gpuRgb565Converter == null)
        {
            GameObject fixedObject = UnityEngine.GameObject.Find("FixedObject");
            gpuRgb565Converter = fixedObject.GetComponent<GpuRgb565Converter>();
        }

        if (!IsTextureCached(path))
        {
            try
                {
                byte[] fileData;
                string processedPath = path + ".aoj.jpg";

                // Load the texture from disk
                if (path.EndsWith(".astc", StringComparison.OrdinalIgnoreCase))
                {
                    fileData = File.ReadAllBytes(path);

                    // Check for 16 bytes header. Skip if needed.  https://github.com/ARM-software/astc-encoder/blob/main/Docs/FileFormat.md
                    if (!StartsWithMagicNumber(fileData, astcMagicNumber))
                    {
                        ConfigManager.WriteConsoleError($"[CabinetTextureCache.LoadAndCacheTexture] {path} is a valid ASTC texture.");
                        throw new IOException();
                    }
                    int width = fileData[7] | (fileData[8] << 8) | (fileData[9] << 16);
                    int height = fileData[10] | (fileData[11] << 8) | (fileData[12] << 16);
                    ConfigManager.WriteConsole($"[CabinetTextureCache.LoadAndCacheTexture] {path} texture size:{width}x{height}");
                    tex = new Texture2D(width, height, TextureFormat.ASTC_6x6, false, true);
                    tex.filterMode = FilterMode.Trilinear; //provides better mip transitions in VR
                    tex.mipMapBias = -0.3f; // setting mip bias to around -0.7 in Unity is recommended by meta for high-detail textures
                    tex.LoadRawTextureData(fileData);
                    tex.Apply(true, true);
                    sizeBytes = tex.width * tex.height * (16f / 36f); // 16 bytes per 6x6 block (~0.4444 bytes/pixel)
                    ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path} is an alread transformed RGB texture:{tex.width}x{tex.height}\n scaled to size: {sizeBytes} bytes.");
                }
#if RESCALED_TEXTURE_LOAD 
                else if (File.Exists(processedPath))
                {                 
                    // Load the processed image if it exists
                    fileData = File.ReadAllBytes(processedPath);
                    tex = new Texture2D(1, 1, TextureFormat.RGBA4444, true); // Initial size doesn't matter, LoadImage will override
                    tex.LoadImage(fileData); // Load processed image
                    tex.filterMode = FilterMode.Trilinear;
                    tex.mipMapBias = -0.3f;
                    tex.Apply(true, true);
                    sizeBytes = tex.width * tex.height * 2f; // RGBA4444: 16 bits (2 bytes) per pixel
                    ConfigManager.WriteConsole($"[LoadAndCacheTexture] Loaded pre-processed texture from {processedPath}\n size: {sizeBytes} bytes.\n format: {tex.format.ToString()}");
                }
#endif
                else
                {

                    // Load and process the original image
                    fileData = File.ReadAllBytes(path);
                    
                    // Create and load texture with original data, ensuring it's readable
                    Texture2D texTmp = new Texture2D(2, 2, TextureFormat.RGBA4444, true, false); // mipmaps = true, linear = false (default sRGB)
                    //
                    texTmp.LoadImage(fileData); // Single LoadImage call to load the image (keeps it readable)
                    ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}: Original format: {texTmp.format.ToString()} - {texTmp.width}x{texTmp.height} size:{CalculateManualSizeBytes(texTmp)}");
                    if (SystemInfo.SupportsTextureFormat(TextureFormat.RGB565) )
                    {
                        tex = gpuRgb565Converter.ConvertTextureToRgb565Texture2DSync(texTmp);
                        if (tex == null)
                            tex = texTmp;
                        else
                            UnityEngine.Object.DestroyImmediate(texTmp);
                    }
                    else
                    {
                        tex = texTmp;
                    }

                    // Get original dimensions using the provided routine
                    //int originalWidth, originalHeight;
                    //GetImageDimensions(fileData, out originalWidth, out originalHeight);
                    // Calculate nearest lower power of 2 dimensions

                    // Check if the image has transparency using a Burst job
                    //bool hasTransparency = HasTransparency(tex);
#if RESCALE
                    int width = Mathf.FloorToInt(Mathf.Log(tex.width, 2));
                    int height = Mathf.FloorToInt(Mathf.Log(tex.height, 2));
                    int newWidth = (int)Mathf.Pow(2, width);
                    int newHeight = (int)Mathf.Pow(2, height);
                    if (tex.width != newWidth || tex.height != newHeight || tex.format != TextureFormat.RGBA4444)
                    {
                        // Step 1: Get original pixel data into a NativeArray for Burst
                        Color[] originalPixelsArray = tex.GetPixels();
                        using (var originalPixels = new NativeArray<Color>(originalPixelsArray, Allocator.TempJob))
                        using (var scaledPixels = new NativeArray<Color>(newWidth * newHeight, Allocator.TempJob))
                        {
                            // Step 2: Schedule the Burst-compiled job
                            var scaleJob = new ScalePixelsJob
                            {
                                OriginalPixels = originalPixels,
                                ScaledPixels = scaledPixels,
                                OldWidth = tex.width,
                                OldHeight = tex.height,
                                NewWidth = newWidth,
                                NewHeight = newHeight
                            };
                            JobHandle jobHandle = scaleJob.Schedule();
                            jobHandle.Complete(); // Wait for the job to finish (synchronous for simplicity)

                            // Step 3: Reinitialize and apply scaled pixels
                            if (tex.Reinitialize(newWidth, newHeight, TextureFormat.RGBA4444, true))
                            {
                                tex.SetPixels(scaledPixels.ToArray());
                                tex.Apply(true, false); // Update mipmaps, keep readable for now
                                ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}: rescaled to format: {tex.format.ToString()} new size: {tex.width}x{tex.height}");
#if RESCALED_TEXTURE_SAVE

                                byte[] pngData = tex.EncodeToJPG(95);
                                File.WriteAllBytes(processedPath, pngData);
#endif
                            }
                            else
                            {
                                ConfigManager.WriteConsoleWarning($"[LoadAndCacheTexture] {path}: reinitialize failed");
                            }
                        }
                    }
#endif
                    /*
                    Texture2D texConverted = ConvertIfAlphaUnused(tex);
                    if (texConverted != null)
                    {
                        UnityEngine.Object.Destroy(tex);
                        tex = texConverted;
                    }
                    */

                    tex.filterMode = FilterMode.Trilinear; // Provides better mip transitions in VR
                    tex.mipMapBias = -0.3f; // Recommended by Meta for high-detail textures
                    tex.Apply(true, true); // Final apply with mipmaps, now safe to make non-readable

                    // Calculate size based on final format
                    sizeBytes = CalculateManualSizeBytes(tex);

                    ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}: FINAL format: {tex.format.ToString()} - {tex.width}x{tex.height} size: {sizeBytes} Bytes");

#if FORCE_565
                    if (tex.format != TextureFormat.RGB565 && !path.ContainsInsensitive("bezel")) // quick fix for bezel textures
                    {
                        ConfigManager.WriteConsole($"Convert texture to RGB565: {path} -> format:{tex.format}");
                        // Convert the texture to RGB565
                        Texture2D rgb565Tex = new Texture2D(tex.width, tex.height, TextureFormat.RGB565, true);
                        Color32[] pixels = tex.GetPixels32();
#if GAMMA_FIX
                        ConfigManager.WriteConsole($"Correct GAMMA: {path}");
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            Color32 pixel = pixels[i];
                            // Apply gamma correction (factor 2)
                            pixel.r = (byte)Mathf.Clamp(Mathf.Pow(pixel.r / 255.0f, 2.0f) * 255.0f, 0.0f, 255.0f);
                            pixel.g = (byte)Mathf.Clamp(Mathf.Pow(pixel.g / 255.0f, 2.0f) * 255.0f, 0.0f, 255.0f);
                            pixel.b = (byte)Mathf.Clamp(Mathf.Pow(pixel.b / 255.0f, 2.0f) * 255.0f, 0.0f, 255.0f);
                            pixels[i] = pixel;
                        }
#endif
                        rgb565Tex.SetPixels32(pixels);
                        rgb565Tex.Apply(true);
                        UnityEngine.Object.Destroy(tex);
                        tex = rgb565Tex;
                    }
#endif
                }

#if TEXTURE_DEBUG
                ConfigManager.WriteConsole($"TEXTURESPECS: {path} -> format:{tex.format}  rawTextureDataLength:{tex.GetRawTextureData().Length}  w/h:{tex.width}x{tex.height}");
#endif
                // Cache the loaded texture
                CachedTextures.Add(path, tex, sizeBytes / (1024f * 1024f));
                return tex;
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"[CabinetTextureCache.LoadAndCacheTexture] {path}.", e);
                return null;
            }
        }
        return GetCachedTexture(path);
    }

    // Helper for fallback (less accurate)
    public static float CalculateManualSizeBytes(Texture2D tex)
    {
        // This is a rough estimate and doesn't handle compressed formats well
        switch (tex.format)
        {
            case TextureFormat.RGB24: return tex.width * tex.height * 3;
            case TextureFormat.RGBA32: return tex.width * tex.height * 4;
            case TextureFormat.ARGB32: return tex.width * tex.height * 4;
            case TextureFormat.RGB565: return tex.width * tex.height * 2;
            case TextureFormat.RGBA4444: return tex.width * tex.height * 2;
            // Add other formats as needed, but this gets complex for compressed ones
            default:
                //Debug.LogWarning($"CalculateManualSizeBytes: Unhandled format {tex.format}. Returning rough estimate.");
                // Very rough guess for others (often 4 bytes uncompressed)
                return tex.width * tex.height * 4;
        }
    }

    public static Texture2D ConvertIfAlphaUnused(Texture2D inputTexture, byte alphaThreshold = 255)
    {

        // GetPixels/SetPixels requires the texture to be readable
        if (!inputTexture.isReadable)
        {
            ConfigManager.WriteConsoleError($"[ConvertIfAlphaUnused] Input texture '{inputTexture.name}' is not readable. Cannot process pixels.");
            return null; // Indicate failure clearly
        }

        // 1. Check if the format even supports an alpha channel
        bool formatHasAlpha = GraphicsFormatUtility.HasAlphaChannel(inputTexture.graphicsFormat);

        if (!formatHasAlpha)
        {
            // Debug.Log($"[ConvertIfAlphaUnused] Texture '{inputTexture.name}' format ({inputTexture.format}) does not have alpha. No conversion needed.");
            return null; // No alpha channel in format
        }

        // 2. Format has alpha, now check the actual pixel data
        // Debug.Log($"[ConvertIfAlphaUnused] Texture '{inputTexture.name}' format ({inputTexture.format}) has alpha. Checking pixel data...");

        Color32[] pixels;
        try
        {
            pixels = inputTexture.GetPixels32(); // Use Color32 for direct byte access to alpha
        }
        catch (UnityException ex)
        {
            ConfigManager.WriteConsoleError($"[ConvertIfAlphaUnused] Failed to GetPixels32 for texture '{inputTexture.name}': {ex.Message}. Texture might be too large or in an unsupported format for GetPixels32.");
            // Decide how to handle: return original? return null?
            // Returning original might be safer if the process fails.
            return null;
        }


        bool alphaIsUsed = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            // Check if any pixel's alpha is below the threshold (e.g., not 255)
            if (pixels[i].a < alphaThreshold) // Common case: Check if not fully opaque
            {
                alphaIsUsed = true;
                break; // Found a used alpha value, no need to check further
            }
        }

        // 3. Decide action based on pixel check
        if (alphaIsUsed)
        {
            // Alpha channel contains transparency data. Keep the original texture.
            // Debug.Log($"[ConvertIfAlphaUnused] Texture '{inputTexture.name}' uses its alpha channel. No conversion needed.");
            return null;
        }
        else
        {
            // Alpha channel exists in the format, but all pixels are opaque (or above threshold). Convert to RGB24.
            Debug.Log($"[ConvertIfAlphaUnused] Texture '{inputTexture.name}' has an alpha channel, but it's unused (all alpha >= {alphaThreshold}). Converting to RGB24.");

            try
            {
                // --- Conversion logic (same as EnsureRGB24 before) ---
                bool createMipmaps = inputTexture.mipmapCount > 1;
                Texture2D rgbTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.RGB24, createMipmaps, false); // false = sRGB

                // SetPixels32 correctly ignores the source alpha when destination is RGB24
                rgbTexture.SetPixels32(pixels); // Reuse the pixels we already read

                // Apply changes. Generate mipmaps if the original had them. Make readable for now.
                rgbTexture.Apply(createMipmaps, false); // makeNoLongerReadable = false

                // Copy relevant properties
                rgbTexture.name = inputTexture.name + "_RGB24_Converted";
                rgbTexture.filterMode = inputTexture.filterMode;
                rgbTexture.wrapMode = inputTexture.wrapMode;
                rgbTexture.anisoLevel = inputTexture.anisoLevel;
                rgbTexture.mipMapBias = inputTexture.mipMapBias;

                // Debug.Log($"[ConvertIfAlphaUnused] Conversion complete for '{inputTexture.name}'. New texture '{rgbTexture.name}' created.");

                // IMPORTANT: Caller is responsible for Destroying the original inputTexture if this new one is used.
                return rgbTexture;
            }
            catch (UnityException ex)
            {
                Debug.LogError($"[ConvertIfAlphaUnused] Error converting texture '{inputTexture.name}' to RGB24: {ex.Message}");
                return null; // Indicate conversion failure
            }
        }
    }


    static void GetImageDimensions(byte[] fileData, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (fileData == null || fileData.Length < 24)
            throw new System.Exception("Image data too short to parse dimensions");

        // PNG check
        if (fileData[0] == 0x89 && fileData[1] == 0x50 && fileData[2] == 0x4E && fileData[3] == 0x47)
        {
            width = (fileData[16] << 24) | (fileData[17] << 16) | (fileData[18] << 8) | fileData[19];
            height = (fileData[20] << 24) | (fileData[21] << 16) | (fileData[22] << 8) | fileData[23];
            return;
        }

        // JPEG check
        if (fileData[0] == 0xFF && fileData[1] == 0xD8)
        {
            int i = 2;
            while (i < fileData.Length - 9)
            {
                if (fileData[i] != 0xFF)
                {
                    i++;
                    continue;
                }

                byte marker = fileData[i + 1];
                if (marker == 0xC0 || marker == 0xC2) // SOF0 or SOF2 markers
                {
                    height = (fileData[i + 5] << 8) | fileData[i + 6];
                    width = (fileData[i + 7] << 8) | fileData[i + 8];
                    return;
                }

                int blockLength = (fileData[i + 2] << 8) | fileData[i + 3];
                i += blockLength + 2;
            }
            // If valid JPEG but no dimensions found, fall through to LoadImage
        }

        // Fallback for other formats
        Texture2D temp = new Texture2D(2, 2);
        try
        {
            temp.LoadImage(fileData);
            width = temp.width;
            height = temp.height;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(temp);
        }
    }
    // Helper function to run the transparency check with Burst
    static bool HasTransparency(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        using (var pixelData = new NativeArray<Color>(pixels, Allocator.TempJob))
        using (var result = new NativeArray<bool>(1, Allocator.TempJob))
        {
            var transparencyJob = new HasTransparencyJob
            {
                Pixels = pixelData,
                Result = result
            };

            JobHandle jobHandle = transparencyJob.Schedule();
            jobHandle.Complete(); // Wait for the job to finish

            return result[0];
        }
    }

    private static bool StartsWithMagicNumber(byte[] byteArray, byte[] magicNumber)
    {
        if (byteArray == null || byteArray.Length < magicNumber.Length)
        {
            return false;
        }

        for (int i = 0; i < magicNumber.Length; i++)
        {
            if (byteArray[i] != magicNumber[i])
            {
                return false;
            }
        }

        return true;
    }

    public static void InvalidateCachedTexture(string path)
    {
        if (CachedTextures == null) return;

        CachedTextures.Remove(path);
    }

    // Method to retrieve a cached texture
    public static Texture2D GetCachedTexture(string path)
    {
        if (CachedTextures == null) return null;
        return CachedTextures.Get(path);
    }

    public static bool IsTextureCached(string path)
    {
        if (CachedTextures == null)
            return false;
        return CachedTextures.ContainsKey(path);
    }
}

// Burst-compiled job for scaling pixels
[BurstCompile]
public struct ScalePixelsJob : IJob
{
    [ReadOnly]
    public NativeArray<Color> OriginalPixels;
    public NativeArray<Color> ScaledPixels;
    public int OldWidth;
    public int OldHeight;
    public int NewWidth;
    public int NewHeight;

    public void Execute()
    {
        for (int y = 0; y < NewHeight; y++)
        {
            for (int x = 0; x < NewWidth; x++)
            {
                // Map new coordinates to old texture
                float u = (float)x / (NewWidth - 1) * (OldWidth - 1);
                float v = (float)y / (NewHeight - 1) * (OldHeight - 1);

                int x0 = (int)Mathf.Floor(u);
                int y0 = (int)Mathf.Floor(v);
                int index = y0 * OldWidth + x0;

                // Ensure no out-of-bounds access
                if (x0 >= 0 && x0 < OldWidth && y0 >= 0 && y0 < OldHeight)
                {
                    ScaledPixels[y * NewWidth + x] = OriginalPixels[index];
                }
            }
        }
    }
}


// Burst-compiled job to detect transparency
[BurstCompile]
public struct HasTransparencyJob : IJob
{
    [ReadOnly]
    public NativeArray<Color> Pixels;
    public NativeArray<bool> Result; // Single-element array to store the result

    public void Execute()
    {
        for (int i = 0; i < Pixels.Length; i++)
        {
            if (Pixels[i].a < 1.0f) // Check if any pixel has alpha less than fully opaque
            {
                Result[0] = true;
                return; // Early exit once transparency is found
            }
        }
        Result[0] = false;
    }
}

