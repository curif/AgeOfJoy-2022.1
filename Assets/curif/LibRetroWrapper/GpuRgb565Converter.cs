using UnityEngine;
using UnityEngine.Rendering; // Required for RenderTextureFormat
using UnityEngine.Experimental.Rendering;
using System; // Required for Action
#if UNITY_EDITOR
using UnityEditor;
#endif
public class GpuRgb565Converter : MonoBehaviour
{
    // Assign in Inspector
    public Texture sourceTexture; // Default source if none provided to method
    public Shader conversionShader; // Assign the Rgb565PassThroughLinearCorrected shader

    // Cached resources
    private Material m_ConversionMaterial = null;

    void Awake()
    {
        // Pre-cache the material instance if the shader is assigned.
        InitializeMaterial();
    }

    void OnDestroy()
    {
        // Clean up the cached material when the component is destroyed.
        if (m_ConversionMaterial != null)
        {
            // Use DestroyImmediate if called from editor context (e.g., exiting play mode)
            // Use Destroy if called during regular runtime object destruction.
            // Checking Application.isEditor is a common way to handle this.
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(m_ConversionMaterial);
            else
                Destroy(m_ConversionMaterial);

            m_ConversionMaterial = null;
        }
    }

    public Texture2D ConvertToRgb565(Texture src)
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        // 1) First, check whether we can do a non-stalling GPU copy into RGB565:
        bool supportsRtToTex = (SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) != 0;
        bool supportsRgb565RT = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565);

        if (supportsRtToTex && supportsRgb565RT)
        {
            // On Quest (with Vulkan or GLES3.1+), this will be true:
            return ConvertRgb565ViaCopy(src);
        }
        else
        {
            // In the Editor on Windows, or on GLES3.0 devices, fall back to the sync path:
            return ConvertTextureToRgb565Texture2DSync(src);
        }
#else
        // All other platforms (including Windows Editor) always fall back:
        return ConvertTextureToRgb565Texture2DSync(src);
