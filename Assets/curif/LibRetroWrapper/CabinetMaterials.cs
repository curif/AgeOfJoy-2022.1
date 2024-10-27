/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using UnityEngine;

public static class CabinetMaterials
{

    public static Material Base;
    public static Material BaseNormal;
    public static Material BlackNoNormal;
    public static Material Black;
    public static Material LightWood;
    public static Material DarkWood;
    public static Material Plastic;
    public static Material LayerGlass;
    public static Material CleanGlass;
    public static Material DirtyGlass;
    public static Material TVBorder;
    public static Material VertexColor;
    public static Material VertexColorNormal;
    public static Material FrontGlassWithBezel;
    public static Material FrontGlassCutOut;
    public static Material FrontGlassDirty;
    public static Material MarqueeNoLamps;
    public static Material MarqueeOneLamp;
    public static Material MarqueeTwoLamps;
    public static Material MarqueeOneTube;
    public static Material MarqueeTwoTubes;
    // public static Material CoinSlotPlastic;
    // public static Material CoinSlotPlasticDouble;
    // public static Material LeftOrRight; //the material used with stickers in the sides of the cabinet.

    //list of the materials by his name, usefull in serialization
    public static Dictionary<string, MaterialInfo> materialList = new Dictionary<string, MaterialInfo>();

    public class MaterialPropertyTranslator
    {
        Dictionary<string, string> propertyTranslator;

        public MaterialPropertyTranslator(Dictionary<string, string> propertyTranslator)
        {
            this.propertyTranslator = propertyTranslator;
        }
        public Dictionary<string, string> Translate(Dictionary<string, string> inputDictionary)
        {
            if (inputDictionary.Count == 0)
                return inputDictionary;

            foreach (var entry in propertyTranslator)
            {
                if (inputDictionary.ContainsKey(entry.Key))
                {
                    string value = inputDictionary[entry.Key];
                    inputDictionary.Remove(entry.Key);
                    inputDictionary.Add(propertyTranslator[entry.Key], value);
                }
            }
            return inputDictionary;
        }

        public string GetRealPropertyName(string name)
        {
            if (propertyTranslator.ContainsKey(name))
            {
                return propertyTranslator[name];
            }
            return null;
        }
    }

    static Dictionary<string, string> MaterialStandardPropertyTranslator = new Dictionary<string, string>()
            {
                //{"smoothness", "_SmoothnessTextureChannel" },
                {"smoothness", "_Glossiness" },
                {"metallic", "_Metallic" },
                {"color", "_Color" },
                {"emission-color", "_EmmisionColor" },
                {"normal", "_BumpMap" }

            };
    static Dictionary<string, string> MaterialVertexPropertyTranslator = new Dictionary<string, string>()
            {
                {"smoothness", "_Smoothness" },
                {"metallic", "_Metallic" },
                {"normal", "_NormalMap" }
            };
    static Dictionary<string, string> MarqueePropertyTranslator = new Dictionary<string, string>()
            {
                {"smoothness", "_Glossiness" },
                {"metallic", "_Metallic" },
                {"emission-color", "_EmissionColor" }
            };
    static Dictionary<string, string> MaterialFrontGlassProperties = new Dictionary<string, string>()
            {
                {"smoothness", "_Glossiness" },
                {"metallic", "_Metallic" },
                {"color", "_Color" }
            };
    static Dictionary<string, string> MaterialFrontGlassCutOutProperties = new Dictionary<string, string>()
            {
                {"smoothness", "_Glossiness" },
                {"metallic", "_Metallic" },
                {"color", "_Color" }
            };
    static Dictionary<string, string> MaterialFrontGlassDirtyProperties = new Dictionary<string, string>()
            {
                {"smoothness", "_Glossiness" },
                {"metallic", "_Metallic" },
                {"color", "_Color" },
                {"glass-translucency", "_GlassTranslucency" },
                {"dirtiness", "_Dirtiness" },
                {"glossiness-high", "_GlossinessHigh" },
                {"glossiness-low", "_GlossinessLow" },
                {"glossy-reflections", "_GlossyReflections" }
            };
    static Dictionary<string, string> MaterialCRTProperties = new Dictionary<string, string>()
            {
                {"tiling", "_CRTTiling" }
            };
    static Dictionary<string, string> MaterialCRTLODProperties = new Dictionary<string, string>()
            {
                {"tiling", "_CRTTiling" },
                {"rotation", "_ScreenRotation" }
            };

