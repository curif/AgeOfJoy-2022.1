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
using System.IO;
using UnityEngine;

public static class ConfigManager
{
    //paths
#if UNITY_EDITOR
    public static string BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cabs");
    public static string BaseAppDir = BaseDir + "/data";
#else
    public static string Bundle = "com.curif.AgeOfJoy";
    public static string BaseAppDir = "/data/data/" + Bundle;
    public static string BaseDir = "/sdcard/Android/data/" + Bundle;
#endif

    public static string Cabinets = Path.Combine(BaseDir, "cabinets"); //$"{BaseDir}/cabinets"; //compressed
    public static string CabinetsDB = Path.Combine(BaseDir, "cabinetsdb"); //uncompressed cabinets
    public static string SystemDir = Path.Combine(BaseDir, "system");
    public static string RomsDir = Path.Combine(BaseDir, "downloads");
    public static string GameSaveDir = Path.Combine(BaseDir, "save");
    public static string GameStatesDir = Path.Combine(BaseDir, "startstates");
    public static string ConfigDir = Path.Combine(BaseDir, "configuration");
    public static string ConfigControllersDir = Path.Combine(ConfigDir, "controllers");
    public static string ConfigControllerSchemesDir = Path.Combine(ConfigDir, "controllers/schemes");
    public static string AGEBasicDir = Path.Combine(BaseDir, "AGEBasic");
    public static string DebugDir = Path.Combine(BaseDir, "debug");
    public static string SamplesDir = Path.Combine(SystemDir, "samples");
    public static string MameConfigDir = Path.Combine(GameSaveDir, "cfg");
    public static string nvramDir = Path.Combine(GameSaveDir, "nvram");
    public static string MusicDir = Path.Combine(BaseDir, "music");
    public static string CoresDir = Path.Combine(BaseDir, "cores");

    public static string InternalCoresDir = Path.Combine(BaseAppDir, "usercores");

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

    static ConfigManager()
    {
        Debug.Log($"[ConfigManager] BaseDir {BaseDir}");

        CreateDirectory(ConfigManager.Cabinets);
        CreateDirectory(ConfigManager.CabinetsDB);
        CreateDirectory(ConfigManager.ConfigDir);
        CreateDirectory(ConfigManager.ConfigControllersDir);
        CreateDirectory(ConfigManager.ConfigControllerSchemesDir);
        CreateDirectory(ConfigManager.AGEBasicDir);
        CreateDirectory(ConfigManager.DebugDir);
        CreateDirectory(ConfigManager.MusicDir);
        CreateDirectory(ConfigManager.CoresDir);
        CreateDirectory(ConfigManager.InternalCoresDir);

        CreateDirectory(ConfigManager.SystemDir);
        CreateDirectory(ConfigManager.RomsDir);
        CreateDirectory(ConfigManager.GameSaveDir);
        CreateDirectory(ConfigManager.SamplesDir);
        CreateDirectory(ConfigManager.MameConfigDir);
        CreateDirectory(ConfigManager.nvramDir);

#if UNITY_EDITOR
        CreateDirectory(BaseAppDir);
#endif
    }

    public static void CreateDirectory(string path)
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
