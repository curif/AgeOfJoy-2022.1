/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using Assets.curif.LibRetroWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;
public static class CabinetInformationCache
{
    private static readonly Dictionary<string, CabinetInformation> _cache = new Dictionary<string, CabinetInformation>();

    public static void AddToCache(string key, CabinetInformation cabInfo)
    {
        _cache[key] = cabInfo;
    }

    public static CabinetInformation GetFromCache(string key)
    {
        return _cache.TryGetValue(key, out var cabInfo) ? cabInfo : null;
    }

    public static bool Contains(string key)
    {
        return _cache.ContainsKey(key);
    }
}


public class CabinetInformation
{
    public string name;
    public string rom;
    public List<string> roms;
    public List<Part> Parts { get; set; }
    public CRT crt; // = new CRT();
    public string style = "galaga";
    public Model model = new Model();
    public string material;
    public RGBColor color;
    public int year;
    public string coinslot = "coin-slot-double";
    public Geometry coinslotgeometry = new Geometry();
    public int timetoload = 3;
    public bool enablesavestate = false; //false to fix #34
    public bool? persistent;
    public string statefile = "state.nv";
    public Video video;// = new Video();
    public Audio audio;
    public string md5sum;
    public string space = "1x1x2";
    public string core = "mame2003+";
    public CoreEnvironment environment;
    public List<CabinetInputDevice> devices;

    [YamlMember(Alias = "mame-files", ApplyNamingConventions = false)]
    public List<MameFile> MameFiles { get; set; }

    [YamlMember(Alias = "controllers", ApplyNamingConventions = false)]
    public ControlMapConfiguration ControlMap;

    [YamlMember(Alias = "light-gun", ApplyNamingConventions = false)]
    public LightGunInformation lightGunInformation = new();

    public CabinetAGEBasicInformation agebasic = new();

    [YamlMember(Alias = "debug-mode", ApplyNamingConventions = false)]
    public bool debug = false;

    [YamlMember(Alias = "control-scheme", ApplyNamingConventions = false)]
    public string controlScheme;

    [YamlMember(Alias = "insert-coin-on-startup", ApplyNamingConventions = false)]
    public bool? insertCoinOnStartup;

    [YamlIgnore]
    public string pathBase;

    private static IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

    private Dictionary<uint, LibretroInputDevice> libretroInputDevices;

    public CabinetInformation() { }

    public void Validate()
    {
        // If "roms" is present, "rom" is ignored and replaced with the first element of "roms"
        if (roms != null && roms.Count >= 1)
        {
            rom = roms[0];
        }
        try
        {
            CheckResourcePath(rom);
            CheckResourcePath(statefile);
            CheckResourcePath(model?.file);
            CheckResourcePath(video?.file);

            if (roms != null)
            {
                foreach (string r in roms)
                {
                    CheckResourcePath(r);
                }
            }

            if (Parts != null)
            {
                foreach (Part p in Parts)
                {
                    CheckResourcePath(p.art?.file);
                    CheckResourcePath(p.emission?.art?.file);
                }
            }

            if (MameFiles != null)
            {
                foreach (MameFile mf in MameFiles)
                {
                    CheckResourcePath(mf?.file);
                }
            }
        }
        catch (Exception ex)
        {
            throw new CabinetValidationException(name, ex.Message, ex);
        }

        CheckAGEBasic(name);
    }

    public bool PartExists(string name)
    {
        if (Parts == null || Parts.Count == 0)
            return false;
        foreach (CabinetInformation.Part p in Parts)
        {
            if (p.name == name)
                return true;
        }
        return false;
    }

    public Dictionary<uint, LibretroInputDevice> GetLibretroInputDevices()
    {
        ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: START");
        if (libretroInputDevices == null)
        {
            ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: libretroInputDevices not computed yet");
            libretroInputDevices = new Dictionary<uint, LibretroInputDevice>();
            if (devices != null && devices.Count > 0)
            {
                ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: some devices have been specified");
                foreach (CabinetInputDevice device in devices)
                {
                    ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: adding device type {device.type} for slot {device.slot}");
                    LibretroInputDevice libretroDevice = LibretroInputDevice.GetInputDeviceType(device.type);
                    ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: found device {libretroDevice.Name} of id {libretroDevice.Id}");
                    libretroInputDevices.Add(device.slot, libretroDevice);
                }
            }
            else
            {
                uint idx = 0;
                ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: no devices have been specified");
                if (lightGunInformation != null && lightGunInformation.active)
                {
                    ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: adding lightgun");
                    libretroInputDevices.Add(idx++, LibretroInputDevice.Lightgun);
                }
                ConfigManager.WriteConsole($"[CabinetInformation.GetLibretroInputDevices]: adding gamepad");
                libretroInputDevices.Add(idx++, LibretroInputDevice.Gamepad);
            }
        }
        return libretroInputDevices;
    }

