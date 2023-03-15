
using UnityEngine;
using System.Collections.Generic;

public class ShaderScreenDamage : ShaderScreenBase
{
  private bool actual_invertx = false, actual_inverty = false;
  private string damage;

  public ShaderScreenDamage(Renderer display, int position, Dictionary<string, string> config) : base(display, position, config)
  {

    string materialPrefab = "Cabinets/PreFab/CRTs/ScreenDamageLow";
    config.TryGetValue("damage", out damage);

    if (damage == "high")
      materialPrefab = "Cabinets/PreFab/CRTs/ScreenDamageHigh";
    else if (damage == "medium")
      materialPrefab = "Cabinets/PreFab/CRTs/ScreenDamageMedium";

    Material mat = Object.Instantiate(Resources.Load<Material>(materialPrefab));
    mat.SetFloat("MirrorX", 0f);
    mat.SetFloat("MirrorY", 0f);

    //materials property of the MeshRenderer component returns a copy of the materials array, not the actual array itself.
    Material[] mats = display.materials;
    mats[this.position] = mat;
    display.materials = mats;
    
    return;
  }

  public override string Name 
  {
    get {
      return "damage" + $"({damage})";
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
      display.materials[position].SetTexture("_MainTex", (Texture)value);
    }
  }
  
  public override void Update() 
  {
    display.materials[position].SetFloat("u_time", Time.fixedTime);
  }
  
  public override ShaderScreenBase Invert(bool invertx, bool inverty)
  {
    if (actual_invertx != invertx || actual_inverty != inverty)
    {
      ConfigManager.WriteConsole($"[ShaderScreenDamage.Invert] {invertx}, {inverty}");
      display.materials[position].SetFloat("MirrorX", invertx ? 1f : 0f);
      display.materials[position].SetFloat("MirrorY", inverty ? 1f : 0f);
      actual_invertx = invertx; 
      actual_inverty = inverty;
    }
    return this;
  }
}
