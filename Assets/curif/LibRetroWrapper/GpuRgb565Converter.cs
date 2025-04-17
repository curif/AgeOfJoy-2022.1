using UnityEngine;
using UnityEngine.Rendering; // Required for RenderTextureFormat
using System.IO; // Required for file operations
using System; // Required for Action
// using Unity.Collections; // Not strictly needed for this specific method anymore
using UnityEngine.Experimental.Rendering; // Still needed for GraphicsFormatUtility

#if UNITY_EDITOR
using UnityEditor; // Required for AssetDatabase
#endif


public class GpuRgb565Converter : MonoBehaviour
{
    public Texture sourceTexture; // Assign your source RGB24/32 texture here
    public Shader conversionShader; // Assign the Rgb565PassThroughLinearCorrected shader

    // --- NEW SYNCHRONOUS METHOD - RETURNS Texture2D ---

    /// <summary>
    /// Converts the input texture to RGB565 format synchronously using the GPU, blocking until complete.
    /// Returns a NEW Texture2D object containing the RGB565 data.
    /// Uses an intermediate ARGB32 RenderTexture for better mobile compatibility.
    /// Relies on ReadPixels to perform the final format conversion into the target RGB565 Texture2D.
    /// NOTE: This WILL cause a stall on the main thread while waiting for the GPU.
    /// </summary>
    /// <param name="sourceInputTexture">The texture to convert.</param>
    /// <param name="useDestroyImmediateForCleanup">Set true if calling from editor code, false if purely runtime.</param>
    /// <returns>A new Texture2D containing the RGB565 pixel data, or null if an error occurred.</returns>
    public Texture2D ConvertTextureToRgb565Texture2DSync(Texture sourceInputTexture,
        int desiredMipCount = 7,
        bool useDestroyImmediateForCleanup = false)
    {
        // --- Input Validation ---
        if (sourceInputTexture == null)
        {
            // Fallback to public variable if argument is null
            sourceInputTexture = this.sourceTexture;
            if (sourceInputTexture == null)
            {
                ConfigManager.WriteConsoleError("ConvertTextureToRgb565Texture2DSync: sourceInputTexture is null (both argument and component field).");
                // Consider using ConfigManager if that's your logging standard
                // ConfigManager.WriteConsoleError("ConvertTextureToRgb565Texture2DSync: sourceInputTexture is null.");
                return null;
            }
        }

        if (conversionShader == null)
        {
            ConfigManager.WriteConsoleError("ConvertTextureToRgb565Texture2DSync: conversionShader is not assigned to the component.");
            // ConfigManager.WriteConsoleError("ConvertTextureToRgb565Texture2DSync: conversionShader is not assigned to the component.");
            return null;
        }

        // --- Check Project Color Space ---
        if (QualitySettings.activeColorSpace != ColorSpace.Linear)
        {
            ConfigManager.WriteConsoleWarning("ConvertTextureToRgb565Texture2DSync: Project color space is not set to Linear. The shader expects Linear input for correct conversions.");
            // ConfigManager.WriteConsoleWarning("ConvertTextureToRgb565Texture2DSync: Project color space is not set to Linear...");
        }


        // --- Temporary Resources ---
        RenderTexture tempRT = null;
        Material conversionShadderMaterial = null;
        // --- Output Resource ---
        Texture2D resultTex = null; // This will be the texture we return

        Action<UnityEngine.Object> destroyAction = useDestroyImmediateForCleanup ? DestroyImmediate : (Action<UnityEngine.Object>)Destroy;

        try
        {
            // 1. Create Temporary Render Texture (Use a common, well-supported format like ARGB32)
            // This is often more reliable on mobile platforms than specific formats like RGB565.
            RenderTextureFormat intermediateFormat = RenderTextureFormat.ARGB32;
            ConfigManager.WriteConsole($"Creating temporary {intermediateFormat} RT ({sourceInputTexture.width}x{sourceInputTexture.height})...");
            // ConfigManager.WriteConsole($"Creating temporary {intermediateFormat} RT ({sourceInputTexture.width}x{sourceInputTexture.height})...");

            // Explicitly set sRGB read/write based on project color space.
            // In Linear mode, RTs default to linear, which is what we want after the shader runs.
            bool readWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear) ? false : true; // Typically false (Linear) for intermediate RTs in Linear workflow
            tempRT = new RenderTexture(sourceInputTexture.width, sourceInputTexture.height, 0, intermediateFormat, readWrite ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
            tempRT.Create();

            // 2. Create Temporary Material
            conversionShadderMaterial = new Material(conversionShader); // Uses the shader for Linear->Gamma->Quantize->Linear
            conversionShadderMaterial.SetTexture("_MainTex", sourceInputTexture);

            // 3. Perform GPU Conversion & Quantization (Blit)
            // The shader writes the *quantized* color (converted back to Linear) into the temp ARGB32 RT.
            ConfigManager.WriteConsole("Performing GPU conversion/quantization (Blit) into temporary ARGB32 RT...");
            // ConfigManager.WriteConsole("Performing GPU conversion/quantization (Blit)...");
            Graphics.Blit(sourceInputTexture, tempRT, conversionShadderMaterial);


            // --- Mipmap Count Calculation ---
            int maxDim = Mathf.Max(sourceInputTexture.width, sourceInputTexture.height);
            int maxPossibleMipCount = Mathf.FloorToInt(Mathf.Log(maxDim, 2f)) + 1;
            int actualMipCountToCreate = Mathf.Clamp(desiredMipCount, 3, maxPossibleMipCount);

            // 4. Create the RESULT Texture2D (Target is RGB565)
            // ****** CHANGE 1: Enable mipmap chain creation ******
            bool createMipmaps = true;
            // Match linear state to project setting for consistency
            bool isLinear = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            ConfigManager.WriteConsole($"Creating result Texture2D with format {TextureFormat.RGB565}, Mipmaps: {createMipmaps}, Linear: {isLinear}");
            resultTex = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGB565, actualMipCountToCreate, isLinear);
            resultTex.name = sourceInputTexture.name;
            resultTex.filterMode = FilterMode.Trilinear; // Provides better mip transitions in VR
            resultTex.mipMapBias = -0.5f; // Recommended by Meta for high-detail textures

            // 5. Synchronous Readback (Copy GPU data from tempRT into resultTex)
            // Let ReadPixels handle the conversion from the tempRT's format (ARGB32)
            // to the resultTex's format (RGB565). This might be more reliable on mobile.
            RenderTexture previousActiveRT = RenderTexture.active;
            RenderTexture.active = tempRT;
            ConfigManager.WriteConsole($"Attempting synchronous ReadPixels from {intermediateFormat} RT into the result {resultTex.format} Texture2D...");
            // ConfigManager.WriteConsole("Attempting synchronous ReadPixels into the result RGB565 Texture2D...");

            // *** This copies from the active RT (tempRT) into resultTex ***
            // *** Crucially, it performs format conversion if RT format != Tex format ***
            resultTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);

