/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class ConfigManager {
    //paths
    public static string BaseDir = "/sdcard/Android/data/com.curif.AgeOfJoy";
    public static string Cabinets = $"{BaseDir}/cabinets"; //compressed
    public static string CabinetsDB = $"{BaseDir}/cabinetsdb"; //uncompressed cabinets
    public static string SystemDir = $"{BaseDir}/system";
    public static string RomsDir = $"{BaseDir}/downloads";
    public static string GameSaveDir = $"{BaseDir}/save";

    static ConfigManager() {
         
        if (!Directory.Exists(ConfigManager.Cabinets)) {
            // Directory.CreateDirectory(BaseDir);
            Directory.CreateDirectory(ConfigManager.Cabinets);
        }
        if (!Directory.Exists(ConfigManager.CabinetsDB)) {
            // Directory.CreateDirectory(BaseDir);
            Directory.CreateDirectory(ConfigManager.CabinetsDB);
        }
        if (!Directory.Exists(ConfigManager.SystemDir)) {
            Directory.CreateDirectory(ConfigManager.SystemDir);
            Directory.CreateDirectory(ConfigManager.RomsDir);
            Directory.CreateDirectory(ConfigManager.GameSaveDir);
        }

    }
    public static void WriteConsole(string st) {
        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, st);
    }
}

public static class CabinetMaterials {

    public static Material Base; //uses the shade mobile/difusse
    public static Material Black; 
    public static Material LightWood;
    public static Material TVBorder;
    public static Material Screen;
    public static Material FrontGlassWithBezel;
    public static Material Marquee;
    // public static Material CoinSlotPlastic;
    // public static Material CoinSlotPlasticDouble;
    // public static Material LeftOrRight; //the material used with stickers in the sides of the cabinet.
    
    //list of the materials by his name, usefull in serialization
    public static Dictionary<string, Material> materialList = new();

    static CabinetMaterials() {
        // the material base for stickers
        Base = Resources.Load<Material>("Cabinets/Materials/Base");

        //pre created in Unity editor
        Black = Resources.Load<Material>("Cabinets/Materials/CabinetBlack");
        LightWood = Resources.Load<Material>("Cabinets/Materials/wood");

        materialList.Add("black", Black);
        materialList.Add("base", Base);
        materialList.Add("lightwood", LightWood);

        TVBorder = Resources.Load<Material>("Cabinets/Materials/TVBorder");
        Marquee = Resources.Load<Material>("Cabinets/Materials/Marquee");
        Screen = Resources.Load<Material>("Cabinets/Materials/Screen");
        FrontGlassWithBezel = Resources.Load<Material>("Cabinets/Materials/FrontGlass");

        // CoinSlotPlastic = Resources.Load<Material>("Cabinets/Materials/CoinSlotPlastic");
        // CoinSlotPlasticDouble = Resources.Load<Material>("Cabinets/Materials/CoinSlotPlasticDouble");

    }

    public static Material fromName(string name) {
        if (! materialList.ContainsKey(name)) {
            ConfigManager.WriteConsole($"ERROR: material name {name} is unknown, fallback to material standard 'black'.");
            name = "black";
        }
        return materialList[name];
    }

}
//other objects in cabinet, interchangeable.
public static class CoinSlotsFactory {
    public static Dictionary<string, GameObject> objects = new();
    
    static CoinSlotsFactory() {
        objects.Add("coin-slot-small", Resources.Load<GameObject>("Cabinets/PreFab/CoinSlots/CoinSlotPlastic"));
        objects.Add("coin-slot-double", Resources.Load<GameObject>("Cabinets/PreFab/CoinSlots/CoinSlotPlasticDouble"));
    }

    public static GameObject Instantiate(string type, Vector3 position, Quaternion rotation, Transform parent) {
        if (objects.ContainsKey(type)) {
            return GameObject.Instantiate<GameObject>(objects[type], position, rotation, parent);
        }
        Debug.LogError($"CoinSlotsFactory Factory don't have a {type} object in list: {objects.Keys.ToString()}");
        return null;
    }
}

