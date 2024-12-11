using UnityEngine;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

public class ShaderScreenClean : ShaderScreenBase
{
    private bool actual_invertx = false, actual_inverty = false;
    private string damage = "none";

    static Material ScreenCleanDamageHigh;
    static Material ScreenCleanDamageLow;
    static Material ScreenCleanDamageMedium;
    static Material ScreenClean;
    static ShaderScreenClean()
    {
        ScreenCleanDamageHigh = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCleanDamageHigh");
        ScreenCleanDamageLow = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCleanDamageLow");
        ScreenCleanDamageMedium = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCleanDamageMedium");
        
        
        //standard shader. Set emission in true, intensity color white and intensity: 1 (increase for damage)
        //albedo color in white.
        ScreenClean = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenClean");
    }
    public ShaderScreenClean(Renderer display, int position, Dictionary<string, string> config) : 
        base(display, position, config, new CabinetMaterials.MaterialStandardProperties())
    {
        
        material = ScreenClean;
        
        config.TryGetValue("damage", out damage);
        if (damage == "high")
            material = ScreenCleanDamageHigh;
        else if (damage == "medium")
            material = ScreenCleanDamageMedium;
        else if (damage == "low")
            material = ScreenCleanDamageLow;
        
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

    public override void Activate(Texture texture = null)
    {
        base.Activate(texture);

        //this shader needs white color to show games like Asteroids.
        display.materials[position].SetColor("_Color", Color.white);
        display.materials[position].SetColor("_EmissionColor", Color.white);
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
