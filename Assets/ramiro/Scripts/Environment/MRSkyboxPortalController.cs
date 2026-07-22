/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>
/// Spawns stencil glass on MRUK <see cref="MRUKAnchor.SceneLabels.WINDOW_FRAME"/> anchors
/// and a camera-following sky sphere (<see cref="MRSkyboxPortalSky"/>) so the selected HDRI
/// is visible only through real-room windows.
/// </summary>
[DisallowMultipleComponent]
public class MRSkyboxPortalController : MonoBehaviour
{
    const string LogPrefix = "[MRSkyboxPortalController]";
    const string PortalRootName = "MRSkyboxPortals";
    const string SkyObjectName = "SkyboxPortalSky";
    const string GlassChildName = "SkyBoxPortalMask";
    const string PortalMatResourcesPath = "ramiro/Materials/SkyboxPortal";
    const float GlassInsetMeters = 0.008f;
    const float FallbackWindowWidth = 1.2f;
    const float FallbackWindowHeight = 1.0f;

    public static MRSkyboxPortalController Instance { get; private set; }

    [Header("Window")]
    [Tooltip(
        "Required. Spawned on each WINDOW_FRAME. If empty, uses MRRuntimeSettings.windowPrefab. " +
        "Must include child SkyBoxPortalMask with SkyboxPortalMask.mat already assigned.")]
    [SerializeField] GameObject windowPrefab;