    public static void CheckResourcePath(string path)
    {
        if (path != null && (path.Contains("..")))
        {
            throw new Exception("Resource path " + path + " cannot contain '..'");
        }
    }
    public void CheckAGEBasic(string cabName)
    {
        if (agebasic == null)
            return;
        agebasic.Validate(cabName);
    }

    public static string debugLogPath(string cabinetName = null)
    {
        string filename;
        if (cabinetName == null)
            filename = "test.log";
        else
            filename = cabinetName + ".log";
        
        return Path.Combine(ConfigManager.DebugDir, filename);
    }

    public static string getNameFromPath(string path)
    {
        string directoryName = Path.GetDirectoryName(path);

        // If directoryName is null, it means the filePath is not a valid path
        if (directoryName == null)
            throw new Exception($"invalid path: {path}");

        // Split the directory path by directory separator and get the last element
        string[] directories = directoryName.Split(Path.DirectorySeparatorChar);
        return directories[directories.Length - 1];
    }

    public static void WriteExceptionLog(string path, Exception exception, string comments = "")
    {
        try
        {
            string cabinetName = getNameFromPath(path);
            string debugpath = debugLogPath();
            ConfigManager.WriteConsole($"[WriteExceptionLog] cab:{cabinetName} {debugpath}");
            using (StreamWriter writer = new StreamWriter(debugpath, true))
            {
                writer.WriteLine($"CABINET: {cabinetName}");
                writer.WriteLine($"YAML file: {path}");
                writer.WriteLine($"DateTime: {DateTime.Now.ToString()}");
                writer.WriteLine($"Exception Type: {exception.GetType()}");
                writer.WriteLine($"Exception Message: {exception.Message}");
                writer.WriteLine($"Exception comments: {comments}");
                writer.WriteLine($"StackTrace: {exception.StackTrace}");
                writer.WriteLine(new string('-', 50)); // Separator
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions related to logging itself
            ConfigManager.WriteConsoleException($"Error writing to log file: {path}", ex);
        }
    }

    public static CabinetInformation fromYaml(string cabPath, bool cache = true)
    {
        if (cache && CabinetInformationCache.Contains(cabPath))
        {
            // ConfigManager.WriteConsole($"[CabinetInformation]: cached: {cabPath}");
            return CabinetInformationCache.GetFromCache(cabPath);
        }
        string yamlPath = Path.Combine(cabPath, "description.yaml");
        // ConfigManager.WriteConsole($"[CabinetInformation]: load from Yaml: {yamlPath}");
        string yaml = yamlFileToString(yamlPath);
        CabinetInformation cabInfo = parseYaml(cabPath, yamlPath, yaml);
        if (cache)
            CabinetInformationCache.AddToCache(cabPath, cabInfo);
        return cabInfo;
    }

    private static CabinetInformation parseYaml(string cabPath, string yamlPath, string yaml)
    {
        try
        {
            //ConfigManager.WriteConsole($"[CabinetInformation]: {yamlPath} \n {yaml}");
            var cabInfo = deserializer.Deserialize<CabinetInformation>(yaml);
            if (cabInfo == null)
                throw new CabinetValidationException(cabPath, "Deserialization error");

            cabInfo.pathBase = cabPath;

            cabInfo.Validate();

            return cabInfo;
        }
        catch (Exception e)
        {
            //ConfigManager.WriteConsoleException($"[CabinetInformation.fromYaml] Description YAML file in cabinet {yamlPath} ", e);
            WriteExceptionLog(yamlPath, e, "ERROR when decoding yaml file, syntax or semantic error");
            throw;
        }
    }

    private static string yamlFileToString(string yamlPath)
    {
        string yaml = null;

        if (!File.Exists(yamlPath))
        {
            ConfigManager.WriteConsoleError($"[CabinetInformation]: Description YAML file (description.yaml) doesn't exists in cabinet folder: {yamlPath}");
            return null;
        }
        try
        {
            StreamReader input = File.OpenText(yamlPath);
            yaml = input.ReadToEnd();
            input.Close();
        }
        catch (Exception e)
        {
            //ConfigManager.WriteConsoleException($"[CabinetInformation.fromYaml] YAML file {yamlPath} ", e);
            WriteExceptionLog(yamlPath, e, "ERROR trying to open the yaml file from disk");
            throw;
        }

        return yaml;
    }

    public class Model
    {
        public string style = ""; //can refer to other cabinet. Set the cabinet Name here. Empty means actual 'file' model in pack
        public string file = ""; //empty falls to main 'style' cabinet
    }
    public class Rotation
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
    }
    public class Ratio
    {
        public float x = 1f;
        public float y = 1f;
        public float z = 1f;
    }

