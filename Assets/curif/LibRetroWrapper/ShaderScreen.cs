using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class ShaderScreenBase
{
  protected int position;
  protected Renderer display;
  protected ShaderScreenBase(Renderer display, int position, Dictionary<string, string> config){
    this.position = position;
    this.display = display;
  }
  public abstract string Name {get;}
  public abstract ShaderScreenBase Invert(bool invertx, bool inverty);
  public override string ToString()
  {
    return this.Name;
  }
  public virtual void Update() {}
  public abstract Texture Texture {get; set;}
  public abstract string TargetMaterialProperty {get;}
}

//Factory
public static class ShaderScreen
{
  //private static Dictionary<string, Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase>> dic = new Dictionary<string, Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase>>();
  private static Dictionary<string, Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase>> dic = new(); 
  
  static ShaderScreen()
  {
    //dic["damage"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenDamage(display, position, config);
    dic["damage"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenDamage(display, position, config);
    dic["clean"] = (Renderer display, int position, Dictionary<string, string> config) => new ShaderScreenClean(display, position,  config);
  }
  
  public static ShaderScreenBase Factory(Renderer display, int position, string shaderName, Dictionary<string, string> config)
  {
    Func<Renderer, int, Dictionary<string, string>, ShaderScreenBase> shd;
    if (! dic.TryGetValue(shaderName, out shd))
      shd = dic["damage"];
    
    return shd(display, position, config);
  }
}
