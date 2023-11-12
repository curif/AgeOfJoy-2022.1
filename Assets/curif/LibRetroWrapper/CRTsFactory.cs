/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;
using System.Collections.Generic;

public static class CRTsFactory {
    public static Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
    
    static CRTsFactory() {
        //CRTs
        // Assets/Resources/Cabinets/PreFab/CRTs/screen19i.prefab
        objects.Add("19i", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i"));
    }

    public static GameObject Instantiate(string type, Vector3 position, Quaternion rotation, Transform parent) {
        if (objects.ContainsKey(type)) {
          ConfigManager.WriteConsole($"[CRTsFactory] {type}");
          return GameObject.Instantiate<GameObject>(objects[type], position, rotation, parent);
        }
        Debug.LogError($"CRTsFactory Factory don't have a {type} object in list: {objects.Keys.ToString()}");
        return null;
    }
}
