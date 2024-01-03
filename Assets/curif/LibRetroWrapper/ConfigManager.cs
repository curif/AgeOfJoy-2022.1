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

using UnityEngine;
using System.IO;
using System;

public static class ConfigManager
{
    //paths
#if UNITY_EDITOR
    public static string BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cabs");
#else
    public static string BaseDir = "/sdcard/Android/data/com.curif.AgeOfJoy";
#endif

    public static string Cabinets = Path.Combine(BaseDir, "cabinets"); //$"{BaseDir}/cabinets"; //compressed
    public static string CabinetsDB = Path.Combine(BaseDir,"cabinetsdb"); //uncompressed cabinets
    public static string SystemDir = Path.Combine(BaseDir,"system");
    public static string RomsDir = Path.Combine(BaseDir,"downloads");
    public static string GameSaveDir = Path.Combine(BaseDir,"save");
    public static string GameStatesDir = Path.Combine(BaseDir, "startstates");
    public static string ConfigDir = Path.Combine(BaseDir,"configuration");
    public static string ConfigControllersDir = Path.Combine(ConfigDir, "controllers");
    public static string AGEBasicDir = Path.Combine(BaseDir,"AGEBasic");

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

        if (!Directory.Exists(ConfigManager.Cabinets))
            Directory.CreateDirectory(ConfigManager.Cabinets);
        if (!Directory.Exists(ConfigManager.CabinetsDB))
            Directory.CreateDirectory(ConfigManager.CabinetsDB);
        if (!Directory.Exists(ConfigManager.ConfigDir))
            Directory.CreateDirectory(ConfigManager.ConfigDir);
        if (!Directory.Exists(ConfigManager.ConfigControllersDir))
            Directory.CreateDirectory(ConfigManager.ConfigControllersDir);
        if (!Directory.Exists(ConfigManager.AGEBasicDir))
            Directory.CreateDirectory(ConfigManager.AGEBasicDir);

        if (!Directory.Exists(ConfigManager.SystemDir))
        {
            Directory.CreateDirectory(ConfigManager.SystemDir);
            Directory.CreateDirectory(ConfigManager.RomsDir);
            Directory.CreateDirectory(ConfigManager.GameSaveDir);
        }

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
 