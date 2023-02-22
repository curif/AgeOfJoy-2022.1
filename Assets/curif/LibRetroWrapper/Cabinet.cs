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

  public static Shader GetShader(string name)
  {
    Shader shader = Shader.Find(name);
    if (shader == null || shader.ToString() == "Hidden/InternalErrorShader (UnityEngine.Shader)")
    {
      UnityEngine.Debug.LogError($"Internal error, Shader not found: {name}");
      shader = Shader.Find("Standard");
    }
    return shader;
  }

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

  private void addRigidBody() {
    
    Rigidbody cabRB = gameObject.AddComponent<Rigidbody>(); //this will excecute Start().
    cabRB.freezeRotation = true;
    cabRB.isKinematic = true;
    cabRB.useGravity = false;
    cabRB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ
                        | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;
    
  }
  private void addBoxCollider() 
  {
    BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
    if (boxCollider != null) 
      return;

    boxCollider = gameObject.AddComponent<BoxCollider>();
    
    // Get the width of the parent game object
    float minX = float.MaxValue;
    float maxX = float.MinValue;
    float minY = float.MaxValue;
    float maxY = float.MinValue;
    float minZ = float.MaxValue;
    float maxZ = float.MinValue;
    foreach (Transform child in gameObject.transform)
    {
        float childX = child.position.x;
        float childY = child.position.y;
        float childZ = child.position.z;
        if (childX < minX)
            minX = childX;
        if (childX > maxX)
            maxX = childX;
        if (childY < minY)
            minY = childY;
        if (childY > maxY)
            maxY = childY;
        if (childZ < minZ)
            minZ = childZ;
        if (childZ > maxZ)
            maxZ = childZ;
    }
    float widthX = maxX - minX + 0.05f;
    float widthY = maxY - minY + 0.05f;
    float widthZ = maxZ - minZ + 0.05f;
    boxCollider.size = new Vector3(widthX, widthY, widthZ);
    boxCollider.center = new Vector3(0, widthY/2, 0);
  }

  private void toFloor() {
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

    //add neccesary components
    addRigidBody();
    toFloor();
    addBoxCollider();

    if (!IsValid)
      throw new System.Exception($"[Cabinet] Malformed Cabinet {Name} , some parts are missing. List of expected parts: {string.Join(",", RequiredParts)}");

    SetMaterial(CabinetMaterials.Black);
    if (PartsExist("bezel"))
      SetMaterial("bezel", CabinetMaterials.FrontGlassWithBezel);

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
    if (childTransform == null)
      return null;
    return childTransform.gameObject;
  }
  public bool PartsExist(string part)
  {
    return gameObject.transform.Find(part) != null;
  }

  public Cabinet ScalePart(string cabinetPart , float percentage) {
    if (!PartsExist(cabinetPart))
      return this;
    
    float scale = percentage / 100f;
    Parts(cabinetPart).transform.localScale *= scale;
    return this;
  }
  public Cabinet RotatePart(string cabinetPart, float angleX, float angleY, float angleZ) {
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

  public Cabinet SetMarquee(string part, string texturePath, bool invertX = false, bool invertY = false)
  {
    return SetTextureTo(part, texturePath, CabinetMaterials.Marquee, invertX: invertX, invertY: invertY);
  }
  public Cabinet SetMarqueeEmissionColor(string part, Color color) {
    if (!PartsExist(part))
      return this;
    Renderer r = Parts(part).GetComponent<Renderer>();
    if (r != null)
      r.material.SetColor("_EmissionColor", color);
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

  public static string CRTName(string cabinetName, string gameFile) {
    return "screen-" + cabinetName + "-" + gameFile;
  }

  public Cabinet addCRT(string type, string orientation, string GameFile, string GameVideoFile, int timeToLoad, string pathBase,
                           bool invertX = false, bool invertY = false,
                           bool GameVideoFileInvertX = false, bool GameVideoFileInvertY = false,
                           bool EnableSaveState = true, string StateFile = "state.nv",
                           Vector3? rotation = null, float scalePercentage = 0, 
                           string gamma = "1.0", string brightness = "1.0",
                           List<GameObject> agentPlayerPositions = null)
  {

    //the order is important
    Material[] ms = new Material[] {
            new Material(CabinetMaterials.TVBorder),
            new Material(CabinetMaterials.Screen)
        };

    string CRTType = $"screen-mock-{orientation}";
    GameObject CRT = Parts(CRTType);
    if (CRT == null)
      throw new System.Exception($"Malformed cabinet {Name} problem: mock CRT not found in model. Type: {CRTType}");
    GameObject newCRT = CRTsFactory.Instantiate(type, CRT.transform.position, CRT.transform.rotation, CRT.transform.parent);
    if (newCRT == null)
      throw new System.Exception($"Cabinet {Name} problem: can't create a CRT. Type: {type}");

    //LibretroScreenController will find the object using this name:
    newCRT.name = CRTName(Name, GameFile);
   
    // rotate and scale
    float scale = scalePercentage / 100f;
    newCRT.transform.localScale *= scale;
    if (rotation != null)
      newCRT.transform.Rotate((Vector3)rotation);

    newCRT.GetComponent<MeshRenderer>().materials = ms;

    Object.Destroy(Parts("screen-mock-horizontal"));
    Object.Destroy(Parts("screen-mock-vertical"));

    //adds a GameVideoPlayer, BoxCollider and a AudioSource to the screen
    LibretroScreenController libretroScreenController = newCRT.GetComponent<LibretroScreenController>();

    libretroScreenController.GameFile = GameFile;
    libretroScreenController.SecondsToWaitToFinishLoad = timeToLoad;
    libretroScreenController.EnableSaveState = EnableSaveState;
    libretroScreenController.StateFile = StateFile;
    libretroScreenController.PathBase = pathBase;
    libretroScreenController.GameInvertX = invertX;
    libretroScreenController.GameInvertY = invertY;
    libretroScreenController.Gamma = gamma;
    libretroScreenController.Brightness = brightness;

    libretroScreenController.AgentPlayerPositions = agentPlayerPositions;
    libretroScreenController.GameVideoFile = GameVideoFile;
    libretroScreenController.GameVideoInvertX = GameVideoFileInvertX;
    libretroScreenController.GameVideoInvertY = GameVideoFileInvertY;

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
