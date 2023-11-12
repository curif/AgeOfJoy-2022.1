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


//[RequireComponent(typeof(CabinetReplace))]
public class CabinetController : MonoBehaviour
{
    public CabinetPosition game;
    public string Name;

    [Tooltip("Positions where the player can stay to load the cabinet")]
    public List<GameObject> AgentPlayerPositions;
    [Tooltip("Teleport anchor for player teleportation")]
    public GameObject AgentPlayerTeleportAnchor;
    [Tooltip("Player/agent position assigned to the cabinet")]
    public AgentScenePosition AgentScenePosition;

    private CabinetReplace cabinetReplaceComponent;
    private List<AgentScenePosition> AgentPlayerPositionComponents;

    void Start()
    {
        AgentPlayerPositionComponents = new List<AgentScenePosition>();
        foreach (GameObject playerPos in AgentPlayerPositions)
        {
            AgentScenePosition asp = playerPos.GetComponent<AgentScenePosition>();
            if (asp != null)
                AgentPlayerPositionComponents.Add(asp);
        }

        StartCoroutine(load());
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

        while (!playerIsInSomePosition())
            yield return new WaitForSeconds(1f);

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

        if (game.CabInfo.Parts != null)
        {
            ConfigManager.WriteConsole($"[CabinetController] {game.CabInfo.name} texture parts");
            //N seconds to load a cabinet
            // float waitForSeconds = 1f / game.CabInfo.Parts.Count;
            foreach (CabinetInformation.Part part in game.CabInfo.Parts)
            {
                yield return new WaitForSeconds(0.01f);
                CabinetFactory.skinCabinetPart(cab, game.CabInfo, part);
            }

            CabinetReplace cabReplaceComp = cab.gameObject.AddComponent<CabinetReplace>();
            cabReplaceComp.AgentPlayerPositions = AgentPlayerPositions;
            cabReplaceComp.game = game;

            //gameObject.SetActive(false);
            yield return new WaitForSeconds(0.01f);
            Destroy(gameObject); //destroy me
        }


        ConfigManager.WriteConsole($"[CabinetController] Cabinet deployed  {game.CabInfo.name} ******");
    }
}
