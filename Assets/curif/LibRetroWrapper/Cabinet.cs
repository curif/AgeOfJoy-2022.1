/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Cabinet
{
    public string Name = "";
    public GameObject gameObject;

    //Al the parts in the gabinet that must exists, can be others.
    static List<string> RequiredParts = new List<string>() { "front", "left", "right", "joystick", "screen-base", "screen-mock-vertical", "screen-mock-horizontal" };
    //Known parts that can exists or not.
    static List<string> NonStandardParts = new List<string>() { "Marquee", "bezel", "coin-slot", "screen" };
    //those parts that the user can configure, but is not limited to.
    public static List<string> userStandarConfigurableParts = new List<string>() { "front", "left", "right", "joystick", "joystick-down", "screen-base" };

    // load a texture from disk.
    private static Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    public bool IsValid
    {
        get
        {
            foreach (string key in RequiredParts)
                if (!PartsExist(key))
                    return false;
            return true;
        }
    }

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

        Bounds bounds = childRenderers[0].bounds;

        for (int i = 1; i < childRenderers.Length; i++)
        {
            bounds.Encapsulate(childRenderers[i].bounds);
        }

        boxCollider.size = bounds.size;
        //boxCollider.center = bounds.center - gameObject.transform.position;
        // Adjust the center and size of the box collider to match the parent game object
        Vector3 centerOffset = bounds.center - gameObject.transform.position;
        Vector3 adjustedCenter = new Vector3(centerOffset.x / gameObject.transform.lossyScale.x, 
                                              centerOffset.y / gameObject.transform.lossyScale.y, 
                                              centerOffset.z / gameObject.transform.lossyScale.z);
        boxCollider.center = adjustedCenter;
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


    private void toFloor()
    {
        gameObject.AddComponent<PutOnFloor>();
    }

    public Cabinet(string name, Vector3 position, Quaternion rotation, Transform parent, GameObject go = null, string model = "galaga")
    {
        Name = name;
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

        //add neccesary components
        //addRigidBody();
        addBoxCollider(gameObject);
        toFloor();

        if (!IsValid)
            throw new System.Exception($"[Cabinet] Malformed Cabinet {Name} , some parts are missing. List of expected parts: {string.Join(",", RequiredParts)}");

        SetMaterial(CabinetMaterials.Black);
        if (PartsExist("bezel"))
            SetMaterial("bezel", CabinetMaterials.FrontGlassWithBezel);

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
    public GameObject Parts(string part)
    {
        Transform childTransform = gameObject.transform.Find(part);
        //ConfigManager.WriteConsole($"Part: {childTransform}");
        if (childTransform == null)
            return null;
        return childTransform.gameObject;
    }
    public bool PartsExist(string part)
    {
        return gameObject.transform.Find(part) != null;
    }

    public Cabinet ScalePart(string cabinetPart, float percentage)
    {
        if (!PartsExist(cabinetPart))
            return this;

        float scale = percentage / 100f;
        Parts(cabinetPart).transform.localScale *= scale;
        return this;
    }
    public Cabinet RotatePart(string cabinetPart, float angleX, float angleY, float angleZ)
    {
        if (!PartsExist(cabinetPart))
            return this;

        Parts(cabinetPart).transform.Rotate(angleX, angleY, angleZ);
        return this;
    }

    //change a material, and
    public Cabinet SetTextureTo(string cabinetPart, string textureFile, Material mat, bool invertX = false, bool invertY = false)
    {

        if (!PartsExist(cabinetPart))
            return this;
        if (mat == null && string.IsNullOrEmpty(textureFile))
            return this;

        Renderer r = Parts(cabinetPart).GetComponent<Renderer>();
        if (r == null)
            return this;

        if (mat == null)
            mat = CabinetMaterials.Base;

        Material m = new Material(mat);
        //tiling
        Vector2 mainTextureScale = new Vector2(1, 1);
        if (invertX)
            mainTextureScale.x = -1;
        if (invertY)
            mainTextureScale.y = -1;
        m.mainTextureScale = mainTextureScale;

        if (!string.IsNullOrEmpty(textureFile))
        {
            //main texture
            Texture2D t = LoadTexture(textureFile);
            if (t == null)
            {
                ConfigManager.WriteConsole($"ERROR Cabinet {Name} texture error {textureFile}");
                return this;
            }
            else
                m.SetTexture("_MainTex", t);
        }

        r.material = m;

        return this;
    }

    public Cabinet SetMarquee(string part, string texturePath, Material marqueeMaterial, bool invertX = false, bool invertY = false)
    {
        return SetTextureTo(part, texturePath, marqueeMaterial, invertX: invertX, invertY: invertY);
    }
    public Cabinet SetMarqueeEmissionColor(string part, Color color, float intensity)
    {
        if (!PartsExist(part))
            return this;

        Renderer r = Parts(part).GetComponent<Renderer>();
        if (r != null)
        {
            r.material.SetColor("_EmissionColor", color);
            float oldToNewIntensity = (intensity + 5) / 10;
            r.material.SetFloat("_EmissionIntensity", oldToNewIntensity);
        }

        return this;
    }

    public Cabinet SetBezel(string part, string texturePath, bool invertX = false, bool invertY = false)
    {
        return SetTextureTo(part, texturePath, CabinetMaterials.FrontGlassWithBezel, invertX: invertX, invertY: invertY);
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

    //set the material to a component. Don't create new.
    public Cabinet SetMaterial(string part, Material mat)
    {
        if (!PartsExist(part))
            throw new System.Exception($"Unknown part {part} to set material in cabinet {Name}");

        Renderer r = Parts(part).GetComponent<Renderer>();
        if (r != null)
            r.material = mat;

        return this;
    }

    public static string CRTName(string cabinetName, string gameFile)
    {
        return "screen-" + cabinetName + "-" + gameFile;
    }

    public Cabinet addCRT(string type, string orientation, string gameFile, string GameVideoFile, int timeToLoad, string pathBase,
                             bool invertX = false, bool invertY = false,
                             bool GameVideoFileInvertX = false, bool GameVideoFileInvertY = false,
                             bool EnableSaveState = true, string StateFile = "state.nv",
                             Vector3? rotation = null, float scalePercentage = 0,
                             string gamma = "1.0", string brightness = "1.0",
                             List<GameObject> agentPlayerPositions = null,
                             string shaderName = "damage", Dictionary<string, string> shaderConfig = null,
                             ControlMapConfiguration cabinetControlMap = null,
                             LightGunInformation lightGunInformation = null,
                             CabinetAGEBasicInformation agebasic = null
                          )
    {
        string CRTType = $"screen-mock-{orientation}";
        GameObject CRT = Parts(CRTType);
        if (CRT == null)
            throw new System.Exception($"Malformed cabinet {Name} problem: mock CRT not found in model. Type: {CRTType}");

        GameObject newCRT = CRTsFactory.Instantiate(type, CRT.transform.position, CRT.transform.rotation, CRT.transform.parent);
        if (newCRT == null)
            throw new System.Exception($"Cabinet {Name} problem: can't create a CRT. Type: {type}");

        //LibretroScreenController will find the object using this name:
        newCRT.name = CRTName(Name, gameFile);

        // rotate and scale
        float scale = scalePercentage / 100f;
        newCRT.transform.localScale *= scale;
        if (rotation != null)
            newCRT.transform.Rotate((Vector3)rotation);

        Object.Destroy(Parts("screen-mock-horizontal"));
        Object.Destroy(Parts("screen-mock-vertical"));

        //adds a GameVideoPlayer, BoxCollider and a AudioSource to the screen
        LibretroScreenController libretroScreenController = newCRT.GetComponent<LibretroScreenController>();

        libretroScreenController.GameFile = gameFile;
        libretroScreenController.SecondsToWaitToFinishLoad = timeToLoad;
        libretroScreenController.EnableSaveState = EnableSaveState;
        libretroScreenController.StateFile = StateFile;
        libretroScreenController.PathBase = pathBase;

        libretroScreenController.GameInvertX = invertX;
        libretroScreenController.GameInvertY = invertY;
        libretroScreenController.Gamma = gamma;
        libretroScreenController.Brightness = brightness;

        libretroScreenController.ShaderName = shaderName;
        libretroScreenController.ShaderConfig = shaderConfig;

        libretroScreenController.AgentPlayerPositions = agentPlayerPositions;

        //video
        libretroScreenController.GameVideoFile = GameVideoFile;
        libretroScreenController.GameVideoInvertX = GameVideoFileInvertX;
        libretroScreenController.GameVideoInvertY = GameVideoFileInvertY;

        //control mapping
        libretroScreenController.CabinetControlMapConfig = cabinetControlMap;
        libretroScreenController.lightGunInformation = lightGunInformation;

        //age basic
        libretroScreenController.ageBasicInformation = agebasic;

        return this;
    }

    public Cabinet AddCoinSlot(string type, float rotationAngleX, float rotationAngleY, float rotationAngleZ, float scalePercentage)
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
        }
        else
            ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] ERROR {Name}: can't create the new coin-slot type: {type}");

        // rotate and scale
        float scale = scalePercentage / 100f;
        newCoinSlot.transform.localScale *= scale;
        newCoinSlot.transform.Rotate(rotationAngleX, rotationAngleY, rotationAngleZ);

        Object.Destroy(coinSlotMock);

        return this;
    }
}