public static class CRTsFactory {
    public static Dictionary<string, GameObject> objects = new();
    
    static CRTsFactory() {
        //CRTs
        // Assets/Resources/Cabinets/PreFab/CRTs/screen19i.prefab
        objects.Add("19i", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i"));
    }

    public static GameObject Instantiate(string type, Vector3 position, Quaternion rotation, Transform parent) {
        if (objects.ContainsKey(type)) {
            return GameObject.Instantiate<GameObject>(objects[type], position, rotation, parent);
        }
        Debug.LogError($"CRTsFactory Factory don't have a {type} object in list: {objects.Keys.ToString()}");
        return null;
    }
}

public class Cabinet {
    public string Name = "";
    public GameObject gameObject;
    // private GameObject Center, Front, Joystick, Left, Marquee, Right, Screen;
    // private Material sharedMaterial;
    Dictionary<string, GameObject> Parts = new();

    //Al the parts in the gabinet that must exists, can be others.
    static List<string> RequiredParts = new List<string>() {"front", "left", "right", "joystick", "screen-base", "screen-mock-vertical", "screen-mock-horizontal"};
    //Known parts that can exists or not.
    static List<string> NonStandardParts = new List<string>() {"Marquee", "bezel", "coin-slot", "screen"};
    //those parts that the user can configure, but is not limited to.
    public static List<string> userStandarConfigurableParts = new List<string>() {"front", "left", "right", "joystick", "joystick-down", "screen-base"};
    
    public static Shader GetShader(string name) {
        Shader shader = Shader.Find(name);
        if (shader == null || shader.ToString() == "Hidden/InternalErrorShader (UnityEngine.Shader)") {
            UnityEngine.Debug.LogError($"Internal error, Shader not found: {name}");
            shader = Shader.Find("Standard");
        }
        return shader;
    }
    
