using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;

public class ShaderScreenClean : ShaderScreenBase
{
    private bool actual_invertx = false, actual_inverty = false;
    private string damage = "none";

    static Material ScreenCleanScanlinesBigPattern;
    static Material ScreenCleanScanlinesBlackBorderPattern;
    static Material ScreenCleanScanlines;
    static Material ScreenClean;
    static ShaderScreenClean()
    {
        ScreenCleanScanlinesBigPattern = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCleanScanlinesBigPattern");
        ScreenCleanScanlinesBlackBorderPattern = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCleanScanlinesBlackBorderPattern");
        ScreenCleanScanlines = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCleanScanlines");
        ScreenClean = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenClean");
    }
    public ShaderScreenClean(Renderer display, int position, Dictionary<string, string> config) : base(display, position, config)
    {

        material = ScreenClean;

        config.TryGetValue("damage", out damage);
        if (damage == "high")
            material = ScreenCleanScanlinesBigPattern;
        else if (damage == "medium")
            material = ScreenCleanScanlinesBlackBorderPattern;
        else if (damage == "low")
            material = ScreenCleanScanlines;

        //material = new Material(originalMat);
        //material.CopyPropertiesFromMaterial(originalMat);

        //material = Object.Instantiate(Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenClean"));
        //material.name = $"instance-ShaderScreenClean-{damage}";

        // material = new Material(Resources.Load<Material>(materialPrefab));
        ConfigManager.WriteConsole($"[ShaderScreenClean] assigned material: {material} damage: {damage}");

        ApplyConfiguration();
    }

    public override string Name
    {
        get
        {
            return "Clean" + $"({damage})";
        }
    }

    public override string TargetMaterialProperty
    {
        get
        {
            return "_EmissionMap";
        }
    }

    public override Texture Texture
    {
        get
        {
            return display.materials[position].GetTexture("_EmissionMap");
        }
        set
        {
            ConfigManager.WriteConsole($"[ShaderScreenClean.Texture] {ToString()} SET tex:{(Texture)value} material: {material}");

            display.materials[position].SetTexture("_EmissionMap", (Texture)value);
            display.materials[position].SetTexture("_MainTex", (Texture)value);
        }
    }
    public override void Refresh(Texture texture)
    {
        Texture = texture;
        Vector2 v2 = new Vector2(actual_invertx ? -1f : 1f, actual_inverty ? -1f : 1f);
        display.materials[position].SetTextureScale("_EmissionMap", v2);
        display.materials[position].SetTextureScale("_MainTex", v2);
    }

    public override ShaderScreenBase Invert(bool invertx, bool inverty)
    {
        ConfigManager.WriteConsole($"[ShaderScreenClean.Invert] {invertx}, {inverty}");
        Vector2 v2 = new Vector2(invertx ? -1f : 1f, inverty ? -1f : 1f);
        display.materials[position].SetTextureScale("_EmissionMap", v2);
        display.materials[position].SetTextureScale("_MainTex", v2);
        actual_invertx = invertx;
        actual_inverty = inverty;
        return this;
    }
}
