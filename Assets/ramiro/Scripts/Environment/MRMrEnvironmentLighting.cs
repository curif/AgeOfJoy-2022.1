/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// MR global fill lighting — neutral white wrap so every virtual object is lit from all sides.
/// Uses strong ambient + one directional key light with soft shadows.
/// Toggle + intensity via CONFIG → GLOBAL LIGHT. Catalog point lights stay separate.
/// Key light can be suspended while a Libretro game runs (ambient stays).
/// </summary>
public class MRMrEnvironmentLighting : MonoBehaviour
{
    const string LogPrefix = "[MRMrEnvironmentLighting]";

    /// <summary>Key “sun” angle — cool daylight, not yellow.</summary>
    [SerializeField] Vector3 keyEulerAngles = new Vector3(55f, -40f, 0f);
    [SerializeField] Color lightColor = new Color(0.96f, 0.97f, 1f);
    [SerializeField] Color ambientColor = new Color(0.78f, 0.78f, 0.78f);
    [SerializeField] float ambientBase = 0.85f;
    [SerializeField] float keyRelative = 0.55f;
    [SerializeField] bool spawnOnStart;

    GameObject lightingRoot;
    Light keyLight;
    AmbientMode savedAmbientMode;
    Color savedAmbientLight;
    float savedAmbientIntensity;
    bool ambientSaved;
    bool keySuspendedForGameplay;

    public bool IsSpawned => lightingRoot != null;

    /// <summary>True when the directional key is held off for an active Libretro session.</summary>
    public bool IsKeySuspendedForGameplay => keySuspendedForGameplay;

    void Start()
    {
        if (spawnOnStart)
            Spawn(transform);
    }

    void OnDestroy()
    {
        if (spawnOnStart)
            Despawn(restoreAmbient: true);
    }

    public void Spawn(Transform mrSpaceOrigin)
    {
        MRAutoLightingSettings.EnsureLoaded();
        if (!MRAutoLightingSettings.Enabled)
        {
            Despawn(restoreAmbient: false);
            return;
        }

        if (mrSpaceOrigin == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} spawn skipped — origin null");
            return;
        }

        if (lightingRoot == null)
        {
            lightingRoot = new GameObject("MRLighting");
            lightingRoot.transform.SetParent(mrSpaceOrigin, false);
            lightingRoot.transform.localPosition = Vector3.zero;
            lightingRoot.transform.localRotation = Quaternion.identity;
            ConfigManager.WriteConsole(
                $"{LogPrefix} spawned global fill intensity={MRAutoLightingSettings.Intensity:F1}");
        }

        if (!keySuspendedForGameplay)
            EnsureKeyLight();
        else
            DestroyKeyLight();

        ApplyIntensity(MRAutoLightingSettings.Intensity);
    }

    public void Despawn(bool restoreAmbient = false)
    {
        if (restoreAmbient)
            RestoreAmbientFill();

        if (lightingRoot != null)
            Destroy(lightingRoot);
        lightingRoot = null;
        keyLight = null;
    }

    /// <summary>Called when leaving MR — remove runtime lights and undo ambient fill.</summary>
    public void DespawnForMrExit()
    {
        keySuspendedForGameplay = false;
        Despawn(restoreAmbient: true);
    }

    /// <summary>
    /// Temporarily remove the shadow-casting key light while a game runs.
    /// Does not change PlayerPrefs; ambient Flat stays so cabinets remain readable.
    /// </summary>
    public void SetKeySuspendedForGameplay(bool suspended)
    {
        if (keySuspendedForGameplay == suspended)
            return;

        keySuspendedForGameplay = suspended;

        if (!IsSpawned)
            return;

        if (suspended)
        {
            DestroyKeyLight();
            ConfigManager.WriteConsole($"{LogPrefix} key suspended for gameplay (ambient kept)");
            return;
        }

        MRAutoLightingSettings.EnsureLoaded();
        if (!MRAutoLightingSettings.Enabled)
            return;

        EnsureKeyLight();
        ApplyIntensity(MRAutoLightingSettings.Intensity);
        ConfigManager.WriteConsole($"{LogPrefix} key resumed after gameplay");
    }

    public void ApplyIntensity(float intensity)
    {
        intensity = Mathf.Max(0f, intensity);

        if (keyLight != null)
            keyLight.intensity = intensity * keyRelative;

        ApplyAmbientFill(intensity);
    }

    void ApplyAmbientFill(float intensity)
    {
        if (!ambientSaved)
        {
            savedAmbientMode = RenderSettings.ambientMode;
            savedAmbientLight = RenderSettings.ambientLight;
            savedAmbientIntensity = RenderSettings.ambientIntensity;
            ambientSaved = true;
        }

        // Ambient is what truly wraps every surface — scale it with the Global Light intensity.
        float ambientScale = ambientBase * Mathf.Clamp(intensity, 0.1f, 5f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor * ambientScale;
        RenderSettings.ambientIntensity = 1f;
    }

    void RestoreAmbientFill()
    {
        if (!ambientSaved)
            return;

        RenderSettings.ambientMode = savedAmbientMode;
        RenderSettings.ambientLight = savedAmbientLight;
        RenderSettings.ambientIntensity = savedAmbientIntensity;
        ambientSaved = false;
    }

    void EnsureKeyLight()
    {
        if (lightingRoot == null || keyLight != null)
            return;

        var keyGo = new GameObject("MRGlobalKey");
        keyGo.transform.SetParent(lightingRoot.transform, false);
        keyGo.transform.localRotation = Quaternion.Euler(keyEulerAngles);
        keyLight = keyGo.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = lightColor;
        keyLight.shadows = LightShadows.Soft;
    }

    void DestroyKeyLight()
    {
        if (keyLight == null)
            return;

        Destroy(keyLight.gameObject);
        keyLight = null;
    }
}