    public class Geometry
    {
        public Rotation rotation = new Rotation();
        public Ratio ratio = new Ratio();
        // 100% maintain the same scale
        // 50% half. 200% double.
        public float scalepercentage = 100;
    }

    public class Art
    {
        public string file;
        public bool invertx = false;
        public bool inverty = false;

        public System.Exception validate(string pathBase)
        {
            string filePath = $"{pathBase}/{file}";
            if (!File.Exists(filePath))
            {
                return new System.IO.FileNotFoundException($"{filePath}");
            }

            return null;
        }
    }
    public class Marquee
    {
        [YamlMember(Alias = "illumination-type", ApplyNamingConventions = false)]
        public string illuminationType = "one-lamp";
        public RGBColor color;
    }

    public class Emission
    {
        public bool emissive = false;
        public RGBColor color;
        public Art art;
    }
    public class Part
    {
        public string name;
        public string material;
        public string normal;
        public Art art;
        public RGBColor color;
        public int transparency = 0;
        public Emission emission;
        public bool visible = true;
        public static List<string> Types = new List<string>() { "normal", "bezel", "marquee", "blocker" };
        public string type = Types[0];
        [YamlMember(Alias = "sub-type", ApplyNamingConventions = false)]
        public string subType = "default";
        public Geometry geometry = new Geometry();
        public Marquee marquee = new Marquee();
        public bool istarget = false; //lightgun
        public Physical physical;

        [YamlMember(Alias = "material-properties", ApplyNamingConventions = false)]
        public Dictionary<string, string> properties = new Dictionary<string, string>();

        // Add audio property
        public CabinetAudioPart speaker;
    }

    [Serializable]
    public class Physical
    {
        [Serializable]
        public class Constraints
        {
            public bool? x, y, z;
        }
        [Serializable]
        //https://docs.unity3d.com/Manual/class-PhysicMaterial.html
        public class Material
        {
            [YamlMember(Alias = "dynamic-friction", ApplyNamingConventions = false)]
            public float dynamicFriction = 0.6f;
            [YamlMember(Alias = "static-friction", ApplyNamingConventions = false)]
            public float staticFriction = 0.6f;
            public float bounciness = 0;
            [YamlMember(Alias = "friction-combine", ApplyNamingConventions = false)]
            public string frictionCombine = "average";
            [YamlMember(Alias = "bounce-combine", ApplyNamingConventions = false)]
            public string bounceCombine = "average";
            public static List<string> combineTypes = new List<string>() { "average", "minimum", "multiply", "maximum" };


            //https://docs.unity3d.com/ScriptReference/PhysicMaterialCombine.html
            //https://docs.unity3d.com/Manual/collider-surfaces-combine.html
            public static PhysicMaterialCombine CombineFromString(string combine)
            {
                switch (combine.ToLower())
                {
                    case "average":
                        return PhysicMaterialCombine.Average;
                    case "minimum":
                        return PhysicMaterialCombine.Minimum;
                    case "multiply":
                        return PhysicMaterialCombine.Multiply;
                    case "maximum":
                        return PhysicMaterialCombine.Maximum;
                    default:
                        return PhysicMaterialCombine.Average;
                }
            }

            public System.ArgumentException IsValid()
            {
                if (!combineTypes.Contains(bounceCombine) || !combineTypes.Contains(frictionCombine))
                    return new System.ArgumentException("Material bounce-combine or friction-combine invalid value");
                return null;
            }
        }

        [Serializable]
        public class ReceiveImpacts
        {
            [Serializable]
            public class Repulsion
            {
                public float force = 1f;
            }

            public List<string> parts = new(); //list of parts to collide.
            public Repulsion repulsion = new Repulsion();
        }

        [Serializable]
        public class PlayerInteraction
        {
            [YamlMember(Alias = "can-grab", ApplyNamingConventions = false)]
            public bool isGrababble = false;
            [YamlMember(Alias = "can-touch", ApplyNamingConventions = false)]
            public bool isTouchable = true;
        }

