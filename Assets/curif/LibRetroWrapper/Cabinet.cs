/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using static CabinetInformation;
using System.IO;

public class Cabinet
{
    public string Name = "";
    public string FilesPath = "";
    public string ControlScheme = "";
    public GameObject gameObject;

    //Al the parts in the gabinet that must exists, can be others.
    // static List<string> RequiredParts = new List<string>() { "screen-mock-vertical", "screen-mock-horizontal" };
    //Known parts that can exists or not.
    static List<string> NonStandardParts = new List<string>() { "Marquee", "bezel", "coin-slot", "screen", "screen-mock-horizontal", "screen-mock-vertical" };
    //those parts that the user can configure, but is not limited to.
    public static List<string> userStandarConfigurableParts = new List<string>() { "front", "left", "right", "joystick", "joystick-down", "screen-base" };

    public LayerMask layerPart = LayerMask.NameToLayer("LightGunTarget");

    public Dictionary<string, CabinetPart> CabinetPartsControllersByName = new();
    public Dictionary<int, CabinetPart> CabinetPartsControllersByIdx = new();

    // Rotation of the cabinet as a whole (the root gameObject), captured right after placement,
    // used as the origin for CABSETROTATION/CABSETGLOBALROTATION and the baseline for the GET* commands.
    private Quaternion initialLocalRotation;
    private Quaternion initialWorldRotation;


    // public bool IsValid
    // {
    //     get
    //     {
    //         foreach (string key in RequiredParts)
    //             if (!PartsExist(key))
    //                 return false;
    //         return true;
    //     }
    // }

    private void addRigidBody()
    {

        Rigidbody cabRB = gameObject.AddComponent<Rigidbody>(); //this will excecute Start().
        cabRB.freezeRotation = true;
        cabRB.isKinematic = true;
        cabRB.useGravity = false;
        cabRB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ
                            | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;

    }

    public void AddAColliderBlocker(string part, bool debugmode = false)
    {
        GameObject blocker = Parts(part);
        if (blocker == null)
            return;

        Renderer renderer = blocker.GetComponent<Renderer>();
        if (renderer != null)
        {
            // get/create a collider on the blocker. The collider will adjust to the blocker size.
            BoxCollider collider = blocker.GetComponent<BoxCollider>();
            if (collider == null)
                collider = blocker.AddComponent<BoxCollider>();

            collider.includeLayers = LayerMask.GetMask("Player");

            renderer.enabled = false;
            if (debugmode)
                DrawBoxCollider(collider);
        }
    }
    /*
        public BoxCollider addBoxCollider()
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
                boxCollider = gameObject.AddComponent<BoxCollider>();

            Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>();

            if (childRenderers.Length == 0)
            {
                ConfigManager.WriteConsoleWarning($"[addBoxCollider] {gameObject.name} No child renderers found.");
                return boxCollider;
            }

            Bounds bounds = childRenderers[0].bounds;

            for (int i = 1; i < childRenderers.Length; i++)
            {
                bounds.Encapsulate(childRenderers[i].bounds);
            }

            boxCollider.size = bounds.size;
            // Adjust the center and size of the box collider to match the parent game object
            Vector3 centerOffset = bounds.center - gameObject.transform.position;
            Vector3 adjustedCenter = new Vector3(centerOffset.x / gameObject.transform.lossyScale.x,
                                                  centerOffset.y / gameObject.transform.lossyScale.y,
                                                  centerOffset.z / gameObject.transform.lossyScale.z);
            boxCollider.center = adjustedCenter;

            return boxCollider;
        }
    */

    void DrawBounds(Bounds bounds)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.001f; // Set the width of the line
        lineRenderer.endWidth = 0.001f;
        lineRenderer.positionCount = 8;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        Vector3[] corners = new Vector3[8];

        // Get the corners of the bounding box
        corners[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        corners[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
        corners[2] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
        corners[3] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);

        corners[4] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
        corners[5] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        corners[6] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        corners[7] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);

        for (int i = 0; i < corners.Length; i++)
        {
            lineRenderer.SetPosition(i, corners[i]);
        }

