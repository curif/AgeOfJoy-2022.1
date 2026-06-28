/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Fixes GLTFUtility materials that fail to render (missing/unsupported shaders).</summary>
public static class MRCustomObjectMaterialFix
{
    const string LogPrefix = "[MRCustomObjectMaterialFix]";

    public static void Apply(GameObject root, string packageName)
    {
        if (root == null)
            return;

        Shader fallback = ResolveFallbackShader();
        if (fallback == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {packageName}: no fallback shader found");
            return;
        }

        int fixedCount = 0;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.materials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat == null || !NeedsFallbackShader(mat))
                    continue;

                Material replacement = new Material(fallback);
                CopySurfaceProperties(mat, replacement);
                materials[i] = replacement;
                changed = true;
                fixedCount++;
            }

            if (changed)
                renderer.materials = materials;
        }

        if (fixedCount > 0)
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} {packageName}: replaced {fixedCount} material slot(s) with {fallback.name}");
        }
    }

    static bool NeedsFallbackShader(Material mat)
    {
        if (mat.shader == null)
            return true;

        if (!mat.shader.isSupported)
            return true;

        string shaderName = mat.shader.name ?? string.Empty;
        if (shaderName.StartsWith("Hidden/"))
            return true;

        if (shaderName.Contains("Error"))
            return true;

        // GLTFUtility shaders that did not resolve on device.
        if (shaderName.Contains("GLTF") && !mat.HasProperty("_Color") && !mat.HasProperty("_BaseColor"))
            return true;

        return false;
    }

    static Shader ResolveFallbackShader()
    {
        Shader shader = Shader.Find("Standard");
        if (shader != null)
            return shader;

        shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null)
            return shader;

        return Shader.Find("Unlit/Texture");
    }

    static void CopySurfaceProperties(Material source, Material destination)
    {
        Texture mainTex = null;
        if (source.HasProperty("_MainTex"))
            mainTex = source.GetTexture("_MainTex");
        if (mainTex == null && source.HasProperty("_BaseMap"))
            mainTex = source.GetTexture("_BaseMap");
        if (mainTex == null)
            mainTex = source.mainTexture;

        Color color = Color.white;
        if (source.HasProperty("_BaseColor"))
            color = source.GetColor("_BaseColor");
        else if (source.HasProperty("_Color"))
            color = source.GetColor("_Color");
        else
            color = source.color;

        color.a = 1f;

        if (destination.HasProperty("_MainTex") && mainTex != null)
            destination.SetTexture("_MainTex", mainTex);
        if (destination.HasProperty("_BaseMap") && mainTex != null)
            destination.SetTexture("_BaseMap", mainTex);

        if (destination.HasProperty("_Color"))
            destination.SetColor("_Color", color);
        if (destination.HasProperty("_BaseColor"))
            destination.SetColor("_BaseColor", color);

        destination.color = color;
        destination.mainTexture = mainTex;
    }
}
