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
using UnityEngine.XR.Interaction.Toolkit;
using static ConfigInformation;
using System.Dynamic;
using YamlDotNet.Core;

//distribute cabinets games in the room for respawn.

public class CabinetsController : MonoBehaviour
{
    public string Room;
    //public LightProbeGroup ClosestLightProbeGroup = null;

    public GameRegistry gameRegistry;
    public BackgroundSoundController backgroundSoundController;

    public bool Loaded = false; //set when the room cabinets where assigned.

    [SerializeField]
    public List<CabinetControllerInformation> CabinetsCtrlInfo;

    public bool CoroutineIsRunning;

    public float TimeToWaitBetweenChecks = 0.5f;

    GameObject PlayerControllerGameObject;


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

        private bool isFaulty = false;

        public bool IsFaulty { get => isFaulty; }

        public GameObject Cabinet()
        {
            if (IsOutOfOrderActive)
                return GameObjectOutOfOrder;

            return GameObjectReplacement;
        }

        public void SetAsFaultyCabinet()
        {
            isFaulty = true;
        }

        public CabinetPosition Game()
        {
            if (IsOutOfOrderActive)
                return CabinetController.game;
            return CabinetReplace.game;
        }

        public void ActivateReplacement()
        {
            if (isFaulty)
                return;
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
        PlayerControllerGameObject = GameObject.Find("OVRPlayerControllerGalery");

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

            CabinetsCtrlInfo.Add(cabInfo);
        }
    }

    void initalizeCabinets()
    {
        if (CabinetsCtrlInfo.Count() == 0)
            throw new Exception("Cabinet tree without cabinets");

        int idx = 0;
        Shader shader = Shader.Find("Standard");
        Vector2 newTiling = new Vector2(-1, -1);

        foreach (CabinetControllerInformation cabInfo in CabinetsCtrlInfo)
        {
            cabInfo.CabinetController.game = new();
            cabInfo.CabinetController.game.Position = idx;

            cabInfo.CabinetController.backgroundSoundController = backgroundSoundController;

            //MaxAllowedSpace to identify NPC animation
            AgentScenePosition pos = cabInfo.CabinetController.AgentScenePosition?.GetComponent<AgentScenePosition>();
            if (pos != null)
                pos.MaxAllowedSpace = cabInfo.CabinetController.Space.MaxAllowedSpace;

            GameObject agentPlayerTeleportAnchor = cabInfo.CabinetController.AgentPlayerTeleportAnchor;
            if (agentPlayerTeleportAnchor == null)
                continue;

            // Assign the cabinet number to the teleport area
            MeshRenderer renderer = agentPlayerTeleportAnchor.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = agentPlayerTeleportAnchor.GetComponent<MeshFilter>();
            
            if (renderer != null && meshFilter != null)
            {
                // Construct the mesh file path based on the index (idx), starting at 0
                string meshPath = $"Cabinets/AgentPlayerPositionsNumbers/NumberMeshes/SM_Number_{idx}";

                // Load the dynamically chosen mesh
                Mesh numberMesh = Resources.Load<Mesh>(meshPath);
                // Load the material
                Material playerNumberMaterial = Resources.Load<Material>("Cabinets/AgentPlayerPositionsNumbers/NumberMeshes/M_Playernumber");

                if (numberMesh != null && playerNumberMaterial != null)
                {
                    // Assign the new mesh to the MeshFilter
                    meshFilter.mesh = numberMesh;

                    // Assign the loaded material to the renderer
                    renderer.material = playerNumberMaterial;

                    // Set the scale of the anchor to 1,1,1
                    cabInfo.CabinetController.AgentPlayerTeleportAnchor.transform.localScale = new Vector3(1, 1, 1);

                    // Subtract 90 degrees from the y-axis rotation
                    Vector3 currentRotation = agentPlayerTeleportAnchor.transform.eulerAngles;
                    agentPlayerTeleportAnchor.transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y - 180, currentRotation.z);

                    // Set Cast Shadows to OFF; a bunch of stuff under here is to force these guys to batch
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    // Set Light Probes to OFF
                    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

                    // Set Reflection Probes to OFF
                    renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

                    // Set Receive Shadows to OFF
                    renderer.receiveShadows = false;

                    // Optionally, enable the renderer if needed
                    renderer.enabled = true;

                    // Remove the original meshcollider if any.
                    MeshCollider meshCollider = agentPlayerTeleportAnchor.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                        Destroy(meshCollider);

                    // assign a new collider to the anchor bcz the mesh has changed.
                    BoxCollider boxCollider = agentPlayerTeleportAnchor.AddComponent<BoxCollider>();
                    Vector3 t = boxCollider.size;
                    boxCollider.size = new Vector3(t.x, 0.01f, t.z);

                    //remove the anchor (will be replaced by a TeleportationArea)
                    TeleportationAnchor anchor = agentPlayerTeleportAnchor.GetComponent<TeleportationAnchor>();
                    if (anchor != null)
                        Destroy(anchor);
                    
                    TeleportationArea area = agentPlayerTeleportAnchor.AddComponent<TeleportationArea>();
                    area.colliders[0] = boxCollider;
                    if (area.colliders.Count > 1)
                        area.colliders.Remove(area.colliders[1]);
                    area.matchOrientation = MatchOrientation.None;
                    area.teleporting.AddListener(OnTeleportingMatchOrientation);

                    /*//this component rotates the player when teleports.
                    CustomTeleportOrientation cstTeleport = agentPlayerTeleportAnchor.AddComponent<CustomTeleportOrientation>();
                    cstTeleport.player = PlayerControllerGameObject.transform;
                    cstTeleport.area = area;
                    */
                }
                else
                {
                    if (numberMesh == null)
                        ConfigManager.WriteConsoleError($"[CabinetsController] initializeCabinets Agent Player Position mesh not found: {meshPath}");
                    if (playerNumberMaterial == null)
                        ConfigManager.WriteConsoleError("[CabinetsController] initializeCabinets Material not found: Cabinets/AgentPlayerPositionsNumbers/NumberMeshes/M_Playernumber");
                }
            }
            else
            {
                ConfigManager.WriteConsoleError("[CabinetsController] initializeCabinets MeshRenderer or MeshFilter component missing on Agent Player Teleport Anchor.");
            }

            idx++;  // Ensure idx is being incremented elsewhere in your code if this block is within a loop
        }
    }

    void OnTeleportingMatchOrientation(TeleportingEventArgs args)
    {
        // Calculate the inverted forward direction
        Quaternion targetRotation = args.teleportRequest.destinationRotation;
        Quaternion invertedForwardRotation = Quaternion.Euler(0f, 180f, 0f);
        Quaternion finalRotation = targetRotation * invertedForwardRotation;

        // Apply the new rotation to the player
        PlayerControllerGameObject.transform.rotation = finalRotation;
    }

    public CabinetController GetCabinetControllerByPosition(int position)
    {
        CabinetControllerInformation cabinet = CabinetsCtrlInfo.FirstOrDefault(c => c.Position == position);
        return cabinet?.CabinetController;
    }

    public CabinetReplace GetCabinetReplaceByPosition(int position)
    {
        CabinetControllerInformation cabinet = CabinetsCtrlInfo.FirstOrDefault(c => c.Position == position);
        return cabinet?.CabinetReplace;
    }

    public int Count()
    {
        //only active gameobjects:
        return CabinetsCtrlInfo.Max(cabinet => cabinet.Position) + 1;
    }

    public CabinetControllerInformation GetCabinetControllerInformationByPosition(int position)
    {
        return CabinetsCtrlInfo.FirstOrDefault(c => c.Position == position);
    }
    public GameObject GetCabinetChildByPosition(int position)
    {
        CabinetControllerInformation cabinet = CabinetsCtrlInfo.FirstOrDefault(c => c.Position == position);
        return cabinet?.Cabinet();
    }

    //returns the game (CabinetPosition) assigned to the position
    public CabinetPosition GetCabinetByPosition(int position)
    {
        CabinetControllerInformation cabinet = CabinetsCtrlInfo.FirstOrDefault(c => c.Position == position);
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
        if (cabsPos.Count() < CabinetsCtrlInfo.Count())
        {
            ConfigManager.WriteConsole($"[CabinetsController.load] {Room} there are {CabinetsCtrlInfo.Count() - cabsPos.Count()} pending assignments");

            List<CabinetControllerInformation> remainingOutOfOrderCabs = CabinetsCtrlInfo.Where(cab =>
                        string.IsNullOrEmpty(cab.CabinetController.game.CabinetDBName)).ToList();
            List<string> unnasignedCabNames = gameRegistry.GetUnassignedCabinets().
                                                           OrderBy(x => UnityEngine.Random.value).ToList();
            List<string> occupiedSpaces = GetOccupiedSpaces(unnasignedCabNames);
            ConfigManager.WriteConsole($"[CabinetsController.load] {Room}"
                                        + $" unnasigned cabinets count: {unnasignedCabNames.Count}");
            foreach (CabinetControllerInformation cabCtrl in remainingOutOfOrderCabs)
            {
                int bestFitIndex = cabCtrl.CabinetController.Space.BestFit(occupiedSpaces);
                if (bestFitIndex != -1)
                {
                    CabinetPosition cabPos = gameRegistry.AssignOrAddCabinet(Room,
                                                                            cabCtrl.Position,
                                                                            unnasignedCabNames[bestFitIndex]);
                    cabCtrl.CabinetController.game = cabPos;
                    ConfigManager.WriteConsole($"[CabinetsController.load] {Room}#{cabCtrl.Position}"
                                                + $" assigned cab: {unnasignedCabNames[bestFitIndex]}"
                                                + $" allowed: {cabCtrl.CabinetController.Space.MaxAllowedSpace} ");

                    occupiedSpaces.RemoveAt(bestFitIndex);
                    unnasignedCabNames.RemoveAt(bestFitIndex);
                }
            }

            if (gameRegistry.NeedsSave())
                gameRegistry.Persist();

            //assign a random to non-assigned.
            //don't persist in gameRegistry.   

            remainingOutOfOrderCabs = CabinetsCtrlInfo.Where(cab =>
                            string.IsNullOrEmpty(cab.CabinetController.game.CabinetDBName)).ToList();
            if (remainingOutOfOrderCabs.Count() > 0)
            {
                ConfigManager.WriteConsole($"[CabinetsController.load] {Room} random {remainingOutOfOrderCabs.Count} pending assignments");

                unnasignedCabNames = gameRegistry.GetRandomizedAllCabinetNames();
                occupiedSpaces = unnasignedCabNames.Select(cabName =>
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
                                                            unnasignedCabNames[bestFitIndex]);
                    cabCtrl.CabinetController.game = cabPos;
                    ConfigManager.WriteConsole($"[CabinetsController.load] {Room}#{cabCtrl.Position} "
                                                + $" randomly assigned cab: {unnasignedCabNames[bestFitIndex]}"
                                                + $" max allowed: {cabCtrl.CabinetController.Space.MaxAllowedSpace} ");

                    occupiedSpaces.RemoveAt(bestFitIndex);
                    unnasignedCabNames.RemoveAt(bestFitIndex);
                }
            }
        }

        ConfigManager.WriteConsole($"[CabinetsController.load] {Room} END loaded cabinets");
        Loaded = true;
    }

    private static List<string> GetOccupiedSpaces(List<string> cabNames)
    {
        List<string> spaces = new List<string>();
        foreach (string cabName in cabNames)
        {
            string cabPath = Path.Combine(ConfigManager.CabinetsDB, cabName);
            try
            {
                CabinetInformation cabInfo = CabinetInformation.fromYaml(cabPath);
                spaces.Add(cabInfo.space);
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"[CabinetController.GetOccupiedSpaces] [{cabName}] invalid cabinet (reading from yaml).", e);
                spaces.Add("1x1x1");

                continue;
            }
        }
        return spaces;
    }

    public bool ReplaceInRoom(int position, string room, string cabinetDBName)
    {
        //replace in the registry
        CabinetPosition toAdd = new();
        toAdd.Room = room;
        toAdd.Position = position;
        toAdd.CabinetDBName = cabinetDBName;

        //get cabinetReplace component first.
        CabinetControllerInformation cabinet = CabinetsCtrlInfo.FirstOrDefault(c => c.Position == position);

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
        if (!cci.IsOutOfOrderActive || cci.IsFaulty)
            return;

        //check conditions to load the cabinet.
        CabinetController cc = cci.CabinetController;

        if (cci.GameObjectReplacement != null)
        {
            if (!cc.ReloadIsAllowed())
                return;

            //replacement exists. Only activate it.
            // don't wait player static.
            cci.ActivateReplacement();
            return;
        }

        if (!cc.LoadIsAllowed())
            return;

        //never loaded. Load from disk/cache
        if (cc.game.CabInfo == null)
        {
            try
            {
                cc.game.CabInfo = CabinetInformation.fromYaml(ConfigManager.CabinetsDB + "/" + cc.game.CabinetDBName);
                if (cc.game.CabInfo == null)
                {
                    cci.SetAsFaultyCabinet(); //prevent to load it next time.
                    ConfigManager.WriteConsoleError($"[CabinetsController.load] loading cabinet from description fails {cc.game}");
                    return;
                }
            }
            catch (Exception e) 
            {
                ConfigManager.WriteConsoleException($"[CabinetsController.load] loading cabinet fails {cc.game}", e);
                cci.SetAsFaultyCabinet(); //prevent to load it next time.
                return;
            }
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
        catch (Exception ex)
        {
            cci.SetAsFaultyCabinet(); //prevent to load it next time.
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

    void checkAndUnloadCabinet(CabinetControllerInformation cabCtrlInfo)
    {
        if (cabCtrlInfo.IsOutOfOrderActive || cabCtrlInfo.IsFaulty)
            return;

        if (!cabCtrlInfo.CabinetReplace.playerIsNotInAnyUnloadPosition())
            return;

        cabCtrlInfo.ActivateOutOfOrder();

        // outOfOrderCabinet.SetActive(true); //reactivate the out of order cabinet before destruction
        // Destroy(gameObject); //destroy me
    }

    IEnumerator run()
    {
        while (true)
        {
            foreach (CabinetControllerInformation cabCtrlInfo in CabinetsCtrlInfo)
            {
                checkAndLoadCabinet(cabCtrlInfo);
                checkAndUnloadCabinet(cabCtrlInfo);
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(TimeToWaitBetweenChecks);
        }
    }
}
