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
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Reflection;

//distribute cabinets games in the room for respawn.

public class CabinetsController : MonoBehaviour
{
    public string Room;
    //public LightProbeGroup ClosestLightProbeGroup = null;

    public GameRegistry gameRegistry;
    public BackgroundSoundController backgroundSoundController;

    public bool Loaded = false; //set when the room cabinets where assigned.

    [SerializeField]
    public List<CabinetControllerInformation> Cabinets;

    public bool CoroutineIsRunning;

    [SerializeField]
    private int cabinetsCount;
    public int CabinetsCount
    {
        get
        {
            return cabinetsCount;
        }
    }

    [Serializable]
    public class CabinetControllerInformation
    {
        public int Position;
        //the cabinet outoforder
        public GameObject GameObjectOutOfOrder;
        public CabinetController CabinetController;

        //the real one assigned
        public GameObject GameObjectReplacement;
        public CabinetReplace CabinetReplace;

        //true: the OutOfOrder cab is active, else is the replacement
        public bool IsOutOfOrderActive = true;

        public GameObject Cabinet()
        {
            if (IsOutOfOrderActive)
                return GameObjectOutOfOrder;

            return GameObjectReplacement;
        }

        public CabinetPosition Game()
        {
            if (IsOutOfOrderActive)
                return CabinetController.game;
            return CabinetReplace.game;
        }

        public void ActivateReplacement()
        {
            IsOutOfOrderActive = false;
            GameObjectOutOfOrder.SetActive(false);
            GameObjectReplacement?.SetActive(true);
        }

        public void ActivateOutOfOrder()
        {
            IsOutOfOrderActive = true;
            GameObjectOutOfOrder.SetActive(true);
            GameObjectReplacement?.SetActive(false);
        }
    }

    void loadCabinetList()
    {
        cabinetsCount = transform.childCount;
        for (int idx = 0; idx < cabinetsCount; idx++)
        {
            CabinetControllerInformation cabInfo = new();
            cabInfo.Position = idx;
            cabInfo.GameObjectOutOfOrder = transform.GetChild(idx).gameObject;
            cabInfo.CabinetController = cabInfo.GameObjectOutOfOrder.GetComponent<CabinetController>();
            if (cabInfo.CabinetController == null)
            {
                cabInfo.CabinetController = cabInfo.GameObjectOutOfOrder.AddComponent<CabinetController>();
            }
            Cabinets.Add(cabInfo);
        }
    }

