/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Isolated Play Mode test for the window stencil skybox portal (scene <c>TestWindow</c>).
/// Prefers the same disk path as MR runtime: <c>{cabs}/MR/Room Skins/Window/</c>.
/// Open <c>Assets/ramiro/ScenesTest/TestWindow.unity</c> and press Play.
/// </summary>
public class MRTestWindowBootstrap : MonoBehaviour
{
    public const string TestSceneName = "TestWindow";

    const string LogPrefix = "[MRTestWindowBootstrap]";
    const string PortalMatPath = "Assets/ramiro/materials/SkyboxPortal.mat";
    const string MaskMatPath = "Assets/ramiro/materials/SkyboxPortalMask.mat";
    const string WindowPrefabPath = "Assets/ramiro/Prefabs/Window.prefab";
    const string GlassChildName = "SkyBoxPortalMask";
    const string PanoramaAssetPath = "Assets/ramiro/PanoramaImage/City360.png";
    const string PanoramaResourcesPath = "ramiro/roomskin/Window/City360";

    [SerializeField] bool startWithFullSkyDebug;
    [SerializeField] float mouseLookSensitivity = 2.2f;
    [SerializeField] float yawStepDegrees = 5f;
    [SerializeField] GameObject windowPrefabOverride;

    MRSkyboxPortalSky sky;
    Material runtimeSkyMaterial;
    Material runtimeMaskMaterial;
    Texture2D panorama;
    /// <summary>True when panorama was loaded from disk (must Destroy on teardown).</summary>
    bool ownsPanorama;
    string panoramaSource = "none";
    string statusLine = "booting…";
    float yawDegrees;
    bool fullSkyDebug;
    float lookYaw;
    float lookPitch;
    GameObject debugFullSky;
    GameObject windowInstance;
    readonly List<string> windowSkyboxKeys = new List<string>();
    int skyboxIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallForTestScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.name != TestSceneName)
            return;

        if (Object.FindObjectOfType<MRTestWindowBootstrap>() != null)
            return;

        var host = new GameObject("TestWindowBootstrap");
        host.AddComponent<MRTestWindowBootstrap>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        fullSkyDebug = startWithFullSkyDebug;
        RenderSettings.skybox = null;

        DisableLegacySceneJunk();
        ConfigureCamera();
        EnsureWindowPrefab();
        EnsureSkySphere();
        ApplyDebugStencilMode(fullSkyDebug);
        LogDiagnostics();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        HandleMouseLook();
        HandleYawKeys();
        HandleSkyboxCycleKeys();

        if (MREditorInput.WasPressed(KeyCode.T))
        {
            fullSkyDebug = !fullSkyDebug;
            ApplyDebugStencilMode(fullSkyDebug);
            statusLine = fullSkyDebug
                ? "DEBUG full sky (no stencil) — press T for portal"
                : "Portal stencil ON — look through Window";
        }

        if (MREditorInput.WasPressed(KeyCode.R))
        {
            yawDegrees = 0f;
            sky?.ApplyYawRotation(yawDegrees);
            statusLine = "Yaw reset to 0°";
        }
    }

    void LateUpdate()
    {
        if (debugFullSky != null && debugFullSky.activeSelf && Camera.main != null)
            debugFullSky.transform.position = Camera.main.transform.position;
    }

    void OnGUI()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        string skyLabel = windowSkyboxKeys.Count > 0
            ? $"{skyboxIndex + 1}/{windowSkyboxKeys.Count}"
            : "0/0";

        const int pad = 10;
        GUI.Box(new Rect(pad, pad, 580, 140), GUIContent.none);
        GUI.Label(new Rect(pad + 8, pad + 6, 560, 120),
            "TestWindow — Window prefab + skybox portal\n" +
            "↑: next skybox   ←/→ or A/D: yaw   T: full-sky   R: yaw 0\n" +
            $"Sky: {skyLabel}   Yaw: {yawDegrees:0}°   Mode: {(fullSkyDebug ? "FULL SKY" : "PORTAL")}\n" +
            $"Source: {panoramaSource}\n" +
            statusLine);
    }

    void HandleMouseLook()
    {
        Camera cam = Camera.main;
        if (cam == null || !MREditorInput.IsMouseRightHeld())
            return;

        Vector2 delta = MREditorInput.ReadMouseDelta();
        lookYaw += delta.x * mouseLookSensitivity * 0.1f;
        lookPitch -= delta.y * mouseLookSensitivity * 0.1f;
        lookPitch = Mathf.Clamp(lookPitch, -80f, 80f);
        cam.transform.rotation = Quaternion.Euler(lookPitch, lookYaw, 0f);
    }

    void HandleYawKeys()
    {
        int dir = 0;
        if (MREditorInput.WasAnyPressed(KeyCode.LeftArrow, KeyCode.A))
            dir = -1;
        else if (MREditorInput.WasAnyPressed(KeyCode.RightArrow, KeyCode.D))
            dir = 1;

        if (dir == 0 || sky == null)
            return;

        yawDegrees = Mathf.Repeat(yawDegrees + dir * yawStepDegrees, 360f);
        sky.ApplyYawRotation(yawDegrees);
        statusLine = $"Yaw {yawDegrees:0}°";
    }

    void HandleSkyboxCycleKeys()
    {
        if (!MREditorInput.WasPressed(KeyCode.UpArrow))
            return;

        if (windowSkyboxKeys.Count == 0)
        {
            RefreshWindowSkyboxKeys();
            if (windowSkyboxKeys.Count == 0)
            {
                statusLine = "No skyboxes in Room Skins/Window";
                return;
            }
        }

        skyboxIndex = (skyboxIndex + 1) % windowSkyboxKeys.Count;
        ApplySkyboxAtIndex(skyboxIndex);
    }

    static void DisableLegacySceneJunk()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root == null)
                continue;
            // Old test cubes / leftover Glass quad — Window prefab owns the opening.
            if (root.name == "SkyboxPortal"
                || root.name == "SkyboxPortalMask"
                || root.name == "Glass"
                || root.name == "WallFrame")
                root.SetActive(false);
        }
    }

    void ConfigureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            statusLine = "ERROR: no Main Camera";
            return;
        }

        // Keep authored scene pose — only fix broken scale and clear flags for the portal test.
        if (cam.transform.localScale.z <= 0.0001f)
            cam.transform.localScale = Vector3.one;

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);

        Vector3 euler = cam.transform.rotation.eulerAngles;
        lookYaw = euler.y;
        lookPitch = euler.x > 180f ? euler.x - 360f : euler.x;
    }

    /// <summary>
    /// Spawn / reuse <c>Assets/ramiro/Prefabs/Window.prefab</c> and assign the stencil
    /// mask material on child <c>SkyBoxPortalMask</c> (same as MR runtime).
    /// </summary>
    void EnsureWindowPrefab()
    {
        Material mask = LoadMaterial(MaskMatPath) ?? CreateRuntimeMaskMaterial();
        if (mask != null)
            runtimeMaskMaterial = new Material(mask) { name = "SkyboxPortalMask (Test)" };

        windowInstance = GameObject.Find("Window");
        if (windowInstance == null)
        {
            GameObject prefab = ResolveWindowPrefab();
            if (prefab == null)
            {
                statusLine = "ERROR: Window prefab missing (Assets/ramiro/Prefabs/Window.prefab)";
                ConfigManager.WriteConsoleError($"{LogPrefix} Window prefab not found");
                return;
            }

            windowInstance = Instantiate(prefab);
            windowInstance.name = "Window";
            ConfigManager.WriteConsole($"{LogPrefix} instantiated Window prefab '{prefab.name}'");
        }
        else
            ConfigManager.WriteConsole($"{LogPrefix} using Window already in scene");

        windowInstance.SetActive(true);
        windowInstance.transform.position = new Vector3(0f, 0f, 0.15f);
        windowInstance.transform.rotation = Quaternion.identity;
        windowInstance.transform.localScale = Vector3.one;

        Transform glassChild = FindDeepChild(windowInstance.transform, GlassChildName);
        if (glassChild == null)
        {
            statusLine = $"ERROR: Window prefab has no child '{GlassChildName}'";
            ConfigManager.WriteConsoleError($"{LogPrefix} missing {GlassChildName} on Window prefab");
            return;
        }

        MeshRenderer[] renderers = glassChild.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            if (renderer == null)
                continue;
            if (runtimeMaskMaterial != null)
                renderer.sharedMaterial = runtimeMaskMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        // Colliders on the opening are not needed for this visual test.
        Collider[] cols = glassChild.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] != null)
                Destroy(cols[i]);
        }
    }

    GameObject ResolveWindowPrefab()
    {
        if (windowPrefabOverride != null)
            return windowPrefabOverride;

        GameObject fromSettings = MRRuntimeSettings.WindowPrefab;
        if (fromSettings != null)
            return fromSettings;

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>(WindowPrefabPath);
#else
        return null;
#endif
    }

    void EnsureSkySphere()
    {
        MRSkyboxPortalSky existing = Object.FindObjectOfType<MRSkyboxPortalSky>();
        if (existing != null)
            Destroy(existing.gameObject);

        Material portalMat = LoadMaterial(PortalMatPath);
        RefreshWindowSkyboxKeys();
        skyboxIndex = ResolveInitialSkyboxIndex();
        panorama = LoadSkyboxByIndex(skyboxIndex, out panoramaSource, out ownsPanorama);

        if (portalMat == null)
        {
            Shader shader = Shader.Find("ramiro/SkyboxPortal");
            if (shader != null)
                portalMat = new Material(shader) { name = "SkyboxPortal (Test Runtime)" };
        }

        if (portalMat != null)
        {
            runtimeSkyMaterial = new Material(portalMat);
            runtimeSkyMaterial.name = "SkyboxPortal (Test Instance)";
            if (panorama != null && runtimeSkyMaterial.HasProperty("_MainTex"))
                runtimeSkyMaterial.SetTexture("_MainTex", panorama);
            if (runtimeSkyMaterial.HasProperty("_Tint"))
                runtimeSkyMaterial.SetColor("_Tint", Color.white);
            if (runtimeSkyMaterial.HasProperty("_Exposure"))
                runtimeSkyMaterial.SetFloat("_Exposure", 1.2f);
        }

        var skyGo = new GameObject("SkyboxPortalSky");
        sky = skyGo.AddComponent<MRSkyboxPortalSky>();
        sky.Configure(runtimeSkyMaterial, panorama, radius: 50f);

        statusLine = panorama != null
            ? $"OK '{panorama.name}' {panorama.width}x{panorama.height} ({skyboxIndex + 1}/{Mathf.Max(1, windowSkyboxKeys.Count)})"
            : $"ERROR: no image in {Path.Combine(MRPaths.RoomSkinsDir, "Window")}";
    }

    void ApplySkyboxAtIndex(int index)
    {
        if (index < 0 || index >= windowSkyboxKeys.Count)
            return;

        Texture2D next = LoadSkyboxByIndex(index, out string source, out bool owned);
        if (next == null)
        {
            statusLine = $"Failed to load {windowSkyboxKeys[index]}";
            return;
        }

        if (ownsPanorama && panorama != null)
            Destroy(panorama);

        panorama = next;
        ownsPanorama = owned;
        panoramaSource = source;
        skyboxIndex = index;

        if (runtimeSkyMaterial != null && runtimeSkyMaterial.HasProperty("_MainTex"))
            runtimeSkyMaterial.SetTexture("_MainTex", panorama);

        sky?.SetSkyTexture(panorama);
        sky?.ApplyYawRotation(yawDegrees);
        RefreshDebugFullSkyTexture();

        statusLine = $"Skybox {index + 1}/{windowSkyboxKeys.Count}: {panorama.name}";
        ConfigManager.WriteConsole($"{LogPrefix} skybox -> {source}");
    }

    void RefreshDebugFullSkyTexture()
    {
        if (debugFullSky == null || panorama == null)
            return;

        MeshRenderer renderer = debugFullSky.GetComponent<MeshRenderer>();
        if (renderer == null || renderer.sharedMaterial == null)
            return;

        if (renderer.sharedMaterial.HasProperty("_MainTex"))
            renderer.sharedMaterial.mainTexture = panorama;
    }

    void RefreshWindowSkyboxKeys()
    {
        windowSkyboxKeys.Clear();
        MRPaths.EnsureFolders();
        MRRoomSkinCatalog.SeedBuiltInPackagesToDevice();

        string windowDir = Path.Combine(MRPaths.RoomSkinsDir, "Window");
        if (!Directory.Exists(windowDir))
            return;

        foreach (string file in Directory
                     .EnumerateFiles(windowDir, "*.*", SearchOption.TopDirectoryOnly)
                     .OrderBy(path => path, System.StringComparer.OrdinalIgnoreCase))
        {
            if (!MRPostersCatalog.IsSupportedImageFile(file))
                continue;

            string name = Path.GetFileName(file);
            if (string.IsNullOrEmpty(name))
                continue;

            windowSkyboxKeys.Add($"Window/{name}");
        }
    }

    int ResolveInitialSkyboxIndex()
    {
        if (windowSkyboxKeys.Count == 0)
            return 0;

        if (MRRoomSkinCatalog.TryGetDefaultWindowSkyboxKey(out string preferred))
        {
            int idx = windowSkyboxKeys.FindIndex(k =>
                string.Equals(k, preferred, System.StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
                return idx;
        }

        return 0;
    }

    Texture2D LoadSkyboxByIndex(int index, out string source, out bool owned)
    {
        source = "none";
        owned = false;

        if (index >= 0 && index < windowSkyboxKeys.Count)
        {
            string key = windowSkyboxKeys[index];
            if (MRRoomSkinDefinition.TryLoad(key, out MRRoomSkinDefinition skin))
            {
                Texture2D fromCabs = MRRoomSkinCatalog.LoadSkyboxTexture(skin);
                if (fromCabs != null)
                {
                    source = Path.Combine(MRPaths.RoomSkinsDir, key.Replace('/', Path.DirectorySeparatorChar));
                    owned = true;
                    return fromCabs;
                }
            }

            ConfigManager.WriteConsoleWarning($"{LogPrefix} failed cabs load '{key}'");
        }

#if UNITY_EDITOR
        Texture2D fromAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(PanoramaAssetPath);
        if (fromAsset != null)
        {
            source = PanoramaAssetPath;
            return fromAsset;
        }
#endif
        Texture2D fromResources = Resources.Load<Texture2D>(PanoramaResourcesPath);
        if (fromResources != null)
        {
            source = "Resources/" + PanoramaResourcesPath;
            return fromResources;
        }

        Texture2D fromSettings = MRRuntimeSettings.DefaultSkyboxImage;
        if (fromSettings != null)
        {
            source = "MRRuntimeSettings.defaultSkyboxImage";
            return fromSettings;
        }

        return null;
    }

    void OnDestroy()
    {
        if (ownsPanorama && panorama != null)
        {
            Destroy(panorama);
            panorama = null;
            ownsPanorama = false;
        }

        if (runtimeSkyMaterial != null)
        {
            Destroy(runtimeSkyMaterial);
            runtimeSkyMaterial = null;
        }

        if (runtimeMaskMaterial != null)
        {
            Destroy(runtimeMaskMaterial);
            runtimeMaskMaterial = null;
        }
    }

    void ApplyDebugStencilMode(bool showFullSky)
    {
        if (showFullSky)
        {
            EnsureDebugFullSkySphere(true);
            if (sky != null)
                sky.gameObject.SetActive(false);
        }
        else
        {
            EnsureDebugFullSkySphere(false);
            if (sky != null)
                sky.gameObject.SetActive(true);
        }
    }

    void EnsureDebugFullSkySphere(bool enabled)
    {
        if (!enabled)
        {
            if (debugFullSky != null)
                debugFullSky.SetActive(false);
            return;
        }

        if (debugFullSky == null)
        {
            debugFullSky = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugFullSky.name = "DebugFullSky";
            Destroy(debugFullSky.GetComponent<Collider>());
            debugFullSky.transform.localScale = Vector3.one * 100f;

            var renderer = debugFullSky.GetComponent<MeshRenderer>();
            Shader unlit = Shader.Find("Unlit/Texture") ?? Shader.Find("Unlit/Color");
            var mat = new Material(unlit != null ? unlit : Shader.Find("Standard"));
            if (panorama != null && mat.HasProperty("_MainTex"))
                mat.mainTexture = panorama;
            else if (mat.HasProperty("_Color"))
                mat.color = Color.magenta;
            // Invert normals visually by scaling negative X so we see inside.
            debugFullSky.transform.localScale = new Vector3(-100f, 100f, 100f);
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        debugFullSky.SetActive(true);
        if (Camera.main != null)
            debugFullSky.transform.position = Camera.main.transform.position;
    }

    void LogDiagnostics()
    {
        Texture matTex = runtimeSkyMaterial != null && runtimeSkyMaterial.HasProperty("_MainTex")
            ? runtimeSkyMaterial.GetTexture("_MainTex")
            : null;

        ConfigManager.WriteConsole(
            $"{LogPrefix} window={(windowInstance != null ? windowInstance.name : "NULL")} " +
            $"source={panoramaSource} " +
            $"panorama={(panorama != null ? panorama.name + $" {panorama.width}x{panorama.height}" : "NULL")} " +
            $"matMainTex={(matTex != null ? matTex.name : "NULL")} " +
            $"shader={(runtimeSkyMaterial != null ? runtimeSkyMaterial.shader.name : "NULL")} " +
            $"roomSkins={MRPaths.RoomSkinsDir}");
    }

    static Transform FindDeepChild(Transform root, string name)
    {
        if (root == null)
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform found = FindDeepChild(go.transform, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        if (root.name == name)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeepChild(root.GetChild(i), name);
            if (found != null)
                return found;
        }

        return null;
    }

    static Material CreateRuntimeMaskMaterial()
    {
        Shader shader = Shader.Find("ramiro/SkyboxPortalMask");
        return shader != null ? new Material(shader) { name = "SkyboxPortalMask (Test)" } : null;
    }

    static Material LoadMaterial(string assetPath)
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
#else
        return null;
#endif
    }
}