    // load a texture from disk.
    private static Texture2D LoadTexture(string filePath) {
 
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))     {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
    public bool IsValid {
        get {
            foreach (string key in RequiredParts) {
                if (! Parts.ContainsKey(key)) {
                    return false;
                }
            }
            return true;
        }
    } 

    public Cabinet(string name, Vector3 position, Quaternion rotation, GameObject go = null,  string model = "Generic") {
        Name = name;
        if (go == null) {
            // Assets/Resources/Cabinets/PreFab/xxx.prefab
            go = Resources.Load<GameObject>($"Cabinets/PreFab/{model}");
            if (go == null) {
                throw new System.Exception($"Cabinet {Name} not found or doesn't load");
            }
        }
        
        //https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
        gameObject = GameObject.Instantiate(go, position, rotation) as GameObject;

        // https://stackoverflow.com/questions/40752083/how-to-find-child-of-a-gameobject-or-the-script-attached-to-child-gameobject-via
        for (int i = 0; i < gameObject.transform.childCount; i++) {
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            if (child != null) {
                Parts.Add(child.name, child);
                ConfigManager.WriteConsole($"Cabinet {name} part {child.name} added");
            }
        }
        if (!IsValid) {
            throw new System.Exception($"Malformed Cabinet, some parts are missing. List of expected parts: {string.Join(",", RequiredParts)}");
        }

        BoxCollider bc = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
        
        SetMaterial(CabinetMaterials.Black);
        if (Parts.ContainsKey("bezel")) {
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
    public GameObject this[string part] {
        get {
            return Parts[part];
        }
    }

    
    //change a material, and
    public Cabinet SetTextureTo(string cabinetPart, string textureFile, Material mat, bool invertX = false, bool invertY = false) {

        if (! Parts.ContainsKey(cabinetPart)) {
            // throw new System.Exception($"Unrecognized part: {cabinetPart} adding the texture: {textureFile}");
            return this;
        }
        if (mat == null && string.IsNullOrEmpty(textureFile)) {
            return this;
        }

        Material m = new Material(mat);
        //tiling
        Vector2 mainTextureScale = new Vector2(1, 1);
        if (invertX) {
            mainTextureScale.x = -1;
        }
        if (invertY) {
            mainTextureScale.y = -1;
        }
        m.mainTextureScale = mainTextureScale;
    
        if (! string.IsNullOrEmpty(textureFile)) {
            //main texture
            Texture2D t = LoadTexture(textureFile);
            if (t == null) {
                ConfigManager.WriteConsole($"ERROR Cabinet {Name} texture error {textureFile}");
                return this;
            }
            else {
                m.SetTexture("_MainTex", t);
            }
        }

        Renderer r = Parts[cabinetPart].GetComponent<Renderer>();
        r.material = m;

        return this;
    }

    public Cabinet SetMarquee(string texturePath, Color color, bool invertX = false, bool invertY = false) {
        SetTextureTo("marquee", texturePath, CabinetMaterials.Marquee, invertX: invertX, invertY: invertY);
        if (color != null) {
            Renderer r = Parts["marquee"].GetComponent<Renderer>();
            Debug.Log($"SetMarquee color: {color}");
            r.material.SetColor("_EmissionColor", color);
        }
        
        return this;
    }
    public Cabinet SetBezel(string texturePath, bool invertX = false, bool invertY = false) {
        SetTextureTo("bezel", texturePath, CabinetMaterials.FrontGlassWithBezel, invertX: invertX, invertY: invertY);
        return this;
    }

    //set the same material to all components.
    public Cabinet SetMaterial(Material mat) {
        foreach(KeyValuePair<string, GameObject> part in Parts) {
            if (!NonStandardParts.Contains(part.Key)) {
                part.Value.GetComponent<Renderer>().material = mat;
            }
        }
        return this;
    }

    //set the material to a component. Don't create new.
    public Cabinet SetMaterial(string part, Material mat) {
        if (! Parts.ContainsKey(part)) {
            throw new System.Exception($"Unknown part {part} to set material in cabinet {Name}");
        }
        Parts[part].GetComponent<Renderer>().material = mat;
        return this;
    }

    public Cabinet addCRT(string type, string orientation, string GameFile, string GameVideoFile, int timeToLoad,
                             bool invertX = false, bool invertY = false,
                             bool GameVideoFileInvertX = false, bool GameVideoFileInvertY = false
                             ) {

        //the order is important
        Material[] ms = new Material[] {
            new Material(CabinetMaterials.TVBorder),
            new Material(CabinetMaterials.Screen)
        };
        
        string CRTType = $"screen-mock-{orientation}";
        GameObject CRT = Parts[CRTType];
        GameObject go = CRTsFactory.Instantiate(type, CRT.transform.position, CRT.transform.rotation, CRT.transform.parent);
        if (go == null) {
            throw new System.Exception($"Cabinet {Name} problem: can't create a CRT. Type: {type}");
        }
        
        //LibretroScreenController will find the object using this name:
        go.name = "screen";
        Parts.Add("screen",go);

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
        libretroScreenController.GameVideoFile = GameVideoFile;
        libretroScreenController.SecondsToWaitToFinishLoad = timeToLoad;
        libretroScreenController.GameVideoInvertX = GameVideoFileInvertX;
        libretroScreenController.GameVideoInvertY = GameVideoFileInvertY;
        libretroScreenController.GameInvertX = invertX;
        libretroScreenController.GameInvertY = invertY;

        //it's very basic, but the spatializer wont work, so may be it's the only alternative.
        AudioSource audio = Parts["screen"].GetComponent<AudioSource>();
        audio.minDistance = 0.6f; //max sound inside this range
        audio.maxDistance = 1f;
        audio.rolloffMode = AudioRolloffMode.Linear;

        return this;
    }

    public Cabinet AddCoinSlot(string type) {
        if (! Parts.ContainsKey("coin-slot")) {
            return this;
        }
        GameObject coinDoor = Parts["coin-slot"]; 
        GameObject go = CoinSlotsFactory.Instantiate(type, coinDoor.transform.position, coinDoor.transform.rotation, coinDoor.transform.parent);
        if (go != null) {
            //LibretroScreenController will find the coinslot using this name:
            go.name = "coin-slot-added";
            Parts.Add("coin-slot-added",go);
        }
        
        Object.Destroy(Parts["coin-slot"]);
        Parts.Remove("coin-slot");

        return this;
    }

    /*
    public Cabinet deleteBezel() {
        MeshRenderer mr = Parts["bezel"].GetComponent<MeshRenderer>() as MeshRenderer;
        mr.enabled = false;
    }
    */
    

}

//store Cabinets resources
public static class CabinetFactory {
    public static Dictionary<string, GameObject> CabinetStyles = new();

    static CabinetFactory() {
        CabinetStyles.Add("generic",  Resources.Load<GameObject>($"Cabinets/PreFab/Generic"));
        CabinetStyles.Add("timeplt",  Resources.Load<GameObject>($"Cabinets/PreFab/TimePilot"));
        CabinetStyles.Add("galaga",  Resources.Load<GameObject>($"Cabinets/PreFab/Galaga"));
        CabinetStyles.Add("pacmancabaret",  Resources.Load<GameObject>($"Cabinets/PreFab/PacManCabaret"));

        foreach(KeyValuePair<string, GameObject> cab in CabinetStyles) {
            cab.Value.AddComponent<MeshCollider>();
        }
    }

    public static Cabinet Factory(string style, string name, Vector3 position, Quaternion rotation) {
        if (!CabinetStyles.ContainsKey(style) ||  CabinetStyles[style] == null) {
            Debug.LogError($"Cabinet Factory: style {style} unknown or not loaded, falls to 'generic' cabinet");
            style = "generic";
        }

        return new Cabinet(name, position, rotation, CabinetStyles[style]);
    }

    public static Cabinet fromInformation(CabinetInformation cbinfo, Vector3 position, Quaternion rotation) {
        Cabinet cb = CabinetFactory.Factory(cbinfo.style, cbinfo.name, position, rotation);
        cb.SetMaterial(CabinetMaterials.fromName(cbinfo.material));

        //process each part
        foreach(CabinetInformation.Part p in cbinfo.Parts) {
            Material mat = CabinetMaterials.Base;
            if (p.material != null) {
                mat = CabinetMaterials.fromName(p.material);
            }

            if (p.art != null) {
                cb.SetTextureTo(p.name, cbinfo.getPath(p.art.file), mat, invertX: p.art.invertx, invertY: p.art.inverty);
            }
            else {
                cb.SetMaterial(p.name, mat);
            }
        }
        if (cbinfo.bezel != null) {
            cb.SetBezel(cbinfo.getPath(cbinfo.bezel.art.file));
        }
        if (cbinfo.marquee != null) {
            cb.SetMarquee(cbinfo.getPath(cbinfo.marquee.art.file), cbinfo.marquee.lightcolor.getColor());
        }
        else {
            cb.SetMarquee("", Color.white);            
        }
        if (!string.IsNullOrEmpty(cbinfo.coinslot)) {
            cb.AddCoinSlot(cbinfo.coinslot);
        }

        cb.addCRT(cbinfo.crt.type, cbinfo.crt.orientation, cbinfo.rom, cbinfo.getPath(cbinfo.video.file), cbinfo.timetoload, 
                    invertX: cbinfo.crt.screen.invertx, invertY: cbinfo.crt.screen.inverty,
                    GameVideoFileInvertX: cbinfo.video.invertx, GameVideoFileInvertY: cbinfo.video.inverty
                    );

        return cb;
    }
}

public class Init {

    static string testedCabinetName = "TestedCabinet";

    //https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute-ctor.html
    [RuntimeInitializeOnLoadMethod]
    static async void OnRuntimeMethodLoad() {
        bool isWorkshop = true; //look for a process to detect if this is a workshop or not.
        ConfigManager.WriteConsole("+++++++++++++++++++++  Initialize Cabinets +++++++++++++++++++++");

        ConfigManager.WriteConsole("Loading cabinets");
        CabinetDBAdmin.loadCabinets();

        GameObject[] cabinetSpots = GameObject.FindGameObjectsWithTag("spot");
        int cabinetFoundIndex = 0;
        ConfigManager.WriteConsole($"{cabinetSpots.Length} spots to fill in the workshop space.");

        ConfigManager.WriteConsole($"processing database: {ConfigManager.CabinetsDB}");
        string[] files = Directory.GetDirectories(ConfigManager.CabinetsDB);
        ConfigManager.WriteConsole($"{files.Length} directories found in database {ConfigManager.CabinetsDB}");
        foreach (string dir in files) {
            if (isWorkshop) {
                ConfigManager.WriteConsole($"processing entry: {dir}");
                if (Directory.Exists(dir)) {
                    CabinetInformation cbInfo = null;
                    try {
                        cbInfo = CabinetInformation.fromYaml(dir); //description.yaml
                        ConfigManager.WriteConsole($"** YAML loaded cabinet {cbInfo.name} rom: {cbInfo.rom}");

                        //all the errors are not a problem because there are defaults for each ones and the cabinet have to be made, exist or not an error.
                        CabinetInformation.showCabinetProblems(cbInfo);
                    }
                    catch (System.Exception e) {
                        ConfigManager.WriteConsole($"ERROR ** YAML cabinet not loaded {dir}: {e}");
                        cbInfo = null;                    
                    }

                    if (cbInfo != null) {
                        GameObject cabSpot = null;
                        string name = "";
                        Cabinet cab;

                        //spot selection
                        if (dir.Contains("/test")) {
                            cabSpot = GameObject.Find("CabSpot");
                            name = testedCabinetName;
                        }
                        else {
                            cabSpot = cabinetSpots[cabinetFoundIndex];
                            cabinetFoundIndex++;
                            name = $"Cabinet-{cabinetFoundIndex}";
                        }

                        //invoque and deploy the new cabinet
                        try {
                            cab = CabinetFactory.fromInformation(cbInfo, cabSpot.transform.position, cabSpot.transform.rotation);
                            Object.Destroy(cabSpot);
                            cab.gameObject.name = name;

                            if (cab.gameObject.name == testedCabinetName) {
                                //cabinet auto reload
                                cab.gameObject.AddComponent(typeof(CabinetAutoReload));
                            }
                        }
                        catch (System.Exception e) {
                            ConfigManager.WriteConsole($"ERROR ** cabinet not deployed {dir}: {e}");
                        }

                        if (cabinetFoundIndex >= cabinetSpots.Length) {
                            break;
                        }
                    }
                }
            }
            
        }

        // if (isWorkshop) {
        //     System.Timers.Timer timer = new System.Timers.Timer(2000);
        //     timer.Start();
        //     timer.Elapsed += ReloadTestCabinet;
        // }

        Debug.Log("+++++++++++++++++++++ Initialized");

    }

    private static void ReloadTestCabinet(object sender,  System.Timers.ElapsedEventArgs e) {
        string testFile = ConfigManager.Cabinets + "/test.zip";
        if (File.Exists(testFile)) {
            ConfigManager.WriteConsole($"New cabinet to test: {testFile}");
                
            //new cabinet to test
            CabinetInformation cbInfo = null;       
            try {
                CabinetDBAdmin.loadCabinetFromZip(testFile);

                cbInfo = CabinetInformation.fromYaml(ConfigManager.CabinetsDB + "/test"); //description.yaml
             
                CabinetInformation.showCabinetProblems(cbInfo);

                GameObject cabSpot = GameObject.Find(testedCabinetName);
                if (cabSpot == null) {
                    ConfigManager.WriteConsole("ERROR ** there is no place where to deploy the cabinet");
                    return;
                }

                ConfigManager.WriteConsole($"Deploy test cabinet {cbInfo.name}");
                Cabinet cab = CabinetFactory.fromInformation(cbInfo, cabSpot.transform.position, cabSpot.transform.rotation);
                cab.gameObject.name = testedCabinetName;

                ConfigManager.WriteConsole($"destroy previous tested cabinet {cabSpot.name}");
                Object.Destroy(cabSpot);
            }
            catch (System.Exception ex) {
                ConfigManager.WriteConsole($"ERROR loading cabinet from description {testFile}: {ex}");
                return;
            }

            ConfigManager.WriteConsole("New Tested Cabinet deployed ******");
        }
    }

}
