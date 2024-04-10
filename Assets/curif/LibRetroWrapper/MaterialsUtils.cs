using UnityEngine;
using UnityEditor;
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
        int index = GetPropertyIndex(material, propertyName);
        if (index == -1)
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] SetShaderValue {material.name}: Unknown property: {propertyName}");
            return null;
        }
        Shader shader = material.shader;
        ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, index);
        object propertyValue = null;
        if (propertyType == ShaderUtil.ShaderPropertyType.Range || propertyType == ShaderUtil.ShaderPropertyType.Float)
        {
            propertyValue = material.GetFloat(propertyName);
        }
        else if (propertyType == ShaderUtil.ShaderPropertyType.Color)
        {
            propertyValue = material.GetColor(propertyName);
        }
        else if (propertyType == ShaderUtil.ShaderPropertyType.Vector)
        {
            propertyValue = material.GetVector(propertyName);
        }
        else if (propertyType == ShaderUtil.ShaderPropertyType.TexEnv)
        {
            propertyValue = material.GetTexture(propertyName);
        }
        return propertyValue;
    }

    private static void SetShaderValue(Material material, string propertyName, string propertyValue)
    {
        int index = GetPropertyIndex(material, propertyName);
        if (index == -1)
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] SetShaderValue {material.name}: Unknown property: {propertyName}");
            return;
        }
        Shader shader = material.shader;
        ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, index);
        if (propertyType == ShaderUtil.ShaderPropertyType.Range || propertyType == ShaderUtil.ShaderPropertyType.Float)
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] SetShaderValue {material.name} {propertyName}: {propertyValue}");
            material.SetFloat(propertyName, float.Parse(propertyValue, CultureInfo.InvariantCulture));
        }
        else
        {
            ConfigManager.WriteConsole($"[MaterialsUtils] SetShaderValue {material.name} {propertyName}: Unsupported operation for property type {propertyType}");
        }
    }

    private static int GetPropertyIndex(Material material, string propertyName)
    {
        Shader shader = material.shader;
        int propertyCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < propertyCount; i++)
        {
            string name = ShaderUtil.GetPropertyName(shader, i);
            if (name == propertyName)
            {
                return i;
            }
        }
        return -1;
    }

    private static MaterialsConfiguration LoadConfiguration()
    {
        return YamlUtils.ParseOptional<MaterialsConfiguration>(Path.Combine(ConfigManager.ConfigDir, "materials.yaml"));
    }

    // Used to dump all properties of a material to the console. For Dev purposes
    public static void ListShaderValues(Material material)
    {
        Shader shader = material.shader;
        int propertyCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < propertyCount; i++)
        {
            string propertyName = ShaderUtil.GetPropertyName(shader, i);
            ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, i);
            object propertyValue = GetShaderValue(material, propertyName);
            ConfigManager.WriteConsole($"[MaterialsUtils] Shader {material.name} property {propertyType} {propertyName} = {propertyValue}");
        }
    }

    // Used to generate a fresh, correct, new materials.yaml file for a given material. For Dev purposes
    public static void SaveConfiguration(Material material)
    {
        MaterialsConfiguration materialsConfiguration = new MaterialsConfiguration();
        MaterialConfiguration materialConfiguration = new MaterialConfiguration();
        materialConfiguration.name = material.name;
        Shader shader = material.shader;
        int propertyCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < propertyCount; i++)
        {
            string propertyName = ShaderUtil.GetPropertyName(shader, i);
            ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, i);
            if (propertyType == ShaderUtil.ShaderPropertyType.Range || propertyType == ShaderUtil.ShaderPropertyType.Float)
            {
                object propertyValue = GetShaderValue(material, propertyName);
                materialConfiguration.properties[propertyName] = propertyValue;
            }
        }
        materialsConfiguration.materials.Add(materialConfiguration);
        YamlUtils.Save(Path.Combine(ConfigManager.ConfigDir, "materials.yaml"), materialsConfiguration);
    }
}