    Transform portalRoot;
    MRSkyboxPortalSky sky;
    Texture2D ownedUserTexture;
    readonly List<GameObject> glassInstances = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Clear(destroyRoot: true);
    }

    /// <summary>Rebuild window glass from current MRUK room and apply the selected skybox.</summary>
    public void Refresh(bool reloadSkybox = true)
    {
        MRSkyboxSettings.EnsureLoaded();
        if (!MRSkyboxSettings.HasActiveWindow)
        {
            Clear(destroyRoot: false);
            ConfigManager.WriteConsole($"{LogPrefix} no active Window skin — prefab/sky not spawned");
            return;
        }

        EnsureRoot();
        RebuildGlassOnWindowAnchors();
        EnsureSky();
        if (reloadSkybox)
            ApplySelectedSkybox();
        else
            sky?.ApplyYawRotation(MRSkyboxSettings.YawDegrees);
    }

    /// <summary>Remove portal glass + sky (MR → VR, or CRT Window Rem).</summary>
    public void Clear(bool destroyRoot = false)
    {
        for (int i = 0; i < glassInstances.Count; i++)
        {
            if (glassInstances[i] != null)
                Destroy(glassInstances[i]);
        }

        glassInstances.Clear();

        if (sky != null)
        {
            Destroy(sky.gameObject);
            sky = null;
        }

        ReleaseOwnedTexture();

        if (destroyRoot && portalRoot != null)
        {
            Destroy(portalRoot.gameObject);
            portalRoot = null;
        }

        ConfigManager.WriteConsole($"{LogPrefix} cleared");
    }

    /// <summary>Reload texture from <see cref="MRSkyboxSettings"/>. Empty selection clears the portal.</summary>
    public void ApplySelectedSkybox()
    {
        MRPaths.EnsureFolders();
        MRRoomSkinCatalog.SeedBuiltInPackagesToDevice();
        MRSkyboxSettings.EnsureLoaded();

        if (!MRSkyboxSettings.HasActiveWindow)
        {
            Clear(destroyRoot: false);
            ConfigManager.WriteConsole($"{LogPrefix} no active Window skin — portal cleared");
            return;
        }

        EnsureSky();
        if (sky == null)
            return;

        Texture2D texture = null;
        Texture2D loadedOwned = null;
        string selected = MRSkyboxSettings.SelectedRelativePath;
        if (MRRoomSkinDefinition.TryLoad(selected, out MRRoomSkinDefinition skin)
            && skin.IsWindowSkyboxOnly)
        {
            loadedOwned = MRRoomSkinCatalog.LoadSkyboxTexture(skin);
            texture = loadedOwned;
            if (texture == null)
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} failed to load window skin '{selected}'");
            }
        }
        else if (MRSkyboxesCatalog.Exists(selected))
        {
            loadedOwned = MRSkyboxesCatalog.LoadTexture(selected);
            texture = loadedOwned;
        }
        else
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} selected skybox not found: '{selected}'");
        }

        // Apply first, destroy previous after — never leave material on a destroyed tex (gray).
        Texture2D previousOwned = ownedUserTexture;
        ownedUserTexture = loadedOwned;
        ApplyTextureToSky(texture, alreadyOwned: loadedOwned != null && ReferenceEquals(texture, loadedOwned));
        if (previousOwned != null && !ReferenceEquals(previousOwned, ownedUserTexture))
            Destroy(previousOwned);

        MeshRenderer renderer = sky != null ? sky.GetComponent<MeshRenderer>() : null;
        Texture bound = renderer != null && renderer.sharedMaterial != null
            ? renderer.sharedMaterial.GetTexture("_MainTex")
            : null;
        ConfigManager.WriteConsole(
            $"{LogPrefix} verify renderer _MainTex=" +
            (bound != null ? $"{bound.name} {bound.width}x{bound.height}" : "NULL/GRAY"));
    }

    /// <summary>
    /// Apply an already-loaded equirect texture (Room Skin Window). When
    /// <paramref name="takeOwnership"/> is false, the caller must destroy the texture.
    /// Spawns Window prefab glass if needed (selection must already be set).
    /// </summary>
    public void ApplySkyboxTexture(Texture2D texture, bool takeOwnership)
    {
        if (!MRSkyboxSettings.HasActiveWindow)
        {
            Clear(destroyRoot: false);
            return;
        }

        EnsureRoot();
        RebuildGlassOnWindowAnchors();
        EnsureSky();
        if (sky == null)
            return;

        Texture2D previousOwned = ownedUserTexture;
        if (takeOwnership)
            ownedUserTexture = texture;
        else if (ownedUserTexture != null && !ReferenceEquals(ownedUserTexture, texture))
            ownedUserTexture = null;

        ApplyTextureToSky(texture, alreadyOwned: takeOwnership);
        if (previousOwned != null
            && !ReferenceEquals(previousOwned, ownedUserTexture)
            && !ReferenceEquals(previousOwned, texture))
            Destroy(previousOwned);
    }

    void ApplyTextureToSky(Texture2D texture, bool alreadyOwned)
    {
        if (sky == null)
            return;

        sky.SetSkyTexture(texture);
        sky.ApplyYawRotation(MRSkyboxSettings.YawDegrees);

        ConfigManager.WriteConsole(
            texture != null
                ? $"{LogPrefix} sky applied '{texture.name}' ({texture.width}x{texture.height}) yaw={MRSkyboxSettings.YawDegrees:0}° owned={alreadyOwned}"
                : $"{LogPrefix} sky applied — no texture (add MR/Room Skins/Window/*.png or MRRuntimeSettings.defaultSkyboxImage)");
    }

    /// <summary>Apply current PlayerPrefs yaw to the portal sky (CRT L/R).</summary>
    public void ApplyYawFromSettings()
    {
        EnsureSky();
        sky?.ApplyYawRotation(MRSkyboxSettings.YawDegrees);
    }

    GameObject ResolveWindowPrefab()
    {
        if (windowPrefab != null)
            return windowPrefab;
        return MRRuntimeSettings.WindowPrefab;
    }

    void EnsureRoot()
    {
        if (portalRoot != null)
            return;

        var go = new GameObject(PortalRootName);
        portalRoot = go.transform;
        portalRoot.SetParent(null, false);
    }

    void EnsureSky()
    {
        EnsureRoot();
        if (sky != null)
            return;

        var skyGo = new GameObject(SkyObjectName);
        skyGo.transform.SetParent(portalRoot, false);
        sky = skyGo.AddComponent<MRSkyboxPortalSky>();

        // Same as TestWindow: authored material template (not bare Shader.Find gray).
        Material template = Resources.Load<Material>(PortalMatResourcesPath);
#if UNITY_EDITOR
        if (template == null)
        {
            template = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/ramiro/materials/SkyboxPortal.mat");
        }
#endif
        Texture2D seedTex = MRRuntimeSettings.DefaultSkyboxImage;
        if (template != null)
            sky.Configure(template, seedTex, radius: 50f);
        else
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {PortalMatResourcesPath} missing — falling back to Shader.Find");
            sky.Rebuild();
        }
    }

    void RebuildGlassOnWindowAnchors()
    {
        for (int i = 0; i < glassInstances.Count; i++)
        {
            if (glassInstances[i] != null)
                Destroy(glassInstances[i]);
        }

        glassInstances.Clear();

        MRUKRoom room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
        if (room?.Anchors == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} no MRUK room — window glass skipped");
            return;
        }

        GameObject prefab = ResolveWindowPrefab();
        if (prefab == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} window prefab missing — assign on portal or MRRuntimeSettings.windowPrefab");
            return;
        }

        if (!MRSkyboxSettings.HasActiveWindow)
            return;

        int created = 0;
        foreach (MRUKAnchor anchor in room.Anchors)
        {
            if (anchor == null || !anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WINDOW_FRAME))
                continue;

            GameObject glass = CreateGlassForAnchor(anchor, prefab);
            if (glass == null)
                continue;

            glassInstances.Add(glass);
            created++;
        }

        ConfigManager.WriteConsole(
            created > 0
                ? $"{LogPrefix} window '{prefab.name}' on {created} WINDOW_FRAME anchor(s)"
                : $"{LogPrefix} no WINDOW_FRAME anchors in room — place a window in Space Setup");
    }

    GameObject CreateGlassForAnchor(MRUKAnchor anchor, GameObject prefab)
    {
        if (anchor == null || prefab == null)
            return null;

        if (!TryGetWindowPlaneSize(anchor, out float width, out float height, out Vector2 planeCenter))
        {
            width = FallbackWindowWidth;
            height = FallbackWindowHeight;
            planeCenter = Vector2.zero;
        }

        GameObject glass = Instantiate(prefab, anchor.transform, false);
        glass.name = prefab.name;
        glass.transform.localPosition = Vector3.zero;
        glass.transform.localRotation = Quaternion.identity;
        glass.transform.localScale = Vector3.one;

        Bounds localBounds = CalculateLocalRendererBounds(glass.transform);
        float meshW = Mathf.Max(0.001f, localBounds.size.x);
        float meshH = Mathf.Max(0.001f, localBounds.size.y);
        float scaleX = width / meshW;
        float scaleY = height / meshH;
        const float scaleZ = 1f;

        glass.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        Vector3 scaledCenter = Vector3.Scale(localBounds.center, glass.transform.localScale);
        glass.transform.localPosition = new Vector3(
            planeCenter.x - scaledCenter.x,
            planeCenter.y - scaledCenter.y,
            GlassInsetMeters - localBounds.center.z);

        ConfigManager.WriteConsole(
            $"{LogPrefix} fit '{glass.name}' to window {width:F2}x{height:F2}m " +
            $"(mesh {meshW:F2}x{meshH:F2} → scale {scaleX:F2},{scaleY:F2},{scaleZ:F2})");

        PreparePrefabGlassChild(glass);
        return glass;
    }

    static bool TryGetWindowPlaneSize(
        MRUKAnchor anchor,
        out float width,
        out float height,
        out Vector2 planeCenter)
    {
        width = 0f;
        height = 0f;
        planeCenter = Vector2.zero;

        if (anchor == null)
            return false;

        if (anchor.PlaneRect.HasValue)
        {
            Rect plane = anchor.PlaneRect.Value;
            width = Mathf.Max(0.05f, plane.size.x);
            height = Mathf.Max(0.05f, plane.size.y);
            planeCenter = plane.center;
            return true;
        }

        if (anchor.VolumeBounds.HasValue)
        {
            Bounds volume = anchor.VolumeBounds.Value;
            width = Mathf.Max(0.05f, volume.size.x);
            height = Mathf.Max(0.05f, volume.size.y);
            planeCenter = new Vector2(volume.center.x, volume.center.y);
            return true;
        }

        return false;
    }

    static Bounds CalculateLocalRendererBounds(Transform root)
    {
        var fallback = new Bounds(Vector3.zero, new Vector3(1f, 1f, 0.05f));
        if (root == null)
            return fallback;

        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        bool hasBounds = false;
        Bounds combined = default;

        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            if (renderer == null)
                continue;

            Bounds world = renderer.bounds;
            Vector3 min = root.InverseTransformPoint(world.min);
            Vector3 max = root.InverseTransformPoint(world.max);
            var local = new Bounds();
            local.SetMinMax(
                new Vector3(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Min(min.z, max.z)),
                new Vector3(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y), Mathf.Max(min.z, max.z)));

            if (!hasBounds)
            {
                combined = local;
                hasBounds = true;
            }
            else
                combined.Encapsulate(local);
        }

        return hasBounds ? combined : fallback;
    }

    /// <summary>
    /// Prefab already has <c>SkyBoxPortalMask</c> + <c>SkyboxPortalMask.mat</c> authored —
    /// do not replace materials; only silence shadows/probes on the opening.
    /// </summary>
    void PreparePrefabGlassChild(GameObject root)
    {
        if (root == null)
            return;

        Transform glassChild = FindGlassChild(root.transform);
        if (glassChild == null)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} prefab '{root.name}' has no child '{GlassChildName}' — " +
                "opening must use ramiro/SkyboxPortalMask");
            return;
        }

        MeshRenderer[] renderers = glassChild.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            if (renderer == null)
                continue;

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            Material mat = renderer.sharedMaterial;
            if (mat == null || mat.shader == null
                || !mat.shader.name.Contains("SkyboxPortalMask"))
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} '{glassChild.name}' material should be ramiro/SkyboxPortalMask " +
                    $"(got {(mat != null && mat.shader != null ? mat.shader.name : "null")})");
            }
        }
    }

    static Transform FindGlassChild(Transform root)
    {
        if (root == null)
            return null;

        if (string.Equals(root.name, GlassChildName, System.StringComparison.OrdinalIgnoreCase))
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindGlassChild(root.GetChild(i));
            if (found != null)
                return found;
        }

        return null;
    }

    void ReleaseOwnedTexture()
    {
        if (ownedUserTexture == null)
            return;

        Destroy(ownedUserTexture);
        ownedUserTexture = null;
    }
}
