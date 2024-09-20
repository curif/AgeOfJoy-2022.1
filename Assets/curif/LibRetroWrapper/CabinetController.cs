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
using System.Configuration;


//[RequireComponent(typeof(CabinetReplace))]
public class CabinetController : MonoBehaviour
{
    public CabinetPosition game;
    public string Name;

    [Tooltip("Positions where the player could stay to see videos")]
    public List<GameObject> AgentPlayerPositions;

    [Tooltip("Positions where the player could stay to load the cabinet")]
    public List<GameObject> AgentPlayerPositionsToLoad;

    [Tooltip("Positions where the player shouldn't be to unload the cabinet")]
    public List<GameObject> AgentPlayerPositionsToUnload;
    [Tooltip("Teleport anchor for player teleportation")]
    public GameObject AgentPlayerTeleportAnchor;
    [Tooltip("Player/agent position assigned to the cabinet")]
    public AgentScenePosition AgentScenePosition;

    [Tooltip("Loaded by script")]
    public GameObject outOfOrderCabinet;

    public BackgroundSoundController backgroundSoundController;

    private CabinetReplace cabinetReplaceComponent;
    
    [Tooltip("Loaded by script")]

    // videos
    public List<AgentScenePosition> AgentPlayerPositionComponents;
    //videos
    [Tooltip("Loaded by script")]
    public List<AgentScenePosition> AgentPlayerPositionComponentsToUnload;

    //load cabinets
    [Tooltip("Loaded by script")]
    public List<AgentScenePosition> AgentPlayerPositionComponentsToLoad;

    private bool coroutineIsRunning;

    //check if the player don't moves.
    public StaticCheck staticCheck;

    public CabinetSpaceType Space;

    void Start()
    {
        AgentPlayerPositionComponents = AgentPlayerPositions
            .Select(playerPos => playerPos.GetComponent<AgentScenePosition>())
            .Where(asp => asp != null)
            .ToList();

        if (AgentPlayerPositionsToLoad == null || !AgentPlayerPositionsToLoad.Any())
        {
            AgentPlayerPositionComponentsToLoad = AgentPlayerPositionComponents;
        }
        else
        {
            AgentPlayerPositionComponentsToLoad = AgentPlayerPositionsToLoad
                .Select(playerPos => playerPos.GetComponent<AgentScenePosition>())
                .Where(asp => asp != null)
                .ToList();
        }

        if (AgentPlayerPositionsToUnload == null || !AgentPlayerPositionsToUnload.Any())
        {
            AgentPlayerPositionComponentsToUnload = AgentPlayerPositionComponentsToLoad;
        }
        else
        {
            AgentPlayerPositionComponentsToUnload = AgentPlayerPositionsToUnload
                .Select(playerPos => playerPos.GetComponent<AgentScenePosition>())
                .Where(asp => asp != null)
                .ToList();
        }

        outOfOrderCabinet = gameObject;

        GameObject player;
        player = GameObject.Find("OVRPlayerControllerGalery");
        staticCheck = player?.GetComponent<StaticCheck>();

    }

    //check if the cabinet should load for first time
    public bool LoadIsAllowed()
    {

        if (string.IsNullOrEmpty(game?.CabinetDBName))
            return false;
        if (!playerIsInSomePosition())
            return false;
        return true;
    }

    // check if the cabinet should load after it was loaded for first tiem
    public bool ReloadIsAllowed()
    {
        return playerIsInSomePosition();
    }

    public bool PlayerIsStatic()
    {
        return staticCheck.isStatic;
    }
    public bool playerIsNotInAnyUnloadPosition()
    {
        return !AgentPlayerPositionComponentsToUnload.Any(asp => asp.IsPlayerPresent);
    }

    public bool playerIsInSomePosition()
    {
        return AgentPlayerPositionComponentsToLoad.Any(asp => asp.IsPlayerPresent);
    }
}
