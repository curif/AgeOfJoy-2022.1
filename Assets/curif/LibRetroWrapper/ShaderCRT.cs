
using UnityEngine;
using System.Collections.Generic;

public class ShaderCRT : ShaderScreenBase
{
    protected virtual Material MaterialPrefabDamageLow { get { return Low; } }
    protected virtual Material MaterialPrefabDamageMedium { get { return Medium; } }
    protected virtual Material MaterialPrefabDamageHigh { get { return High; } }
    protected string damage;

    protected static Material Low, Medium, High;
    protected Vector4? v4Invert;

    static ShaderCRT()
    {
        Low = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRTLow");
        Medium = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRTMedium");
        High = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRTHigh");
    }

    public ShaderCRT(Renderer display, int position, Dictionary<string, string> config,
        CabinetMaterials.MaterialPropertyTranslator translator = null) : 
        base(display, position, config, translator == null ? new CabinetMaterials.MaterialCRTShaderProperties() :  translator)
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
            return "CRT" + $"({damage})";
        }
    }

    public override string TargetMaterialProperty
    {
        get
        {
            return "_MainTex";
        }
    }
    public override string AlternativeShaderForAttractionVideos() { return "crtlod"; }

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
            if (v4Invert != null)
                display.materials[position].SetVector("_CRTTiling", (Vector4)v4Invert);
        }
    }

    public override void Refresh(Texture texture)
    {
        Texture = texture;
    }

    public override ShaderScreenBase Invert(bool invertx, bool inverty)
    {
        v4Invert = new Vector4(invertx ? -1f : 1f, inverty ? -1f : 1f, 0, 0);
        display.materials[position].SetVector("_CRTTiling", (Vector4)v4Invert);

        return this;
    }
}
