/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;
using System.Linq.Expressions;


public class CabinetInformation
{
    public string name;
    public string rom;
    public List<Part> Parts { get; set; }
    public CRT crt = new CRT();
    public string style = "galaga";
    public Model model = new Model();
    public string material;
    public RGBColor color;
    public int year;
    public string coinslot = "coin-slot-double";
    public Geometry coinslotgeometry = new Geometry();
    public int timetoload = 3;
    public bool enablesavestate = false; //false to fix #34
    public string statefile = "state.nv";
    public Video video = new Video();
    public string md5sum;

    [YamlMember(Alias = "controllers", ApplyNamingConventions = false)]
    public ControlMapConfiguration ControlMap;

    [YamlMember(Alias = "light-gun", ApplyNamingConventions = false)]
    public LightGunInformation lightGunInformation = new();

    public CabinetAGEBasicInformation agebasic = new();

    [YamlMember(Alias = "debug-mode", ApplyNamingConventions = false)]
    public bool debug = false;

    [YamlIgnore]
    public string pathBase;

    private static IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

    public CabinetInformation() { }

    public static CabinetInformation fromName(string cabName)
    {
        return CabinetInformation.fromYaml(ConfigManager.CabinetsDB + "/" + cabName);
    }

    private static string debugLogPath(string cabinetName)
    {
        string filename = cabinetName + ".log";
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
            string debugpath = debugLogPath(cabinetName);
            ConfigManager.WriteConsole($"[WriteExceptionLog] cab:{cabinetName} {debugpath}");
            using (StreamWriter writer = new StreamWriter(debugpath, true))
            {
                writer.WriteLine($"CABINET: {cabinetName}");
                writer.WriteLine($"YAML file: {path}");
                writer.WriteLine($"{comments}");
                writer.WriteLine($"[DateTime: {DateTime.Now.ToString()}]");
                writer.WriteLine($"[Exception Type]: {exception.GetType()}");
                writer.WriteLine($"[Message]: {exception.Message}");
                writer.WriteLine($"[StackTrace]: {exception.StackTrace}");
                writer.WriteLine(new string('-', 50)); // Separator
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions related to logging itself
            ConfigManager.WriteConsoleException($"Error writing to log file: {path}", ex);
        }
    }

    public static CabinetInformation fromYaml(string cabPath)
    {
        string yamlPath = $"{cabPath}/description.yaml";
        ConfigManager.WriteConsole($"[CabinetInformation]: load from Yaml: {yamlPath}");

        if (!File.Exists(yamlPath))
        {
            ConfigManager.WriteConsoleError($"[CabinetInformation]: Description YAML file (description.yaml) doesn't exists in cabinet folder: {cabPath}");
            return null;
        }

        string yaml = "";
        try
        {
            var input = File.OpenText(yamlPath);
            yaml = input.ReadToEnd();
            input.Close();
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetInformation.fromYaml] YAML file {yamlPath} ", e);
            WriteExceptionLog(yamlPath, e, "ERROR trying to open the yaml file from disk");
        }

