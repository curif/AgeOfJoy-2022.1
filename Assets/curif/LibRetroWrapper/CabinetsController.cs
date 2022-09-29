using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//distribute cabinets games in the room for respawn.

public class CabinetsController : MonoBehaviour
{
  public string Room;

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
      CabinetController cc = transform.GetChild(idx).gameObject.GetComponent<CabinetController>();
      if (cc != null)
      {
        ConfigManager.WriteConsole($"[CabinetsController] Assigned {g.CabInfo.name} to #{idx}");
        cc.game = g;
        yield return new WaitForSeconds(1f / 2f);
      }
      else
        ConfigManager.WriteConsole($"[CabinetsController] ERROR child #{idx} don´t have a CabinetController component");
      
      idx++;
    }
  }
}
