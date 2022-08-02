using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CabinetController : MonoBehaviour
{
  public CabinetPosition game;

  void Start()
  {
    StartCoroutine(load());
  }

  IEnumerator load()
  {

    while (game == null || game.CabInfo == null)
      yield return new WaitForSeconds(1f);

    try
    {
      //cabinet inseption
      ConfigManager.WriteConsole($"[CabinetController] Deploy cabinet {game.CabInfo.name} #{game.Position}");
      Cabinet cab = CabinetFactory.fromInformation(game.CabInfo, transform.position, transform.rotation);
      cab.gameObject.transform.parent = transform.parent;
      cab.gameObject.name = $"Cabinet-{game.Position}";
      // UnityEngine.Object.Destroy(gameObject);
      gameObject.SetActive(false);
    }
    catch (System.Exception ex)
    {
      ConfigManager.WriteConsole($"[CabinetController] ERROR loading cabinet from description {game.CabInfo.name}: {ex}");
    }
    ConfigManager.WriteConsole($"[CabinetController] Cabinet deployed  {game.CabInfo.name} ******");
  }
}
