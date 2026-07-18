/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.Rendering;

/// <summary>MR wall poster — applies MR/Posters textures onto Resources/ramiro/Poster.fbx.</summary>
[DisallowMultipleComponent]
public class MRWallPoster : MonoBehaviour
{
    const string LogPrefix = "[MRWallPoster]";
    const float LandscapeScaleZ = 2f;
    const float PortraitScaleZ = 2f;

    [SerializeField] float userScale = 1f;

    static readonly string[] PreferredRendererNames =
    {
        "picture",
        "poster",
        "Poster",
        "Plane",
        "mesh",
        "Mesh"
    };

    [SerializeField] string textureRelativePath;
    [SerializeField] Renderer targetRenderer;
    [SerializeField] bool flipTextureX = false;

    Material runtimeMaterial;
    Texture2D ownedTexture;

    public string TextureRelativePath => textureRelativePath;
    public float UserScale => userScale > 0f ? userScale : 1f;

    public void SetUserScale(float scale)
    {
        userScale = MRPosterPlacement.SnapScale(scale);
        ApplyTextureAndScale();
    }

    void Awake()
    {
        ResolveTargetRenderer();
    }

    void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);

        if (ownedTexture != null)
            Destroy(ownedTexture);
    }

    public void Configure(string relativePath)
    {
        textureRelativePath = MRPostersCatalog.NormalizeRelativePath(relativePath);
        ApplyTextureAndScale();
    }

    public void ApplyTextureAndScale()
    {
        ResolveTargetRenderer();

        if (targetRenderer == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} no renderer on {name}");
            return;
        }

        if (ownedTexture != null)
        {
            Destroy(ownedTexture);
            ownedTexture = null;
        }

        ownedTexture = MRPostersCatalog.LoadTexture(textureRelativePath);
        EnsureRuntimeMaterial();
        ApplyLightingPolicy();

        if (ownedTexture == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} texture missing or unsupported: {textureRelativePath} " +
                "(use .png or .jpg on Quest — .tif often fails)");
            runtimeMaterial.color = new Color(1f, 0.2f, 0.85f, 1f);
            ApplyPosterScale(null);
            return;
        }

        ApplyTextureToMaterial(ownedTexture);
        ApplyPosterScale(ownedTexture);

        bool landscape = ownedTexture.width >= ownedTexture.height;
        ConfigManager.WriteConsole(
            $"{LogPrefix} applied {textureRelativePath} ({ownedTexture.width}x{ownedTexture.height} " +
            $"{(landscape ? "landscape" : "portrait")}) scale={transform.localScale}");
    }

    void ResolveTargetRenderer()
    {
        if (targetRenderer != null)
            return;

        foreach (string childName in PreferredRendererNames)
        {
            Transform child = FindChildDeep(transform, childName);
            if (child == null)
                continue;

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                targetRenderer = renderer;
                return;
            }
        }

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is MeshRenderer)
            {
                targetRenderer = renderer;
                return;
            }
        }
    }

    static Transform FindChildDeep(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child != root && child.name == childName)
                return child;
        }

        return null;
    }

    void EnsureRuntimeMaterial()
    {
        if (runtimeMaterial != null)
            return;

        Material template = Resources.Load<Material>("ramiro/Materials/MRWallPoster");
        if (template != null)
            runtimeMaterial = new Material(template);
        else
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            runtimeMaterial = new Material(shader);
        }

        if (runtimeMaterial.HasProperty("_Metallic"))
            runtimeMaterial.SetFloat("_Metallic", 0f);
        if (runtimeMaterial.HasProperty("_Glossiness"))
            runtimeMaterial.SetFloat("_Glossiness", 0.15f);

        targetRenderer.material = runtimeMaterial;
        ApplyLightingPolicy();
    }

    void ApplyLightingPolicy()
    {
        if (targetRenderer == null)
            return;

        // Receive global/local light + shadows; do not cast (thin wall cards).
        targetRenderer.shadowCastingMode = ShadowCastingMode.Off;
        targetRenderer.receiveShadows = true;
    }

    void ApplyTextureToMaterial(Texture2D texture)
    {
        runtimeMaterial.mainTexture = texture;
        if (runtimeMaterial.HasProperty("_MainTex"))
            runtimeMaterial.SetTexture("_MainTex", texture);
        if (runtimeMaterial.HasProperty("_BaseMap"))
            runtimeMaterial.SetTexture("_BaseMap", texture);

        // NegativeX wall facing mirrors U vs the old PositiveZ path — keep UV unflipped by default.
        // Always write every scale/offset channel (template mats may ship with -1 on _MainTex).
        Vector2 scale = flipTextureX ? new Vector2(-1f, 1f) : Vector2.one;
        Vector2 offset = flipTextureX ? new Vector2(1f, 0f) : Vector2.zero;

        if (runtimeMaterial.HasProperty("_MainTex"))
        {
            runtimeMaterial.SetTextureScale("_MainTex", scale);
            runtimeMaterial.SetTextureOffset("_MainTex", offset);
        }

        if (runtimeMaterial.HasProperty("_BaseMap"))
        {
            runtimeMaterial.SetTextureScale("_BaseMap", scale);
            runtimeMaterial.SetTextureOffset("_BaseMap", offset);
        }

        runtimeMaterial.mainTextureScale = scale;
        runtimeMaterial.mainTextureOffset = offset;
        runtimeMaterial.color = Color.white;
    }

    void ApplyPosterScale(Texture2D texture)
    {
        float baseY = 1f;
        float baseZ = PortraitScaleZ;
        if (texture != null && texture.width >= texture.height)
            baseZ = LandscapeScaleZ;

        float scale = UserScale;
        // Two negatives keep winding so Standard front faces the room (-X), while -Z mirrors L/R.
        transform.localScale = new Vector3(-1f, baseY * scale, -baseZ * scale);
    }
}
