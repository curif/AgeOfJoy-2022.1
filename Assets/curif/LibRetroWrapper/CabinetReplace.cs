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

public class CabinetReplace : MonoBehaviour
{
    [Tooltip("The system will find it")]
    public List<GameObject> AgentPlayerPositions;
    public CabinetPosition game;

    public void ReplaceWith(CabinetPosition newCabGame)
    {
        ConfigManager.WriteConsole($"[CabinetReplace.ReplaceWith] game: {newCabGame}");

        string cabinetPath = ConfigManager.CabinetsDB + "/" + newCabGame.CabinetDBName;
        string descriptionPath = cabinetPath + "/description.yaml";
        if (!File.Exists(descriptionPath))
        {
            ConfigManager.WriteConsoleError($"[CabinetReplace.ReplaceWith] not found: {descriptionPath}");
            return;
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
                return;
            }

            ConfigManager.WriteConsole($"[CabinetReplace.ReplaceWith] cabinet problems (if any):...");
            CabinetInformation.showCabinetProblems(cbInfo);

            //cabinet inseption
            ConfigManager.WriteConsole($"[CabinetReplace.ReplaceWith] Deploy replacement cabinet {cbInfo.name}");
            //note: factory will add this CabinetReplace component (this component) to the new cabinet.

            Vector3 adjustedPosition = transform.position + Vector3.up * 0.5f;
            Cabinet cab = CabinetFactory.fromInformation(cbInfo, newCabGame.Room, newCabGame.Position,
                                                         adjustedPosition, transform.rotation,
                                                         transform.parent, AgentPlayerPositions);
            /*Cabinet cab = CabinetFactory.fromInformation(cbInfo, newCabGame.Room, newCabGame.Position,
                                                            transform.position, transform.rotation,
                                                            transform.parent, AgentPlayerPositions);
            */
            
            CabinetFactory.skinFromInformation(cab, cbInfo);

            //add CabinetReplace for the next replacement. CabinetController do the same.
            CabinetReplace cabReplaceComp = cab.gameObject.AddComponent<CabinetReplace>();
            cabReplaceComp.AgentPlayerPositions = AgentPlayerPositions;
            cabReplaceComp.game = newCabGame;

            UnityEngine.Object.Destroy(gameObject);

            ConfigManager.WriteConsole("[CabinetReplace.ReplaceWith] New Tested Cabinet deployed ******");
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleError($"[CabinetReplace.ReplaceWith] ERROR loading cabinet from description {descriptionPath}: {ex}");
        }
    }
}
