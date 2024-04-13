using UnityEngine;
using static MaterialsConfiguration;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

public class MaterialsUtils
{
    public static void ApplyConfiguration(Material material)
    {
        MaterialsConfiguration materialsConfiguration = LoadConfiguration();
        if (materialsConfiguration != null)
        {
            MaterialConfiguration materialConfiguration = materialsConfiguration.materials.Find(x => x.name == material.name);
            if (materialConfiguration == null)
            {
                ConfigManager.WriteConsole($"[MaterialsUtils] {material.name}: No configuration found");
                return;
            }
            ConfigManager.WriteConsole($"[MaterialsUtils] Applying configuration for {material.name}");
            foreach (KeyValuePair<string, object> entry in materialConfiguration.properties)
            {
                SetShaderValue(material, entry.Key, entry.Value.ToString());
            }
        }
    }

    private static object GetShaderValue(Material material, MaterialPropertyType type, string propertyName)
    {
        if (!IsPropertySupported(material, type, propertyName))
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] GetShaderValue {material.name}: Unsupported property: {propertyName}");
            return null;
        }
        if (type == MaterialPropertyType.Float)
        {
            return material.GetFloat(propertyName);
        }
        else if (type == MaterialPropertyType.Vector)
        {
            return material.GetVector(propertyName);
        }
        return null;
    }

    private static void SetShaderValue(Material material, string propertyName, string propertyValue)
    {
        if (IsPropertySupported(material, MaterialPropertyType.Float, propertyName))
        {
            material.SetFloat(propertyName, float.Parse(propertyValue, CultureInfo.InvariantCulture));
        }
        else if (IsPropertySupported(material, MaterialPropertyType.Vector, propertyName))
        {
            material.SetVector(propertyName, ParseVector(propertyValue));
        }
        else
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] SetShaderValue {material.name}: Unsupported property: {propertyName}");
        }
    }

    public static bool IsPropertySupported(Material material, MaterialPropertyType type, string propertyName)
    {
        return type != MaterialPropertyType.Float && type != MaterialPropertyType.Vector
            ? false
            : GetSupportedPropertyNames(material, type).Contains(propertyName);
    }

    public static List<string> GetSupportedPropertyNames(Material material, MaterialPropertyType type)
    {
        return new List<string>(material.GetPropertyNames(type));
    }

    private static MaterialsConfiguration LoadConfiguration()
    {
        return YamlUtils.ParseOptional<MaterialsConfiguration>(Path.Combine(ConfigManager.ConfigDir, "materials.yaml"));
    }

    // Used to dump all properties of a material to the console. For Dev purposes
    public static void ListShaderValues(Material material)
    {
        GetSupportedPropertyNames(material, MaterialPropertyType.Float).ForEach(propertyName =>
        {
            object propertyValue = GetShaderValue(material, MaterialPropertyType.Float, propertyName);
            ConfigManager.WriteConsole($"[MaterialsUtils] Shader {material.name} property {propertyName} = {propertyValue}");
        });
        GetSupportedPropertyNames(material, MaterialPropertyType.Vector).ForEach(propertyName =>
        {
            object propertyValue = ToString((Vector4)GetShaderValue(material, MaterialPropertyType.Vector, propertyName));
            ConfigManager.WriteConsole($"[MaterialsUtils] Shader {material.name} property {propertyName} = {propertyValue}");
        });
    }

    public static void ListShaderValue(MaterialPropertyType type, Material material, string propertyName)
    {
        object propertyValue = GetShaderValue(material, type, propertyName);
        ConfigManager.WriteConsole($"[MaterialsUtils] Shader {material.name} property {propertyName} = {propertyValue}");
    }

    // Used to generate a fresh, correct, new materials.yaml file for a given material. For Dev purposes
    public static void SaveConfiguration(Material material)
    {
        MaterialsConfiguration materialsConfiguration = new MaterialsConfiguration();
        MaterialConfiguration materialConfiguration = new MaterialConfiguration();
        materialConfiguration.name = material.name;

        GetSupportedPropertyNames(material, MaterialPropertyType.Float).ForEach(propertyName =>
        {
            object propertyValue = GetShaderValue(material, MaterialPropertyType.Float, propertyName);
            materialConfiguration.properties[propertyName] = propertyValue;
        });
        GetSupportedPropertyNames(material, MaterialPropertyType.Vector).ForEach(propertyName =>
        {
            object propertyValue = GetShaderValue(material, MaterialPropertyType.Vector, propertyName);
            materialConfiguration.properties[propertyName] = ToString((Vector4)propertyValue);
        });
        materialsConfiguration.materials.Add(materialConfiguration);
        YamlUtils.Save(Path.Combine(ConfigManager.ConfigDir, "materials.yaml"), materialsConfiguration);
    }

    public static Vector4 ParseVector(string vectorString)
    {
        string[] vectorValues = vectorString.Split(',');
        return new Vector4(
                       float.Parse(vectorValues[0].Trim(), CultureInfo.InvariantCulture),
                       float.Parse(vectorValues[1].Trim(), CultureInfo.InvariantCulture),
                       float.Parse(vectorValues[2].Trim(), CultureInfo.InvariantCulture),
                       float.Parse(vectorValues[3].Trim(), CultureInfo.InvariantCulture)
                                                               );
    }

    public static string ToString(Vector4 vector)
    {
        string x = vector.x.ToString(CultureInfo.InvariantCulture);
        string y = vector.y.ToString(CultureInfo.InvariantCulture);
        string z = vector.z.ToString(CultureInfo.InvariantCulture);
        string w = vector.w.ToString(CultureInfo.InvariantCulture);
        return $"{x}, {y}, {z}, {w}";
    }
}
