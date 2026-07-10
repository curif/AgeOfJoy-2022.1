using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class ShaderScreenBase
{
    protected int position;
    protected Renderer display;
    protected Material material;
    Material instantiatedMaterial; // material instance created by a previous Activate() call, owned by this object
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

    public virtual string AlternativeShaderForAttractionVideos() { return RecommendedReplacementForAttractionVideos(null); }
    public virtual Dictionary<string, string> AlternativeConfigForAttractionVideos() { return configuration; }
    public static string RecommendedReplacementForAttractionVideos(string shaderName) 
    {
        if (shaderName == null)
            return "clean";
        if (shaderName == "crt")
            return "crtlod";
        return shaderName;
    }


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
        Material newInstance = display.materials[position];

        if (instantiatedMaterial != null && instantiatedMaterial != newInstance)
            UnityEngine.Object.Destroy(instantiatedMaterial);

        material = newInstance;
        instantiatedMaterial = newInstance;

        if (texture != null)
            Texture = texture; //child should change it in render material by position
    }

    // releases the material instance created by Activate(), if any; call from owning MonoBehaviour's OnDestroy
    public void ReleaseMaterialInstance()
    {
        if (instantiatedMaterial != null)
            UnityEngine.Object.Destroy(instantiatedMaterial);
        instantiatedMaterial = null;
    }
    public virtual void Refresh(Texture texture) { }

    public abstract Texture Texture { get; set; }
    public abstract string TargetMaterialProperty { get; }

    // The live (instanced) screen material. For callers that must rebind the texture every frame
    // (hardware-rendered cores swap among triple-buffered external textures) — the Texture setter
    // logs on each set. Only valid after Activate().
    public Material ScreenMaterial { get { return material; } }

    public void ApplyConfiguration()
    {
        // `material` here is still one of the shared static Resources.Load() singletons (Low/Medium/High
        // etc.) assigned by the subclass constructor, shared by every cabinet in the game. Cloning it
        // before mutating avoids stomping the shared template's shader properties (e.g. _CRTTiling used
        // for invert) for every other cabinet still to construct/Activate — that cross-contamination is
        // what caused screens to intermittently render flipped depending on cabinet init order.
        string originalName = material.name;
        material = UnityEngine.Object.Instantiate(material);
        material.name = originalName; // MaterialsUtils.ApplyConfiguration looks materials.yaml entries up by name
        MaterialsUtils.ApplyCabinetConfiguration(material, configuration);
        MaterialsUtils.ApplyConfiguration(material);
    }
}
//Factory
public static class ShaderScreen
{
    private static Dictionary<string, Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase>> dic = new();
    private static string[] ShaderNames = new[] { "damage", "clean", "crt", "crtlod", "projector",                                                                                                  "projectorLOD", "crt-additive" };

    static ShaderScreen()
    {
        dic["damage"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenDamage(display, position, config);
        dic["clean"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenClean(display, position, config);
        dic["crt"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderCRT(display, position, config);
        dic["crtlod"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderCRTLOD(display, position, config);
        dic["projector"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderProjector(display, position, config);
        dic["crt-additive"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderCRTAdditive(display, position, config);
    }

    public static ShaderScreenBase Factory(Renderer display, int position, string shaderName, Dictionary<string, string> config)
    {
        Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase> shd;
        if (!dic.TryGetValue(shaderName, out shd))
            shd = dic["damage"];

        if (config == null)
            config = new Dictionary<string, string>();
        
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
