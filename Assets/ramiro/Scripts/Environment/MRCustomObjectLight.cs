/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Runtime Unity Light driven by object.yaml <c>light</c> (point or spot).</summary>
[DisallowMultipleComponent]
public class MRCustomObjectLight : MonoBehaviour
{
    const string LogPrefix = "[MRCustomObjectLight]";
    const string LightChildName = "MRCustomObjectLight";

    Light attachedLight;

    public Light AttachedLight => attachedLight;

    public void Configure(string packageName, MRCustomObjectLightYaml config, Transform attachParent)
    {
        if (config == null)
            config = new MRCustomObjectLightYaml();

        if (attachParent == null)
            attachParent = transform;

        if (!TryResolveLightType(config.Type, out LightType lightType))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} {packageName}: unknown light.type '{config.Type}', using point");
            lightType = LightType.Point;
        }

        attachedLight = EnsureLight(attachParent);
        attachedLight.type = lightType;
        attachedLight.intensity = Mathf.Max(0f, config.Intensity);
        attachedLight.range = Mathf.Max(0.01f, config.Range > 0f ? config.Range : 4f);
        attachedLight.color = config.Color != null ? config.Color.ToColor() : Color.white;
        attachedLight.useColorTemperature = false;
        attachedLight.shadows = config.Shadows ? LightShadows.Soft : LightShadows.None;

        if (lightType == LightType.Spot)
        {
            float outer = Mathf.Clamp(config.SpotAngle > 0f ? config.SpotAngle : 60f, 1f, 179f);
            float inner = Mathf.Clamp(
                config.InnerSpotAngle > 0f ? config.InnerSpotAngle : outer * 0.5f,
                0f,
                outer);
            attachedLight.spotAngle = outer;
            attachedLight.innerSpotAngle = inner;
        }

        attachedLight.enabled = true;

        ConfigManager.WriteConsole(
            $"{LogPrefix} {packageName}: {lightType} on '{attachParent.name}' "
            + $"intensity={attachedLight.intensity:F2} range={attachedLight.range:F2} "
            + $"color=({attachedLight.color.r:F2},{attachedLight.color.g:F2},{attachedLight.color.b:F2})"
            + (lightType == LightType.Spot
                ? $" spotAngle={attachedLight.spotAngle:F0} inner={attachedLight.innerSpotAngle:F0}"
                : string.Empty));
    }

    static bool TryResolveLightType(string typeName, out LightType lightType)
    {
        lightType = LightType.Point;
        if (string.IsNullOrEmpty(typeName)
            || string.Equals(typeName, MRCustomObjectLightYaml.TypePoint, System.StringComparison.OrdinalIgnoreCase))
        {
            lightType = LightType.Point;
            return true;
        }

        if (string.Equals(typeName, MRCustomObjectLightYaml.TypeSpot, System.StringComparison.OrdinalIgnoreCase))
        {
            lightType = LightType.Spot;
            return true;
        }

        return false;
    }

    static Light EnsureLight(Transform attachParent)
    {
        Light existing = attachParent.GetComponent<Light>();
        if (existing != null)
            return existing;

        Transform child = attachParent.Find(LightChildName);
        if (child != null)
        {
            Light childLight = child.GetComponent<Light>();
            if (childLight != null)
                return childLight;
        }

        GameObject lightGo = new GameObject(LightChildName);
        lightGo.transform.SetParent(attachParent, worldPositionStays: false);
        lightGo.transform.localPosition = Vector3.zero;
        lightGo.transform.localRotation = Quaternion.identity;
        lightGo.transform.localScale = Vector3.one;
        return lightGo.AddComponent<Light>();
    }
}
