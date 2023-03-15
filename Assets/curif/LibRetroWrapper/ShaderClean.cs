using UnityEngine;
using System.Collections.Generic;

public class ShaderScreenClean: ShaderScreenBase
{
  private bool actual_invertx = false, actual_inverty = false;
  private string damage = "none";

  public ShaderScreenClean(Renderer display, int position, Dictionary<string, string> config) : base(display, position, config) 
  {

    string materialPrefab = "Cabinets/PreFab/CRTs/ScreenClean";
    //Material mat = Object.Instantiate(Resources.Load<Material>(materialPrefab));
    config.TryGetValue("damage", out damage);

    if (damage == "high")
      materialPrefab = "Cabinets/PreFab/CRTs/ScreenCleanScanlinesBigPattern";
    else if (damage == "medium")
      materialPrefab = "Cabinets/PreFab/CRTs/ScreenCleanScanlinesBlackBorderPattern";
    else if (damage == "low")
      materialPrefab = "Cabinets/PreFab/CRTs/ScreenCleanScanlines";

    Material mat = new Material(Resources.Load<Material>(materialPrefab));

    //materials property of the MeshRenderer component returns a copy of the materials array, not the actual array itself.
    Material[] mats = display.materials;
    mats[position] = mat;
    display.materials = mats;
    
    return;
  }

  public override string Name 
  {
    get {
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

  public override Texture Texture { 
    get { 
      return display.materials[position].GetTexture("_EmissionMap");
    }
    set 
    {
      display.materials[position].SetTexture("_EmissionMap", (Texture)value);
      display.materials[position].SetTexture("_MainTex", (Texture)value);
    }
  }
  
  public override ShaderScreenBase Invert(bool invertx, bool inverty)
  {
      ConfigManager.WriteConsole($"[ShaderScreenClean.Invert] {invertx}, {inverty}");
      Vector2 v2 = new Vector2(invertx? -1f : 1f, inverty? -1f : 1f);
      display.materials[position].SetTextureScale("_EmissionMap", v2); 
      display.materials[position].SetTextureScale("_MainTex", v2); 
      actual_invertx = invertx; 
      actual_inverty = inverty;
    return this;
  }
}
