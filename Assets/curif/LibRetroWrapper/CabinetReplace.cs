/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class CabinetReplace : MonoBehaviour
{
    public CabinetPosition game;
    public GameObject outOfOrderCabinet;
    public Cabinet cabinet;

    public BackgroundSoundController backgroundSoundController;

    [Tooltip("The system will find it")]
    public List<AgentScenePosition> AgentPlayerPositionComponents;
    [Tooltip("The system will find it")]
    public List<AgentScenePosition> AgentPlayerPositionComponentsToUnload;
    [Tooltip("The system will find it")]
    public List<AgentScenePosition> AgentPlayerPositionComponentsToLoad;

    public bool playerIsNotInAnyUnloadPosition()
    {
        List<AgentScenePosition> positions = AgentPlayerPositionComponentsToUnload;

        if (positions == null || !positions.Any())
            positions = AgentPlayerPositionComponentsToLoad;

        return positions.All(asp => !asp.IsPlayerPresent);
    }

    public GameObject ReplaceWith(CabinetPosition newCabGame)
    {
        ConfigManager.WriteConsole($"[CabinetReplace.ReplaceWith] game: {newCabGame}");

        string cabinetPath = ConfigManager.CabinetsDB + "/" + newCabGame.CabinetDBName;
        string descriptionPath = cabinetPath + "/description.yaml";
        if (!File.Exists(descriptionPath))
        {
            ConfigManager.WriteConsoleError($"[CabinetReplace.ReplaceWith] not found: {descriptionPath}");
            return null;
        }

        ConfigManager.WriteConsole($"[CabinetReplace.ReplaceWith] replace {name} with {descriptionPath}");

        //new cabinet to test
        CabinetInformation cbInfo = null;
        try
        {
            cbInfo = CabinetInformation.fromYaml(cabinetPath);
            if (cbInfo == null)
            {
                ConfigManager.WriteConsoleError($"[CabinetReplace.ReplaceWith] ERROR NULL cabinet - new cabinet from yaml: {descriptionPath}");
                return null;
            }

            ConfigManager.WriteConsole($"[CabinetReplace.ReplaceWith] cabinet problems (if any):...");
            CabinetInformation.showCabinetProblems(cbInfo);

            //cabinet inseption
            ConfigManager.WriteConsole($"[CabinetReplace.ReplaceWith] Deploy replacement cabinet {cbInfo.name}");
            //note: factory will add this CabinetReplace component (this component) to the new cabinet.

            Vector3 adjustedPosition = transform.position + Vector3.up * 0.5f;
            Cabinet cab = CabinetFactory.fromInformation(cbInfo, newCabGame.Room, newCabGame.Position,
                                                         adjustedPosition, transform.rotation,
                                                         transform.parent, 
                                                         AgentPlayerPositionComponentsToLoad,
                                                         backgroundSoundController
                                                         );

            cab.gameObject.SetActive(false);
            CabinetFactory.skinFromInformation(cab, cbInfo);

            //add CabinetReplace for the next replacement. CabinetController do the same.
            CabinetReplace cabReplaceComp = cab.gameObject.AddComponent<CabinetReplace>();
            cabReplaceComp.AgentPlayerPositionComponents = AgentPlayerPositionComponents;
            cabReplaceComp.AgentPlayerPositionComponentsToUnload = AgentPlayerPositionComponentsToUnload;
            cabReplaceComp.AgentPlayerPositionComponentsToLoad = AgentPlayerPositionComponentsToLoad;
            cabReplaceComp.game = newCabGame;
            cabReplaceComp.outOfOrderCabinet = gameObject;
            cabReplaceComp.backgroundSoundController = backgroundSoundController;
            cab.gameObject.SetActive(true);

            UnityEngine.Object.Destroy(gameObject);

            ConfigManager.WriteConsole("[CabinetReplace.ReplaceWith] New Tested Cabinet deployed ******");
            return cab.gameObject;
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleError($"[CabinetReplace.ReplaceWith] ERROR loading cabinet from description {descriptionPath}: {ex}");
            return null;
        }
    }


}
