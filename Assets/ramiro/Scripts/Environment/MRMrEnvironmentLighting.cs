/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.Rendering;

/// <summary>MR room lighting — point lights only (no 3D lamp mesh) for passthrough visibility.</summary>
public class MRMrEnvironmentLighting : MonoBehaviour
{
    const string LogPrefix = "[MRMrEnvironmentLighting]";

    [SerializeField] float defaultCeilingHeightMeters = 2.6f;
    [SerializeField] float ceilingLampDropMeters = 0.18f;
    [SerializeField] float lampForwardOffsetMeters = 0.6f;
    [SerializeField] float ceilingLightRange = 5f;
    [SerializeField] float ceilingLightIntensity = 1.6f;
    [SerializeField] Color ceilingLightColor = new Color(0.82f, 0.93f, 1f);
    [SerializeField] float handFillLightHeightMeters = 1.85f;
    [SerializeField] float handFillLightIntensity = 1.35f;
    [SerializeField] float handFillLightRange = 3.5f;
    [SerializeField] Color handFillLightColor = new Color(1f, 0.95f, 0.88f);
    [SerializeField] Color ambientFillColor = new Color(0.32f, 0.32f, 0.36f);

    GameObject lightingRoot;
    AmbientMode savedAmbientMode;
    Color savedAmbientLight;
    float savedAmbientIntensity;
    bool ambientSaved;

    public void Spawn(Transform mrSpaceOrigin)
    {
        if (mrSpaceOrigin == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} spawn skipped — origin null");
            return;
        }

        if (lightingRoot != null)
            return;

        ApplyAmbientFill();

        lightingRoot = new GameObject("MRLighting");
        lightingRoot.transform.SetParent(mrSpaceOrigin, false);
        lightingRoot.transform.localPosition = Vector3.zero;
        lightingRoot.transform.localRotation = Quaternion.identity;

        Vector3 localCenter = ResolveLocalCenter(mrSpaceOrigin);
        Vector3 worldProbe = mrSpaceOrigin.TransformPoint(
            new Vector3(localCenter.x, 0f, localCenter.z + lampForwardOffsetMeters));

        float lampLocalY = defaultCeilingHeightMeters - ceilingLampDropMeters;
        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        if (surfaces != null && surfaces.TryGetCeilingPointAt(worldProbe, out Vector3 ceilingWorld))
        {
            lampLocalY = mrSpaceOrigin.InverseTransformPoint(ceilingWorld).y - ceilingLampDropMeters;
        }
        else if (surfaces != null && surfaces.HasFloor && surfaces.HasCeiling)
        {
            lampLocalY = surfaces.CeilingHeight - surfaces.FloorHeight - ceilingLampDropMeters;
        }

        lampLocalY = Mathf.Max(lampLocalY, handFillLightHeightMeters + 0.35f);

        Vector3 lampLocalPos = new Vector3(localCenter.x, lampLocalY, localCenter.z + lampForwardOffsetMeters);
        Vector3 fillLocalPos = new Vector3(localCenter.x, handFillLightHeightMeters, localCenter.z + lampForwardOffsetMeters * 0.5f);

        CreateCeilingLight(lampLocalPos);
        CreateHandFillLight(fillLocalPos);
        ConfigManager.WriteConsole($"{LogPrefix} spawned lights only localY={lampLocalY:F2}");
    }

    public void Despawn()
    {
        RestoreAmbientFill();

        if (lightingRoot != null)
            Destroy(lightingRoot);
        lightingRoot = null;
    }

    void ApplyAmbientFill()
    {
        if (ambientSaved)
            return;

        savedAmbientMode = RenderSettings.ambientMode;
        savedAmbientLight = RenderSettings.ambientLight;
        savedAmbientIntensity = RenderSettings.ambientIntensity;
        ambientSaved = true;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientFillColor;
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

    static Vector3 ResolveLocalCenter(Transform mrSpaceOrigin)
    {
        Transform player = FindPlayerTransform();
        if (player == null)
            return Vector3.zero;

        return mrSpaceOrigin.InverseTransformPoint(player.position);
    }

    void CreateCeilingLight(Vector3 localPos)
    {
        var lampGo = new GameObject("MRCeilingLight");
        lampGo.transform.SetParent(lightingRoot.transform, false);
        lampGo.transform.localPosition = localPos;
        lampGo.transform.localRotation = Quaternion.identity;

        Light light = lampGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = ceilingLightRange;
        light.intensity = ceilingLightIntensity;
        light.color = ceilingLightColor;
        light.shadows = LightShadows.Soft;
    }

    void CreateHandFillLight(Vector3 localPos)
    {
        var fillGo = new GameObject("MRHandFillLight");
        fillGo.transform.SetParent(lightingRoot.transform, false);
        fillGo.transform.localPosition = localPos;

        Light light = fillGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = handFillLightRange;
        light.intensity = handFillLightIntensity;
        light.color = handFillLightColor;
        light.shadows = LightShadows.None;
    }

    static Transform FindPlayerTransform()
    {
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
            return pc.PlayerControllerGameObject.transform;
        if (pc != null)
            return pc.transform;

        var tagged = GameObject.FindGameObjectWithTag("Player");
        return tagged != null ? tagged.transform : null;
    }
}
