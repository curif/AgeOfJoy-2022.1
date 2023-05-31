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

    public GameRegistry gameRegistry;

    public bool Loaded = false; //set when the room cabinets where assigned.

    [SerializeField]
    private int cabinetsCount; 
    public int CabinetsCount
    {
        get
        {
            return cabinetsCount;
        }
    }

    void Start()
    {
        cabinetsCount = transform.childCount;
        gameRegistry = GameObject.Find("RoomInit").GetComponent<GameRegistry>();
        if (gameRegistry != null)
            StartCoroutine(load());
        else
            ConfigManager.WriteConsoleError("[CabinetsController] gameRegistry component not found");
    }

    IEnumerator load()
    {
        List<CabinetPosition> games = gameRegistry.GetSetCabinetsAssignedToRoom(Room, transform.childCount); //persist registry with the new assignation if any.
        ConfigManager.WriteConsole($"[CabinetsController] Assigned {games.Count} cabinets to room {Room}");
        Loaded = false;
        int idx = 0;
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
                    ConfigManager.WriteConsoleError(
                      $"[CabinetsController.load] child #{idx} don´t have a CabinetController component");
            }
            else
            {
                ConfigManager.WriteConsoleError($"[CabinetsController.load] Assigned {g.CabinetDBName} in #{idx} doesn't have a Cabinet Information assigned, possible error when load cabinet.");
            }

            idx++;
        }
        ConfigManager.WriteConsole($"[CabinetsController] loaded to {idx - 1} cabinets");
        Loaded = true;
    }
}