        try
        {
            //ConfigManager.WriteConsole($"[CabinetInformation]: {yamlPath} \n {yaml}");
            var cabInfo = deserializer.Deserialize<CabinetInformation>(yaml);
            if (cabInfo == null)
                throw new IOException();

            cabInfo.pathBase = cabPath;

            return cabInfo;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetInformation.fromYaml] Description YAML file in cabinet {yamlPath} ", e);
            WriteExceptionLog(yamlPath, e, "ERROR when decoding yaml file, syntax or semantic error");
        }
        return null;
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
    public class Geometry
    {
        public Rotation rotation = new Rotation();
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
    }
    public class Part
    {
        public string name;
        public string material;
        public Art art;
        public RGBColor color;
        public static List<string> Types = new List<string>() { "normal", "bezel", "marquee", "blocker"};
        public string type = Types[0]; 
        public Geometry geometry = new Geometry();
        public Marquee marquee = new Marquee();
    }

    public class CRT
    {
        public string type = "19i";
        public string orientation = "vertical";
        public Screen screen = new Screen();
        public Geometry geometry = new Geometry();

        public System.Exception validate(List<string> crtTypes)
        {
            if (!crtTypes.Contains(type))
            {
                return new System.ArgumentException($"{type} is not a known CRT type");
            }

            if (orientation != "vertical" && orientation != "horizontal")
            {
                return new System.ArgumentException($"{orientation} Position must be 'vertical' or 'horizontal' (lower case)");
            }

            return screen.validate();
        }
    }

    public class Screen
    {
        public string shader = "damage";
        public string damage = "low";
        public bool invertx = false;
        public bool inverty = false;
        public string gamma = LibretroMameCore.DefaultGamma;
        public string brightness = LibretroMameCore.DefaultBrightness;

        public Dictionary<string, string> config()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["damage"] = damage;
            return dic;
        }

        public System.Exception validate()
        {
            if (shader != "damage" && shader != "clean")
            {
                return new System.ArgumentException($"Erroneous Shader {shader}");
            }

            if (!LibretroMameCore.IsGammaValid(gamma))
            {
                return new System.ArgumentException($"Erroneous Gamma {gamma}");
            }

            if (!LibretroMameCore.IsBrightnessValid(brightness))
            {
                return new System.ArgumentException($"Erroneous Brightness {brightness}");
            }

            return null;
        }
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
            return cf;
        }
        public virtual Color getColorNoIntensity()
        {
            return new Color32(r, g, b, a);
        }
    }

    public class Video
    {
        public string file;
        public bool invertx = false;
        public bool inverty = false;

    }

    public string getPath(string file)
    {
        if (String.IsNullOrEmpty(file))
            return null;
        return $"{pathBase}/{file}";
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
                exceptions.Add($"Part #{number}: {p.name} TYPE", !Part.Types.Contains(p.type)?
                        new System.Exception($"Unknown part type {p.type}")
                        : null);
                exceptions.Add($"Part #{number}: {p.name} MATERIAL",
                    !string.IsNullOrEmpty(p.material) && !materialListNames.Contains(p.material)
                        ? new System.Exception($"Unknown material {p.material}")
                        : null);
                exceptions.Add($"Part #{number}: {p.name} MATERIAL/ART",
                    !string.IsNullOrEmpty(p.material) && p.art != null
                        ? new System.Exception("Can't assign a material and ART to the same part")
                        : null);

                number++;
            }
        }

        // exceptions.Add($"Bezel ART", bezel != null && bezel.art != null? bezel.art.validate(pathBase) : null);
        // exceptions.Add($"Marquee ART", marquee != null && marquee.art != null? marquee.art.validate(pathBase) : null);
        // exceptions.Add($"Marquee Light Color", marquee != null? marquee.lightcolor.checkForProblems() : null);
        exceptions.Add($"Year", year >= 1970 && year < 2010 ? null : new System.ArgumentException("Year out of range"));
        exceptions.Add($"Style",
            cabinetStyles.Contains(style) ? null : new System.ArgumentException($"Unknown cabinet style: {style}"));
        exceptions.Add($"Coin Slot",
            coinSlots.Contains(coinslot) ? null : new System.ArgumentException($"Unknown coin slot style: {coinslot}"));
        exceptions.Add($"CRT", crt.validate(crtTypes));
        if (lightGunInformation != null && lightGunInformation.active)
        {
            exceptions.Add($"lightgun", lightGunInformation.Validate(pathBase));
        }
        if (video != null)
        {
            exceptions.Add($"video", String.IsNullOrEmpty(video.file) || !fileExists(video.file)? 
                                        new System.ArgumentException($"video undeclared or file [{video.file}] doesn't exists") :
                                        null);
        }

        return exceptions;
    }

    private static void showCabinetProblemsLog(CabinetInformation cbInfo,
                                                Dictionary<string, System.Exception> exceptions)
    {

        string path = debugLogPath(cbInfo.name);
        ConfigManager.WriteConsole($"[showCabinetProblemsLog] {path}");
        // Write exception details to the log file
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine($"CABINET: {cbInfo.name}");
            foreach (KeyValuePair<string, System.Exception> error in exceptions)
            {
                writer.WriteLine($"{error.Key}: {(error.Value == null ? "OK" : error.Value.ToString())}");
            }
            writer.WriteLine(new string('-', 50)); // Separator
        }
    }

    public static void showCabinetProblems(CabinetInformation cbInfo)
    {
        //all the errors are not a problem because there are defaults for each ones and the cabinet have to be made, exist or not an error.
        ConfigManager.WriteConsole("[showCabinetProblems] Alerts and errors");
        Dictionary<string, System.Exception> exceptions = cbInfo.checkForProblems(
            new List<string>(CabinetMaterials.materialList.Keys),
            Cabinet.userStandarConfigurableParts,
            new List<string>(CabinetFactory.CabinetStyles.Keys),
            new List<string>(CoinSlotsFactory.objects.Keys),
            new List<string>(CRTsFactory.objects.Keys));
        foreach (KeyValuePair<string, System.Exception> error in exceptions)
        {
            ConfigManager.WriteConsole($"[showCabinetProblems] {cbInfo.name} - {error.Key}: {(error.Value == null ? "-OK-" : error.Value.ToString())}");
        }

        ConfigManager.WriteConsole("[showCabinetProblems] ===================");

        if (cbInfo.debug)
        {
            showCabinetProblemsLog(cbInfo, exceptions);
        }

        return;
    }
}
