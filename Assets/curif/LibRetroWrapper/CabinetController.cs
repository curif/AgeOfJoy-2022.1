/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;

//[RequireComponent(typeof(CabinetReplace))]
public class CabinetController : MonoBehaviour
{
    public CabinetPosition game;
    public string Name;

    [Tooltip("Positions where the player can stay to load the cabinet")]
    public List<GameObject> AgentPlayerPositions;
    [Tooltip("Positions where the player shouldn't be to unload the cabinet")]
    public List<GameObject> AgentPlayerPositionsToUnload;
    [Tooltip("Teleport anchor for player teleportation")]
    public GameObject AgentPlayerTeleportAnchor;
    [Tooltip("Player/agent position assigned to the cabinet")]
    public AgentScenePosition AgentScenePosition;

    public GameObject outOfOrderCabinet;

    private CabinetReplace cabinetReplaceComponent;
    private List<AgentScenePosition> AgentPlayerPositionComponents;
    private List<AgentScenePosition> AgentPlayerPositionComponentsToUnload;

    private bool coroutineIsRunning;

    void Start()
    {
        AgentPlayerPositionComponents = AgentPlayerPositions
       .Select(playerPos => playerPos.GetComponent<AgentScenePosition>())
       .Where(asp => asp != null)
       .ToList();

        AgentPlayerPositionComponentsToUnload = AgentPlayerPositionsToUnload
        .Select(playerPos => playerPos.GetComponent<AgentScenePosition>())
        .Where(asp => asp != null)
        .ToList();

        outOfOrderCabinet = gameObject;

        StartLoad();
    }

    void OnEnable()
    {
        StartLoad();
    }

    void StartLoad()
    {
        if (!coroutineIsRunning)
        {
            coroutineIsRunning = true;
            StartCoroutine(load());
        }
    }

    private bool playerIsNotInAnyUnloadPosition()
    {
        foreach (AgentScenePosition asp in AgentPlayerPositionComponentsToUnload)
        {
            if (asp.IsPlayerPresent)
                return false;
        }
        return true;
    }

    private bool playerIsInSomePosition()
    {
        foreach (AgentScenePosition asp in AgentPlayerPositionComponents)
        {
            if (asp.IsPlayerPresent)
                return true;
        }
        return false;
    }

    IEnumerator load()
    {

        while (game == null || string.IsNullOrEmpty(game.CabinetDBName))
            yield return new WaitForSeconds(2f);

        //load the new cabinet. 
        // the load process repeats forever, because the new cabinet created can be unloaded an re-activate
        // this object when that occurs.
        while (!playerIsInSomePosition())
        {
            ConfigManager.WriteConsole($"[CabinetController] waiting for  {game.CabInfo?.name}");
            yield return new WaitForSeconds(1f);
        }
        if (game.CabInfo == null)
        {
            game.CabInfo = CabinetInformation.fromName(game.CabinetDBName);
            if (game.CabInfo == null)
            {
                ConfigManager.WriteConsoleError($"[CabinetController.load] loading cabinet from description fails {game}");
                yield break;
            }
            yield return new WaitForSeconds(0.01f);
        }

        ConfigManager.WriteConsole($"[CabinetController] Loading cabinet  {game.CabInfo.name} ******");

        Cabinet cab;
        Transform parent = transform.parent;
        try
        {
            //cabinet inception
            ConfigManager.WriteConsole($"[CabinetController] Deploy cabinet {game}");
            cab = CabinetFactory.fromInformation(game.CabInfo, game.Room, game.Position, transform.position, transform.rotation, parent, AgentPlayerPositions);
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetController] loading cabinet from description {game.CabInfo.name}", ex);
            yield break;
        }
        if (cab == null)
        {
            ConfigManager.WriteConsoleError($"[CabinetController] loading cabinet from description {game.CabInfo.name}");
            yield break;
        }

        cab.gameObject.SetActive(false);

        if (game.CabInfo.Parts != null)
        {
            ConfigManager.WriteConsole($"[CabinetController] {game.CabInfo.name} texture parts");
            foreach (CabinetInformation.Part part in game.CabInfo.Parts)
            {
                yield return new WaitForSeconds(0.01f);
                CabinetFactory.skinCabinetPart(cab, game.CabInfo, part);
            }

        }

        CabinetReplace cabReplaceComp = cab.gameObject.AddComponent<CabinetReplace>();
        cabReplaceComp.AgentPlayerPositions = AgentPlayerPositions;
        cabReplaceComp.AgentPlayerPositionsToUnload = AgentPlayerPositionsToUnload;
        cabReplaceComp.outOfOrderCabinet = outOfOrderCabinet;
        cabReplaceComp.game = game;

        //activate the new cabinet
        cab.gameObject.SetActive(true);
        ConfigManager.WriteConsole($"[CabinetController] Cabinet deployed  {game.CabInfo.name} ******");

        //this didn't work:
        //Coroutines are also stopped when the MonoBehaviour is destroyed or if the GameObject the 
        // MonoBehaviour is attached to is disabled. Coroutines are not stopped when a MonoBehaviour 
        // is disabled.

        //deactivate the out of order cabinet.
        coroutineIsRunning = false;
        gameObject.SetActive(false); //this corroutine is suspended.
                                     // yield return new WaitForSeconds(1f);

        //if the code reach this position is because was reactivated.
        // ConfigManager.WriteConsole($"[CabinetController] Cabinet out of order was reactivated: game {game.CabInfo.name} ******");

    }
}
