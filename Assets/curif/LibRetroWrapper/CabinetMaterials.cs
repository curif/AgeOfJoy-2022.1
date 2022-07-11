/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;
using System.Collections.Generic;

public static class CabinetMaterials {

    public static Material Base; //uses the shade mobile/difusse
    public static Material Black; 
    public static Material LightWood;
    public static Material DarkWood;
    public static Material TVBorder;
    public static Material Screen;
    public static Material FrontGlassWithBezel;
    public static Material Marquee;
    // public static Material CoinSlotPlastic;
    // public static Material CoinSlotPlasticDouble;
    // public static Material LeftOrRight; //the material used with stickers in the sides of the cabinet.
    
    //list of the materials by his name, usefull in serialization
    public static Dictionary<string, Material> materialList = new();

    static CabinetMaterials() {
        // the material base for stickers
        Base = Resources.Load<Material>("Cabinets/Materials/Base");

        //pre created in Unity editor
        Black = Resources.Load<Material>("Cabinets/Materials/CabinetBlack");
        LightWood = Resources.Load<Material>("Cabinets/Materials/LightWood");
        DarkWood = Resources.Load<Material>("Cabinets/Materials/DarkWood");

        materialList.Add("black", Black);
        materialList.Add("base", Base);
        materialList.Add("lightwood", LightWood);
        materialList.Add("darkwood", DarkWood);

        TVBorder = Resources.Load<Material>("Cabinets/Materials/TVBorder");
        Marquee = Resources.Load<Material>("Cabinets/Materials/Marquee");
        Screen = Resources.Load<Material>("Cabinets/Materials/Screen");
        FrontGlassWithBezel = Resources.Load<Material>("Cabinets/Materials/FrontGlass");

        // CoinSlotPlastic = Resources.Load<Material>("Cabinets/Materials/CoinSlotPlastic");
        // CoinSlotPlasticDouble = Resources.Load<Material>("Cabinets/Materials/CoinSlotPlasticDouble");

    }

    public static Material fromName(string name) {
        if (! materialList.ContainsKey(name)) {
            ConfigManager.WriteConsole($"ERROR: material name {name} is unknown, fallback to material standard 'black'.");
            name = "black";
        }
        return materialList[name];
    }

}
