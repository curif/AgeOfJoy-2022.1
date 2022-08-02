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
  // private GameObject Center, Front, Joystick, Left, Marquee, Right, Screen;
  // private Material sharedMaterial;
  Dictionary<string, GameObject> Parts = new();

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
      {
        if (!Parts.ContainsKey(key))
        {
          return false;
        }
      }
      return true;
    }
  }

  public Cabinet(string name, Vector3 position, Quaternion rotation, GameObject go = null, string model = "Generic")
  {
    Name = name;
    if (go == null)
    {
      // Assets/Resources/Cabinets/PreFab/xxx.prefab
      go = Resources.Load<GameObject>($"Cabinets/PreFab/{model}");
      if (go == null)
      {
        throw new System.Exception($"Cabinet {Name} not found or doesn't load");
      }
    }

    //https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
    gameObject = GameObject.Instantiate(go, position, rotation) as GameObject;

    // https://stackoverflow.com/questions/40752083/how-to-find-child-of-a-gameobject-or-the-script-attached-to-child-gameobject-via
    for (int i = 0; i < gameObject.transform.childCount; i++)
    {
      GameObject child = gameObject.transform.GetChild(i).gameObject;
      if (child != null)
      {
        Parts.Add(child.name, child);
        ConfigManager.WriteConsole($"[Cabinet] Cabinet {name} part {child.name} added");
      }
    }
    if (!IsValid)
    {
      throw new System.Exception($"[Cabinet] Malformed Cabinet {Name} , some parts are missing. List of expected parts: {string.Join(",", RequiredParts)}");
    }

    //BoxCollider bc = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;

    SetMaterial(CabinetMaterials.Black);
    if (Parts.ContainsKey("bezel"))
    {
      SetMaterial("bezel", CabinetMaterials.FrontGlassWithBezel);
    }
    // SetMaterial("screen", CabinetMaterials.TVBorder);
    // SetMaterial("screen-border", CabinetMaterials.TVBorder);

    /*
    var tFront = gameObject.transform.Find("front");
    var tJoystick = gameObject.transform.Find("joystick");
    var tCenter = gameObject.transform.Find("Center");
    var tLeft = gameObject.transform.Find("left");
    var tMarquee = gameObject.transform.Find("Marquee");
    var tRight = gameObject.transform.Find("right");
    var tScreen = gameObject.transform.Find("screen");
    var tGlass = gameObject.transform.Find("Glass");

    if (tFront == null || tJoystick == null || tCenter == null || tLeft == null || tRight == null || tScreen == null) {
        throw new System.Exception("Malformed Cabinet, some meshes are missing.");
    }
    parts.Add("Center", tCenter.gameObject);
    parts.Add("front", tFront.gameObject);
    parts.Add("joystick", tJoystick.gameObject);
    parts.Add("left", tLeft.gameObject);
    parts.Add("right", tRight.gameObject);
    parts.Add("screen", tScreen.gameObject);
    if (tMarquee != null) {
        parts.Add("Marquee", tMarquee.gameObject);
    }
    if (tGlass != null) {
        parts.Add("Glass", tGlass.gameObject);
    }
    */
  }
  public GameObject this[string part]
  {
    get
    {
      return Parts[part];
    }
  }


  //change a material, and
  public Cabinet SetTextureTo(string cabinetPart, string textureFile, Material mat, bool invertX = false, bool invertY = false)
  {

    if (!Parts.ContainsKey(cabinetPart))
    {
      // throw new System.Exception($"Unrecognized part: {cabinetPart} adding the texture: {textureFile}");
      return this;
    }
    if (mat == null && string.IsNullOrEmpty(textureFile))
    {
      return this;
    }

    Material m = new Material(mat);
    //tiling
    Vector2 mainTextureScale = new Vector2(1, 1);
    if (invertX)
    {
      mainTextureScale.x = -1;
    }
    if (invertY)
    {
      mainTextureScale.y = -1;
    }
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
      {
        m.SetTexture("_MainTex", t);
      }
    }

    Renderer r = Parts[cabinetPart].GetComponent<Renderer>();
    r.material = m;

    return this;
  }

  public Cabinet SetMarquee(string texturePath, Color color, bool invertX = false, bool invertY = false)
  {
    SetTextureTo("marquee", texturePath, CabinetMaterials.Marquee, invertX: invertX, invertY: invertY);
    if (color != null)
    {
      Renderer r = Parts["marquee"].GetComponent<Renderer>();
      // Debug.Log($"SetMarquee color: {color}");
      r.material.SetColor("_EmissionColor", color);
    }

    return this;
  }
  public Cabinet SetBezel(string texturePath, bool invertX = false, bool invertY = false)
  {
    SetTextureTo("bezel", texturePath, CabinetMaterials.FrontGlassWithBezel, invertX: invertX, invertY: invertY);
    return this;
  }

  //set the same material to all components.
  public Cabinet SetMaterial(Material mat)
  {
    foreach (KeyValuePair<string, GameObject> part in Parts)
    {
      if (!NonStandardParts.Contains(part.Key))
      {
        part.Value.GetComponent<Renderer>().material = mat;
      }
    }
    return this;
  }

  //set the material to a component. Don't create new.
  public Cabinet SetMaterial(string part, Material mat)
  {
    if (!Parts.ContainsKey(part))
    {
      throw new System.Exception($"Unknown part {part} to set material in cabinet {Name}");
    }
    Parts[part].GetComponent<Renderer>().material = mat;
    return this;
  }

  public Cabinet addCRT(string type, string orientation, string GameFile, string GameVideoFile, int timeToLoad,
                           bool invertX = false, bool invertY = false,
                           bool GameVideoFileInvertX = false, bool GameVideoFileInvertY = false
                           )
  {

    //the order is important
    Material[] ms = new Material[] {
            new Material(CabinetMaterials.TVBorder),
            new Material(CabinetMaterials.Screen)
        };

    string CRTType = $"screen-mock-{orientation}";
    GameObject CRT = Parts[CRTType];
    GameObject newCRT = CRTsFactory.Instantiate(type, CRT.transform.position, CRT.transform.rotation, CRT.transform.parent);
    if (newCRT == null)
    {
      throw new System.Exception($"Cabinet {Name} problem: can't create a CRT. Type: {type}");
    }

    //LibretroScreenController will find the object using this name:
    newCRT.name = "screen";
    Parts.Add("screen", newCRT);

    Parts["screen"].GetComponent<MeshRenderer>().materials = ms;

    Object.Destroy(Parts["screen-mock-horizontal"]);
    Object.Destroy(Parts["screen-mock-vertical"]);
    Parts.Remove("screen-mock-horizontal");
    Parts.Remove("screen-mock-vertical");
    //mr.receiveShadows = false;

    LibretroScreenController libretroScreenController = Parts["screen"].AddComponent<LibretroScreenController>();
    // Parts["screen"].AddComponent(typeof(MeshRenderer)); added by default (as sound)
    // Parts["screen"].AddComponent(typeof(MeshCollider));
    // AudioSource audioSource = Parts["screen"].AddComponent(typeof(AudioSource)) as AudioSource;

    libretroScreenController.GameFile = GameFile;
    libretroScreenController.SecondsToWaitToFinishLoad = timeToLoad;
    libretroScreenController.GameInvertX = invertX;
    libretroScreenController.GameInvertY = invertY;

    libretroScreenController.GameVideoFile = GameVideoFile;
    libretroScreenController.GameVideoInvertX = GameVideoFileInvertX;
    libretroScreenController.GameVideoInvertY = GameVideoFileInvertY;

    //it's very basic, but the spatializer wont work, so may be it's the only alternative.
    AudioSource audio = Parts["screen"].GetComponent<AudioSource>();
    audio.minDistance = 0.6f; //max sound inside this range
    audio.maxDistance = 1f;
    audio.rolloffMode = AudioRolloffMode.Linear;

    return this;
  }

  public Cabinet AddCoinSlot(string type)
  {
    if (!Parts.ContainsKey("coin-slot"))
    {
      ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] ERROR coin-slot part not present in cabinet {Name}");
      return this;
    }

    GameObject coinSlotMock = Parts["coin-slot"];
    GameObject newCoinSlot = CoinSlotsFactory.Instantiate(type, coinSlotMock.transform.position, coinSlotMock.transform.rotation, coinSlotMock.transform.parent);
    if (newCoinSlot != null)
    {
      //LibretroScreenController will find the coinslot using this name:
      newCoinSlot.name = "coin-slot-added";
      Parts.Add("coin-slot-added", newCoinSlot);
      ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] {Name} added part coin-slot-added");
    }
    else
      ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] ERROR {Name}: can't create the new coin-slot type: {type}");

    Object.Destroy(Parts["coin-slot"]);
    //Parts["coin-slot"].SetActive(false);
    Parts.Remove("coin-slot");
    ConfigManager.WriteConsole($"[Cabinet.AddCoinSlot] {Name}: old coin-slot part unactive");

    return this;
  }
}
