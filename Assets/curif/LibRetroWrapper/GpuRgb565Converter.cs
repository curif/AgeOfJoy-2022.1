using UnityEngine;
using UnityEngine.Rendering; // Required for RenderTextureFormat
using System.IO; // Required for file operations
using System; // Required for Action
// using Unity.Collections; // Not strictly needed for this specific method anymore

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
    /// Relies on ReadPixels directly into an RGB565 Texture2D format. No CPU conversion loops.
    /// NOTE: Using ReadPixels into TextureFormat.RGB565 might be less reliable across platforms than other methods.
    /// NOTE: This WILL cause a stall on the main thread while waiting for the GPU.
    /// </summary>
    /// <param name="sourceInputTexture">The texture to convert.</param>
    /// <param name="useDestroyImmediateForCleanup">Set true if calling from editor code, false if purely runtime.</param>
    /// <returns>A new Texture2D containing the RGB565 pixel data, or null if an error occurred.</returns>
    public Texture2D ConvertTextureToRgb565Texture2DSync(Texture sourceInputTexture, bool useDestroyImmediateForCleanup = false)
    {
        // --- Input Validation ---
        if (sourceInputTexture == null)
        {
            sourceInputTexture = sourceTexture;
            ConfigManager.WriteConsoleError("ConvertTextureToRgb565Texture2DSync: sourceInputTexture is null.");
        }

        if (conversionShader == null)
        {
            ConfigManager.WriteConsoleError("ConvertTextureToRgb565Texture2DSync: conversionShader is not assigned to the component.");
            return null;
        }

        // --- Temporary Resources ---
        RenderTexture tempRT = null;
        Material tempMat = null;
        // --- Output Resource ---
        Texture2D resultTex = null; // This will be the texture we return

        Action<UnityEngine.Object> destroyAction = useDestroyImmediateForCleanup ? DestroyImmediate : (Action<UnityEngine.Object>)Destroy;

        try
        {
            // 1. Create Temporary Render Texture (GPU Target is RGB565)
            ConfigManager.WriteConsole($"Creating temporary RGB565 RT ({sourceInputTexture.width}x{sourceInputTexture.height})...");
            tempRT = new RenderTexture(sourceInputTexture.width, sourceInputTexture.height, 0, RenderTextureFormat.RGB565);
            tempRT.Create();

            // 2. Create Temporary Material
            tempMat = new Material(conversionShader); // Uses the shader for Linear->Gamma etc.
            tempMat.SetTexture("_MainTex", sourceInputTexture);

            // 3. Perform GPU Conversion & Quantization (Blit)
            // The GPU writes color-corrected AND quantized RGB565 data into tempRT here.
            ConfigManager.WriteConsole("Performing GPU conversion/quantization (Blit)...");
            Graphics.Blit(sourceInputTexture, tempRT, tempMat);

            // 4. Create the RESULT Texture2D (Target is also RGB565)
            // This is the texture that will eventually be returned.
            resultTex = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGB565, false); // false = no mipmaps

            // 5. Synchronous Readback (Copy RGB565 GPU data directly into resultTex)
            RenderTexture previousActiveRT = RenderTexture.active;
            RenderTexture.active = tempRT;
            ConfigManager.WriteConsole("Attempting synchronous ReadPixels into the result RGB565 Texture2D...");

            // *** This copies from the active RT (tempRT) into resultTex ***
            resultTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);

            // Apply is needed to upload the pixel data from CPU memory to the GPU texture memory
            // if you intend to use this texture for rendering immediately.
            // false = don't make unreadable, false = don't update mipmaps immediately
            resultTex.Apply(false, false);
            RenderTexture.active = previousActiveRT; // IMPORTANT: Restore

            ConfigManager.WriteConsole($"Successfully read pixels into new RGB565 Texture2D: {resultTex.name}");

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
                destroyAction(tempRT);
            }
            if (tempMat != null)
            {
                destroyAction(tempMat);
            }
            ConfigManager.WriteConsole("Synchronous RGB565 Texture2D conversion process finished.");
        }

        // Return the newly created and populated texture (or null if error)
        return resultTex;
    }
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
                Debug.LogWarning("Preview button should only be used when not in Play Mode.");
                return; // Don't proceed if playing
            }
            if (converterScript.sourceTexture == null)
            {
                Debug.LogError("Assign a Source Texture to the GpuRgb565Converter component first!");
                return;
            }
            if (converterScript.conversionShader == null)
            {
                Debug.LogError("Assign a Conversion Shader to the GpuRgb565Converter component first!");
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
            Debug.Log("Calling ConvertTextureToRgb565Texture2DSync...");
            previewTexture = converterScript.ConvertTextureToRgb565Texture2DSync(converterScript.sourceTexture, true);

            if (previewTexture != null)
            {
                Debug.Log($"Conversion successful. Preview texture created ({previewTexture.width}x{previewTexture.height}, Format: {previewTexture.format}).");
                // Optional: Set name for clarity in memory profiler etc.
                previewTexture.name = converterScript.sourceTexture.name + "_RGB565_Preview";
            }
            else
            {
                Debug.LogError("Conversion failed. ConvertTextureToRgb565Texture2DSync returned null.");
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