        public Material material;

        [YamlMember(Alias = "player-interaction", ApplyNamingConventions = false)]
        public PlayerInteraction playerInteraction;  //to be touched grabed by the user.

        //shape: sphere, box, capsule
        public string shape = "box";
        public static List<string> shapeTypes = new List<string>() { "box", "sphere", "capsule" };

        [YamlMember(Alias = "receive-impacts", ApplyNamingConventions = false)]
        public ReceiveImpacts receiveImpacts;

        //When enabled, the object will be affected by Unity's global gravity setting. The object will fall unless supported by another object (e.g., a floor).
        public bool gravity = false;
        //This defines the weight of the object in the physics system. Higher mass makes the object harder to move, while a lower mass makes it easier to move.
        public float mass = 1.0f;
        [YamlMember(Alias = "freeze-position", ApplyNamingConventions = false)]
        public Constraints freezePosition = new Constraints();
        [YamlMember(Alias = "freeze-rotation", ApplyNamingConventions = false)]
        public Constraints freezeRotation = new Constraints();


        public System.ArgumentException IsValid()
        {
            if (!shapeTypes.Contains(shape))
                return new System.ArgumentException($"Incorrect shape, use only: {string.Join(",", shapeTypes)}");
            return null;
        }

    }

    public class CabinetAudioPart
    {
        public string file;
        public float volume = 1.0f;
        public bool loop = false;

        public Distance distance = new Distance();

        public class Distance
        {
            public float min = 1.0f;
            public float max = 5.0f;
        }
    }

    public class CabinetInputDevice
    {
        public uint slot;
        public string type;
    }

    public class CRT
    {
        public string type = "19i";
        public string orientation = "vertical";
        public Screen screen = new Screen();
        public Geometry geometry = new Geometry();
        public string name;

        public System.Exception validate(List<string> crtTypes)
        {
            if (!crtTypes.Contains(type))
                return new System.ArgumentException($"{type} is not a known CRT type");

            if (orientation != "vertical" && orientation != "horizontal")
                return new System.ArgumentException($"{orientation} Position must be 'vertical' or 'horizontal' (lower case)");

            return screen.validate();
        }
    }

    public class Screen
    {
        public string shader;
        public string damage = "none";
        public bool invertx = false;
        public bool inverty = false;
        public string gamma = "1.0"; // LibretroMameCore.DefaultGamma;
        public string brightness = "1.0"; // LibretroMameCore.DefaultBrightness;
        public Dictionary<string, string> properties = new Dictionary<string, string>();

        public Dictionary<string, string> config()
        {
            //Dictionary<string, string> dic = new Dictionary<string, string>();
            properties["damage"] = damage;
            return properties;
        }

        public System.ArgumentException validate()
        {
            if (!ShaderScreen.Exists(shader))
            {
                return new System.ArgumentException($"Erroneous Shader {shader}");
            }

            return null;
        }

    }

    public class Video
    {
        public string file;
        public bool invertx = false;
        public bool inverty = false;
        public Screen screen;
        [YamlMember(Alias = "max-player-distance", ApplyNamingConventions = false)]
        public float MaxPlayerDistance = 3f;
    }

    public class Audio
    {
        public string file;

        [YamlMember(Alias = "max-player-distance", ApplyNamingConventions = false)]
        public float MaxPlayerDistance = 6f;
    }

    public class RGBColor
    {
        public byte r = 255;
        public byte g = 255;
        public byte b = 255;
        public byte a = 255;

        public float intensity = 0;

        /*
        public virtual System.Exception checkForProblems() {
            if (
                r < 0 || r > 255 || 
                g < 0 || g > 255 || 
                b < 0 || b > 255 
            ) {
                return new System.Exception("Each value in a RGB color must be between zero and 255");
            }
            return null;
        }
        */
        public virtual Color getColor()
        {
            float factor = Mathf.Pow(2, intensity);
            Color c = new Color32(r, g, b, a);
            Color cf = new Color(c.r * factor, c.g * factor, c.b * factor, a);
            // LibretroMameCore.WriteConsole($" marquee color: {cf}");
            return cf.linear;
        }
        public virtual Color getColorNoIntensity()
        {
            Color color = new Color32(r, g, b, a);
            return color.linear;
        }
    }

    public static class MameFileType
    {
        private static readonly Dictionary<string, string> FileTypes = new Dictionary<string, string>
            {
                { "config", ConfigManager.MameConfigDir },
                { "disk-image", ConfigManager.RomsDir },
                { "sample", ConfigManager.SamplesDir },
                { "music", ConfigManager.MusicDir },
                { "nvram", ConfigManager.nvramDir }
            };


