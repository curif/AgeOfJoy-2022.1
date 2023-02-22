/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

//distribute cabinets games in the room for respawn.

public class CabinetsController : MonoBehaviour
{
  public string Room;
  //public LightProbeGroup ClosestLightProbeGroup = null;

  GameRegistry gameRegistry;

  void Start()
  {
    gameRegistry = GameObject.Find("RoomInit").GetComponent<GameRegistry>();
    if (gameRegistry != null)
      StartCoroutine(load());
    else
      ConfigManager.WriteConsole("[CabinetsController] ERROR gameregistry component not found");
  }

  IEnumerator load()
  {
    List<CabinetPosition> games = gameRegistry.GetCabinetsAssignedToRoom(Room, transform.childCount); //persist registry with the new assignation if any.
    ConfigManager.WriteConsole($"[CabinetsController] Assigned {games.Count} cabinets to room {Room}");
    int idx = 0;
    /* not neccesary. Its possible to order the cabinets in design time
    //games.Sort((x, y) => random.Next() > random.Next() ? 1 : -1);
    List<CabinetController> controllers = (
      from t in transform
      let cc = t.gameObject.GetComponent<CabinetController>()
      where cc != null 
      orderby cc.priority descending
      select cc
    ).ToList<CabinetController>();
    */
    foreach (CabinetPosition g in games)
    {
      if (g.CabInfo != null)
      {
        CabinetController cc = transform.GetChild(idx).gameObject.GetComponent<CabinetController>();
        if (cc != null && (cc.game == null || String.IsNullOrEmpty(cc.game.CabinetDBName)))
        {
          ConfigManager.WriteConsole($"[CabinetsController] Assigned {g.CabInfo.name} to #{idx}");
          cc.game = g;
          yield return new WaitForSeconds(1f / 2f);
        }
        else
          ConfigManager.WriteConsole(
            $"[CabinetsController.load] ERROR child #{idx} don´t have a CabinetController component");
      }
      else
      {
        ConfigManager.WriteConsole($"[CabinetsController.load] ERROR Assigned {g.CabinetDBName} in #{idx} doesn't have a Cabinet Information assigned, possible error when load cabinet.");
      }

      idx++;
    }
  }
}
