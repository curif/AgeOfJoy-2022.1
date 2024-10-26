using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class ShaderScreenBase
{
    protected int position;
    protected Renderer display;
    protected Material material;
    Dictionary<string, string> configuration;
    CabinetMaterials.MaterialPropertyTranslator translator;

    public static Texture2D StandByTexture { get; private set; }
    static ShaderScreenBase()
    {
        StandByTexture = Resources.Load<Texture2D>("Cabinets/OutOfOrder/Prefab/CRTOff");
    }
    protected ShaderScreenBase(Renderer display, int position, Dictionary<string, string> config, CabinetMaterials.MaterialPropertyTranslator translator)
    {
        this.position = position;
        this.display = display;
        if (translator != null)
        {
            this.translator = translator;
            this.translator.Translate(config);
        }

        this.configuration = config;
    }
    public abstract string Name { get; }
    public abstract ShaderScreenBase Invert(bool invertx, bool inverty);
    public override string ToString()
    {
        return this.Name;
    }
    public virtual void Update() { }


  
    // sets the material to the display
    public virtual void Activate(Texture texture = null)
    {
        //materials property of the MeshRenderer component returns a copy of the materials array, not the actual array itself.
        if (material == null)
            return;

        /* Note that like all arrays returned by Unity, this returns a COPY of materials array. 
        * If you want to change some materials in it, get the value, change an entry and set materials back.
        * This function automatically **instantiates** the materials and makes them unique to this renderer. 
        * It is your responsibility to destroy the materials when the game object is being destroyed. 
        * Resources.UnloadUnusedAssets also destroys the materials but it is usually only called when 
        * loading a new level.  
        * In Unity, when you assign a material to a renderer, Unity automatically creates a new instance 
        * of that material specific to that renderer.
        */
        ConfigManager.WriteConsole($"[ShaderScreenBase.Activate] {ToString()} tex:{texture} material: {material} position: {position}");
        Material[] mats = display.materials;
        mats[position] = material;
        display.materials = mats;
        
        if (texture != null)
            Texture = texture; //child should change it in render material by position
    }
    public virtual void Refresh(Texture texture) { }

    public abstract Texture Texture { get; set; }
    public abstract string TargetMaterialProperty { get; }

    public void ApplyConfiguration()
    {
        MaterialsUtils.ApplyCabinetConfiguration(material, configuration);
        MaterialsUtils.ApplyConfiguration(material);
    }
}
//Factory
public static class ShaderScreen
{
    private static Dictionary<string, Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase>> dic = new();
    private static string[] ShaderNames = new[] { "damage", "clean", "crt", "crtlod", "projector",                                                      "projectorLOD" };

    static ShaderScreen()
    {
        dic["damage"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenDamage(display, position, config);
        dic["clean"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenClean(display, position, config);
        dic["crt"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderCRT(display, position, config);
        dic["crtlod"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderCRTLOD(display, position, config);
        dic["projector"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderProjector(display, position, config);
        dic["projectorlod"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderProjectorLOD(display, position, config);
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

    public static List<string> list()
    {
        return dic.Keys.ToList();
    }

}