        // Method to verify if the key is in the dictionary
        public static bool IsValid(string fileType)
        {
            return FileTypes.ContainsKey(fileType);
        }

        // Method to get the path associated with a given key
        public static string GetAndCreatePath(string fileType, string romPath = "")
        {
            if (!FileTypes.TryGetValue(fileType, out string path))
                throw new Exception("unknown file type:" + fileType);

            //special case:
            if (fileType == "disk-image")
            {
                if (String.IsNullOrEmpty(romPath))
                    throw new Exception("you should provide a rom name to save disk images (chd) files");

                string romName = Path.GetFileNameWithoutExtension(romPath);
                path = Path.Combine(ConfigManager.RomsDir, romName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            return path;
        }
    }

    public class MameFile
    {
        public string type;
        public string file; //file name

        public bool IsValid(string pathBase)
        {
            return File.Exists(Path.Combine(pathBase, file))
                        && MameFileType.IsValid(type);
        }
    }

    public string getPath(string file)
    {
        if (String.IsNullOrEmpty(file))
            return null;
        return Path.Combine(pathBase, file);
    }

    public bool fileExists(string filename)
    {
        string path = Path.Combine(pathBase, filename);
        return File.Exists(path);
    }

    public Dictionary<string, System.Exception> checkForProblems(List<string> materialListNames,
        List<string> cabinetPartNames,
        List<string> cabinetStyles,
        List<string> coinSlots,
        List<string> crtTypes
    )
    {
        Dictionary<string, System.Exception> exceptions = new Dictionary<string, System.Exception>();
        exceptions.Add("Cabinet",
            string.IsNullOrEmpty(name) ? new System.Exception("Cabinet doesn't have a name") : null);
        int number = 1;
        if (Parts != null)
        {
            foreach (Part p in Parts)
            {
                if (string.IsNullOrEmpty(name))
                    exceptions.Add($"Part #{number}", new System.Exception($"Doesn't have a name"));

                exceptions.Add($"Part #{number}: {p.name} ART", p.art != null ? p.art.validate(pathBase) : null);
                exceptions.Add($"Part #{number}: {p.name} TYPE", !Part.Types.Contains(p.type) ?
                        new System.Exception($"Unknown part type {p.type}")
                        : null);
                exceptions.Add($"Part #{number}: {p.name} MATERIAL",
                    !string.IsNullOrEmpty(p.material) && !materialListNames.Contains(p.material)
                        ? new System.Exception($"Unknown material: {p.material}")
                        : null);
                if (p.transparency != 0)
                    exceptions.Add($"Part #{number}: {p.name} TRANSPARENCY",
                        p.transparency < 0 || p.transparency > 100 ?
                            new System.Exception("Transparency 0 to 100 only.")
                            : null);
                if (p.physical != null)
                {
                    exceptions.Add($"Part #{number}: {p.name} PHYSICS",
                           p.physical.material != null ? p.physical.material.IsValid() : null);
                    exceptions.Add($"Part #{number}: {p.name} PHYSICS MATERIAL", p.physical.IsValid());
                }

                number++;
            }
        }

        if (MameFiles != null)
        {
            foreach (MameFile mf in MameFiles)
            {
                if (!mf.IsValid(pathBase))
                {
                    exceptions.Add($"MAME file {mf.file} type: {mf.type}",
                        new System.Exception($"type unknown or file doesn't exists"));
                }
            }
        }

        // exceptions.Add($"Bezel ART", bezel != null && bezel.art != null? bezel.art.validate(pathBase) : null);
        // exceptions.Add($"Marquee ART", marquee != null && marquee.art != null? marquee.art.validate(pathBase) : null);
        // exceptions.Add($"Marquee Light Color", marquee != null? marquee.lightcolor.checkForProblems() : null);
        // exceptions.Add($"Year", year >= 1970 && year < 2010 ? null : new System.ArgumentException("Year out of range"));
        exceptions.Add($"Space", CabinetSpaceType.IsValidSpace(space) ? null :
                        new System.ArgumentException($"Unknown space type: {space} valids are: {CabinetSpaceType.GetValidSpaceTypes()}"));
        exceptions.Add($"Style",
            cabinetStyles.Contains(style) ? null : new System.ArgumentException($"Unknown cabinet style: {style}"));
        exceptions.Add($"Coin Slot",
            coinSlots.Contains(coinslot) ? null : new System.ArgumentException($"Unknown coin slot style: {coinslot}"));
        if (crt != null)
            exceptions.Add($"CRT", crt.validate(crtTypes));
        exceptions.Add($"CORE", CoresController.CoreExists(core) ? null :
                    new System.ArgumentException($"Unknown core: {core}"));

        if (lightGunInformation != null && lightGunInformation.active)
        {
            exceptions.Add($"lightgun", lightGunInformation.Validate(pathBase));
        }
        if (video != null && string.IsNullOrEmpty(video.file))
        {
            exceptions.Add($"video", String.IsNullOrEmpty(video.file) || !fileExists(video.file) ?
                                        new System.ArgumentException($"video undeclared or file [{video.file}] doesn't exists") :
                                        null);
        }

        if (agebasic != null)
        {
            exceptions.Add($"agebasic", (Exception)agebasic.IsValid(name));
        }

        return exceptions;
    }
    /*    private static int CountFacesRecursively(Transform parent)
        {
            int totalFaces = 0;

            // Loop through each child of the current parent
            foreach (Transform child in parent)
            {
                // Check if the child has a MeshFilter component
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.mesh != null)
                {
                    // If the child has a MeshFilter and a mesh is assigned
                    Mesh mesh = meshFilter.sharedMesh;
                    if (!mesh.isReadable)
                    {
                        // Set the mesh to be readable
                        mesh.MarkDynamic(); // Alternatively, you can use mesh.UploadMeshData(true) if MarkDynamic() is not sufficient

                        if (!mesh.isReadable)
                            mesh.UploadMeshData(true);
                        // Log a warning message
                        ConfigManager.WriteConsoleWarning("Mesh is not marked as readable. Marking the mesh as readable. Performance may be affected.");
                    }
                    // Get the triangles of the mesh
                    int[] triangles = mesh.triangles;
                    // Check if triangles array is null or empty
                    if (triangles != null || triangles.Length > 0)
                        totalFaces += triangles.Length / 3; 

                }

                // Check recursively for the children of the child
                totalFaces += CountFacesRecursively(child);
            }

            return totalFaces;
        }
        */

    private static void showCabinetProblemsLog(CabinetInformation cbInfo,
                                                Dictionary<string, System.Exception> exceptions,
                                                string moreProblems, string debugFileName = null)
    {

        string path;
        if (string.IsNullOrEmpty(debugFileName))
            path = debugLogPath(cbInfo.name);
        else
            path = debugLogPath(debugFileName);

        ConfigManager.WriteConsole($"[showCabinetProblemsLog] {path}");
        // Write exception details to the log file
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.WriteLine($"CABINET: {cbInfo.name}");
            foreach (KeyValuePair<string, System.Exception> error in exceptions)
            {
                writer.WriteLine($"{error.Key}: {(error.Value == null ? "OK" : error.Value.ToString())}");
            }
            writer.WriteLine(new string('-', 50)); // Separator
            if (!String.IsNullOrEmpty(moreProblems))
                writer.WriteLine(moreProblems);
        }
    }

