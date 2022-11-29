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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class CabinetInformation
{
    public string name;
    public string rom;
    public List<Part> Parts { get; set; }

//    public string type = "TimePilot";
//    public Screen screen;
    // public Bezel bezel;
    // public Marquee marquee;
    public CRT crt;
    public string style = "galaga";
    public string material;
    public RGBColor color;
    public int year;
    public string coinslot = "coin-slot-double";
    public int timetoload = 3;
    public bool enablesavestate = true;
    public Video video = new();

    [YamlIgnore]
    public string pathBase;

    //[YamlIgnore] public List<System.Exception> problems;

    public static CabinetInformation fromName(string cabName)
    {
        return CabinetInformation.fromYaml(ConfigManager.CabinetsDB + "/" + cabName);
    }

    public static CabinetInformation fromYaml(string cabPath)
    {
        string yamlPath = $"{cabPath}/description.yaml";

        if (!File.Exists(yamlPath))
        {
            throw new System.Exception("Description YAML file (description.yaml) don't exists in cabinet subdir");
        }

        var input = File.OpenText(yamlPath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var cabInfo = deserializer.Deserialize<CabinetInformation>(input);
        cabInfo.pathBase = cabPath;

        return cabInfo;
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

    public class Part
    {
        public string name;
        public string material;
        public Art art;
        public RGBColor color;
        public string type = "normal"; // or bezel or marquee
    }

    public class CRT
    {
        public string type;
        public string orientation = "vertical";
        public Screen screen;

        public System.Exception validate(List<string> crtTypes)
        {
            if (!crtTypes.Contains(type))
            {
                return new System.ArgumentException($"{type} is not a known CRT type");
            }

            if (orientation != "vertical" && orientation != "horizontal")
            {
                return new System.ArgumentException(
                    $"{orientation} Position must be 'vertical' or 'horizontal' (lower case)");
            }

            return null;
        }
    }

    public class Screen
    {
        public string damage;
        public bool invertx = false;
        public bool inverty = false;
    }

    /*
    public class Bezel {
        public Art art;
    }
    public class Marquee {
        public Art art;
        public RGBColor lightcolor = new RGBColor();
    }
    */
    //   lightcolor:
    // r: 255
    // g: 194
    // b: 71
    // intensity: 0
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
            // Debug.Log($" marquee color: {cf}");
            return cf;
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

    public Dictionary<string, System.Exception> checkForProblems(List<string> materialListNames,
        List<string> cabinetPartNames,
        List<string> cabinetStyles,
        List<string> coinSlots,
        List<string> crtTypes
    )
    {
        Dictionary<string, System.Exception> exceptions = new();
        exceptions.Add("Cabinet",
            string.IsNullOrEmpty(name) ? new System.Exception("Cabinet doesn't have a name") : null);
        int number = 1;
        if (Parts != null)
        {
            foreach (Part p in Parts)
            {
                if (string.IsNullOrEmpty(name))
                {
                    exceptions.Add($"Part #{number}", new System.Exception($"Doesn't have a name"));
                }

                /*
                else if (! cabinetPartNames.Contains(p.name)) {
                    exceptions.Add($"Part #{number}: {p.name}", new System.Exception($"The part name is not a part of the cabinet"));
                }
                */
                exceptions.Add($"Part #{number}: {p.name} ART", p.art != null ? p.art.validate(pathBase) : null);
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

        return exceptions;
    }


    public static void showCabinetProblems(CabinetInformation cbInfo)
    {
        //all the errors are not a problem because there are defaults for each ones and the cabinet have to be made, exist or not an error.
        ConfigManager.WriteConsole("Alerts and errors");
        Dictionary<string, System.Exception> exceptions = cbInfo.checkForProblems(
            new List<string>(CabinetMaterials.materialList.Keys),
            Cabinet.userStandarConfigurableParts,
            new List<string>(CabinetFactory.CabinetStyles.Keys),
            new List<string>(CoinSlotsFactory.objects.Keys),
            new List<string>(CRTsFactory.objects.Keys));
        foreach (KeyValuePair<string, System.Exception> error in exceptions)
        {
            ConfigManager.WriteConsole($"{cbInfo.name} - {error.Key}: {(error.Value == null ? "-OK-" : error.Value)}");
        }

        ConfigManager.WriteConsole("===================");
        return;
    }
}