    public class FrontGlassProperties : MaterialPropertyTranslator
    {
        public FrontGlassProperties() : base(MaterialFrontGlassProperties) { }
    }
    public class FrontGlassCutOutProperties : MaterialPropertyTranslator
    {
        public FrontGlassCutOutProperties() : base(MaterialFrontGlassCutOutProperties) { }
    }
    public class FrontGlassDirtyProperties : MaterialPropertyTranslator
    {
        public FrontGlassDirtyProperties() : base(MaterialFrontGlassDirtyProperties) { }
    }
    public class MarqueeProperties : MaterialPropertyTranslator
    {
        public MarqueeProperties() : base(MarqueePropertyTranslator) { }
    }
    public class MaterialStandardProperties : MaterialPropertyTranslator
    {
        public MaterialStandardProperties() : base(MaterialStandardPropertyTranslator) { }
    }

    // shaders ---------------------
    public class MaterialCRTShaderProperties : MaterialPropertyTranslator
    {
        public MaterialCRTShaderProperties() : base(MaterialCRTProperties) { }
    }

    public class MaterialCRTLODShaderProperties : MaterialPropertyTranslator
    {
        public MaterialCRTLODShaderProperties() : base(MaterialCRTLODProperties) { }
    }

    // ------------------------
    public class MaterialInfo
    {
        public Material material;
        public MaterialPropertyTranslator propertyTranslator;

        public MaterialInfo(Material material, MaterialPropertyTranslator propertyTranslator)
        {
            this.material = material;
            this.propertyTranslator = propertyTranslator;
        }
    }


    static CabinetMaterials()
    {
        // the material base for stickers
        Base = Resources.Load<Material>("Cabinets/Materials/Base");
        BaseNormal = Resources.Load<Material>("Cabinets/Materials/BaseNormal");
        BlackNoNormal = Resources.Load<Material>("Cabinets/Materials/CabinetBlack"); //used by default in un-named parts.

        //pre created in Unity editor
        //They should support normals ("normal map" loaded)
        Black = Resources.Load<Material>("Cabinets/Materials/CabinetBlackNormal");
        LightWood = Resources.Load<Material>("Cabinets/Materials/LightWoodStandard");
        DarkWood = Resources.Load<Material>("Cabinets/Materials/DarkWoodStandard");
        Plastic = Resources.Load<Material>("Cabinets/Materials/Plastic");
        CleanGlass = Resources.Load<Material>("Cabinets/Materials/GlassClean");
        DirtyGlass = Resources.Load<Material>("Cabinets/Materials/GlassDirt");
        LayerGlass = Resources.Load<Material>("Cabinets/Materials/GlassTranspLayer");
        VertexColor = Resources.Load<Material>("Cabinets/Materials/Base_VertexColor");
        VertexColorNormal = Resources.Load<Material>("Cabinets/Materials/Base_VertexColor_Normal");

        TVBorder = Resources.Load<Material>("Cabinets/PreFab/CRTs/TVBorder");

        MarqueeNoLamps = Resources.Load<Material>("Cabinets/Materials/MarqueeNoLamps");
        MarqueeOneLamp = Resources.Load<Material>("Cabinets/Materials/MarqueeOneLamp");
        MarqueeTwoLamps = Resources.Load<Material>("Cabinets/Materials/MarqueeTwoLamps");
        MarqueeOneTube = Resources.Load<Material>("Cabinets/Materials/MarqueeOneTube");
        MarqueeTwoTubes = Resources.Load<Material>("Cabinets/Materials/MarqueeTwoTubes");

        //MarqueeStandardShader = Resources.Load<Material>("Cabinets/Materials/MarqueeStandardShader");
        FrontGlassWithBezel = Resources.Load<Material>("Cabinets/Materials/FrontGlass");
        FrontGlassCutOut = Resources.Load<Material>("Cabinets/Materials/FrontGlass_Cutout");
        FrontGlassDirty = Resources.Load<Material>("Cabinets/Materials/FrontGlass_Bezel");


        //user configurable list:
        materialList.Add("black", new MaterialInfo(Black, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("base", new MaterialInfo(BaseNormal, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("lightwood", new MaterialInfo(LightWood, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("darkwood", new MaterialInfo(DarkWood, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("plastic", new MaterialInfo(Plastic, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("dirty glass", new MaterialInfo(DirtyGlass, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("layer glass", new MaterialInfo(LayerGlass, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("clean glass", new MaterialInfo(CleanGlass, new MaterialPropertyTranslator(MaterialStandardPropertyTranslator)));
        materialList.Add("Vertex Color", new MaterialInfo(VertexColorNormal, new MaterialPropertyTranslator(MaterialVertexPropertyTranslator)));

    }

    public static Material fromName(string name)
    {
        if (!materialList.ContainsKey(name))
        {
            ConfigManager.WriteConsole($"ERROR: material name {name} is unknown, fallback to material standard 'black'.");
            name = "black";
        }
        return materialList[name].material;
    }

    public static MaterialPropertyTranslator PropertyTranslator(string name)
    {
        if (!materialList.ContainsKey(name))
        {
            ConfigManager.WriteConsole($"ERROR: material name {name} is unknown, fallback to property translator standard 'black'.");
            name = "black";
        }
        return materialList[name].propertyTranslator;
    }
}
