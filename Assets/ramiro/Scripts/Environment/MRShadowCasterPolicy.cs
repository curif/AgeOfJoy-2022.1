/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Reduces realtime MR shadow cost by retaining casters only on substantial opaque geometry.
/// Applied to runtime MR instances, so VR scene assets are never modified.
/// </summary>
public static class MRShadowCasterPolicy
{
    const float MinimumCasterSizeMeters = 0.35f;

    static readonly string[] NonCasterNameTokens =
    {
        "screen",
        "display",
        "crt",
        "glass",
        "glow",
        "emiss",
        "particle",
        "beam",
        "line"
    };

    public static void Apply(GameObject root)
    {
        if (root == null)
            return;

        bool isPoster = root.GetComponentInChildren<MRWallPoster>(true) != null;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || ShouldKeepShadows(renderer, isPoster))
                continue;

            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    static bool ShouldKeepShadows(Renderer renderer, bool isPoster)
    {
        if (isPoster
            || renderer is ParticleSystemRenderer
            || renderer is LineRenderer
            || renderer is TrailRenderer)
            return false;

        string rendererName = renderer.transform.name;
        foreach (string token in NonCasterNameTokens)
        {
            if (rendererName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
        }

        if (UsesTransparentMaterial(renderer))
            return false;

        Vector3 size = renderer.bounds.size;
        float largestDimension = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
        return largestDimension >= MinimumCasterSizeMeters;
    }

    static bool UsesTransparentMaterial(Renderer renderer)
    {
        foreach (Material material in renderer.sharedMaterials)
        {
            if (material == null)
                continue;

            if (material.renderQueue >= (int)RenderQueue.Transparent)
                return true;

            string renderType = material.GetTag("RenderType", searchFallbacks: false, defaultValue: string.Empty);
            if (string.Equals(renderType, "Transparent", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
