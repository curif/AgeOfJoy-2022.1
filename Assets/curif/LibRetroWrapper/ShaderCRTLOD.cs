
using UnityEngine;
using System.Collections.Generic;

public class ShaderCRTLOD : ShaderCRT
{
    protected override string MaterialPrefabDamageLow { get { return "Cabinets/PreFab/CRTs/ScreenCRTLow_LOD"; } }
    protected override string MaterialPrefabDamageMedium { get { return "Cabinets/PreFab/CRTs/ScreenCRTMedium_LOD"; } }
    protected override string MaterialPrefabDamageHigh { get { return "Cabinets/PreFab/CRTs/ScreenCRTHigh_LOD"; } }
    public override string Name { get { return "CRT" + $"({damage})_LOD";}}

    public ShaderCRTLOD(Renderer display, int position, Dictionary<string, string> config) :
       base(display, position, config)
    { }

}
