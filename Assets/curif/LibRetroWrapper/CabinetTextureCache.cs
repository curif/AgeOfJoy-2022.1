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
using UnityEngine.Networking;  // for UnityWebRequest & DownloadHandlerTexture
using System.Collections; 

public static class CabinetTextureCache
{

    private static byte[] astcMagicNumber = new byte[] { 0x13, 0xAB, 0xA1, 0x5C };

    private static ResourceCache<string, Texture2D> CachedTextures = null;
    private static GpuRgb565Converter gpuRgb565Converter = null;

    //convert textures > X mb
    private const float ORIGINAL_SIZE_THRESHOLD = 5f * 1024 * 1024;
    public const float CACHE_SIZE = 1024;
    public const float CACHE_SIZE_Q3 = 1536f;

    public static IEnumerator LoadAndCacheAsync(string path, Action<Texture2D> onComplete)
    {
        if (IsTextureCached(path))
        {
            onComplete?.Invoke(GetCachedTexture(path));
            yield break;
        }

        if (CachedTextures == null)
        {
            // lazy creation:
            float cacheSize = CACHE_SIZE;
            if (DeviceController.IsQ3)
                cacheSize = CACHE_SIZE_Q3;
            CachedTextures = ResourceCacheManager.Create<string, Texture2D>("texturesCache", cacheSize);
            ConfigManager.WriteConsole($"[LoadAndCacheAsync] created cache textures size: {cacheSize}");
        }

        //load file
        using (var www = UnityWebRequestTexture.GetTexture("file://" + path, /*nonReadable=*/ true))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                ConfigManager.WriteConsoleError($"Failed to load {path}: {www.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            // Grab the decoded RGBA32 texture
            Texture2D texTmp = DownloadHandlerTexture.GetContent(www);
            texTmp.name = path;
            texTmp.filterMode = FilterMode.Trilinear;
            texTmp.mipMapBias = -0.3f;

            //size check
            float originalSizeInBytes = CalculateManualSizeBytes(texTmp);
            //bool useOriginal = originalSizeInBytes < ORIGINAL_SIZE_THRESHOLD || DeviceController.originalTextures;

            /*
             * The conversion to RGB565 wasn't possible for
             * Meta Quest. Even when the docs
             * says it suport RGB565 the shader conversion didn't 
             * work as expected and the GPU was 
             * blocked
             */
            bool useOriginal = true;
            if (useOriginal)
            {
                ConfigManager.WriteConsole($"[LoadAndCacheAsync] useOriginal by size {originalSizeInBytes} or player conf {path}");

                texTmp.name = "ORIGINALBYSIZE-" + path;
                Texture2D cached = CachedTextures.Add(path, texTmp, originalSizeInBytes / (1024f * 1024f));
                if (cached != texTmp)
                {
                    UnityEngine.Object.Destroy(texTmp);
                    texTmp = cached;
                }
                onComplete?.Invoke(texTmp);
                yield break;
            }

            yield return null;

            bool hasAlphaUsed = false; // Default assumption or based on format first?
            if (GraphicsFormatUtility.HasAlphaChannel(texTmp.graphicsFormat))
            {
                // Use the GPU check instead of GetPixels32 + CPU loop or Job
                // Note: texTmp MUST be readable for Graphics.Blit to work correctly from it.
                // LoadImage already makes it readable, so we should be okay here.
                // If texTmp could be non-readable here, you'd need to handle that.
                try
                {

                    //hasAlphaUsed = GpuAlphaCheck.HasAnyTransparencyGpu(texTmp);
                    hasAlphaUsed = GpuAlphaCheckCompute.HasAnyTransparencyComputeSync(texTmp);

                    /* has alpha check
                    Color32[] pixels = texTmp.GetPixels32();
                    bool hasAlphaUsedToCheck = IsAlphaUsed(pixels);
                    if (hasAlphaUsed != hasAlphaUsedToCheck)
                        throw new Exception($">>>>>>>>>>>>>>>> ERROR alpha analysis <<<<<<<<<<<<<<<<<<< format: {texTmp.format} HasAnyTransparencyGpu: {hasAlphaUsed} IsAlphaUsed: {hasAlphaUsedToCheck} {path}");
                    */
                    ConfigManager.WriteConsole($"[LoadAndCacheAsync] {path}: GPU Alpha Check Result: {hasAlphaUsed}");
                }
                catch (System.Exception gpuCheckError)
                {
                    ConfigManager.WriteConsoleError($"[LoadAndCacheAsync] {path}: GPU Alpha Check texfailed: {gpuCheckError.Message}. Assuming alpha is used.");
                    hasAlphaUsed = true; // Fail safe: assume alpha is used if check fails
                }
            }

            if (hasAlphaUsed)
            {
                ConfigManager.WriteConsole($"[LoadAndCacheAsync] useOriginal hasAlphaUsed {path}");

                texTmp.name = "ALPHA-" + path;
                Texture2D cached = CachedTextures.Add(path, texTmp, originalSizeInBytes / (1024f * 1024f));
                if (cached != texTmp)
                {
                    UnityEngine.Object.Destroy(texTmp);
                    texTmp = cached;
                }
                onComplete?.Invoke(texTmp);

                yield break;
            }

            if (gpuRgb565Converter == null)
            {
                var go = GameObject.Find("FixedObject");
                gpuRgb565Converter = go.GetComponent<GpuRgb565Converter>();
            }

            Texture2D finalTex = null;
            bool done = false;
            gpuRgb565Converter.ConvertTextureToRgb565Texture2DAsync(texTmp, (converted) => {
                finalTex = converted;
                done = true;
            });

            // Wait until the async readback finishes
            yield return new WaitUntil(() => done);

            //finalTex = gpuRgb565Converter.ConvertToRgb565(texTmp);         // picks copy‐vs‐sync for you
            UnityEngine.Object.Destroy(texTmp);

            //yield return null;

            // Cache it
            finalTex.name = "RGB565-" + path;
            float sizeMB = CalculateManualSizeBytes(finalTex) / (1024f * 1024f);
            CachedTextures.Add(path, finalTex, sizeMB);

            // And hand it back
            onComplete?.Invoke(finalTex);
        }
    }
    /*
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

                    texTmp.LoadImage(fileData); // Single LoadImage call to load the image (keeps it readable)
                    float originalSizeInBytes = CalculateManualSizeBytes(texTmp);
                    ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}: Original format: {texTmp.format.ToString()} - {texTmp.width}x{texTmp.height} size:{originalSizeInBytes}");

                    bool useOriginal = originalSizeInBytes < ORIGINAL_SIZE_THRESHOLD || DeviceController.originalTextures;
                    if (useOriginal)
                    {
                        ConfigManager.WriteConsole($"[LoadAndCacheTexture] useOriginal {path}");

                        tex = texTmp;
                        keepOriginalForVR(tex, path);

                        CachedTextures.Add(path, tex, originalSizeInBytes / (1024f * 1024f));
                        return tex;
                    }

                    bool hasAlphaUsed = false; // Default assumption or based on format first?
                    if (GraphicsFormatUtility.HasAlphaChannel(texTmp.graphicsFormat))
                    {
                        // Use the GPU check instead of GetPixels32 + CPU loop or Job
                        // Note: texTmp MUST be readable for Graphics.Blit to work correctly from it.
                        // LoadImage already makes it readable, so we should be okay here.
                        // If texTmp could be non-readable here, you'd need to handle that.
                        try
                        {
                            //hasAlphaUsed = GpuAlphaCheck.HasAnyTransparencyGpu(texTmp);
                            hasAlphaUsed = GpuAlphaCheckCompute.HasAnyTransparencyComputeSync(texTmp);
                            
                            // has alpha check
                            //Color32[] pixels = texTmp.GetPixels32();
                            //bool hasAlphaUsedToCheck = IsAlphaUsed(pixels);
                            //if (hasAlphaUsed != hasAlphaUsedToCheck)
                            //    throw new Exception($">>>>>>>>>>>>>>>> ERROR alpha analysis <<<<<<<<<<<<<<<<<<< format: {texTmp.format} HasAnyTransparencyGpu: {hasAlphaUsed} IsAlphaUsed: {hasAlphaUsedToCheck} {path}");
                            //
                            ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}: GPU Alpha Check Result: {hasAlphaUsed}");
                        }
                        catch (System.Exception gpuCheckError)
                        {
                            ConfigManager.WriteConsoleError($"[LoadAndCacheTexture] {path}: GPU Alpha Check texfailed: {gpuCheckError.Message}. Assuming alpha is used.");
                            hasAlphaUsed = true; // Fail safe: assume alpha is used if check fails
                        }
                    }
                    else
                    {
                        ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}: Format {texTmp.graphicsFormat} has no alpha channel.");
                        hasAlphaUsed = false;
                    }

                    // SystemInfo.SupportsTextureFormat(TextureFormat.RGB565)
                    if (!hasAlphaUsed)
                    {
                        //tex = gpuRgb565Converter.ConvertTextureToRgb565Texture2DSync(texTmp);
                        //tex = gpuRgb565Converter.ConvertRgb565ViaCopy(texTmp); //must be supported by the platform. Windows for example can't support this format.
                        tex = gpuRgb565Converter.ConvertToRgb565(texTmp);
                        
                        if (tex == null)
                        {
                            // no RGB565 conversion, no alpha -> delete alpha channel.
                            //tex = removeAlpha(path, texTmp, pixels);
                            Texture2D convertedTex = GpuAlphaCheck.RemoveAlphaGpu(texTmp);
                            if (convertedTex != null)
                            {
                                // Success! Destroy the temporary source texture
                                UnityEngine.Object.DestroyImmediate(texTmp);
                                tex = convertedTex; // Assign the result
                                tex.name = "RGB24-" + path; // Set name
                                ConfigManager.WriteConsoleError($"[LoadAndCacheTexture] {path}:ConvertTextureToRgb565Texture2DSync texfailed and Removed alpha using GPU.");
                            }
                            else
                            {
                                ConfigManager.WriteConsoleError($"[LoadAndCacheTexture] not originalTextures {path}: ConvertTextureToRgb565Texture2DSync texfailed and GPU removeAlpha texfailed.");
                                tex = texTmp;
                                keepOriginalForVR(tex, path, "ERRGPU");
                            }
                        }
                        else
                        {
                            ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}:ConvertTextureToRgb565Texture2DSync ok.");
                            tex.name = "RGB565-" + path;
                            UnityEngine.Object.DestroyImmediate(texTmp);
                        }
                    }
                    else
                    {
                        ConfigManager.WriteConsole($"[LoadAndCacheTexture] not originalTextures {path}:  hasAlphaUsed back to original.");
                        tex = texTmp;
                        keepOriginalForVR(tex, path, "ALPHA");
                    }

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

                    // Calculate size based on final format
                    sizeBytes = CalculateManualSizeBytes(tex);

                    ConfigManager.WriteConsole($"[LoadAndCacheTexture] {path}: FINAL format: {tex.format.ToString()} - {tex.width}x{tex.height} size: {sizeBytes} Bytes");

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
    */

