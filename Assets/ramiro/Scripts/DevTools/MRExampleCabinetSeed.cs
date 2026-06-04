/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Copies <see cref="ProjectRelativeDir"/> (ExempleYamlCabinet) into ConfigManager.CabinetsDB for TestUI / MR dev.
/// </summary>
public static class MRExampleCabinetSeed
{
    const string LogPrefix = "[MRExampleCabinetSeed]";
    public const string ProjectRelativeDir = "curif/MixedReality/ExempleYamlCabinet";

    public static bool TryInstallFromProject(bool overwriteYaml = true)
    {
        string sourceDir = Path.Combine(Application.dataPath, ProjectRelativeDir);
        string yamlSource = Path.Combine(sourceDir, "description.yaml");
        if (!File.Exists(yamlSource))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} missing {yamlSource}");
            return false;
        }

        string cabinetName = ReadCabinetNameFromYaml(yamlSource);
        if (string.IsNullOrEmpty(cabinetName))
            cabinetName = "umk3";

        ConfigManager.CreateFolder(ConfigManager.BaseDir);
        ConfigManager.CreateFolder(ConfigManager.CabinetsDB);

        string destDir = Path.Combine(ConfigManager.CabinetsDB, cabinetName);
        ConfigManager.CreateFolder(destDir);

        string destYaml = Path.Combine(destDir, "description.yaml");
        if (File.Exists(destYaml) && !overwriteYaml)
        {
            ConfigManager.WriteConsole($"{LogPrefix} already present: {destYaml}");
            return true;
        }

        File.Copy(yamlSource, destYaml, overwrite: true);
        CopyOptionalAssets(sourceDir, destDir);

        ConfigManager.WriteConsole(
            $"{LogPrefix} installed example cabinet '{cabinetName}' → {destDir} (add umk3.glb, rom, art files there for full spawn)");
        return true;
    }

    static void CopyOptionalAssets(string sourceDir, string destDir)
    {
        if (!Directory.Exists(sourceDir))
            return;

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string name = Path.GetFileName(file);
            if (string.Equals(name, "description.yaml", System.StringComparison.OrdinalIgnoreCase))
                continue;
            string dest = Path.Combine(destDir, name);
            File.Copy(file, dest, overwrite: true);
        }
    }

    static string ReadCabinetNameFromYaml(string yamlPath)
    {
        try
        {
            string yaml = File.ReadAllText(yamlPath);
            Match match = Regex.Match(yaml, @"^\s*name:\s*(\S+)\s*$", RegexOptions.Multiline);
            if (match.Success)
                return match.Groups[1].Value.Trim();
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} read yaml {yamlPath}", e);
        }

        return null;
    }
}
