using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CM = ControlMapPathDictionary;
using LC = LibretroControlMapDictionnary;
public class ControlMapConfiguration
{
    [YamlMember(Alias = "maps", ApplyNamingConventions = false)]
    public List<Maps> mapList { get; set; }

    public class ControlMap
    {
        [YamlMember(Alias = "control", ApplyNamingConventions = false)]
        public string RealControl = ""; //quest-x-button
        public string Path = "";

        public ControlMap()
        {
        }
        public ControlMap(string realControl, string path = "")
        {
            RealControl = realControl;
            Path = path;
        }

    }

    public ControlMapConfiguration.Maps GetMap(string mameControl, int port)
    {
        return mapList.FirstOrDefault(map => map.mameControl == mameControl && map.port == port);
    }
    public bool Exists(string mameControl, int port)
    {
        return mapList.Any(map => map.mameControl == mameControl && map.port == port);
    }

    public class Maps
    {
        [YamlMember(Alias = "libretro-id", ApplyNamingConventions = false)]
        public string mameControl;
        public int port = 0;
        public string behavior = "button"; // or axis 
        [YamlMember(Alias = "maps-to", ApplyNamingConventions = false)]
        public List<ControlMap> controlMaps { get; set; }
        public Maps()
        {

        }
        public Maps(string mameControl, int port = 0, string behavior = "button")
        {
            this.mameControl = mameControl;
            this.behavior = behavior;
            this.port = port;
            this.controlMaps = new();
        }
        public ControlMap GetAction(string realControl)
        {
            return controlMaps.SingleOrDefault(ctrlMap => ctrlMap.RealControl == realControl);
        }
        public ControlMap AddAction(string realControl, string path = "")
        {
            ControlMap act = GetAction(realControl);
            if (act == null)
            {
                act = new(realControl, path);
                controlMaps.Add(act);
            }
            return act;
        }

        public string InputActionName()
        {
            return $"{mameControl}_{port}";
        }

    }

    public void AddMap(string mameControl, string realControl, string behavior = null, int port = 0)
    {
        ControlMapConfiguration.Maps map;
        if (behavior == null)
            behavior = ControlMapPathDictionary.GetBehavior(realControl);
        map = GetMap(mameControl, port);
        if (map == null)
        {
            map = new(mameControl, port, behavior);
            mapList.Add(map);
        }

        string path = ControlMapPathDictionary.GetInputPath(realControl);
        if (string.IsNullOrEmpty(path))
            ConfigManager.WriteConsole($"[ControlMapConfiguration.AddMap] ERROR path unknown action:{mameControl} maped control: {realControl}");
        else
            map.AddAction(realControl, path);
    }

    public void AddMap(string mameControl, string[] realControlsToAssign, string behavior = null, int port = 0)
    {
        foreach (string realControl in realControlsToAssign)
        {
            AddMap(mameControl, realControl, behavior, port);
        }

        return;
    }

    public void RemoveMaps(string mameControl, int port = 0)
    {
        ControlMapConfiguration.Maps map;
        map = GetMap(mameControl, port);
        if (map != null)
        {
            mapList.Remove(map);
        }
    }

    public void Merge(ControlMapConfiguration other)
    {
        if (other == null)
        {
            return;
        }

        // Iterate through each map in the other ControlMapConfiguration
        foreach (Maps otherMap in other.mapList)
        {
            // Find a matching map in this ControlMapConfiguration based on the mameControl name
            Maps thisMap = mapList.Find(x => x.mameControl == otherMap.mameControl && x.port == otherMap.port);

            // If a matching map is found, replace its controlMaps list with the one from the other ControlMapConfiguration
            if (thisMap != null)
            {
                thisMap.controlMaps = otherMap.controlMaps;
                thisMap.behavior = otherMap.behavior;
            }
            // If a matching map is not found, add the map from the other ControlMapConfiguration to this ControlMapConfiguration
            else
            {
                mapList.Add(otherMap);
            }
        }
    }
    public void SaveAsYaml(string fileName)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml = serializer.Serialize(this);

        File.WriteAllText(fileName, yaml);
    }
    public static ControlMapConfiguration LoadFromYaml(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }
        ConfigManager.WriteConsole($"[ControlMapConfiguration.LoadFromYaml] {fileName}");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml = File.ReadAllText(fileName);

        return deserializer.Deserialize<ControlMapConfiguration>(yaml);
    }

    public virtual void Load()
    {
        //to implement
        throw new NotImplementedException("[controlMapConfiguration] Method Load");
    }
    public virtual void Save()
    {
        //to implement
        throw new NotImplementedException("[controlMapConfiguration] Method Save");
    }

    public void ToDebug()
    {
        ConfigManager.WriteConsole("MAME \t Control \t behavior \t Unity Path ");
        foreach (ControlMapConfiguration.Maps m in mapList)
        {
            foreach (ControlMapConfiguration.ControlMap a in m.controlMaps)
            {
                ConfigManager.WriteConsole($"{m.mameControl} \t {a.RealControl} \t {m.behavior} \t {a.Path} ");
            }
        }
    }

    public string AsMarkdown()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("| MAME | Control | Behavior | Port | Unity Path |\n");
        sb.Append("| --- | --- | --- | --- | --- |\n");
        foreach (ControlMapConfiguration.Maps m in mapList)
        {
            foreach (ControlMapConfiguration.ControlMap a in m.controlMaps)
            {
                sb.Append($"| {m.mameControl} | {a.RealControl} | {m.port} | {m.behavior} | `{a.Path}` |\n");
            }
        }
        return sb.ToString();
    }

    public override string ToString()
    {
        return $" ControlMapConfiguration: {mapList.Count}";
    }
}