#endif
    }


    // Helper to initialize or re-initialize the material
    private bool InitializeMaterial()
    {
        if (m_ConversionMaterial != null && m_ConversionMaterial.shader == conversionShader)
        {
            return true; // Already initialized with the correct shader
        }

        // Clean up old material if shader changed
        if (m_ConversionMaterial != null)
        {
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(m_ConversionMaterial);
            else
                Destroy(m_ConversionMaterial);
            m_ConversionMaterial = null;
        }

        // Create new material
        if (conversionShader != null)
        {
            m_ConversionMaterial = new Material(conversionShader);
            m_ConversionMaterial.hideFlags = HideFlags.HideAndDontSave; // Prevent saving with scene
            return true;
        }
        else
        {
            ConfigManager.WriteConsoleError("[GpuRgb565Converter] conversionShader is not assigned. Cannot initialize material.");
            return false;
        }
    }
    public Texture2D ConvertRgb565ViaCopy(Texture src)
    {
        InitializeMaterial();

        // 1) Create an RTDesc that *allocates* mips, but *does not* auto-generate them
        var desc = new RenderTextureDescriptor(src.width, src.height,
                                               GraphicsFormat.R5G6B5_UNormPack16, 0)
        {
            useMipMap = true,   // allocate space for all mip levels
            autoGenerateMips = false,  // we’ll generate them manually
            sRGB = (QualitySettings.activeColorSpace != ColorSpace.Linear)
        };

        var rt = RenderTexture.GetTemporary(desc);

        // 2) Blit your source into level-0 of the RT
        Graphics.Blit(src, rt, m_ConversionMaterial);

        // 3) Manually regenerate the mips on the GPU
        //    (RenderTexture.GenerateMips is the instance method you call here)
        rt.GenerateMips();  // :contentReference[oaicite:0]{index=0}

        // 4) Create your Texture2D with mipChain=true
        bool isLinear = (QualitySettings.activeColorSpace == ColorSpace.Linear);
        var dst = new Texture2D(src.width, src.height,
                                TextureFormat.RGB565,
                                /*mipChain*/ true,
                                isLinear);

        // 5) Copy *all* levels (Unity will match mip counts automatically)
        Graphics.CopyTexture(rt, dst);  // GPU→GPU, non-blocking

        // 6) Upload the GPU texture into Unity and discard the CPU-side copy
        dst.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        RenderTexture.ReleaseTemporary(rt);
        return dst;
    }

    /// <summary>
    /// Converts the input texture to RGB565 format synchronously using the GPU, blocking until complete.
    /// Returns a NEW Texture2D object containing the RGB565 data.
    /// Optimizes resource usage by caching the material and using temporary RenderTextures.
    /// NOTE: This WILL cause a stall on the main thread while waiting for the GPU ReadPixels.
    /// </summary>
    /// <param name="sourceInputTexture">The texture to convert. If null, uses the component's sourceTexture field.</param>
    /// <returns>A new Texture2D containing the RGB565 pixel data, or null if an error occurred.</returns>
    public Texture2D ConvertTextureToRgb565Texture2DSync(Texture sourceInputTexture)
    {
        // --- Input Validation & Resource Setup ---
        if (sourceInputTexture == null)
        {
            sourceInputTexture = this.sourceTexture; // Fallback to public field
            if (sourceInputTexture == null)
            {
                ConfigManager.WriteConsoleError("[ConvertTextureToRgb565Texture2DSync] sourceInputTexture is null (both argument and component field).");
                return null;
            }
        }

        // Ensure the cached material is ready and matches the assigned shader
        if (!InitializeMaterial()) // Checks shader assignment and creates/updates material
        {
            return null; // Error already logged by InitializeMaterial
        }

        // --- Check Project Color Space (Good Practice) ---
        if (QualitySettings.activeColorSpace != ColorSpace.Linear)
        {
            ConfigManager.WriteConsoleWarning("[ConvertTextureToRgb565Texture2DSync] Project color space is not set to Linear. The shader expects Linear input for correct conversions.");
        }

        // --- Temporary GPU Resource ---
        RenderTexture tempRT = null;
        // --- Output Resource ---
        Texture2D resultTex = null;

        try
        {
            // 1. Get Temporary Render Texture (Use Pooling)
            // ARGB32 is generally safe and well-supported.
            RenderTextureFormat intermediateFormat = RenderTextureFormat.ARGB32;
            // Use linear RT in linear color space, sRGB in gamma space for intermediate steps.
            RenderTextureReadWrite readWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear)
                                                ? RenderTextureReadWrite.Linear
                                                : RenderTextureReadWrite.sRGB;

            ConfigManager.WriteConsole($"[ConvertTextureToRgb565Texture2DSync] Getting temporary {intermediateFormat} RT ({sourceInputTexture.width}x{sourceInputTexture.height}, {readWrite})...");
            tempRT = RenderTexture.GetTemporary(sourceInputTexture.width, sourceInputTexture.height, 0, intermediateFormat, readWrite);


            // 2. Perform GPU Conversion & Quantization (Blit) using cached material
            ConfigManager.WriteConsole("[ConvertTextureToRgb565Texture2DSync] Performing GPU conversion/quantization (Blit) into temporary RT...");
            Graphics.Blit(sourceInputTexture, tempRT, m_ConversionMaterial);


            // 3. Calculate Mipmap Count (Correct)
            int maxDim = Mathf.Max(sourceInputTexture.width, sourceInputTexture.height);
            int calculatedMipCount = Mathf.FloorToInt(Mathf.Log(maxDim, 2f)) + 1;
            calculatedMipCount = Mathf.Max(1, calculatedMipCount);


            // 4. Create the RESULT Texture2D (Target is RGB565)
            bool isLinear = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            ConfigManager.WriteConsole($"[ConvertTextureToRgb565Texture2DSync] Creating result Texture2D with format {TextureFormat.RGB565}, Mips: {calculatedMipCount}, Linear: {isLinear}");
            resultTex = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGB565, calculatedMipCount > 1, isLinear); // Mipmap bool simpler

            if (calculatedMipCount > 1)
            {
                resultTex.filterMode = FilterMode.Trilinear;
                resultTex.mipMapBias = -0.5f;
            }
            else
            {
                resultTex.filterMode = FilterMode.Bilinear;
                resultTex.mipMapBias = 0f;
            }

            // 5. Synchronous Readback (GPU->CPU->GPU for format packing)
            // ReadPixels handles the conversion from tempRT's format (ARGB32) to resultTex's format (RGB565).
            RenderTexture previousActiveRT = RenderTexture.active;
            RenderTexture.active = tempRT;
            ConfigManager.WriteConsole($"[ConvertTextureToRgb565Texture2DSync] Attempting synchronous ReadPixels from {intermediateFormat} RT into the result {resultTex.format} Texture2D...");

            resultTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0); // Reads into Texture2D CPU buffer

            // Apply needed to:
            // a) Upload the pixel data (now in RGB565 format) from CPU to the GPU texture object.
            // b) Generate Mipmaps if calculatedMipCount > 1.
            // c) Optionally make the texture non-readable on CPU side (good for memory).
            // Since this texture is being returned to be potentially cached/used immediately,
            // making it non-readable is usually desired.
            resultTex.Apply(true, true); // Generate Mips=true, Make Non-Readable=true

            RenderTexture.active = previousActiveRT; // IMPORTANT: Restore

            ConfigManager.WriteConsole($"[ConvertTextureToRgb565Texture2DSync] Successfully read pixels and applied to {resultTex.format} Texture2D: {resultTex.name}");

        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException("Error during synchronous RGB565 Texture2D conversion", e);
            // Clean up the output texture if created before the error
            if (resultTex != null)
            {
                // Use DestroyImmediate in editor if not playing, otherwise Destroy
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(resultTex);
                else
                    Destroy(resultTex);
                resultTex = null;
            }
        }
        finally
        {
            // --- 6. Cleanup ONLY Temporary GPU Resources ---
            // Release the temporary RT back to the pool.
            if (tempRT != null)
            {
                // It's good practice to ensure RT isn't active before releasing, though RenderTecture.active restoration should handle this.
                if (RenderTexture.active == tempRT) RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(tempRT);
                ConfigManager.WriteConsole("Released temporary RT.");
            }
            // Do NOT destroy m_ConversionMaterial here; it's cached.
            // Do NOT destroy resultTex here; it's the return value.
            ConfigManager.WriteConsole("Synchronous RGB565 Texture2D conversion process finished.");
        }

        return resultTex;
    }
}


