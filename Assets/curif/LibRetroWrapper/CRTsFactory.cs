/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;
using System.Collections.Generic;
using System;

public static class CRTsFactory
{
    public static Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    static CRTsFactory()
    {
        //CRTs
        // Assets/Resources/Cabinets/PreFab/CRTs/screen19i.prefab
        // Assets/Resources/Cabinets/PreFab/CRTs/screen50i.prefab
        objects.Add("19i", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i"));
        objects.Add("19i-fresnel", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i_fresnel"));
        objects.Add("32i", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen32i"));
        objects.Add("50i", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen50i"));
        objects.Add("circle", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen_circle"));
        objects.Add("square", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen_square"));
        objects.Add("19i-1x2", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i_1x2"));
        objects.Add("19i-2x1", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i_2x1"));
        objects.Add("19i-3x1", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i_3x1"));
        objects.Add("19i-3x1-18deg", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i_3x1_18deg"));
        objects.Add("dome-concave", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen_dome_concave"));
        objects.Add("dome-convex", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen_dome_convex"));
        objects.Add("19i-agebasic", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19iAGEBasic"));
        objects.Add("no-crt", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/noScreen"));
    }

    public static GameObject Instantiate(string type, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (!objects.ContainsKey(type))
            throw new Exception($"[CRTFactory] screen type {type} doesn't exists");

        ConfigManager.WriteConsole($"[CRTsFactory] {type}");
        return GameObject.Instantiate<GameObject>(objects[type], position, rotation, parent);
    }
}