public class DefaultControlMap : ControlMapConfiguration
{
    static DefaultControlMap instance = null;

    public DefaultControlMap() : base()
    {
        mapList = new();

        mapList = new();

        //fire with b-button and trigger.
        AddMap(LC.JOYPAD_B, new string[] { CM.VR_CONTROLLER_B, CM.GAMEPAD_B, CM.VR_CONTROLLER_RIGHT_TRIGGER, CM.KEYBOARD_ENTER });
        AddMap(LC.JOYPAD_A, new string[] { CM.GAMEPAD_A, CM.VR_CONTROLLER_A });
        AddMap(LC.JOYPAD_X, new string[] { CM.GAMEPAD_X, CM.VR_CONTROLLER_X });
        AddMap(LC.JOYPAD_Y, new string[] { CM.GAMEPAD_Y, CM.VR_CONTROLLER_Y });
        AddMap(LC.JOYPAD_START, new string[] { CM.GAMEPAD_START, CM.VR_CONTROLLER_START });
        AddMap(LC.JOYPAD_SELECT, new string[] { CM.GAMEPAD_SELECT, CM.VR_CONTROLLER_SELECT });

        AddMap(LC.JOYPAD_UP, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.JOYPAD_DOWN, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.JOYPAD_RIGHT, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.JOYPAD_LEFT, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");

        AddMap(LC.JOYPAD_UP, new string[] { CM.GAMEPAD_DPAD_UP });
        AddMap(LC.JOYPAD_DOWN, new string[] { CM.GAMEPAD_DPAD_DOWN });
        AddMap(LC.JOYPAD_RIGHT, new string[] { CM.GAMEPAD_DPAD_RIGHT });
        AddMap(LC.JOYPAD_LEFT, new string[] { CM.GAMEPAD_DPAD_LEFT });

        //also map port 1 to the right one (roboton issue #204)
        AddMap(LC.JOYPAD_UP, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);
        AddMap(LC.JOYPAD_DOWN, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);
        AddMap(LC.JOYPAD_RIGHT, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);
        AddMap(LC.JOYPAD_LEFT, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);

        AddMap(LC.JOYPAD_LEFT_RUMBLE, new string[] { CM.VR_CONTROLLER_LEFT_HAPTIC_DEVICE });
        AddMap(LC.JOYPAD_RIGHT_RUMBLE, new string[] { CM.VR_CONTROLLER_RIGHT_HAPTIC_DEVICE });

        AddMap(LC.JOYPAD_L, new string[] { CM.VR_CONTROLLER_LEFT_TRIGGER, CM.GAMEPAD_LEFT_TRIGGER });
        AddMap(LC.JOYPAD_R, new string[] { CM.VR_CONTROLLER_RIGHT_TRIGGER, CM.GAMEPAD_RIGHT_TRIGGER });

        AddMap(LC.JOYPAD_L2, new string[] { CM.VR_CONTROLLER_LEFT_GRIP, CM.GAMEPAD_LEFT_BUMPER });
        AddMap(LC.JOYPAD_R2, new string[] { CM.VR_CONTROLLER_RIGHT_GRIP, CM.GAMEPAD_RIGHT_BUMPER });

        // mapped for mame menu in LibretroMameCore
        //AddMap(LibretroMameCore.RETRO_DEVICE_ID_JOYPAD_L3,  new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK_PRESS, CM.GAMEPAD_LEFT_THUMBSTICK_PRESS });
        AddMap(LC.JOYPAD_R3, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK_PRESS, CM.GAMEPAD_RIGHT_THUMBSTICK_PRESS });

        AddMap(LC.EXIT, new string[] { CM.VR_CONTROLLER_LEFT_GRIP, CM.GAMEPAD_LEFT_BUMPER, CM.KEYBOARD_ESC });
        AddMap(LC.INSERT, CM.GAMEPAD_SELECT);

        AddMap(LC.MOUSE_X, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis");
        AddMap(LC.MOUSE_Y, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis");
        AddMap(LC.MOUSE_LEFT, new string[] { CM.VR_CONTROLLER_B, CM.GAMEPAD_B });
        AddMap(LC.MOUSE_RIGHT, new string[] { CM.VR_CONTROLLER_A, CM.GAMEPAD_A });
        AddMap(LC.MOUSE_MIDDLE, new string[] { CM.VR_CONTROLLER_X, CM.GAMEPAD_X });
        AddMap(LC.MOUSE_WHEELUP, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.MOUSE_WHEELDOWN, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.MOUSE_HORIZ_WHEELUP, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.MOUSE_HORIZ_WHEELDOWN, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.MOUSE_BUTTON_4, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK_PRESS, CM.GAMEPAD_LEFT_THUMBSTICK_PRESS });
        AddMap(LC.MOUSE_BUTTON_5, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK_PRESS, CM.GAMEPAD_RIGHT_THUMBSTICK_PRESS });

        AddMap(LC.LIGHTGUN_AUX_A, new string[] { CM.VR_CONTROLLER_A, CM.GAMEPAD_A });
        AddMap(LC.LIGHTGUN_AUX_B, new string[] { CM.VR_CONTROLLER_B, CM.GAMEPAD_B, CM.VR_CONTROLLER_RIGHT_GRIP, CM.KEYBOARD_ENTER });
        AddMap(LC.LIGHTGUN_AUX_C, new string[] { CM.GAMEPAD_X, CM.VR_CONTROLLER_X });
        AddMap(LC.LIGHTGUN_DPAD_UP, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.LIGHTGUN_DPAD_DOWN, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.LIGHTGUN_DPAD_LEFT, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.LIGHTGUN_DPAD_RIGHT, new string[] { CM.VR_CONTROLLER_LEFT_THUMBSTICK, CM.GAMEPAD_LEFT_THUMBSTICK }, "axis");
        AddMap(LC.LIGHTGUN_DPAD_UP, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);
        AddMap(LC.LIGHTGUN_DPAD_DOWN, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);
        AddMap(LC.LIGHTGUN_DPAD_LEFT, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);
        AddMap(LC.LIGHTGUN_DPAD_RIGHT, new string[] { CM.VR_CONTROLLER_RIGHT_THUMBSTICK, CM.GAMEPAD_RIGHT_THUMBSTICK }, "axis", 1);
        AddMap(LC.LIGHTGUN_START, new string[] { CM.GAMEPAD_START, CM.VR_CONTROLLER_START });
        AddMap(LC.LIGHTGUN_SELECT, new string[] { CM.GAMEPAD_SELECT, CM.VR_CONTROLLER_SELECT });
        AddMap(LC.LIGHTGUN_TRIGGER, new string[] { CM.VR_CONTROLLER_RIGHT_TRIGGER, CM.GAMEPAD_RIGHT_TRIGGER });
        AddMap(LC.LIGHTGUN_RELOAD, new string[] { CM.GAMEPAD_START, CM.VR_CONTROLLER_START });
    }

