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

    private static object GetShaderValue(Material material, string propertyName)
    {
        if (!IsPropertySupported(material, propertyName))
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] GetShaderValue {material.name}: Unsupported property: {propertyName}");
            return null;
        }
        return material.GetFloat(propertyName);
    }

    private static void SetShaderValue(Material material, string propertyName, string propertyValue)
    {
        if (!IsPropertySupported(material, propertyName))
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] SetShaderValue {material.name}: Unsupported property: {propertyName}");
        }
        else
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] SetShaderValue {material.name} {propertyName}: {propertyValue}");
            material.SetFloat(propertyName, float.Parse(propertyValue, CultureInfo.InvariantCulture));
        }
    }

    public static bool IsPropertySupported(Material material, string propertyName)
    {
        return GetSupportedPropertyNames(material).Contains(propertyName);
    }

    public static List<string> GetSupportedPropertyNames(Material material)
    {
        return new List<string>(material.GetPropertyNames(MaterialPropertyType.Float));
    }

    private static MaterialsConfiguration LoadConfiguration()
    {
        return YamlUtils.ParseOptional<MaterialsConfiguration>(Path.Combine(ConfigManager.ConfigDir, "materials.yaml"));
    }

    // Used to dump all properties of a material to the console. For Dev purposes
    public static void ListShaderValues(Material material)
    {
        GetSupportedPropertyNames(material).ForEach(propertyName =>
        {
            object propertyValue = GetShaderValue(material, propertyName);
            ConfigManager.WriteConsole($"[MaterialsUtils] Shader {material.name} property {propertyName} = {propertyValue}");
        });
    }

    // Used to generate a fresh, correct, new materials.yaml file for a given material. For Dev purposes
    public static void SaveConfiguration(Material material)
    {
        MaterialsConfiguration materialsConfiguration = new MaterialsConfiguration();
        MaterialConfiguration materialConfiguration = new MaterialConfiguration();
        materialConfiguration.name = material.name;

        GetSupportedPropertyNames(material).ForEach(propertyName =>
        {
            object propertyValue = GetShaderValue(material, propertyName);
            materialConfiguration.properties[propertyName] = propertyValue;
        });
        materialsConfiguration.materials.Add(materialConfiguration);
        YamlUtils.Save(Path.Combine(ConfigManager.ConfigDir, "materials.yaml"), materialsConfiguration);
    }
}
