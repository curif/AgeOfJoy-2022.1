
using UnityEngine;
using System.Collections.Generic;

public class ShaderCRTLOD : ShaderCRT
{
    protected override Material MaterialPrefabDamageLow { get { return LowLOD; } }
    protected override Material MaterialPrefabDamageMedium { get { return MediumLOD; } }
    protected override Material MaterialPrefabDamageHigh { get { return HighLOD; } }
    public override string Name { get { return "CRT" + $"({damage})_LOD";}}

    protected static Material LowLOD, MediumLOD, HighLOD;

    static ShaderCRTLOD()
    {
      LowLOD = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRTLow_LOD");
      MediumLOD = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRTMedium_LOD");
      HighLOD = Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRTHigh_LOD");
    }
    
    public ShaderCRTLOD(Renderer display, int position, Dictionary<string, string> config) :
       base(display, position, config)
    {
    }
}