            // Apply is needed to upload the pixel data from CPU memory to the GPU texture memory
            // if you intend to use this texture for rendering immediately or want it visible in Inspector preview.
            // false = don't make unreadable, false = don't update mipmaps immediately
            bool makeUnreadable = true;
            resultTex.Apply(true, makeUnreadable);
            RenderTexture.active = previousActiveRT; // IMPORTANT: Restore

            ConfigManager.WriteConsole($"Successfully read pixels into new {resultTex.format} Texture2D: {resultTex.name} ({resultTex.width}x{resultTex.height})");
            // ConfigManager.WriteConsole($"Successfully read pixels into new RGB565 Texture2D: {resultTex.name}");

            // The resultTex now holds the RGB565 data.

        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException("Error during synchronous RGB565 Texture2D conversion", e);
            // If an error occurred, make sure we destroy any partially created result texture
            if (resultTex != null)
            {
                destroyAction(resultTex);
                resultTex = null; // Ensure null is returned
            }
        }
        finally
        {
            // --- 6. Cleanup ONLY Temporary Resources ---
            // Do NOT destroy resultTex here, as it's the return value!
            if (tempRT != null)
            {
                if (RenderTexture.active == tempRT) RenderTexture.active = null; // Ensure not active
                tempRT.Release();
                ConfigManager.WriteConsole("Releasing temporary RT...");
                // ConfigManager.WriteConsole("Releasing temporary RT...");
                destroyAction(tempRT);
            }
            if (conversionShadderMaterial != null)
            {
                ConfigManager.WriteConsole("Destroying temporary material...");
                destroyAction(conversionShadderMaterial);
            }
            ConfigManager.WriteConsole("Synchronous RGB565 Texture2D conversion process finished.");
            // ConfigManager.WriteConsole("Synchronous RGB565 Texture2D conversion process finished.");
        }

        // Return the newly created and populated texture (or null if error)
        return resultTex;
    }

    // --- Include other methods like Start, OnDestroy, Async, Editor button handlers etc. ---
    // Make sure the editor script still calls this modified version.
}

