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

        //set cabinets ID. using a For to ensure the order.
        int idx = 0;
        Shader shader = Shader.Find("Standard");
        Vector2 newTiling = new Vector2(-1, -1);
        for (idx = 0; idx < cabinetsCount; idx++)
        {
            CabinetController cc = transform.GetChild(idx).gameObject.GetComponent<CabinetController>();
            if (cc == null)
            {
                cc = transform.GetChild(idx).gameObject.AddComponent<CabinetController>();
            }
            if (cc != null)
            {
                cc.game = new();
                cc.game.Position = idx;

                //assign the cabinet number to the teleport area
                MeshRenderer renderer = cc.AgentPlayerTeleportAnchor?.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    string textureName = $"Cabinets/AgentPlayerPositionsNumbers/AgentPlayerPosition ({idx.ToString()})";
                    Texture2D numberTexture = Resources.Load<Texture2D>(textureName);
                    if (numberTexture != null)
                    {
                        Material cabinetNumberMaterial = new Material(shader);

                        // Assign the texture to the material
                        cabinetNumberMaterial.mainTexture = numberTexture;
                        //invert the texture number:
                        cabinetNumberMaterial.mainTextureScale = newTiling;
                        // Assign the material to the GameObject
                        renderer.material = cabinetNumberMaterial;

                        renderer.enabled = true;
                    }
                    else
                        ConfigManager.WriteConsoleError($"[CabinetsController] Agent Player Position number texture not found: {textureName} idx: {idx}");

                }
            }
        }

        gameRegistry = GameObject.Find("RoomInit").GetComponent<GameRegistry>();
        if (gameRegistry != null)
            StartCoroutine(load());
        else
            ConfigManager.WriteConsoleError("[CabinetsController] gameRegistry component not found");
    }

    public CabinetController GetCabinetControllerByPosition(int position)
    {
        return transform.GetComponentsInChildren<CabinetController>()
            .FirstOrDefault(cc => cc.game.Position == position);
    }

    public CabinetReplace GetCabinetReplaceByPosition(int position)
    {
        return transform.GetComponentsInChildren<CabinetReplace>()
            .FirstOrDefault(cc => cc.game.Position == position);
    }

    public int Count()
    {
        //only active gameobjects:
        int maxPosition = 0;

        foreach (Transform childTransform in transform)
        {
            // Get the CabinetController component if it exists
            CabinetController cabinetController = childTransform.GetComponent<CabinetController>();
            if (cabinetController != null)
            {
                if (maxPosition < cabinetController.game.Position)
                    maxPosition = cabinetController.game.Position;
            }
            else
            {
                // Get the CabinetReplace component if it exists
                CabinetReplace cabinetReplace = childTransform.GetComponent<CabinetReplace>();
                if (cabinetReplace != null)
                {
                    if (maxPosition < cabinetReplace.game.Position)
                        maxPosition = cabinetReplace.game.Position;
                }
            }

        }
        return maxPosition + 1;
    }

    public GameObject GetCabinetChildByPosition(int position)
    {
        // Loop through all the child objects
        foreach (Transform childTransform in transform)
        {
            // Get the CabinetController component if it exists
            CabinetController cabinetController = childTransform.GetComponent<CabinetController>();
            if (cabinetController != null && cabinetController.game.Position == position)
            {
                // Return the GameObject if the position matches
                return childTransform.gameObject;
            }

            // Get the CabinetReplace component if it exists
            CabinetReplace cabinetReplace = childTransform.GetComponent<CabinetReplace>();
            if (cabinetReplace != null && cabinetReplace.game.Position == position)
            {
                // Return the GameObject if the position matches
                return childTransform.gameObject;
            }
        }

        // Return null if no GameObject with the specified position was found
        return null;
    }

    IEnumerator load()
    {
        List<CabinetPosition> games = gameRegistry.GetSetCabinetsAssignedToRoom(
                                                Room, 
                                                transform.childCount); //persist registry with the new assignation if any.
        ConfigManager.WriteConsole($"[CabinetsController.load] Assigned {games.Count} cabinets to room {Room}");
        Loaded = false;
        foreach (CabinetPosition g in games)
        {
            if (!String.IsNullOrEmpty(g.CabinetDBName))
            {
                CabinetController cc = GetCabinetControllerByPosition(g.Position);
                if (cc != null)
                {
                    ConfigManager.WriteConsole($"[CabinetsController.load] Assigned {g}");
                    cc.game = g; //CabinetController will load the cabinet once asigned a cabinetName
                    yield return new WaitForSeconds(1f / 2f);
                }
                else
                    ConfigManager.WriteConsole($"[CabinetsController.load] child #{g.Position} don´t have a CabinetController component or was assigned previously.");
            }
        }
        ConfigManager.WriteConsole($"[CabinetsController.load] loaded cabinets");
        Loaded = true;
    }

    public CabinetPosition GetCabinetByPosition(int position)
    {
        foreach (Transform childTransform in transform)
        {
            // Get the CabinetController component if it exists
            CabinetController cabinetController = childTransform.GetComponent<CabinetController>();
            if (cabinetController?.game != null && cabinetController.game.Position == position)
                // Return the GameObject if the position matches
                return cabinetController.game;

            // Get the CabinetReplace component if it exists
            CabinetReplace cabinetReplace = childTransform.GetComponent<CabinetReplace>();
            if (cabinetReplace?.game != null && cabinetReplace.game.Position == position)
                // Return the GameObject if the position matches
                return cabinetReplace.game;
        }
        return null;
    }

    

    public bool ReplaceInRoom(int position, string room, string cabinetDBName)
    {
        //replace in the registry
        CabinetPosition toAdd = new();
        toAdd.Room = room;
        toAdd.Position = position;
        toAdd.CabinetDBName = cabinetDBName;

        //get cabinetReplace component first.
        CabinetReplace cr = GetCabinetReplaceByPosition(position);
        if (cr != null)
        {
            ConfigManager.WriteConsole($"[CabinetController.ReplaceInRoom] replacing a cabinet by [{toAdd}]");
            cr.ReplaceWith(toAdd);
            return true;
        }
        else
        {
            CabinetController cc = GetCabinetControllerByPosition(position);
            if (cc != null)
            {
                //its a non-loaded cabinet. It will load just with the assignment
                ConfigManager.WriteConsole($"[CabinetController.ReplaceInRoom] adding [{toAdd}] to a not-loaded cabinet. It will load soon...");
                cc.game = toAdd;
                return true;
            }
        }
        ConfigManager.WriteConsoleError($"[CabinetController.ReplaceInRoom] no cabinet found to replace by [{toAdd}]");
        return false;
    }

    public bool Replace(int position, string room, string cabinetDBName)
    {
        //replace in the registry
        CabinetPosition toAdd = new();
        toAdd.Room = room;
        toAdd.Position = position;
        toAdd.CabinetDBName = cabinetDBName;

        CabinetPosition toBeReplaced = gameRegistry.GetCabinetPositionInRoom(position, room);
        ConfigManager.WriteConsole($"[CabinetsController.Replace] [{toBeReplaced}] by [{toAdd}] ");
        gameRegistry.Replace(toBeReplaced, toAdd); //persists changes

        return ReplaceInRoom(position, room, cabinetDBName);
    }
}
