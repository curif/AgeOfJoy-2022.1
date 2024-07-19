//#define FORCE_565
//#define GAMMA_FIX
//#define TEXTURE_DEBUG

using System;
using System.IO;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public static class CabinetTextureCache
{

    private static byte[] astcMagicNumber = new byte[] { 0x13, 0xAB, 0xA1, 0x5C };

    // Dictionary to store cached textures
    private static ResourceCache<string, Texture2D> cachedTextures = ResourceCacheManager.Create<string, Texture2D>();

    // Method to load and cache a texture
    public static Texture2D LoadAndCacheTexture(string path)
    {
        Texture2D tex = null;

        if (!IsTextureCached(path))
        {
            try
                {

                // Load the texture from disk
                byte[] fileData = File.ReadAllBytes(path);
                if (path.EndsWith(".astc", StringComparison.OrdinalIgnoreCase))
                {
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
                }
                else
                {
                    ConfigManager.WriteConsole($"[CabinetTextureCache.LoadAndCacheTexture] {path} is an RGB texture.");
                    tex = new Texture2D(2, 2, TextureFormat.RGB565, true);
                    tex.filterMode = FilterMode.Trilinear; //provides better mip transitions in VR
                    tex.mipMapBias = -0.3f; // setting mip bias to around -0.7 in Unity is recommended by meta for high-detail textures
                    tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                    tex.Apply(true, true);

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
                cachedTextures.Add(path, tex);
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
        cachedTextures.Remove(path);
    }

    // Method to retrieve a cached texture
    public static Texture2D GetCachedTexture(string path)
    {
        return cachedTextures.Get(path);
    }

    public static bool IsTextureCached(string path)
    {
        return cachedTextures.ContainsKey(path);
    }
}