// --- Include all other necessary methods from your class here ---
// Start, OnDestroy, Async methods, Editor methods, etc.

#if UNITY_EDITOR

// This attribute tells Unity to use this script to draw the inspector
// for components of type GpuRgb565Converter.
[CustomEditor(typeof(GpuRgb565Converter))]
public class GpuRgb565ConverterEditor : Editor // Inherit from Editor
{
    // Private variable within the Editor script to hold the preview texture
    private Texture2D previewTexture = null;

    // This method is called when the custom editor is disabled or destroyed
    // (e.g., when the selection changes or the editor recompiles)
    private void OnDisable()
    {
        // --- IMPORTANT: Cleanup the preview texture to prevent memory leaks ---
        if (previewTexture != null)
        {
            // Use DestroyImmediate in the editor for non-scene objects
            DestroyImmediate(previewTexture);
            previewTexture = null;
            // ConfigManager.WriteConsole("Preview texture cleaned up."); // Optional log
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector elements (like the public variables: sourceTexture, conversionShader)
        DrawDefaultInspector();

        // Get a reference to the script instance being inspected
        GpuRgb565Converter converterScript = (GpuRgb565Converter)target;

        EditorGUILayout.Space(10); // Add some vertical space

        // --- Button for Conversion and Preview ---
        if (GUILayout.Button("Convert and Preview RGB565 Texture"))
        {
            // --- Safety Checks ---
            if (EditorApplication.isPlaying)
            {
                ConfigManager.WriteConsoleWarning("Preview button should only be used when not in Play Mode.");
                return; // Don't proceed if playing
            }
            if (converterScript.sourceTexture == null)
            {
                ConfigManager.WriteConsoleError("Assign a Source Texture to the GpuRgb565Converter component first!");
                return;
            }
            if (converterScript.conversionShader == null)
            {
                ConfigManager.WriteConsoleError("Assign a Conversion Shader to the GpuRgb565Converter component first!");
                return;
            }

            // --- Cleanup previous preview if it exists ---
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
                previewTexture = null;
            }

            // --- Call the synchronous conversion method ---
            // Pass 'true' for useDestroyImmediateForCleanup as this is editor code
            ConfigManager.WriteConsole("Calling ConvertTextureToRgb565Texture2DSync...");
            previewTexture = converterScript.ConvertTextureToRgb565Texture2DSync(converterScript.sourceTexture, 5, true);

            if (previewTexture != null)
            {
                ConfigManager.WriteConsole($"Conversion successful. Preview texture created ({previewTexture.width}x{previewTexture.height}, Format: {previewTexture.format}).");
                // Optional: Set name for clarity in memory profiler etc.
                previewTexture.name = converterScript.sourceTexture.name + "_RGB565_Preview";
            }
            else
            {
                ConfigManager.WriteConsoleError("Conversion failed. ConvertTextureToRgb565Texture2DSync returned null.");
            }
        }

        EditorGUILayout.HelpBox("Click the button above to perform the GPU conversion and show a preview of the resulting RGB565 Texture below. This preview is not saved as an asset.", MessageType.Info);

        // --- Display the Preview Texture ---
        if (previewTexture != null)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Preview (RGB565):", EditorStyles.boldLabel);

            // Draw the texture preview in the Inspector
            // Calculate aspect ratio for better preview sizing
            float aspectRatio = (float)previewTexture.width / previewTexture.height;
            float previewWidth = EditorGUIUtility.currentViewWidth - 40; // Use available width minus some padding
            float previewHeight = previewWidth / aspectRatio;

            // Use GUILayout.Box or EditorGUI.DrawPreviewTexture / DrawTextureTransparent
            // GUILayout.Box is simple:
            Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
            EditorGUI.DrawTextureTransparent(previewRect, previewTexture, ScaleMode.ScaleToFit);

            // Alternative using GUILayout.Box:
            // GUILayout.Box(previewTexture, GUILayout.Width(previewWidth), GUILayout.Height(previewHeight));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Format:", previewTexture.format.ToString());
            EditorGUILayout.LabelField("Size:", $"{previewTexture.width} x {previewTexture.height}");
        }
    }
}

#endif