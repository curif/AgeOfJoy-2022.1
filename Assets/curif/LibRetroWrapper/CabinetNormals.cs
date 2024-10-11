using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CabinetMaterials;

public class CabinetNormals
{
    public static Dictionary<string, Texture2D> normals = new ();
    static CabinetNormals()
    {
        normals["wood"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_Wood_N");
        normals["plastic"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_Plastic_N");
        normals["rubber"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_Rubber_N");
        normals["scratches"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_Scratches_N");
        normals["treadplate round"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_TreadplateRound_N");
        normals["treadplate diamond"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_TreadplateDiamond_N");
        normals["cloth"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_Cloth_N");
        normals["paint"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_Paint_N");
        normals["scratches 2"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_Scratches2_N");
        normals["corroded metal"] = Resources.Load<Texture2D>("Cabinets/Materials/T_Base_CorrodedMetal_N");

    }
    public static Texture2D GetNormal(string name)
    {
        if (normals.ContainsKey(name))
            return normals[name];
        
        return null;
    }

}
