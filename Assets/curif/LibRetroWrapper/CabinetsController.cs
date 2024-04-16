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
using System.IO;

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

    /// <summary> Gameobjects in the cabinets' tree in the room </summary>
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

    void Start()
    {
        gameRegistry = GameObject.Find("FixedObject").GetComponent<GameRegistry>();

        loadCabinetList();
        initalizeCabinets();
        load();
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
                cabInfo.CabinetController = cabInfo.GameObjectOutOfOrder.AddComponent<CabinetController>();

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

            //MaxAllowedSpace to identify NPC animation
            AgentScenePosition pos = cabInfo.CabinetController.AgentScenePosition?.GetComponent<AgentScenePosition>();
            if (pos != null)
                pos.MaxAllowedSpace = cabInfo.CabinetController.Space.MaxAllowedSpace;

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

        if (string.IsNullOrEmpty(Room))
            throw new Exception("[cabinetsController.load] room id not assigned.");

        ConfigManager.WriteConsole($"[CabinetsController.load] ==== {Room} ====");

        //persist registry with the new assignation if any.
        List<CabinetPosition> cabsPos = gameRegistry.GetSetCabinetsAssignedToRoom(Room,
                                                                                transform.childCount);
        ConfigManager.WriteConsole($"[CabinetsController.load] Assigning {cabsPos.Count} cabinets to room {Room}");

        //load already assigned games to cabinets
        foreach (CabinetPosition cabPos in cabsPos)
        {
            CabinetControllerInformation cabInfo = GetCabinetControllerInformationByPosition(cabPos.Position);
            //CabinetController will load the cabinet once asigned a cabinetName
            cabInfo.CabinetController.game = cabPos;
            ConfigManager.WriteConsole($"[CabinetsController.load] Load previously assigned {cabPos}");
        }

        //load unnasigned cabinets. Cabinets that aren't assigned to any room. New cabinets.
        if (cabsPos.Count() < Cabinets.Count())
        {
            ConfigManager.WriteConsole($"[CabinetsController.load] {Room} there are {Cabinets.Count() - cabsPos.Count()} pending assignments");

            List<CabinetControllerInformation> remainingOutOfOrderCabs = Cabinets.Where(cab =>
                        string.IsNullOrEmpty(cab.CabinetController.game.CabinetDBName)).ToList();
            List<string> cabNames = gameRegistry.GetUnassignedCabinets().
                                                    OrderBy(x => UnityEngine.Random.value).ToList();
            List<string> occupiedSpaces = cabNames.Select(cabName =>
            {
                string cabPath = Path.Combine(ConfigManager.CabinetsDB, cabName);
                CabinetInformation cabInfo = CabinetInformation.fromYaml(cabPath);
                return cabInfo.space;
            }).ToList();
            ConfigManager.WriteConsole($"[CabinetsController.load] {Room}"
                                        + $" unnasigned cabinets count: {cabNames.Count}");
            foreach (CabinetControllerInformation cabCtrl in remainingOutOfOrderCabs)
            {
                int bestFitIndex = cabCtrl.CabinetController.Space.BestFit(occupiedSpaces);
                if (bestFitIndex != -1)
                {
                    CabinetPosition cabPos = gameRegistry.AssignOrAddCabinet(Room,
                                                            cabCtrl.Position,
                                                            cabNames[bestFitIndex]);
                    cabCtrl.CabinetController.game = cabPos;
                    ConfigManager.WriteConsole($"[CabinetsController.load] {Room}#{cabCtrl.Position}"
                                                + $" assigned cab: {cabNames[bestFitIndex]}"
                                                + $" allowed: {cabCtrl.CabinetController.Space.MaxAllowedSpace} ");

                    occupiedSpaces.RemoveAt(bestFitIndex);
                    cabNames.RemoveAt(bestFitIndex);
                }
            }

            if (gameRegistry.NeedsSave())
                gameRegistry.Persist();

            //assign a random to non-assigned.
            //don't persist in gameRegistry.   

            remainingOutOfOrderCabs = Cabinets.Where(cab =>
                            string.IsNullOrEmpty(cab.CabinetController.game.CabinetDBName)).ToList();
            if (remainingOutOfOrderCabs.Count() > 0)
            {
                ConfigManager.WriteConsole($"[CabinetsController.load] {Room} random {remainingOutOfOrderCabs.Count} pending assignments");

                cabNames = gameRegistry.GetRandomizedAllCabinetNames();
                occupiedSpaces = cabNames.Select(cabName =>
                    {
                        string cabPath = Path.Combine(ConfigManager.CabinetsDB, cabName);
                        CabinetInformation cabInfo = CabinetInformation.fromYaml(cabPath);
                        return cabInfo.space;
                    }).ToList();
                ConfigManager.WriteConsole($"[CabinetsController.load] {Room} {occupiedSpaces.Count} occupied spaces found in cabinets");

                foreach (CabinetControllerInformation cabCtrl in remainingOutOfOrderCabs)
                {
                    int bestFitIndex = cabCtrl.CabinetController.Space.BestFit(occupiedSpaces);
                    if (bestFitIndex == -1)
                    {
                        ConfigManager.WriteConsole($"[CabinetsController.load] {Room}#{cabCtrl.Position}"
                                                + $" allowed {cabCtrl.CabinetController.Space.MaxAllowedSpace} "
                                                + $" not fit found in: {occupiedSpaces.ToString()}");
                        continue;
                    }
                    CabinetPosition cabPos = gameRegistry.AssignOrAddCabinet(Room,
                                                            cabCtrl.Position,
                                                            cabNames[bestFitIndex]);
                    cabCtrl.CabinetController.game = cabPos;
                    ConfigManager.WriteConsole($"[CabinetsController.load] {Room}#{cabCtrl.Position} "
                                                + $" randomly assigned cab: {cabNames[bestFitIndex]}"
                                                + $" max allowed: {cabCtrl.CabinetController.Space.MaxAllowedSpace} ");

                    occupiedSpaces.RemoveAt(bestFitIndex);
                    cabNames.RemoveAt(bestFitIndex);
                }
            }
        }

        ConfigManager.WriteConsole($"[CabinetsController.load] {Room} END loaded cabinets");
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
                try
                {
                    CabinetFactory.skinCabinetPart(cab, cc.game.CabInfo, part);
                }
                catch (System.Exception ex)
                {
                    ConfigManager.WriteConsoleException($"[CabinetController] skinCabinetPart {cc.game.CabInfo.name}", ex);
                }
            }
        }

        CabinetReplace cabReplaceComp = cab.gameObject.AddComponent<CabinetReplace>();
        cabReplaceComp.AgentPlayerPositionComponents = cc.AgentPlayerPositionComponents;
        cabReplaceComp.AgentPlayerPositionComponentsToUnload = cc.AgentPlayerPositionComponentsToUnload;
        cabReplaceComp.AgentPlayerPositionComponentsToLoad = cc.AgentPlayerPositionComponentsToLoad;
        cabReplaceComp.outOfOrderCabinet = cci.GameObjectOutOfOrder;
        cabReplaceComp.backgroundSoundController = backgroundSoundController;
        cabReplaceComp.cabinet = cab;
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
