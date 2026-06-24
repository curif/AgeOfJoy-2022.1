/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>CRT screen shader bound to a custom material template (e.g. ScreenPortableGames).</summary>
public class PortableShaderCRT : ShaderScreenBase
{
    Vector4? v4Invert;

    public PortableShaderCRT(
        Renderer display,
        int position,
        Dictionary<string, string> config,
        Material template)
        : base(display, position, config, new CabinetMaterials.MaterialCRTShaderProperties())
    {
        material = template != null ? Object.Instantiate(template) : null;
        ApplyConfiguration();
    }

    public override string Name => "PortableCRT";

    public override string TargetMaterialProperty => "_MainTex";

    public override Texture Texture
    {
        get => material.GetTexture("_MainTex");
        set
        {
            Texture t = value;
            Vector4 crtParameters = new Vector4(t.width, t.height, 0f, 0f);
            material.SetTexture("_MainTex", t);
            material.SetVector("_CRTParameters", crtParameters);
            if (v4Invert != null)
                material.SetVector("_CRTTiling", (Vector4)v4Invert);
        }
    }

    public override void Refresh(Texture texture) => Texture = texture;

    public override ShaderScreenBase Invert(bool invertx, bool inverty)
    {
        v4Invert = new Vector4(invertx ? -1f : 1f, inverty ? -1f : 1f, 0, 0);
        material.SetVector("_CRTTiling", (Vector4)v4Invert);
        return this;
    }

    public void ApplyContentScale(float scaleX, float scaleY)
    {
        if (material == null)
            return;

        string prop = TargetMaterialProperty;
        material.SetTextureScale(prop, new Vector2(scaleX, scaleY));
        material.SetTextureOffset(prop, new Vector2((1f - scaleX) * 0.5f, (1f - scaleY) * 0.5f));
    }

    public override string AlternativeShaderForAttractionVideos() => "crtlod";
}

public static class PortableShaderScreen
{
    public static ShaderScreenBase FromMaterial(
        Renderer display,
        int position,
        Material template,
        Dictionary<string, string> config)
    {
        return new PortableShaderCRT(display, position, config, template);
    }
}
