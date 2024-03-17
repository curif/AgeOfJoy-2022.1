
using UnityEngine;
using System.Collections.Generic;

public class ShaderCRT : ShaderScreenBase
{
  private bool actual_invertx = false, actual_inverty = false;
  private string damage;

  public ShaderCRT(Renderer display, int position, Dictionary<string, string> config) : base(display, position, config)
  {

    string materialPrefab = "Cabinets/PreFab/CRTs/ScreenCRTLow";
    config.TryGetValue("damage", out damage);

    if (damage == "high")
      materialPrefab = "Cabinets/PreFab/CRTs/ScreenCRTHigh";
    else if (damage == "medium")
      materialPrefab = "Cabinets/PreFab/CRTs/ScreenCRTMedium";

    Material mat = Object.Instantiate(Resources.Load<Material>(materialPrefab));

    //materials property of the MeshRenderer component returns a copy of the materials array, not the actual array itself.
    Material[] mats = display.materials;
    mats[this.position] = mat;
    display.materials = mats;
    
    return;
  }

  public override string Name 
  {
    get {
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

  public override Texture Texture { 
    get { 
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
  
  public override void Update() 
  {
  }
  
  public override ShaderScreenBase Invert(bool invertx, bool inverty)
  {
    if (actual_invertx != invertx || actual_inverty != inverty)
    {
      Vector4 v4 = new Vector4(invertx? -1f : 1f, inverty? -1f : 1f, 0, 0);
      display.materials[position].SetVector("_CRTTiling", v4); 
      ConfigManager.WriteConsole($"[ShaderCRT.Invert] {invertx}, {inverty} = {v4}");

      actual_invertx = invertx; 
      actual_inverty = inverty;
    }
    return this;
  }
}
