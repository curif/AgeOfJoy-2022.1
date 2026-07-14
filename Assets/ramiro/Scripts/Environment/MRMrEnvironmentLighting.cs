/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// MR global fill lighting — cool white wrap so every virtual object is lit from all sides.
/// Uses strong ambient + two opposite directionals (key with soft shadows, fill without).
/// Toggle + intensity via CONFIG → GLOBAL LIGHT. Catalog point lights stay separate.
/// </summary>
public class MRMrEnvironmentLighting : MonoBehaviour
{
    const string LogPrefix = "[MRMrEnvironmentLighting]";

    /// <summary>Key “sun” angle — cool daylight, not yellow.</summary>
    [SerializeField] Vector3 keyEulerAngles = new Vector3(55f, -40f, 0f);
    [SerializeField] Color lightColor = new Color(0.90f, 0.95f, 1f);
    [SerializeField] Color ambientColor = new Color(0.72f, 0.76f, 0.82f);
    [SerializeField] float ambientBase = 0.85f;
    [SerializeField] float keyRelative = 0.55f;
    [SerializeField] float fillRelative = 0.40f;

    GameObject lightingRoot;
    Light keyLight;
    Light fillLight;
    AmbientMode savedAmbientMode;
    Color savedAmbientLight;
    float savedAmbientIntensity;
    bool ambientSaved;

    public bool IsSpawned => lightingRoot != null;

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

        if (lightingRoot != null)
        {
            ApplyIntensity(MRAutoLightingSettings.Intensity);
            return;
        }

        lightingRoot = new GameObject("MRLighting");
        lightingRoot.transform.SetParent(mrSpaceOrigin, false);
        lightingRoot.transform.localPosition = Vector3.zero;
        lightingRoot.transform.localRotation = Quaternion.identity;

        CreateWrapLights();
        ApplyIntensity(MRAutoLightingSettings.Intensity);
        ConfigManager.WriteConsole(
            $"{LogPrefix} spawned global cool fill intensity={MRAutoLightingSettings.Intensity:F1}");
    }

    public void Despawn(bool restoreAmbient = false)
    {
        if (restoreAmbient)
            RestoreAmbientFill();

        if (lightingRoot != null)
            Destroy(lightingRoot);
        lightingRoot = null;
        keyLight = null;
        fillLight = null;
    }

    /// <summary>Called when leaving MR — remove runtime lights and undo ambient fill.</summary>
    public void DespawnForMrExit()
    {
        Despawn(restoreAmbient: true);
    }

    public void ApplyIntensity(float intensity)
    {
        intensity = Mathf.Max(0f, intensity);

        if (keyLight != null)
            keyLight.intensity = intensity * keyRelative;
        if (fillLight != null)
            fillLight.intensity = intensity * fillRelative;

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

    void CreateWrapLights()
    {
        // Key + fill opposite each other, no shadows — models receive light from both hemispheres.
        var keyGo = new GameObject("MRGlobalKey");
        keyGo.transform.SetParent(lightingRoot.transform, false);
        keyGo.transform.localRotation = Quaternion.Euler(keyEulerAngles);
        keyLight = keyGo.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = lightColor;
        keyLight.shadows = LightShadows.Soft;

        var fillGo = new GameObject("MRGlobalFill");
        fillGo.transform.SetParent(lightingRoot.transform, false);
        fillGo.transform.localRotation = Quaternion.Euler(keyEulerAngles) * Quaternion.Euler(0f, 180f, 0f);
        fillLight = fillGo.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = lightColor;
        // Fill only softens the dark side — keep shadows on the key so cabinets cast contact shadows.
        fillLight.shadows = LightShadows.None;
    }
}

