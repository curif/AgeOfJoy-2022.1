/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Read/apply Unity Light intensity, range, and warm glow colors on MR light prefab instances.</summary>
public static class MRLightPlacement
{
    public const float TuneStep = 0.1f;
    public const float TemperatureStep = 100f;
    public const float MinIntensity = 0f;
    public const float MaxIntensity = 8f;
    public const float MinRange = 0.1f;
    public const float MaxRange = 20f;
    public const float MinTemperature = 2000f;
    public const float MaxTemperature = 12000f;
    public const float DefaultTemperature = 6500f;

    static readonly string[] GlowLayerNames = { "Core", "InnerGlow", "MidGlow", "OuterGlow", "EdgeGlow" };
    static readonly Color[] GlowLayerColors =
    {
        HexToColor("FFFFFF"),
        HexToColor("FFF4C2"),
        HexToColor("FFC45C"),
        HexToColor("FF7A1A"),
        HexToColor("CC2200"),
    };

    // AgeOfJoy/Neon_Houdini_Simple: outside (Edge) → inside (Core)
    static readonly (string property, int colorIndex)[] NeonShaderColorMap =
    {
        ("_ColorA", 4), // EdgeGlow
        ("_ColorB", 3), // OuterGlow
        ("_ColorC", 2), // MidGlow
        ("_ColorCore", 0), // Core (InnerGlow sits between Core and Mid on mesh UV)
    };

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    static readonly int EmissiveTintId = Shader.PropertyToID("_EmissiveTint");
    static readonly int EmissiveTintFresnelId = Shader.PropertyToID("_EmissiveTintFresnel");
    static readonly int UseEmissiveTintId = Shader.PropertyToID("_useEmissiveTint");

    public static bool TryReadPrefabDefaults(
        GameObject prefab,
        out float intensity,
        out float range,
        out float temperature)
    {
        intensity = 0f;
        range = 0f;
        temperature = 0f;
        if (prefab == null)
            return false;

        Light light = FindPrimaryLight(prefab);
        if (light == null)
            return false;

        intensity = light.intensity;
        range = light.range;
        temperature = ReadTemperature(light);
        return true;
    }

    public static bool TryReadInstanceValues(
        GameObject instance,
        out float intensity,
        out float range,
        out float temperature)
    {
        intensity = 0f;
        range = 0f;
        temperature = 0f;
        if (instance == null)
            return false;

        Light light = FindPrimaryLight(instance);
        if (light == null)
            return false;

        intensity = light.intensity;
        range = light.range;
        temperature = ReadTemperature(light);
        return true;
    }

    public static void ApplyAllLights(GameObject root, float intensity, float range, float temperature)
    {
        if (root == null)
            return;

        float kelvin = SnapTemperature(temperature);
        Color lightColor = ResolveTemperatureColor(kelvin);

        Light[] lights = root.GetComponentsInChildren<Light>(true);
        if (lights != null)
        {
            foreach (Light light in lights)
            {
                if (light == null)
                    continue;

                light.intensity = intensity;
                light.range = range;
                light.useColorTemperature = false;
                light.color = lightColor;
            }
        }

        ApplyGlowLayerColors(root, kelvin);
    }

    public static float SnapTune(float value, float min, float max) =>
        Mathf.Clamp(Mathf.Round(value / TuneStep) * TuneStep, min, max);

    public static float SnapTemperature(float value) =>
        Mathf.Clamp(Mathf.Round(value / TemperatureStep) * TemperatureStep, MinTemperature, MaxTemperature);

    /// <summary>Sample warm→cool gradient: 2000K = EdgeGlow, 12000K = Core.</summary>
    public static Color ResolveTemperatureColor(float kelvin)
    {
        float t = Mathf.InverseLerp(MinTemperature, MaxTemperature, kelvin);
        float scaled = t * (GlowLayerColors.Length - 1);
        int i = Mathf.FloorToInt(scaled);
        int j = Mathf.Min(i + 1, GlowLayerColors.Length - 1);
        float frac = scaled - i;
        return Color.Lerp(GlowLayerColors[i], GlowLayerColors[j], frac);
    }

    static void ApplyGlowLayerColors(GameObject root, float kelvin)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
                continue;

            if (TryGetLayerIndex(renderer.transform.name, out int layerIndex))
            {
                ApplyColorToRenderer(renderer, ResolveLayerColor(layerIndex, kelvin));
                continue;
            }

