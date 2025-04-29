using UnityEngine;
using UnityEngine.Rendering; // For AsyncGPUReadback

public static class GpuAlphaCheckCompute
{
    private static ComputeShader checkComputeShader;
    private static ComputeBuffer resultBuffer;
    private static int kernelHandle = -1;
    private const string COMPUTE_SHADER_NAME = "ComputeShaders/CheckAlphaCompute"; // Assign this compute shader asset

    private static bool InitializeComputeResources()
    {
        if (checkComputeShader != null && resultBuffer != null && kernelHandle != -1) return true;

        try
        {
            if (checkComputeShader == null)
            {
                checkComputeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME); // Or use direct reference
                if (checkComputeShader == null) throw new System.Exception($"Compute shader '{COMPUTE_SHADER_NAME}' not found.");
                kernelHandle = checkComputeShader.FindKernel("CSMain");
            }
            if (resultBuffer == null)
            {
                // Buffer size 1, stores uint (4 bytes)
                resultBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            }
            return true;
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleError($"Failed to initialize compute resources: {e.Message}");
            ReleaseComputeResources();
            return false;
        }
    }

    public static void ReleaseComputeResources()
    {
        resultBuffer?.Release(); // Safe release using null-conditional operator
        resultBuffer = null;
        // ComputeShader is an asset, don't DestroyImmediate unless loaded dynamically without Resources.Load
        checkComputeShader = null;
        kernelHandle = -1;
    }

    /// <summary>
    /// Checks for transparency using a Compute Shader (synchronous version).
    /// </summary>
    public static bool HasAnyTransparencyComputeSync(Texture sourceTexture)
    {
        if (sourceTexture == null || !InitializeComputeResources()) return false; // Assume opaque on error

        if (!sourceTexture.isReadable && sourceTexture.dimension != TextureDimension.Tex2D)
        {
            // Compute shader might need texture marked UAV compatible or specific setup
            // Also check dimension compatibility
            ConfigManager.WriteConsoleError($"[HasAnyTransparencyComputeSync] Source texture '{sourceTexture.name}' not suitable for compute shader access (readable/dimension/UAV?).");
            return false; // Assume opaque if we can't process
        }


        bool hasTransparency = false;
        uint[] resultData = new uint[1] { 0 }; // Initialize result to 0

        try
        {

            // this code will work correctly even if the sourceTexture was loaded or created with the nonReadable flag set to true.

            // 1. Reset buffer data to 0
            resultBuffer.SetData(resultData);

            // 2. Set resources for compute shader
            checkComputeShader.SetTexture(kernelHandle, "SourceTex", sourceTexture);
            checkComputeShader.SetBuffer(kernelHandle, "ResultBuffer", resultBuffer);
            // Pass texture dimensions to the compute shader
            checkComputeShader.SetFloat("texWidth", sourceTexture.width);
            checkComputeShader.SetFloat("texHeight", sourceTexture.height);

            // 3. Dispatch compute shader
            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
            checkComputeShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            int dispatchX = Mathf.CeilToInt((float)sourceTexture.width / threadGroupSizeX);
            int dispatchY = Mathf.CeilToInt((float)sourceTexture.height / threadGroupSizeY);
            checkComputeShader.Dispatch(kernelHandle, dispatchX, dispatchY, 1);

            // 4. Read back result (Synchronous)
            resultBuffer.GetData(resultData); // Blocks until compute is done

            // 5. Check result
            hasTransparency = (resultData[0] != 0);

            // ConfigManager.WriteConsole($"Compute Check for '{sourceTexture.name}': Result={resultData[0]}, HasTransparency={hasTransparency}");

        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleError($"Compute alpha check failed: {e.Message}");
            hasTransparency = true; // Fail safe: assume transparency on error
        }

        return hasTransparency;
    }

    // Optional: Async version using AsyncGPUReadbackRequest
    // public static void HasAnyTransparencyComputeAsync(Texture sourceTexture, System.Action<bool> callback) { ... }
}