    public static DefaultControlMap Instance
    {
        get
        {
            if (instance == null)
                instance = new();
            return instance;
        }
    }
}

public class GlobalControlMap : DefaultControlMap
{
    public GlobalControlMap()
    {
        Load();
    }
    public override void Save()
    {
        SaveAsYaml(ConfigManager.ConfigControllersDir + "/global.yaml");
    }
    public override void Load()
    {
        ControlMapConfiguration global = LoadFromYaml(ConfigManager.ConfigControllersDir + "/global.yaml");
        Merge(global);
    }
}


public class GameControlMap : GlobalControlMap
{
    private string cabinetDBName;
    public GameControlMap(string cabDBName) : base()
    {
        this.cabinetDBName = cabDBName;
        Load();
    }

    public static string Path(string cabDBName)
    {
        return ConfigManager.ConfigControllersDir + "/" + cabDBName + ".yaml";
    }

    private string getFileName()
    {
        return Path(cabinetDBName);
    }

    public static bool ExistsConfiguration(string cabDBName)
    {
        string path = Path(cabDBName);
        bool exists = File.Exists(Path(cabDBName));
        ConfigManager.WriteConsole($"[GameControlMap] configuration exists for game {cabDBName}: {exists}");
        return exists;
    }

    public override void Save()
    {
        SaveAsYaml(getFileName());
    }
    public override void Load()
    {
        ControlMapConfiguration gameConfig = ControlMapConfiguration.LoadFromYaml(getFileName());
        Merge(gameConfig);
    }
}

public class ControlSchemeControlMap : DefaultControlMap
{
    private string controlScheme;
    public ControlSchemeControlMap(string controlScheme) : base()
    {
        this.controlScheme = controlScheme;
        Load();
    }

    public static string Path(string controlScheme)
    {
        return ConfigManager.ConfigControllerSchemesDir + "/" + controlScheme + ".yaml";
    }

    private string getFileName()
    {
        return Path(controlScheme);
    }

    public static bool ExistsConfiguration(string controlScheme)
    {
        string path = Path(controlScheme);
        bool exists = File.Exists(Path(controlScheme));
        ConfigManager.WriteConsole($"[GameControlMap] configuration exists for control scheme {controlScheme}: {exists}");
        return exists;
    }
    public override void Load()
    {
        ControlMapConfiguration schemeConfig = ControlMapConfiguration.LoadFromYaml(getFileName());
        Merge(schemeConfig);
    }
}

public class CustomControlMap : GlobalControlMap
{
    public CustomControlMap(ControlMapConfiguration defaultConf) : base()
    {

        Merge(defaultConf);
        ConfigManager.WriteConsole($"[CustomControlMap] Merged {defaultConf.ToString()}");
    }
}
