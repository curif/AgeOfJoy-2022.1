
using UnityEngine;
using System.Collections.Generic;

public class ShaderCRTAdditive : ShaderScreenBase
{
    protected virtual Material MaterialPrefabDamageLow { get { return Low; } }
    protected virtual Material MaterialPrefabDamageMedium { get { return Medium; } }
    protected virtual Material MaterialPrefabDamageHigh { get { return High; } }
    private bool actual_invertx = false, actual_inverty = false;
    protected string damage;

    protected static Material Low, Medium, High;

    static ShaderCRTAdditive()
    {
        Low = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRT_Additive_Low");
        Medium = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRT_Additive_Medium");
        High = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRT_Additive_High");
    }

    public ShaderCRTAdditive(Renderer display, int position, Dictionary<string, string> config) :
        base(display, position, config, new CabinetMaterials.MaterialCRTShaderProperties())
    {

        material = MaterialPrefabDamageLow;
        config.TryGetValue("damage", out damage);

        if (damage == "high")
            material = MaterialPrefabDamageMedium;
        else if (damage == "medium")
            material = MaterialPrefabDamageHigh;

        ApplyConfiguration();
    }

    public override string Name
    {
        get
        {
            return "DOME" + $"({damage})";
        }
    }

    public override string TargetMaterialProperty
    {
        get
        {
            return "_MainTex";
        }
    }

    public override string AlternativeShaderForAttractionVideos() { return "crt-additive"; }
    public override Dictionary<string, string> AlternativeConfigForAttractionVideos() { 
        Dictionary<string, string> config = new();
        config["damage"] = "low";
        return config;    
    }


    public override Texture Texture
    {
        get
        {
            return display.materials[position].GetTexture("_MainTex");
        }
        set
        {
            Texture t = (Texture)value;
            Vector4 crtParameters = new Vector4(t.width, t.height, 0f, 0f);

            display.materials[position].SetTexture("_MainTex", t);
            display.materials[position].SetVector("_CRTParameters", crtParameters);
        }
    }

    public override void Refresh(Texture texture)
    {
        Texture = texture;
        Vector4 v4 = new Vector4(actual_invertx ? -1f : 1f, actual_inverty ? -1f : 1f, 0, 0);
        display.materials[position].SetVector("_CRTTiling", v4);
    }

    public override ShaderScreenBase Invert(bool invertx, bool inverty)
    {
        if (actual_invertx != invertx || actual_inverty != inverty)
        {
            Vector4 v4 = new Vector4(invertx ? -1f : 1f, inverty ? -1f : 1f, 0, 0);
            display.materials[position].SetVector("_CRTTiling", v4);
            ConfigManager.WriteConsole($"[ShaderDome.Invert] {invertx}, {inverty} = {v4}");

            actual_invertx = invertx;
            actual_inverty = inverty;
        }
        return this;
    }
}
