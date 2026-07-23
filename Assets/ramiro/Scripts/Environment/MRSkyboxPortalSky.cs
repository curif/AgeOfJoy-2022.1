/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Large inverted sphere around the camera that draws equirect sky only where
/// <c>ramiro/SkyboxPortalMask</c> wrote stencil (window glass).
/// A sphere interpolates view directions more evenly than a cube for equirect sampling.
/// </summary>
[DisallowMultipleComponent]
public class MRSkyboxPortalSky : MonoBehaviour
{
    const string LogPrefix = "[MRSkyboxPortalSky]";
    const string ShaderName = "ramiro/SkyboxPortal";
    const string MainTexProperty = "_MainTex";
    const string RotationProperty = "_Rotation";
    const string SkyMeshName = "MRSkyboxPortalSphere";

    [SerializeField] Material skyMaterial;
    [SerializeField] Texture2D skyTexture;
    [SerializeField] float sphereRadius = 50f;
    [SerializeField] bool followMainCamera = true;
    [SerializeField] bool createMeshIfMissing = true;

    Material runtimeMaterial;
    Transform cameraTransform;
    bool built;

    void Awake()
    {
        Rebuild();
    }

    void LateUpdate()
    {
        if (!followMainCamera)
            return;

        if (cameraTransform == null)
            cameraTransform = ResolveCameraTransform();

        if (cameraTransform == null)
            return;

        transform.position = cameraTransform.position;
    }

    static Transform ResolveCameraTransform()
    {
        Camera main = Camera.main;
        if (main != null)
            return main.transform;

        var xrOrigin = Object.FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null && xrOrigin.Camera != null)
            return xrOrigin.Camera.transform;

        Camera any = Object.FindObjectOfType<Camera>();
        return any != null ? any.transform : null;
    }

    void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }

    /// <summary>Configure material/texture and rebuild the sky mesh (used by test bootstrap).</summary>
    public void Configure(Material material, Texture2D texture, float radius = 50f)
    {
        skyMaterial = material;
        skyTexture = texture;
        sphereRadius = radius;
        Rebuild();
    }

    /// <summary>Swap the equirect image at runtime (user skybox / default).</summary>
    public void SetSkyTexture(Texture2D texture)
    {
        skyTexture = texture;
        ApplySkyTexture(ResolveSkyTexture());
        ApplyYawRotation(MRSkyboxSettings.YawDegrees);
    }

    /// <summary>Y-axis sky rotation in degrees (shader <c>_Rotation</c>).</summary>
    public void ApplyYawRotation(float degrees)
    {
        Material mat = ResolveMaterial();
        if (mat == null)
            return;

        if (mat.HasProperty(RotationProperty))
            mat.SetFloat(RotationProperty, degrees);

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.sharedMaterial = mat;
    }

    public void Rebuild()
    {
        EnsureSkyMesh();
        ApplySkyTexture(ResolveSkyTexture());
        ApplyYawRotation(MRSkyboxSettings.YawDegrees);
        built = true;
    }

    Texture2D ResolveSkyTexture()
    {
        if (skyTexture != null)
            return skyTexture;

        if (skyMaterial != null && skyMaterial.HasProperty(MainTexProperty))
        {
            if (skyMaterial.GetTexture(MainTexProperty) is Texture2D fromMat)
                return fromMat;
        }

        Texture2D fromSettings = MRRuntimeSettings.DefaultSkyboxImage;
        if (fromSettings != null)
            return fromSettings;

        return null;
    }

    void EnsureSkyMesh()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (filter == null && createMeshIfMissing)
            filter = gameObject.AddComponent<MeshFilter>();
        if (renderer == null && createMeshIfMissing)
            renderer = gameObject.AddComponent<MeshRenderer>();

        if (filter != null && NeedsSphereMesh(filter.sharedMesh))
            filter.sharedMesh = CreateUnitSphereMesh();

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (Application.isPlaying)
                Destroy(col);
            else
                DestroyImmediate(col);
        }

        // Unity sphere mesh radius ≈ 0.5; scale so world radius ≈ sphereRadius.
        float diameter = Mathf.Max(1f, sphereRadius) * 2f;
        transform.localScale = Vector3.one * diameter;

        if (renderer != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            renderer.sharedMaterial = ResolveMaterial();
        }
    }

    static bool NeedsSphereMesh(Mesh mesh)
    {
        if (mesh == null)
            return true;
        return mesh.name != SkyMeshName && !mesh.name.StartsWith("Sphere", System.StringComparison.Ordinal);
    }

    static Mesh CreateUnitSphereMesh()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh source = temp.GetComponent<MeshFilter>().sharedMesh;
        Mesh copy = Object.Instantiate(source);
        copy.name = SkyMeshName;
        if (Application.isPlaying)
            Destroy(temp);
        else
            DestroyImmediate(temp);
        return copy;
    }

    Material ResolveMaterial()
    {
        if (runtimeMaterial != null)
        {
            // Keep the runtime instance. Only recreate when an authored skyMaterial is
            // assigned/changed (TestWindow Configure). MR path uses Shader.Find with
            // skyMaterial == null — the old check destroyed the textured material on
            // every ApplyYawRotation and left the shader default gray.
            if (skyMaterial == null)
                return runtimeMaterial;

            if (runtimeMaterial.shader == skyMaterial.shader)
                return runtimeMaterial;

            if (Application.isPlaying)
                Destroy(runtimeMaterial);
            else
                DestroyImmediate(runtimeMaterial);
            runtimeMaterial = null;
        }

        if (skyMaterial != null)
        {
            runtimeMaterial = new Material(skyMaterial);
            runtimeMaterial.name = skyMaterial.name + " (Portal Instance)";
            return runtimeMaterial;
        }

        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} shader '{ShaderName}' not found");
            return null;
        }

        runtimeMaterial = new Material(shader) { name = "SkyboxPortal (Runtime)" };
        return runtimeMaterial;
    }

    void ApplySkyTexture(Texture2D texture)
    {
        Material mat = ResolveMaterial();
        if (mat == null)
            return;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.sharedMaterial = mat;

        if (texture == null)
        {
            if (!built)
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} no sky texture — assign skyTexture, material _MainTex, or MRRuntimeSettings.defaultSkyboxImage");
            }
            return;
        }

        if (mat.HasProperty(MainTexProperty))
            mat.SetTexture(MainTexProperty, texture);

        // Prefer sharp sampling for equirect sky (default import often has soft mips).
        // Only touch import settings when the texture is still readable / owned runtime copy.
        try
        {
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 0;
            texture.wrapModeU = TextureWrapMode.Repeat;
            texture.wrapModeV = TextureWrapMode.Clamp;
            texture.wrapModeW = TextureWrapMode.Clamp;
        }
        catch (UnityException)
        {
            // Shared / non-readable assets may reject wrap changes — texture still works.
        }
    }
}