    void initalizeCabinets()
    {
        if (Cabinets.Count() == 0)
            throw new Exception("Cabinet tree without cabinets");

        int idx = 0;
        Shader shader = Shader.Find("Standard");
        Vector2 newTiling = new Vector2(-1, -1);

        foreach (CabinetControllerInformation cabInfo in Cabinets)
        {
            cabInfo.CabinetController.game = new();
            cabInfo.CabinetController.game.Position = idx;
            cabInfo.CabinetController.backgroundSoundController = backgroundSoundController;

            //assign the cabinet number to the teleport area
            MeshRenderer renderer = cabInfo.CabinetController.AgentPlayerTeleportAnchor?.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                string textureName = $"Cabinets/AgentPlayerPositionsNumbers/AgentPlayerPosition_{idx.ToString()}";
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
                    ConfigManager.WriteConsoleError($"[CabinetsController] initalizeCabinets Agent Player Position number texture not found: {textureName} idx: {idx}");

            }
            idx++;
        }
    }

    void Start()
    {
        gameRegistry = GameObject.Find("FixedObject").GetComponent<GameRegistry>();

        loadCabinetList();
        initalizeCabinets();
        load();
    }

    public CabinetController GetCabinetControllerByPosition(int position)
    {
        CabinetControllerInformation cabinet = Cabinets.FirstOrDefault(c => c.Position == position);
        return cabinet?.CabinetController;
    }

    public CabinetReplace GetCabinetReplaceByPosition(int position)
    {
        CabinetControllerInformation cabinet = Cabinets.FirstOrDefault(c => c.Position == position);
        return cabinet?.CabinetReplace;
    }

    public int Count()
    {
        //only active gameobjects:
        return Cabinets.Max(cabinet => cabinet.Position) + 1;
    }

    public CabinetControllerInformation GetCabinetControllerInformationByPosition(int position)
    {
        return Cabinets.FirstOrDefault(c => c.Position == position);
    }
    public GameObject GetCabinetChildByPosition(int position)
    {
        CabinetControllerInformation cabinet = Cabinets.FirstOrDefault(c => c.Position == position);
        return cabinet?.Cabinet();
    }

    //returns the game (CabinetPosition) assigned to the position
    public CabinetPosition GetCabinetByPosition(int position)
    {
        CabinetControllerInformation cabinet = Cabinets.FirstOrDefault(c => c.Position == position);
        return cabinet?.Game();
    }

    void load()
    {
        Loaded = false;

        //persist registry with the new assignation if any.
        List<CabinetPosition> games = gameRegistry.GetSetCabinetsAssignedToRoom(Room,
                                                                                transform.childCount);
        ConfigManager.WriteConsole($"[CabinetsController.load] Assigning {games.Count} cabinets to room {Room}");

        //assign games to cabinets
        foreach (CabinetPosition g in games)
        {
            CabinetControllerInformation cabInfo = GetCabinetControllerInformationByPosition(g.Position);
            //CabinetController will load the cabinet once asigned a cabinetName
            cabInfo.CabinetController.game = g;
            ConfigManager.WriteConsole($"[CabinetsController.load] Assigned {g}");
        }

        if (games.Count() < Cabinets.Count())
        {
            //assign a random to non-assigned.        
            List<string> RandomCabsName = gameRegistry.GetRandomizedAllCabinetNames();
            int idx = 0;
            foreach (CabinetControllerInformation cabInfo in Cabinets)
            {
                if (string.IsNullOrEmpty(cabInfo.CabinetController.game.CabinetDBName))
                {
                    cabInfo.CabinetController.game.CabinetDBName = RandomCabsName[idx];
                    ConfigManager.WriteConsole($"[CabinetsController.load] randomly assigned {cabInfo.CabinetController.game}");
                    idx++;
                }
                if (idx + 1 > RandomCabsName.Count())
                    break;
            }
            ConfigManager.WriteConsole($"[CabinetsController.load] loaded cabinets");
        }
        Loaded = true;
    }

    public bool ReplaceInRoom(int position, string room, string cabinetDBName)
    {
        //replace in the registry
        CabinetPosition toAdd = new();
        toAdd.Room = room;
        toAdd.Position = position;
        toAdd.CabinetDBName = cabinetDBName;

        //get cabinetReplace component first.
        CabinetControllerInformation cabinet = Cabinets.FirstOrDefault(c => c.Position == position);

        CabinetReplace cr = cabinet.CabinetReplace;
        if (cr != null)
        {
            ConfigManager.WriteConsole($"[CabinetController.ReplaceInRoom] replacing a cabinet by [{toAdd}]");
            GameObject newCab = cr.ReplaceWith(toAdd);
            if (newCab != null)
            {
                cabinet.GameObjectReplacement = newCab;
                cabinet.CabinetReplace = newCab.GetComponent<CabinetReplace>();
            }
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

    void checkAndLoadCabinet(CabinetControllerInformation cci)
    {
        if (!cci.IsOutOfOrderActive)
            return;

        CabinetController cc = cci.CabinetController;
        if (string.IsNullOrEmpty(cc?.game?.CabinetDBName))
            return;

        //load the new cabinet. 
        // the load process repeats forever, because the new cabinet created can be unloaded an re-activate
        // this object when that occurs.
        if (!cc.playerIsInSomePosition())
            return;

        if (cci.GameObjectReplacement != null)
        {
            //replacement exists. Only activate it.
            // don't wait player static.
            cci.ActivateReplacement();
            return;
        }

        //don't load a new cabinet if the player isn't static.
        if (!cc.staticCheck.isStatic)
            return;

        if (cc.game.CabInfo == null)
        {
            cc.game.CabInfo = CabinetInformation.fromName(cc.game.CabinetDBName);
            if (cc.game.CabInfo == null)
                ConfigManager.WriteConsoleError($"[CabinetsController.load] loading cabinet from description fails {cc.game}");
            return;
        }

        ConfigManager.WriteConsole($"[CabinetsController] Loading cabinet  {cc.game.CabInfo.name} ******");

        Cabinet cab;
        try
        {
            //cabinet inception
            ConfigManager.WriteConsole($"[CabinetController] Deploy cabinet {cc.game}");
            cab = CabinetFactory.fromInformation(cc.game.CabInfo, cc.game.Room, cc.game.Position,
                                                 cci.GameObjectOutOfOrder.transform.position,
                                                 cci.GameObjectOutOfOrder.transform.rotation,
                                                 cci.GameObjectOutOfOrder.transform.parent,
                                                 cc.AgentPlayerPositionComponents,
                                                 cc.backgroundSoundController);
        }
        catch (System.Exception ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetController] loading cabinet from description {cc.game.CabInfo.name}", ex);
            return;
        }
        if (cab == null)
        {
            ConfigManager.WriteConsoleError($"[CabinetController] loading cabinet from description {cc.game.CabInfo.name}");
            return;
        }

        cci.GameObjectReplacement = cab.gameObject;

        if (cc.game.CabInfo.Parts != null)
        {
            ConfigManager.WriteConsole($"[CabinetController] {cc.game.CabInfo.name} texture parts");
            foreach (CabinetInformation.Part part in cc.game.CabInfo.Parts)
            {
                // yield return new WaitForSeconds(0.01f);
                CabinetFactory.skinCabinetPart(cab, cc.game.CabInfo, part);
            }
        }

        CabinetReplace cabReplaceComp = cab.gameObject.AddComponent<CabinetReplace>();
        cabReplaceComp.AgentPlayerPositionComponents = cc.AgentPlayerPositionComponents;
        cabReplaceComp.AgentPlayerPositionComponentsToUnload = cc.AgentPlayerPositionComponentsToUnload;
        cabReplaceComp.AgentPlayerPositionComponentsToLoad = cc.AgentPlayerPositionComponentsToLoad;
        cabReplaceComp.outOfOrderCabinet = cci.GameObjectOutOfOrder;
        cabReplaceComp.backgroundSoundController = backgroundSoundController;
        cabReplaceComp.game = cc.game;
        cci.CabinetReplace = cabReplaceComp;

        //this didn't work:
        //Coroutines are also stopped when the MonoBehaviour is destroyed or if the GameObject the 
        // MonoBehaviour is attached to is disabled. Coroutines are not stopped when a MonoBehaviour 
        // is disabled.

        //activate the new cabinet
        cci.ActivateReplacement();
        ConfigManager.WriteConsole($"[CabinetController] Cabinet deployed  {cc.game.CabInfo.name} ******");

    }

    void OnEnable()
    {
        startRun();
    }

    void startRun()
    {
        if (!CoroutineIsRunning)
        {
            CoroutineIsRunning = true;
            StartCoroutine(run());
        }
    }

    void checkAndUnloadCabinet(CabinetControllerInformation cabInfo)
    {
        if (cabInfo.IsOutOfOrderActive)
            return;

        if (!cabInfo.CabinetReplace.playerIsNotInAnyUnloadPosition())
            return;

        cabInfo.ActivateOutOfOrder();

        // outOfOrderCabinet.SetActive(true); //reactivate the out of order cabinet before destruction
        // Destroy(gameObject); //destroy me
    }

    IEnumerator run()
    {
        while (true)
        {
            foreach (CabinetControllerInformation cabInfo in Cabinets)
            {
                checkAndLoadCabinet(cabInfo);
                checkAndUnloadCabinet(cabInfo);
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
