/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;
using System.Collections.Generic;
//other objects in cabinet, interchangeable.

public static class CoinSlotsFactory {
    public static Dictionary<string, GameObject> objects = new();
    
    static CoinSlotsFactory() {
        objects.Add("coin-slot-small", Resources.Load<GameObject>("Cabinets/PreFab/CoinSlots/CoinSlotPlastic"));
        objects.Add("coin-slot-double", Resources.Load<GameObject>("Cabinets/PreFab/CoinSlots/CoinSlotPlasticDouble"));
    }

    public static GameObject Instantiate(string type, Vector3 position, Quaternion rotation, Transform parent) {
        if (objects.ContainsKey(type)) {
            return GameObject.Instantiate<GameObject>(objects[type], position, rotation, parent);
        }
        Debug.LogError($"CoinSlotsFactory Factory don't have a {type} object in list: {objects.Keys.ToString()}");
        return null;
    }
}