    public static void showCabinetProblems(CabinetInformation cbInfo, string moreProblems = "", string outputFile = "")
    {
        //all the errors are not a problem because there are defaults for each ones and the cabinet have to be made, exist or not an error.
        ConfigManager.WriteConsole("[showCabinetProblems] Alerts and errors");
        Dictionary<string, System.Exception> exceptions = cbInfo.checkForProblems(
            new List<string>(CabinetMaterials.materialList.Keys),
            Cabinet.userStandarConfigurableParts,
            new List<string>(CabinetFactory.CabinetStyles.Keys),
            new List<string>(CoinSlotsFactory.objects.Keys),
            new List<string>(CRTsFactory.objects.Keys));

        string cabName = "unknown";
        if (cbInfo != null)
            cabName = cbInfo.name;
        foreach (KeyValuePair<string, System.Exception> error in exceptions)
        {
            ConfigManager.WriteConsole($"[showCabinetProblems] {cabName} - {error.Key}: {(error.Value == null ? "-OK-" : error.Value.ToString())}");
        }

        ConfigManager.WriteConsole($"[showCabinetProblems] {moreProblems}");
        ConfigManager.WriteConsole("[showCabinetProblems] ===================");

        if (cbInfo.debug)
        {
            showCabinetProblemsLog(cbInfo, exceptions, "ERROR: " + moreProblems, outputFile);
        }

        return;
    }
}