            if (ApplyNamedShaderLayerColors(renderer, kelvin))
                continue;

            if (ApplyNeonShaderColors(renderer, kelvin))
                continue;

            ApplyLitVtxGlowColors(renderer, kelvin);
        }
    }

    static Color ResolveLayerColor(int layerIndex, float kelvin)
    {
        Color layer = GlowLayerColors[layerIndex];
        Color sample = ResolveTemperatureColor(kelvin);
        float coolBlend = Mathf.InverseLerp(MinTemperature, MaxTemperature, kelvin);
        return Color.Lerp(layer, sample, coolBlend);
    }

    static bool TryGetLayerIndex(string objectName, out int layerIndex)
    {
        for (int i = 0; i < GlowLayerNames.Length; i++)
        {
            if (objectName != GlowLayerNames[i])
                continue;

            layerIndex = i;
            return true;
        }

        layerIndex = -1;
        return false;
    }

    static bool ApplyNamedShaderLayerColors(Renderer renderer, float kelvin)
    {
        Material mat = renderer.material;
        if (mat == null)
            return false;

        bool applied = false;
        for (int i = 0; i < GlowLayerNames.Length; i++)
        {
            string property = "_" + GlowLayerNames[i];
            if (!mat.HasProperty(property))
                continue;

            mat.SetColor(property, ResolveLayerColor(i, kelvin));
            applied = true;
        }

        return applied;
    }

    static bool ApplyNeonShaderColors(Renderer renderer, float kelvin)
    {
        Material mat = renderer.material;
        if (mat == null)
            return false;

        bool applied = false;
        foreach ((string property, int colorIndex) in NeonShaderColorMap)
        {
            if (!mat.HasProperty(property))
                continue;

            mat.SetColor(property, ResolveLayerColor(colorIndex, kelvin));
            applied = true;
        }

        return applied;
    }

    static void ApplyLitVtxGlowColors(Renderer renderer, float kelvin)
    {
        Material mat = renderer.material;
        if (mat == null)
            return;

        if (!mat.HasProperty(EmissiveTintId) && !mat.HasProperty(EmissiveTintFresnelId))
            return;

        if (mat.HasProperty(UseEmissiveTintId))
            mat.SetFloat(UseEmissiveTintId, 1f);

        if (mat.HasProperty(EmissiveTintId))
            mat.SetColor(EmissiveTintId, ResolveLayerColor(1, kelvin));
        if (mat.HasProperty(EmissiveTintFresnelId))
            mat.SetColor(EmissiveTintFresnelId, ResolveLayerColor(4, kelvin));
    }

    static void ApplyColorToRenderer(Renderer renderer, Color color)
    {
        Material mat = renderer.material;
        if (mat == null)
            return;

        if (mat.HasProperty(ColorId))
            mat.SetColor(ColorId, color);
        if (mat.HasProperty(EmissionColorId))
            mat.SetColor(EmissionColorId, color);
        if (mat.HasProperty(EmissiveTintId))
            mat.SetColor(EmissiveTintId, color);

        foreach ((string property, int colorIndex) in NeonShaderColorMap)
        {
            if (mat.HasProperty(property))
                mat.SetColor(property, color);
        }
    }

    static float ReadTemperature(Light light)
    {
        if (light == null)
            return DefaultTemperature;

        if (light.useColorTemperature && light.colorTemperature > 0f)
            return light.colorTemperature;

        if (light.colorTemperature > 0f)
            return light.colorTemperature;

        if (!light.useColorTemperature)
            return EstimateTemperatureFromColor(light.color);

        return DefaultTemperature;
    }

    static float EstimateTemperatureFromColor(Color color)
    {
        float bestKelvin = DefaultTemperature;
        float bestDistance = float.MaxValue;

        for (float kelvin = MinTemperature; kelvin <= MaxTemperature; kelvin += TemperatureStep)
        {
            float distance = ColorDistance(color, ResolveTemperatureColor(kelvin));
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestKelvin = kelvin;
        }

        return bestKelvin;
    }

    static float ColorDistance(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return dr * dr + dg * dg + db * db;
    }

    static Light FindPrimaryLight(GameObject root)
    {
        if (root == null)
            return null;

        Light onRoot = root.GetComponent<Light>();
        if (onRoot != null)
            return onRoot;

        return root.GetComponentInChildren<Light>(true);
    }

    static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Color.white;

        if (hex[0] == '#')
            hex = hex.Substring(1);

        if (!ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            return Color.white;

        return color;
    }
}
