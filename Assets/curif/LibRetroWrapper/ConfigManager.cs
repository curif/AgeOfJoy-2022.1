/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

// coment the next line for releases build
#define FORCE_DEBUG
#if UNITY_EDITOR
#define DEBUG_ACTIVE
#elif FORCE_DEBUG
#define DEBUG_ACTIVE
#endif

using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class ConfigManager
{
    //paths
#if UNITY_EDITOR
    public static string BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cabs");
    public static string BaseAppDir = BaseDir + "/data";
    public static string BasePublicDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "publiccabs");
#else
    public static string Bundle = "com.curif.AgeOfJoy";
    public static string BaseAppDir = "/data/data/" + Bundle;
    public static string BaseDir = "/sdcard/Android/data/" + Bundle;
    public static string BasePublicDir = "/storage/emulated/0/AgeOfJoy";
#endif

    public static string Cabinets;
    public static string CabinetsDB;
    public static string SystemDir;
    public static string RomsDir;
    public static string GameSaveDir;
    public static string GameStatesDir;
    public static string ConfigDir;
    public static string ConfigControllersDir;
    public static string ConfigControllerSchemesDir;
    public static string AGEBasicDir;
    public static string DebugDir;
    public static string SamplesDir;
    public static string MameConfigDir;
    public static string nvramDir;
    public static string MusicDir;
    public static string CoresDir;
    public static string InternalCoresDir;
    public static string ConfigCoresDir;

    public static ConfigInformation configuration;

    public static bool DebugActive
    {
        get
        {
#if DEBUG_ACTIVE
            return true;
#else
            return false;
#endif
        }
    }

    private static void createFolders()
    {
#if UNITY_EDITOR
        CreateFolder(BaseDir);
#endif
        RomsDir = Path.Combine(BaseDir, "downloads");
        if (!Directory.Exists(BasePublicDir) && !Directory.Exists(RomsDir))
        {
            BaseDir = BasePublicDir;
            CreateFolder(BaseDir);
        }
        else if (Directory.Exists(BasePublicDir))
        {
            BaseDir = BasePublicDir;
        }

        RomsDir = Path.Combine(BaseDir, "downloads");
        Cabinets = Path.Combine(BaseDir, "cabinets"); //$"{BaseDir}/cabinets"; //compressed
        CabinetsDB = Path.Combine(BaseDir, "cabinetsdb"); //uncompressed cabinets
        SystemDir = Path.Combine(BaseDir, "system");
        GameSaveDir = Path.Combine(BaseDir, "save");
        GameStatesDir = Path.Combine(BaseDir, "startstates");
        ConfigDir = Path.Combine(BaseDir, "configuration");
        ConfigControllersDir = Path.Combine(ConfigDir, "controllers");
        ConfigControllerSchemesDir = Path.Combine(ConfigDir, "controllers/schemes");
        AGEBasicDir = Path.Combine(BaseDir, "AGEBasic");
        DebugDir = Path.Combine(BaseDir, "debug");
        SamplesDir = Path.Combine(SystemDir, "samples");
        MameConfigDir = Path.Combine(GameSaveDir, "cfg");
        nvramDir = Path.Combine(GameSaveDir, "nvram");
        MusicDir = Path.Combine(BaseDir, "music");
        CoresDir = Path.Combine(BaseDir, "cores");
        ConfigCoresDir = Path.Combine(ConfigDir, "cores");
        InternalCoresDir = Path.Combine(BaseAppDir, "usercores");

        CreateFolder(ConfigManager.Cabinets);
        CreateFolder(ConfigManager.CabinetsDB);
        CreateFolder(ConfigManager.ConfigDir);
        CreateFolder(ConfigManager.ConfigControllersDir);
        CreateFolder(ConfigManager.ConfigControllerSchemesDir);
        CreateFolder(ConfigManager.AGEBasicDir);
        CreateFolder(ConfigManager.DebugDir);
        CreateFolder(ConfigManager.MusicDir);
        CreateFolder(ConfigManager.CoresDir);
        CreateFolder(ConfigManager.InternalCoresDir);
        CreateFolder(ConfigManager.SystemDir);
        CreateFolder(ConfigManager.RomsDir);
        CreateFolder(ConfigManager.GameSaveDir);
        CreateFolder(ConfigManager.SamplesDir);
        CreateFolder(ConfigManager.MameConfigDir);
        CreateFolder(ConfigManager.nvramDir);
        CreateFolder(ConfigManager.ConfigCoresDir);
    }

    static ConfigManager()
    {
        createFolders();
        WriteConsole($"[ConfigManager] BaseDir {BaseDir}");
    }

    public static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    /*
    It didn't works: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.conditionalattribute?view=netstandard-2.1
    fallback to #if DEBUG_ACTIVE but is less performant, because the call exists to the routine.
    */
    // [System.Diagnostics.Conditional("DEBUG_ACTIVE")]

    public static void WriteConsole(string st)
    {
#if DEBUG_ACTIVE
        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "[AGE] {0}", st);
#endif
    }
    // [System.Diagnostics.Conditional("DEBUG_ACTIVE")]
    public static void WriteConsoleError(string st)
    {
#if DEBUG_ACTIVE
        UnityEngine.Debug.LogFormat(LogType.Error, LogOption.None, null, "[AGE ERROR] {0}", st);
#endif
    }
    // [System.Diagnostics.Conditional("DEBUG_ACTIVE")]
    public static void WriteConsoleWarning(string st)
    {
#if DEBUG_ACTIVE
        UnityEngine.Debug.LogFormat(LogType.Warning, LogOption.None, null, "[AGE WARNING] {0}", st);
#endif
    }
    // [System.Diagnostics.Conditional("DEBUG_ACTIVE")]
    public static void WriteConsoleException(string st, Exception e)
    {
#if DEBUG_ACTIVE
        UnityEngine.Debug.LogFormat(LogType.Exception, LogOption.None, null,
                    "[AGE ERROR EXCEPTION] {0} Exception {1} StackTrace: \n {2}", st, e, e.StackTrace);
#endif
    }

}