// --- Optimized Editor Script ---
#if UNITY_EDITOR

[CustomEditor(typeof(GpuRgb565Converter))]
public class GpuRgb565ConverterEditor : Editor
{
    private Texture2D m_PreviewTexture = null; // Use prefix for member variable

    private void OnDisable()
    {
        // --- IMPORTANT: Cleanup the preview texture ---
        // This is called when the inspector loses focus or the object is deselected.
        CleanupPreviewTexture();
    }

    // Helper for cleanup
    private void CleanupPreviewTexture()
    {
        if (m_PreviewTexture != null)
        {
            // Always use DestroyImmediate in editor scripts for assets/objects
            // not managed by the scene lifecycle.
            DestroyImmediate(m_PreviewTexture);
            m_PreviewTexture = null;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GpuRgb565Converter converterScript = (GpuRgb565Converter)target;

        EditorGUILayout.Space(10);

        // --- Button for Conversion and Preview ---
        if (GUILayout.Button("Convert and Preview RGB565 Texture"))
        {
            if (EditorApplication.isPlaying)
            {
                ConfigManager.WriteConsoleWarning("Preview button should only be used when not in Play Mode.");
                return;
            }
            if (converterScript.sourceTexture == null || converterScript.conversionShader == null)
            {
                ConfigManager.WriteConsoleError("Assign Source Texture and Conversion Shader first!");
                return;
            }

            // --- Cleanup previous preview ---
            CleanupPreviewTexture(); // Use the helper method

            // --- Call the synchronous conversion method ---
            // No longer needs the cleanup flag parameter
            ConfigManager.WriteConsole("Editor: Calling ConvertTextureToRgb565Texture2DSync...");
            m_PreviewTexture = converterScript.ConvertTextureToRgb565Texture2DSync(converterScript.sourceTexture);

            if (m_PreviewTexture != null)
            {
                ConfigManager.WriteConsole($"Editor: Conversion successful. Preview texture created ({m_PreviewTexture.format}).");
                // Name was set inside the conversion method
            }
            else
            {
                ConfigManager.WriteConsoleError("Editor: Conversion failed. See previous errors.");
            }
        }

        EditorGUILayout.HelpBox("Click button to convert using GPU and preview the result (not saved).", MessageType.Info);

        // --- Display the Preview Texture ---
        if (m_PreviewTexture != null)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Preview (RGB565):", EditorStyles.boldLabel);

            float aspectRatio = (float)m_PreviewTexture.width / m_PreviewTexture.height;
            float previewWidth = EditorGUIUtility.currentViewWidth - 40;
            float previewHeight = previewWidth / aspectRatio;
            Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
            // DrawTextureTransparent handles non-readable textures better sometimes
            EditorGUI.DrawTextureTransparent(previewRect, m_PreviewTexture, ScaleMode.ScaleToFit);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Format:", m_PreviewTexture.format.ToString());
            EditorGUILayout.LabelField("Size:", $"{m_PreviewTexture.width} x {m_PreviewTexture.height}");
        }
    }
}
#endif