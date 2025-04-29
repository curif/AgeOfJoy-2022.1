using UnityEngine;
using UnityEngine.Rendering; // Required for AsyncGPUReadback if you switch later
using System.Collections.Generic; // For caching RTs
using System; // For Exception

public static class GpuAlphaCheck
{
    private static Material checkMaterial = null;
    private static Texture2D resultTex = null; // Still needed for GetPixel
    private const string CHECK_SHADER_NAME = "Hidden/CheckAlpha";
    //private static RenderTexture tempRT = null;

    // Cache 1x1 RT, no need for dictionary
    //private static RenderTexture checkTempRT = null;
    
    //private const string SHADER_NAME = "Hidden/CheckAlpha";

    // --- Resources for Remove Alpha ---
    private static Material copyMaterial = null;
    private const string COPY_SHADER_NAME = "Hidden/CopyRGB";
    // Cache temporary RenderTextures by size to reuse them
    //private static Dictionary<Vector2Int, RenderTexture> tempRTs = new Dictionary<Vector2Int, RenderTexture>();

    /// <summary>
    /// Checks synchronously if ANY pixel in the source texture has an alpha value < 1.0f (i.e., not fully opaque).
    /// Uses GPU acceleration via Blit and a small readback.
    /// </summary>
    /// <param name="sourceTexture">The texture to check. Must be readable.</param>
    /// <returns>True if any pixel has transparency, false if all pixels are fully opaque (alpha=1.0), false on error.</returns>
    public static bool HasAnyTransparencyGpu(Texture sourceTexture)
    {
        if (sourceTexture == null)
        {
            ConfigManager.WriteConsoleError("HasAnyTransparencyGpu: Source texture is null.");
            return false; // Or throw? Assuming opaque is safer maybe?
        }

        // Ensure check resources are initialized
        if (!InitializeCheckResources())
        {
            ConfigManager.WriteConsoleError("HasAnyTransparencyGpu: Failed to initialize alpha check resources.");
            return false; // Assume opaque on error? Or transparent? Let's assume opaque.
        }

        // Sanity check for readability - Blit requires it!
        if (!sourceTexture.isReadable)
        {
            ConfigManager.WriteConsoleError($"HasAnyTransparencyGpu: Source texture '{sourceTexture.name}' is not readable!");
            // Fail safe: Assume it might have transparency if we can't read it? Or assume opaque?
            // Let's assume opaque if unreadable, as we can't process it.
            return false;
        }


        RenderTexture tempRT = null; // Temporary RT for the check result
        bool hasTransparency = false;

        try
        {
            // 1. Get a 1x1 temporary RT
            tempRT = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear); // Linear should be fine for alpha check

            // 2. Set the threshold in the material to 1.0f (for the 'alpha < 1.0' check)
            checkMaterial.SetFloat("_AlphaThreshold", 1.0f);

            // 3. Perform the Blit
            Graphics.Blit(sourceTexture, tempRT, checkMaterial);

            // 4. Copy the 1x1 result to our reusable Texture2D
            Graphics.CopyTexture(tempRT, 0, 0, resultTex, 0, 0);

            // 5. Read the single pixel result from the Texture2D
            Color resultPixel = resultTex.GetPixel(0, 0);

            // 6. Check if the result is non-black.
            // If R > 0, it means at least one source pixel had alpha < 1.0f.
            // Use a small tolerance for floating point comparison.
            hasTransparency = resultPixel.r > 0.5f;

            // --- Debugging ---
            ConfigManager.WriteConsole($"HasAnyTransparencyGpu Check for '{sourceTexture.name}': Result Pixel R={resultPixel.r}, HasTransparency={hasTransparency}");

        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"HasAnyTransparencyGpu: Error during GPU check for '{sourceTexture.name}'", e);
            // Fail safe: If check fails, maybe assume it DOES have transparency to be safe?
            // This prevents accidentally removing an alpha channel needed due to a check error.
            hasTransparency = true;
        }
        finally
        {
            // Cleanup the temporary RT
            if (tempRT != null)
            {
                RenderTexture.ReleaseTemporary(tempRT);
            }
        }

        return hasTransparency;
    }

    // --- New GPU Accelerated Remove Alpha Method ---
    public static Texture2D RemoveAlphaGpu(Texture2D sourceTex)
    {
        if (sourceTex == null)
        {
            ConfigManager.WriteConsoleError("RemoveAlphaGpu: Source texture is null.");
            return null;
        }

        // Ensure copy resources are initialized
        if (!InitializeCopyResources())
        {
            ConfigManager.WriteConsoleError("RemoveAlphaGpu: Failed to initialize copy resources.");
            return null; // Or fallback to CPU version?
        }

        RenderTexture tempRT = null;
        Texture2D rgbTexture = null;
        RenderTexture previousActive = RenderTexture.active; // Store current active RT

        try
        {
            // Get a temporary RenderTexture matching source dimensions
            Vector2Int size = new Vector2Int(sourceTex.width, sourceTex.height);
            tempRT = GetTemporaryRT(size);
            if (tempRT == null)
            {
                ConfigManager.WriteConsoleError("RemoveAlphaGpu: Failed to get temporary RenderTexture.");
                return null;
            }

            // Perform the Blit on the GPU: sourceTex -> tempRT using copy shader
            // sourceTex MUST be readable for Blit to work.
            Graphics.Blit(sourceTex, tempRT, copyMaterial);

            // Create the destination Texture2D (RGB24 format, with mipmaps enabled)
            // Setting linear = false means sRGB color space (standard for color textures)
            rgbTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGB24, true, false);

            // Copy the GPU Render Texture content to the GPU Texture2D content
            // This is a fast GPU-to-GPU copy, avoiding CPU readback/upload.
            // Copies mip level 0 from source RT to mip level 0 of dest texture.
            Graphics.CopyTexture(tempRT, 0, 0, rgbTexture, 0, 0);

            // --- Apply settings and generate Mipmaps ---
            rgbTexture.filterMode = FilterMode.Trilinear; // Provides better mip transitions in VR
            rgbTexture.mipMapBias = -0.5f; // Recommended by Meta for high-detail textures

            // Apply needed to:
            // 1. Generate Mipmaps (since we created Texture2D with mipmaps=true)
            // 2. Make the texture non-readable (optional but recommended for cache)
            // Although we didn't use SetPixels, Apply(true, true) is the standard way
            // to trigger mip generation after modifying the base level (e.g., via CopyTexture)
            // and mark it as non-readable for performance.
            rgbTexture.Apply(true, true);

            return rgbTexture;
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleError($"RemoveAlphaGpu: Error during GPU alpha removal. {e.Message}\n{e.StackTrace}");
            // Clean up partially created texture if error occurred
            if (rgbTexture != null) UnityEngine.Object.DestroyImmediate(rgbTexture);
            return null; // Indicate failure
        }
        finally
        {
            // Restore previous active RT
            RenderTexture.active = previousActive;
            // Release the temporary RT back to our cache/pool (or destroy if not caching)
            ReleaseTemporaryRT(tempRT);
        }
    }


    // --- Resource Initialization and Management ---
    // Helper to initialize resources specific to alpha check
    private static bool InitializeCheckResources()
    {
        // Only initialize if needed
        bool resourcesExist = checkMaterial != null && resultTex != null;
        if (resourcesExist) return true;

        try
        {
            // Initialize Material
            if (checkMaterial == null)
            {
                Shader checkShader = Shader.Find(CHECK_SHADER_NAME);
                if (checkShader == null) throw new System.Exception($"Shader '{CHECK_SHADER_NAME}' not found.");
                checkMaterial = new Material(checkShader) { hideFlags = HideFlags.HideAndDontSave };
            }

            // Initialize resultTex (1x1, readable, ARGB32)
            if (resultTex == null)
            {
                resultTex = new Texture2D(1, 1, TextureFormat.ARGB32, false, true); // Linear = true often better for data
                resultTex.hideFlags = HideFlags.HideAndDontSave;
            }

            // ConfigManager.WriteConsole("GpuAlphaCheck: Check Resources Initialized");
            return true;
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleError($"GpuAlphaCheck: Failed to initialize check resources. {e.Message}");
            ReleaseCheckResources(); // Clean up partials
            return false;
        }
    }


    // Helper to initialize resources specific to copy/removeAlpha
    private static bool InitializeCopyResources()
    {
        if (copyMaterial != null) return true; // Only need material statically

        try
        {
            Shader copyShader = Shader.Find(COPY_SHADER_NAME);
            if (copyShader == null) throw new System.Exception($"Shader '{COPY_SHADER_NAME}' not found.");
            copyMaterial = new Material(copyShader) { hideFlags = HideFlags.HideAndDontSave };

            ConfigManager.WriteConsole("GpuAlphaCheck: Copy Resources Initialized");
            return true;
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleError($"GpuAlphaCheck: Failed to initialize copy resources. {e.Message}");
            ReleaseCopyResources(); // Clean up partials
            return false;
        }
    }

    // Get a temporary RT from Unity's pool (often better than manual caching)
    private static RenderTexture GetTemporaryRT(Vector2Int size)
    {
        // ARGB32 is a safe default. No depth buffer needed.
        // Using Unity's temporary system is generally preferred.
        return RenderTexture.GetTemporary(size.x, size.y, 0, RenderTextureFormat.ARGB32);
    }

    // Release temporary RT back to Unity's pool
    private static void ReleaseTemporaryRT(RenderTexture rt)
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
        }
    }


    // Call this manually if needed (e.g., OnDestroy, OnApplicationQuit)
    public static void ReleaseAllResources()
    {
        ReleaseCheckResources();
        ReleaseCopyResources();

        ConfigManager.WriteConsole("GpuAlphaCheck: All Resources Released");
    }

    // Keep ReleaseCheckResources and ReleaseAllResources as before...
    private static void ReleaseCheckResources()
    {
        if (checkMaterial != null) { UnityEngine.Object.DestroyImmediate(checkMaterial); checkMaterial = null; }
        if (resultTex != null) { UnityEngine.Object.DestroyImmediate(resultTex); resultTex = null; }
    }
    private static void ReleaseCopyResources()
    {
        if (copyMaterial != null) { UnityEngine.Object.DestroyImmediate(copyMaterial); copyMaterial = null; }
        // Release cached RTs if using manual cache
    }
   

}