
using UnityEngine;
using System.Collections.Generic;

public class ShaderScreenDamage : ShaderScreenBase
{
  private bool actual_invertx = false, actual_inverty = false;
  private string damage;

    static Material DamageLow;
    static Material DamageHigh;
    static Material DamageMedium;
    
    static ShaderScreenDamage()
    {
        DamageLow = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenDamageLow");
        DamageHigh = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenDamageHigh");
        DamageMedium = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenDamageMedium");
    }

    public ShaderScreenDamage(Renderer display, int position, Dictionary<string, string> config) : base(display, position, config)
  {
    string damage;
    material = DamageLow;
    config.TryGetValue("damage", out damage);

    if (damage == "high")
        material = DamageHigh;
    else if (damage == "medium")
        material = DamageMedium;

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
      ConfigManager.WriteConsole($"[ShaderScreenDamage.Texture] SET tex:{(Texture)value} material: {material}");

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
