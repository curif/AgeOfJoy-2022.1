/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Prepares cabinetsdb for MR catalog (same disk pipeline as Init + CabinetDBAdmin).
/// TestUI does not run FixedScene/Init; this mirrors zip extract + folder scan.
/// </summary>
public static class MRCatalogBootstrap
{
    const string LogPrefix = "[MRCatalogBootstrap]";

    /// <summary>InitFolders, extract zips from cabinets/, reload GameRegistry, optional example seed.</summary>
    public static void PrepareCatalog(bool seedExampleInEditor)
    {
        ConfigManager.InitFolders();

        int extracted = SyncExtractCabinetZips();
        if (extracted > 0)
            ConfigManager.WriteConsole($"{LogPrefix} extracted {extracted} cabinet zip(s) from {ConfigManager.Cabinets}");

        GameRegistry.ReloadCabinetDirectoriesFromDisk();

#if UNITY_EDITOR
        if (seedExampleInEditor && GameRegistry.cabinetDirectories.Length == 0)
        {
            if (MRExampleCabinetSeed.TryInstallFromProject())
                GameRegistry.ReloadCabinetDirectoriesFromDisk();
        }
#endif

        int count = GameRegistry.cabinetDirectories != null ? GameRegistry.cabinetDirectories.Length : 0;
        ConfigManager.WriteConsole(
            $"{LogPrefix} cabinetsdb={ConfigManager.CabinetsDB} catalog={count} (zips in {ConfigManager.Cabinets})");
    }

    /// <summary>Runs CabinetDBAdmin.loadCabinets() if present (async unzip), then callback.</summary>
    public static IEnumerator PrepareCatalogWithCabinetDbAdmin(MonoBehaviour host, bool seedExampleInEditor)
    {
        ConfigManager.InitFolders();

        CabinetDBAdmin admin = Object.FindObjectOfType<CabinetDBAdmin>();
        if (admin != null)
        {
            ConfigManager.WriteConsole($"{LogPrefix} waiting for CabinetDBAdmin.loadCabinets...");
            admin.loadCabinets();
            yield return WaitForCabinetDbAdminIdle(admin, 120f);
        }
        else
        {
            int extracted = SyncExtractCabinetZips();
            ConfigManager.WriteConsole($"{LogPrefix} no CabinetDBAdmin in scene — sync extracted {extracted} zip(s)");
        }

        GameRegistry.ReloadCabinetDirectoriesFromDisk();

#if UNITY_EDITOR
        if (seedExampleInEditor && GameRegistry.cabinetDirectories.Length == 0)
        {
            if (MRExampleCabinetSeed.TryInstallFromProject())
                GameRegistry.ReloadCabinetDirectoriesFromDisk();
        }
#endif

        int count = GameRegistry.cabinetDirectories != null ? GameRegistry.cabinetDirectories.Length : 0;
        ConfigManager.WriteConsole($"{LogPrefix} catalog ready: {count} cabinet(s)");
    }

    static IEnumerator WaitForCabinetDbAdminIdle(CabinetDBAdmin admin, float timeoutSeconds)
    {
        float elapsed = 0f;
        while (elapsed < timeoutSeconds)
        {
            if (admin == null)
                yield break;

            if (!IsCabinetDbAdminLoading(admin))
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        ConfigManager.WriteConsoleWarning($"{LogPrefix} CabinetDBAdmin load timeout ({timeoutSeconds}s)");
    }

    static bool IsCabinetDbAdminLoading(CabinetDBAdmin admin)
    {
        var field = typeof(CabinetDBAdmin).GetField(
            "loadCabsCoroutine",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field == null)
            return false;

        return field.GetValue(admin) != null;
    }

    public static int SyncExtractCabinetZips()
    {
        ConfigManager.CreateFolder(ConfigManager.Cabinets);
        ConfigManager.CreateFolder(ConfigManager.CabinetsDB);

        if (!Directory.Exists(ConfigManager.Cabinets))
            return 0;

        string[] zips = Directory.GetFiles(ConfigManager.Cabinets, "*.zip");
        if (zips.Length == 0)
            return 0;

        var host = new GameObject("MRCatalogBootstrap_ZipExtract");
        CabinetDBAdmin admin = host.AddComponent<CabinetDBAdmin>();
        int extracted = 0;

        try
        {
            foreach (string zipPath in zips)
            {
                if (!CabinetDBAdmin.ZipFileContainsDescriptionYaml(zipPath))
                    continue;

                string cabName = CabinetDBAdmin.GetNameFromPath(zipPath);
                string destDir = Path.Combine(ConfigManager.CabinetsDB, cabName);
                if (Directory.Exists(destDir) && File.Exists(Path.Combine(destDir, "description.yaml")))
                    continue;

                string result = admin.loadCabinetFromZip(zipPath);
                if (!string.IsNullOrEmpty(result))
                    extracted++;
            }
        }
        finally
        {
            Object.Destroy(host);
        }

        GameRegistry.ReloadCabinetDirectoriesFromDisk();
        return extracted;
    }
}