    private static void keepOriginalForVR(Texture2D tex, string path, string prefix = "ORIGINAL")
    {
        tex.name = prefix + "-" + path;

        tex.filterMode = FilterMode.Trilinear; // Provides better mip transitions in VR
        tex.mipMapBias = -0.5f; // Recommended by Meta for high-detail textures
        /*
         * normally LoadImage() on a texture constructed with mipmaps will auto-generate them (on most platforms), and so you don’t have to manually call Apply for that purpose. Indeed, Unity’s API specifically says after Texture2D.LoadImage(...), “Texture will be uploaded to the GPU automatically; there’s no need to call Apply.”​
.          In short: for a Texture2D created with mipmaps, calling LoadImage is enough to send it (and its mips) to the GPU. Changing filterMode or                 mipMapBias after that does not require another Apply – those are sampler settings that take effect immediately. You would only call Apply                   (true,true) if you had used SetPixels/SetPixelData on the CPU and want to regenerate mipmaps or to release the CPU copy. In typical dynamic-        load use (just loading a PNG/JPG byte array), an extra Apply isn’t necessary​
        */
        //tex.Apply(true, true);
    }
    private static Texture2D removeAlpha(string path, Texture2D texTmp, Color32[] pixels)
    {
        //remove alpha channel as it is unused
        Texture2D rgbTexture = new Texture2D(texTmp.width, texTmp.height, TextureFormat.RGB24, true, false); // false = sRGB

        // SetPixels32 correctly ignores the source alpha when destination is RGB24
        rgbTexture.SetPixels32(pixels); // Reuse the pixels we already read

        rgbTexture.name = "RGB24-" + path;
        rgbTexture.filterMode = FilterMode.Trilinear; // Provides better mip transitions in VR
        rgbTexture.mipMapBias = -0.5f; // Recommended by Meta for high-detail textures
        rgbTexture.Apply(true, true);
        return rgbTexture;
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

    public static bool IsAlphaUsed(Color32[] pixels, byte alphaThreshold = 255)
    {

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
        return alphaIsUsed;
    }
    // Helper function to run the transparency check with Burst
    static bool IsAlphaUsedJob(Color32[] pixels)
    {
        using (var pixelData = new NativeArray<Color32>(pixels, Allocator.TempJob))
        using (var result = new NativeArray<bool>(1, Allocator.TempJob))
        {
            var transparencyJob = new HasTransparencyJob
            {
                Pixels = pixelData,
                alphaThreshold = 255,
                Result = result
            };

            JobHandle jobHandle = transparencyJob.Schedule();
            jobHandle.Complete(); // Wait for the job to finish

            return result[0];
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
    public NativeArray<Color32> Pixels;
    public byte alphaThreshold;
    public NativeArray<bool> Result; // Single-element array to store the result

    public void Execute()
    {
        for (int i = 0; i < Pixels.Length; i++)
        {
            if (Pixels[i].a < alphaThreshold) // Check if any pixel has alpha less than fully opaque
            {
                Result[0] = true;
                return; // Early exit once transparency is found
            }
        }
        Result[0] = false;
    }
}

