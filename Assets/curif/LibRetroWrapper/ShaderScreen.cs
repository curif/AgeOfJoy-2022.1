using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class ShaderScreenBase
{
    protected int position;
    protected Renderer display;
    protected Material material;

    public static Texture2D StandByTexture { get; private set; }
    static ShaderScreenBase()
    {
        StandByTexture = Resources.Load<Texture2D>("Cabinets/OutOfOrder/Prefab/CRTOff");
    }
    protected ShaderScreenBase(Renderer display, int position, Dictionary<string, string> config)
    {
        this.position = position;
        this.display = display;
    }
    public abstract string Name { get; }
    public abstract ShaderScreenBase Invert(bool invertx, bool inverty);
    public override string ToString()
    {
        return this.Name;
    }
    public virtual void Update() { }

    // sets the material to the display
    public virtual void Activate()
    {
        //materials property of the MeshRenderer component returns a copy of the materials array, not the actual array itself.
        if (material == null)
            return;

        Material[] mats = display.materials;
        mats[this.position] = material;
        display.materials = mats;
    }
    public abstract Texture Texture { get; set; }
    public abstract string TargetMaterialProperty { get; }
}

//Factory
public static class ShaderScreen
{
    private static Dictionary<string, Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase>> dic = new();
    private static string[] ShaderNames = new[] { "damage", "clean", "crt", "crtlod" };

    static ShaderScreen()
    {
        dic["damage"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenDamage(display, position, config);
        dic["clean"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenClean(display, position, config);
        dic["crt"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderCRT(display, position, config);
        dic["crtlod"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderCRTLOD(display, position, config);
    }

    public static ShaderScreenBase Factory(Renderer display, int position, string shaderName, Dictionary<string, string> config)
    {
        Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase> shd;
        if (!dic.TryGetValue(shaderName, out shd))
            shd = dic["damage"];

        return shd(display, position, config);
    }

    public static bool Exists(string shaderName)
    {
        return ShaderNames.Contains(shaderName.ToLower());
    }

}
