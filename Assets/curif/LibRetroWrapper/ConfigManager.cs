/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;
using System.IO;
using System;

public static class ConfigManager
{
  //paths
#if UNITY_EDITOR
  //public static string BaseDir = Environment.GetEnvironmentVariable("HOME") + "/cabs";
  public static string BaseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+ "/cabs";
#else
  public static string BaseDir = "/sdcard/Android/data/com.curif.AgeOfJoy";
#endif
  public static string Cabinets = $"{BaseDir}/cabinets"; //compressed
  public static string CabinetsDB = $"{BaseDir}/cabinetsdb"; //uncompressed cabinets
  public static string SystemDir = $"{BaseDir}/system";
  public static string RomsDir = $"{BaseDir}/downloads";
  public static string GameSaveDir = $"{BaseDir}/save";
  public static string GameStatesDir = $"{BaseDir}/startstates";
  public static string ConfigDir = $"{BaseDir}/configuration";
  public static string ConfigControllersDir = $"{BaseDir}/configuration/controllers";

  public static bool GameVideosStopped = false;

  public static ConfigInformation configuration;


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

    if (!Directory.Exists(ConfigManager.SystemDir))
    {
      Directory.CreateDirectory(ConfigManager.SystemDir);
      Directory.CreateDirectory(ConfigManager.RomsDir);
      Directory.CreateDirectory(ConfigManager.GameSaveDir);
    }

  }
  public static void WriteConsole(string st)
  {
    string formattedMessage = $"[AGE] {st}";
    UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", formattedMessage);
  }
  public static void WriteConsoleError(string st)
  {
    string formattedMessage = $"[AGE ERROR] {st}";
    UnityEngine.Debug.LogFormat(LogType.Error, LogOption.None, null, "{0}", formattedMessage);
  }
  public static void WriteConsoleWarning(string st)
  {
    string formattedMessage = $"[AGE WARNING] {st}";
    UnityEngine.Debug.LogFormat(LogType.Warning, LogOption.None, null, "{0}", formattedMessage);
  }
  public static void WriteConsoleException(string st, Exception e)
  {
    string formattedMessage = $"[AGE ERROR EXCEPTION] {st}";
    UnityEngine.Debug.LogFormat(LogType.Exception, LogOption.None, null, "{0} Exception {1} StackTrace: \n {2}", formattedMessage, e, e.StackTrace);
  }

  public static void SignalToStopVideos()
  {
    GameVideosStopped = true;
  }
}