        // Connect the corners to form the Bounds lines
        lineRenderer.loop = true;
    }
    void DrawBoxCollider(BoxCollider collider)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 8;
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;

        lineRenderer.startWidth = 0.001f; // Set the width of the line
        lineRenderer.endWidth = 0.001f;

        Vector3 center = collider.bounds.center;
        Vector3 size = collider.size / 2;

        // Get the corners of the BoxCollider
        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(size.x, size.y, size.z);
        corners[1] = center + new Vector3(size.x, size.y, -size.z);
        corners[2] = center + new Vector3(-size.x, size.y, -size.z);
        corners[3] = center + new Vector3(-size.x, size.y, size.z);

        corners[4] = center + new Vector3(size.x, -size.y, size.z);
        corners[5] = center + new Vector3(size.x, -size.y, -size.z);
        corners[6] = center + new Vector3(-size.x, -size.y, -size.z);
        corners[7] = center + new Vector3(-size.x, -size.y, size.z);

        // Draw lines between corners to visualize the BoxCollider
        for (int i = 0; i < corners.Length; i++)
        {
            lineRenderer.SetPosition(i, corners[i]);
        }

        // Connect the corners to form the BoxCollider lines
        lineRenderer.loop = true;
    }
    public BoxCollider addBoxCollider(bool debugmode = false)
    {
        //https://www.reddit.com/r/Unity3D/comments/8qma81/how_to_add_a_collider_at_runtime_to_an_object/
        Quaternion currentRotation = gameObject.transform.rotation;
        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Vector3 currentScale = gameObject.transform.localScale;
        gameObject.transform.localScale = Vector3.one;

        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>();
        if (childRenderers.Length == 0)
        {
            ConfigManager.WriteConsoleWarning($"[addBoxCollider] {gameObject.name} No child renderers found.");
            return boxCollider;
        }

        Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in childRenderers)
        {
            bounds.Encapsulate(renderer.bounds);
            if (debugmode)
                DrawBounds(renderer.bounds);
        }

        // Set the size of the box collider to match the bounds.
        boxCollider.size = bounds.size;

        // Calculate the center offset relative to the transform position.
        Vector3 centerOffset = bounds.center - gameObject.transform.position;
        boxCollider.center = centerOffset;

        gameObject.transform.rotation = currentRotation;
        gameObject.transform.localScale = currentScale;

        if (debugmode)
            DrawBoxCollider(boxCollider);

        return boxCollider;
    }

    /*
    Another approach:
    private void addBoxCollider(GameObject gameObject)
    {
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>();

        if (childRenderers.Length == 0)
        {
            ConfigManager.WriteConsoleWarning($"[addBoxCollider] {gameObject.name} No child renderers found.");
            return;
        }

        // Initialize minimum and maximum corner points in world space
        Vector3 minWorldCorner = childRenderers[0].bounds.min;
        Vector3 maxWorldCorner = childRenderers[0].bounds.max;

        for (int i = 1; i < childRenderers.Length; i++)
        {
            Vector3 rendererMin = childRenderers[i].bounds.min;
            Vector3 rendererMax = childRenderers[i].bounds.max;

            // Update min and max corner points in world space
            minWorldCorner = Vector3.Min(minWorldCorner, rendererMin);
            maxWorldCorner = Vector3.Max(maxWorldCorner, rendererMax);
        }

        // Convert world space corner points to local space
        Vector3 minLocalCorner = gameObject.transform.InverseTransformPoint(minWorldCorner);
        Vector3 maxLocalCorner = gameObject.transform.InverseTransformPoint(maxWorldCorner);

        // Calculate the center and size of the box collider in local space
        Vector3 center = (minLocalCorner + maxLocalCorner) * 0.5f;
        Vector3 size = maxLocalCorner - minLocalCorner;

        // Adjust the center based on the parent's scale
        Vector3 adjustedCenter = new Vector3(center.x / gameObject.transform.localScale.x,
                                              center.y / gameObject.transform.localScale.y,
                                              center.z / gameObject.transform.localScale.z);
        boxCollider.center = adjustedCenter;
        boxCollider.size = size;
    }
    */


    public PutOnFloor toFloor()
    {
        PutOnFloor onFloor = gameObject.GetComponent<PutOnFloor>();
        if (onFloor == null)
            onFloor = gameObject.AddComponent<PutOnFloor>();
        return onFloor;
    }

    public CabinetPart RegisterChild(GameObject child)
    {
        CabinetPart cp = child.GetComponent<CabinetPart>();
        if (cp == null)
            cp = child.AddComponent<CabinetPart>();
        CabinetPartsControllersByName[child.name] = cp;
        CabinetPartsControllersByIdx[child.transform.GetSiblingIndex()] = cp;
        return cp;
    }
    public void RegisterChildren(GameObject cabinetRootGameObject)
    {
        // Get all first-level child objects
        foreach (Transform child in cabinetRootGameObject.transform)
        {
            RegisterChild(child.gameObject);
        }
    }
    public Cabinet(string name, string path, string controlScheme, Vector3 position, Quaternion rotation, Transform parent,
                        GameObject go = null, string model = "galaga")
    {
        Name = name;
        FilesPath = path;
        ControlScheme = controlScheme;
        if (go == null)
        {
            // Assets/Resources/Cabinets/PreFab/xxx.prefab
            go = Resources.Load<GameObject>($"Cabinets/PreFab/{model}");
            if (go == null)
                throw new System.Exception($"Cabinet tyle {Name} not found or doesn't load");
        }

        //https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
        gameObject = GameObject.Instantiate<GameObject>(go, position, rotation, parent);
        gameObject.name = name;
        initialLocalRotation = gameObject.transform.localRotation;
        initialWorldRotation = gameObject.transform.rotation;
        RegisterChildren(gameObject);

        CabinetPart bezel = GetPartControllerOrNull("bezel");
        if (bezel != null)
        {
            switch (bezel.SubType)
            {
                case "simple":
                    bezel.SetMaterial(CabinetMaterials.FrontGlassCutOut);
                    break;
                case "dirty glass":
                    bezel.SetMaterial(CabinetMaterials.FrontGlassDirty);
                    break;
                default:
                    bezel.SetMaterial(CabinetMaterials.FrontGlassWithBezel);
                    break;
            }
        }

        //cached gameObjects could be inactive.
        gameObject.SetActive(true);
    }

    public GameObject this[string part]
    {
        get
        {
            return Parts(part);
        }
    }

    public CabinetPart GetPartController(string partName)
    {
        if (CabinetPartsControllersByName.ContainsKey(partName))
            return CabinetPartsControllersByName[partName];

        throw new Exception($"Unknown cabinet part: {partName}");
    }

    public CabinetPart GetPartController(int idx)
    {
        if (idx >= 0 && idx < CabinetPartsControllersByIdx.Count)
            return CabinetPartsControllersByIdx[idx];

        throw new Exception($"Unknown cabinet part: #{idx}");
    }

    public CabinetPart GetPartControllerOrNull(string partName)
    {
        if (CabinetPartsControllersByName.ContainsKey(partName))
            return CabinetPartsControllersByName[partName];

        return null;
    }

    public CabinetPart GetPartControllerOrNull(int idx)
    {
        if (idx >= 0 && idx < CabinetPartsControllersByIdx.Count)
            return CabinetPartsControllersByIdx[idx];

        return null;
    }

    private static Quaternion EulerAngleByAxis(string axis, float angle)
    {
        if (angle < -360 || angle > 360)
            throw new Exception($"Rotation: {axis} value must be between -360 and 360 degrees. value {angle}");

        switch (axis)
        {
            case "X":
                return Quaternion.Euler(angle, 0, 0);
            case "Y":
                return Quaternion.Euler(0, angle, 0);
            case "Z":
                return Quaternion.Euler(0, 0, angle);
            default:
                throw new Exception($"Rotation: axis should be X, Y, or Z: {axis}");
        }
    }

    public Cabinet RotateLocalEulerAngleByAxisFromOrigin(string axis, float angle)
    {
        gameObject.transform.localRotation = initialLocalRotation * EulerAngleByAxis(axis, angle);
        return this;
    }

    public Cabinet RotateLocalEulerAngleByAxis(string axis, float angle)
    {
        gameObject.transform.localRotation *= EulerAngleByAxis(axis, angle);
        return this;
    }

    public float GetLocalRotationByAxis(string axis)
    {
        Quaternion deltaRotation = gameObject.transform.localRotation * Quaternion.Inverse(initialLocalRotation);
        Vector3 deltaEuler = deltaRotation.eulerAngles;

        switch (axis)
        {
            case "X":
                return deltaEuler.x;
            case "Y":
                return deltaEuler.y;
            case "Z":
                return deltaEuler.z;
            default:
                throw new Exception($"Rotation: axis should be X, Y, or Z: {axis}");
        }
    }

    public Cabinet RotateWorldEulerAngleByAxis(string axis, float angle)
    {
        gameObject.transform.rotation = initialWorldRotation * EulerAngleByAxis(axis, angle);
        return this;
    }

    public float GetWorldRotationByAxis(string axis)
    {
        Quaternion deltaRotation = gameObject.transform.rotation * Quaternion.Inverse(initialWorldRotation);
        Vector3 deltaEuler = deltaRotation.eulerAngles;

        switch (axis)
        {
            case "X":
                return deltaEuler.x;
            case "Y":
                return deltaEuler.y;
            case "Z":
                return deltaEuler.z;
            default:
                throw new Exception($"Rotation: axis should be X, Y, or Z: {axis}");
        }
    }

    public GameObject PartsOrNull(string partName)
    {
        Transform childTransform = gameObject.transform.Find(partName);
        return childTransform?.gameObject;
    }
    public GameObject PartsOrNull(int partNum)
    {
        Transform childTransform = gameObject.transform.GetChild(partNum);
        return childTransform?.gameObject;
    }

    public GameObject Parts(string partName)
    {
        GameObject go = PartsOrNull(partName);
        if (go == null)
            throw new Exception($"Unknown cabinet part: {partName}");
        return go;
    }
    public GameObject Parts(int partNum)
    {
        GameObject go = PartsOrNull(partNum);
        if (go == null)
            throw new Exception($"Unknown cabinet part: #{partNum}");
        return go;
    }

    public Transform PartsTransform(string partName)
    {
        Transform childTransform = gameObject.transform.Find(partName);
        if (childTransform == null)
            throw new CabinetPartException("", partName, $"Unknown cabinet part: {partName}");
        return childTransform;
    }
    public Transform PartsTransform(int partNum)
    {
        Transform childTransform = gameObject.transform.GetChild(partNum);
        if (childTransform == null)
            throw new Exception($"Unknown cabinet part: #{partNum}");
        return childTransform;
    }
    public string PartsName(int partNum)
    {
        Transform t = PartsTransform(partNum);
        return t.name;
    }
    public int PartsPosition(string partName)
    {
        Transform t = PartsTransform(partName);
        return t.GetSiblingIndex();
    }
    public bool PartsExist(string part)
    {
        return gameObject.transform.Find(part) != null;
    }
    public bool PartsExist(int partNum)
    {
        return partNum >= 0 && gameObject.transform.childCount < partNum + 1;
    }
    public int PartsCount()
    {
        return gameObject.transform.childCount;
    }


    public static void Scale(GameObject go, float percentage, float xratio, float yratio, float zratio)
    {
        float scale = percentage / 100f;
        Vector3 localScale = go.transform.localScale;
        go.transform.localScale = new Vector3(localScale.x * xratio * scale,
                                              localScale.y * yratio * scale,
                                              localScale.z * zratio * scale);
    }

    public static void Scale(GameObject go, float percentage)
    {
        Scale(go, percentage, 1, 1, 1);
    }



    //set the same material to all components.
    public Cabinet SetMaterial(Material mat)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!NonStandardParts.Contains(renderer.gameObject.name))
            {
                renderer.material = mat;
            }
        }
        return this;
    }

    //set the same material to all unknown components.
    public Cabinet SetMaterialToUnknownComponents(Material mat, CabinetInformation cbInfo)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!NonStandardParts.Contains(renderer.gameObject.name) &&  /* && renderer.sharedMaterial == null &&*/
                !cbInfo.PartExists(renderer.gameObject.name))
                //renderer.materials = new Material[] { mat }; //can't do it, some cabinet aren't working. 
                renderer.material = mat; //can't fix it, some cabinet aren't working. 
        }
        return this;
    }

    //set the same color to all unknown components.
    public Cabinet SetVertexColorToUnknownComponents(Color32 color, CabinetInformation cbInfo)
    {

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!NonStandardParts.Contains(renderer.gameObject.name) && !cbInfo.PartExists(renderer.gameObject.name)
                && (renderer.sharedMaterial == null || renderer.sharedMaterial.name == "Default-Material"))
            {
                renderer.material = CabinetMaterials.VertexColor;
                //renderer.materials = new Material[] { mat }; //can't do it, some cabinet aren't working.
                CabinetPart.PaintVertexColorMaterialToGameobject(color, renderer.gameObject);
            }
        }
        return this;
    }

    //set a part as lightgun target.
    public Cabinet SetLightGunTarget(string partName, LightGunInformation linfo)
    {
        return SetLightGunTarget(PartsPosition(partName), linfo);
    }
    public Cabinet SetLightGunTarget(int partNum, LightGunInformation linfo)
    {
        GameObject part = Parts(partNum);
        LightGunTarget l = part.GetComponent<LightGunTarget>();
        if (l != null)
            return this;

        part.layer = layerPart;

        //needs a Mesh collider
        MeshCollider collider = part.GetComponent<MeshCollider>();
        if (collider == null)
            part.AddComponent<MeshCollider>();

        return this;
    }

    //add the Touch component and the gameobject to collide
    public Cabinet SetAudio(string partName, string cabPathBase, CabinetAudioPart audioInfo)
    {
        SetAudio(PartsPosition(partName), cabPathBase, audioInfo);
        return this;
    }

    public Cabinet SetAudio(int partNum, string cabPathBase, CabinetInformation.CabinetAudioPart audioInfo)
    {
        GameObject part = Parts(partNum);
        CabinetPartAudioController.GetOrAdd(part, cabPathBase, audioInfo);
        return this;
    }

    public Cabinet SetPhysics(string partName, Physical physicalInfo)
    {
        SetPhysics(PartsPosition(partName), physicalInfo);
        return this;
    }
    public Cabinet SetPhysics(int partNum, Physical physicalInfo)
    {
        if (physicalInfo == null)
            return this;

        GameObject part = Parts(partNum);
        InteractablePart.Factory(part, physicalInfo);
        /* artist should mark all parts involved as physicals
        if (physicalInfo.receiveImpacts != null)
        {

            foreach (string name in physicalInfo.receiveImpacts.parts)
            {
                GameObject collider = Parts(name);
                collider.
                if (collider != null)
                {
                    InteractablePart.GetOrAdd(collider);
                }
            }
        }
        */
        return this;
    }

    public Cabinet PhyActivate()
    {
        foreach (Transform t in gameObject.transform)
        {
            InteractablePart ip = t.GetComponent<InteractablePart>();
            if (ip != null)
                ip.Activate();
        }
        return this;
    }
    public Cabinet PhyDeactivate()
    {
        foreach (Transform t in gameObject.transform)
        {
            InteractablePart ip = t.GetComponent<InteractablePart>();
            if (ip != null)
                ip.Deactivate();
        }
        return this;
    }

    public Cabinet PhyActivateGravity(string partName)
    {
        return PhyActivateGravity(PartsPosition(partName));

    }
    public Cabinet PhyActivateGravity(int partNum)
    {
        GameObject part = Parts(partNum);
        InteractablePart gd = part.GetComponent<InteractablePart>();
        if (gd != null)
        {
            gd.ActivateGravity();
        }
        return this;
    }
    public Cabinet PhyDeactivateGravity(string partName)
    {
        return PhyDeactivateGravity(PartsPosition(partName));

    }
    public Cabinet PhyDeactivateGravity(int partNum)
    {
        GameObject part = Parts(partNum);
        InteractablePart gd = part.GetComponent<InteractablePart>();
        if (gd != null)
        {
            gd.DeactivateGravity();
        }
        return this;
    }


    public bool IsLightGunTarget(string partName)
    {
        return IsLightGunTarget(PartsPosition(partName));
    }
    public bool IsLightGunTarget(int partNum)
    {
        return Parts(partNum).layer == layerPart;
    }
    public static string CRTName(string cabinetName, string gameFile)
    {
        if (gameFile == null)
            gameFile = "no_file_game";
        return "screen-" + cabinetName + "-" + gameFile.Replace("/", "-").Replace(".", "-");
    }

    //used instead of addCRT for cabinets without a CRT
    public Cabinet addController(string pathBase,
                                     ControlMapConfiguration cabinetControlMap = null,
                                     LightGunInformation lightGunInformation = null,
                                     CabinetAGEBasicInformation agebasic = null,
                                     BackgroundSoundController backgroundSoundController = null
                                     )
    {
        GameObject newCRT = CRTsFactory.Instantiate("no-crt", Vector3.zero, Quaternion.identity, gameObject.transform);
        if (newCRT == null)
            throw new System.Exception($"Cabinet {Name} problem: can't create a no-crt controller");

        //LibretroScreenController will find the object using this name:
        newCRT.name = CRTName(Name, "no-crt");
        AGEBasicCabinetController agec = newCRT.GetComponent<AGEBasicCabinetController>();
        agec.PathBase = pathBase;
        agec.cabinet = this;
        //control mapping
        agec.CabinetControlMapConfig = cabinetControlMap;
        agec.lightGunInformation = lightGunInformation;

        //age basic
        if (agebasic != null)
            agec.ageBasicInformation = agebasic;

        //sound
        agec.backgroundSoundController = backgroundSoundController;
        return this;
    }

    public Cabinet addCRT(CabinetInformation cbinfo, List<AgentScenePosition> _agentPlayerPositions, BackgroundSoundController _backgroundSoundController, Vector3 _rotation)
    {
        string type = cbinfo.crt.type;
        string orientation = cbinfo.crt.orientation;
        string gameFile = cbinfo.rom;

        List<string> playList = cbinfo.roms;
        if (playList == null)
            playList = new List<string>() { gameFile };

        int timeToLoad = cbinfo.timetoload;
        string pathBase = cbinfo.pathBase;
        bool invertX = cbinfo.crt.screen.invertx;
        bool invertY = cbinfo.crt.screen.inverty;
        bool EnableSaveState = cbinfo.enablesavestate;
        string StateFile = cbinfo.statefile ?? "state.nv";
        Vector3? rotation = _rotation;
        float crtScalePercentage = cbinfo.crt.geometry?.scalepercentage ?? 100f;
        float crtXratio = cbinfo.crt.geometry?.ratio?.x ?? 1f;
        float crtYratio = cbinfo.crt.geometry?.ratio?.y ?? 1f;
        float crtZratio = cbinfo.crt.geometry?.ratio?.z ?? 1f;
        string gamma = cbinfo.crt.screen.gamma ?? "1.0";
        string brightness = cbinfo.crt.screen.brightness ?? "1.0";
        List<AgentScenePosition> agentPlayerPositions = _agentPlayerPositions;
        ControlMapConfiguration cabinetControlMap = cbinfo.ControlMap;
        LightGunInformation lightGunInformation = cbinfo.lightGunInformation;
        CabinetAGEBasicInformation agebasic = cbinfo.agebasic;
        BackgroundSoundController backgroundSoundController = _backgroundSoundController;
        string core = cbinfo.core ?? "mame2003+";
        CoreEnvironment coreEnvironment = cbinfo.environment;
        bool? insertCoinOnStartup = cbinfo.insertCoinOnStartup;
        bool? persistent = cbinfo.persistent;
        Dictionary<uint, LibretroInputDevice> libretroInputDevices = cbinfo.GetLibretroInputDevices();

        string GameVideoFile = null;
        string GameAudioFile = null;
        bool GameVideoFileInvertX = false;
        bool GameVideoFileInvertY = false;
        float AudioMaxPlayerDistance = float.MaxValue;
        float VideoMaxPlayerDistance = float.MaxValue;

        if (cbinfo.video != null)
        {
            GameVideoFile = cbinfo.getPath(cbinfo.video.file);
            GameVideoFileInvertX = cbinfo.video.invertx;
            GameVideoFileInvertY = cbinfo.video.inverty;
            VideoMaxPlayerDistance = cbinfo.video.MaxPlayerDistance;
        }

        if (cbinfo.audio != null)
        {
            GameAudioFile = cbinfo.getPath(cbinfo.audio.file);
            AudioMaxPlayerDistance = cbinfo.audio.MaxPlayerDistance;
        }

        GameObject newCRT;
        if (type.ToLower() == "custom")
        {
            newCRT = buildCustomCRT(cbinfo, type);
        }
        else
        {
            string CRTType = $"screen-mock-{orientation}";
            GameObject CRT = GetPartController(CRTType).GameObject;

            newCRT = CRTsFactory.Instantiate(type.ToLower(), CRT.transform.position, CRT.transform.rotation, CRT.transform.parent);
            if (newCRT == null)
                throw new System.Exception($"Cabinet {Name} problem: can't create a CRT. Type: {type}");
        }
        newCRT.AddComponent<CabinetPart>();
        CabinetPart cp = RegisterChild(newCRT);

        //LibretroScreenController will find the object using this name:
        if (string.IsNullOrEmpty(cbinfo.crt.name))
            newCRT.name = CRTName(Name, gameFile);
        else
            newCRT.name = cbinfo.crt.name;

        cp.Scale(crtScalePercentage, crtXratio, crtYratio, crtZratio);
        if (rotation != null)
            cp.Rotate((Vector3)rotation);

        RegisterChild(newCRT);

        GetPartControllerOrNull("screen-mock-vertical")?.GameObject.SetActive(false);
        GetPartControllerOrNull("screen-mock-horizontal")?.GameObject.SetActive(false);

        if (type.ToLower() == "19i-agebasic")
        {
            AGEBasicScreenController ageb = newCRT.GetComponent<AGEBasicScreenController>();
            ageb.PathBase = pathBase;
            ageb.cabinet = this;

            ageb.AgentPlayerPositions = agentPlayerPositions;

            ageb.InvertX = invertX;
            ageb.InvertY = invertY;
            //video
            ageb.VideoFile = GameVideoFile;
            ageb.VideoInvertX = GameVideoFileInvertX;
            ageb.VideoInvertY = GameVideoFileInvertY;

            //control mapping
            ageb.CabinetControlMapConfig = cabinetControlMap;
            ageb.lightGunInformation = lightGunInformation;

            //age basic
            if (agebasic != null)
                ageb.ageBasicInformation = agebasic;

            //sound
            ageb.backgroundSoundController = backgroundSoundController;
        }
        else
        {
            //adds a GameVideoPlayer, BoxCollider and a AudioSource to the screen
            LibretroScreenController libretroScreenController = newCRT.GetComponent<LibretroScreenController>();

            libretroScreenController.GameFile = gameFile;
            libretroScreenController.GameVolumeDecibels = cbinfo.gameVolumeDecibels;
            libretroScreenController.PlayList = playList;
            libretroScreenController.CabEnvironment = coreEnvironment;
            libretroScreenController.SecondsToWaitToFinishLoad = timeToLoad;
            libretroScreenController.EnableSaveState = EnableSaveState;
            libretroScreenController.StateFile = StateFile;
            libretroScreenController.PathBase = pathBase;
            libretroScreenController.InsertCoinOnStartup = insertCoinOnStartup;

            //needs refactor
            libretroScreenController.GameInvertX = invertX;
            libretroScreenController.GameInvertY = invertY;
            libretroScreenController.Gamma = gamma;
            libretroScreenController.Brightness = brightness;
            libretroScreenController.screen = cbinfo.crt.screen;

            libretroScreenController.cabinet = this;
            libretroScreenController.Core = core;
            libretroScreenController.AnalogStick = cbinfo.input?.analogStick ?? false;
            libretroScreenController.TwinStick = cbinfo.input?.twinStick ?? false;
            libretroScreenController.Persistent = persistent;
            libretroScreenController.LibretroInputDevices = libretroInputDevices;

            libretroScreenController.AgentPlayerPositions = agentPlayerPositions;

            //video & audio
            libretroScreenController.GameVideoFile = GameVideoFile;
            libretroScreenController.GameAudioFile = GameAudioFile;
            libretroScreenController.GameVideoInvertX = GameVideoFileInvertX;
            libretroScreenController.GameVideoInvertY = GameVideoFileInvertY;
            libretroScreenController.GameVideoConfig = cbinfo.video;
            libretroScreenController.DistanceMaxToPlayerToActivateAudio = AudioMaxPlayerDistance;
            libretroScreenController.DistanceMaxToPlayerToActivateVideo = VideoMaxPlayerDistance;

            //control mapping
            libretroScreenController.CabinetControlMapConfig = cabinetControlMap;
            libretroScreenController.lightGunInformation = lightGunInformation;

            //age basic
            if (agebasic != null)
                libretroScreenController.ageBasicInformation = agebasic;

            //sound
            libretroScreenController.backgroundSoundController = backgroundSoundController;
        }
        return this;
    }

    // Builds the CRT for crt.type: custom. Instead of a screen-mock placeholder, the author
    // supplies their own screen mesh as a named part in the cabinet .glb (crt.mesh). We reuse
    // the screen19i prefab as the component template, drop the author's mesh + collider into it,
    // collapse the template's two-slot material array (0 = bezel, 1 = screen) down to just the
    // screen surface, and mark that surface as slot 0 / submesh 0 via ScreenSurfaceInfo so the
    // shader and light-gun code target it. The donor part is consumed (deactivated) — it was
    // only a mesh source. The shared naming/scale/controller wiring in addCRT runs afterwards.
    private GameObject buildCustomCRT(CabinetInformation cbinfo, string type)
    {
        string meshPartName = cbinfo.crt.mesh;
        ConfigManager.WriteConsole($"[buildCustomCRT] {Name}: begin, crt.mesh='{meshPartName}'");
        try
        {
            if (string.IsNullOrEmpty(meshPartName))
                throw new System.Exception($"crt.type custom requires 'mesh: <part-name>'");

            CabinetPart donor = GetPartControllerOrNull(meshPartName);
            if (donor == null)
                throw new System.Exception($"mesh part '{meshPartName}' not found — must be a top-level named part in the .glb. Known parts: {string.Join(", ", CabinetPartsControllersByName.Keys)}");

            GameObject donorGO = donor.GameObject;

            // The mesh may sit directly on the named part or on a child (common with GLB import),
            // so fall back to a child search.
            MeshFilter donorMF = donorGO.GetComponent<MeshFilter>();
            if (donorMF == null || donorMF.sharedMesh == null)
                donorMF = donorGO.GetComponentInChildren<MeshFilter>();
            if (donorMF == null || donorMF.sharedMesh == null)
                throw new System.Exception($"mesh part '{meshPartName}' has no MeshFilter/mesh (searched the part and its children)");

            Mesh screenMesh = donorMF.sharedMesh;
            Transform meshTransform = donorMF.transform;   // where the mesh actually lives in space
            bool readable = screenMesh.isReadable;
            ConfigManager.WriteConsole($"[buildCustomCRT] {Name}: donor='{donorGO.name}' meshOwner='{meshTransform.name}' submeshes={screenMesh.subMeshCount} verts={screenMesh.vertexCount} readable={readable} uvs={(readable ? screenMesh.uv.Length : -1)} worldPos={meshTransform.position} lossyScale={meshTransform.lossyScale}");

            if (screenMesh.subMeshCount != 1)
                throw new System.Exception($"mesh part '{meshPartName}' must have exactly one material slot / submesh (has {screenMesh.subMeshCount})");

            GameObject newCRT = CRTsFactory.Instantiate(type.ToLower(), meshTransform.position, meshTransform.rotation, meshTransform.parent);
            if (newCRT == null)
                throw new System.Exception($"can't instantiate CRT template. Type: {type}");

            // occupy exactly the space the author modeled for the mesh (parent shared, so copy local TRS)
            newCRT.transform.localPosition = meshTransform.localPosition;
            newCRT.transform.localRotation = meshTransform.localRotation;
            newCRT.transform.localScale = meshTransform.localScale;

            // swap geometry: both the render mesh and the light-gun ray collider use the author's mesh
            MeshFilter mf = newCRT.GetComponent<MeshFilter>();
            if (mf != null)
                mf.sharedMesh = screenMesh;
            MeshCollider mc = newCRT.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.convex = false;              // hit.triangleIndex is only valid on a non-convex MeshCollider
                mc.sharedMesh = screenMesh;
            }

            // collapse the template's 2-slot material array to the screen surface only. A one-submesh
            // mesh would otherwise double-render under the leftover bezel material (Unity renders extra
            // materials against the last submesh).
            Renderer rend = newCRT.GetComponent<Renderer>();
            if (rend != null)
            {
                Material[] mats = rend.sharedMaterials;
                ConfigManager.WriteConsole($"[buildCustomCRT] {Name}: template materials={mats.Length} [{string.Join(", ", System.Array.ConvertAll(mats, m => m == null ? "null" : m.name))}]");
                if (mats.Length > 1)
                    rend.sharedMaterials = new Material[] { mats[1] };
            }

            // tell the shader + light-gun consumers the screen surface is at slot 0 / submesh 0
            ScreenSurfaceInfo ssi = newCRT.GetComponent<ScreenSurfaceInfo>();
            if (ssi == null)
                ssi = newCRT.AddComponent<ScreenSurfaceInfo>();
            ssi.materialSlot = 0;
            ssi.submeshIndex = 0;

            // the donor part was only a mesh source
            donorGO.SetActive(false);

            ConfigManager.WriteConsole($"[buildCustomCRT] {Name}: DONE — screen clone at world {newCRT.transform.position}, lossyScale {newCRT.transform.lossyScale}; donor '{donorGO.name}' deactivated.");
            return newCRT;
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleError($"[buildCustomCRT] {Name}: FAILED — {e.Message}");
            throw;
        }
    }

    public Cabinet AddCoinSlot(string type, float rotationAngleX, float rotationAngleY, float rotationAngleZ, float scalePercentage, bool soundEnabled = true)
    {
        GameObject coinSlotMock = Parts("coin-slot");
        if (coinSlotMock == null)
        {
            ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] ERROR coin-slot part not present in cabinet {Name}");
            return this;
        }

        GameObject newCoinSlot = CoinSlotsFactory.Instantiate(type, coinSlotMock.transform.position, coinSlotMock.transform.rotation, coinSlotMock.transform.parent);
        if (newCoinSlot != null)
        {
            //LibretroScreenController will find the coinslot using this name:
            newCoinSlot.name = "coin-slot-added";
            ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] {Name} added part coin-slot-added");
            newCoinSlot.GetComponent<CoinSlotController>().SoundEnabled = soundEnabled;
        }
        else
            ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] ERROR {Name}: can't create the new coin-slot type: {type}");

        // rotate and scale
        float scale = scalePercentage / 100f;
        newCoinSlot.transform.localScale *= scale;
        newCoinSlot.transform.Rotate(rotationAngleX, rotationAngleY, rotationAngleZ);

        //never destroy a component.
        //UnityEngine.Object.Destroy(coinSlotMock);
        coinSlotMock.SetActive(false);
        RegisterChild(newCoinSlot);

        return this;
    }
}

