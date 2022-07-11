/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;
using System.IO;

public static class ConfigManager {
    //paths
    public static string BaseDir = "/sdcard/Android/data/com.curif.AgeOfJoy";
    public static string Cabinets = $"{BaseDir}/cabinets"; //compressed
    public static string CabinetsDB = $"{BaseDir}/cabinetsdb"; //uncompressed cabinets
    public static string SystemDir = $"{BaseDir}/system";
    public static string RomsDir = $"{BaseDir}/downloads";
    public static string GameSaveDir = $"{BaseDir}/save";

    public static bool GameVideosStopped = false;

    static ConfigManager() {
         
        if (!Directory.Exists(ConfigManager.Cabinets)) {
            // Directory.CreateDirectory(BaseDir);
            Directory.CreateDirectory(ConfigManager.Cabinets);
        }
        if (!Directory.Exists(ConfigManager.CabinetsDB)) {
            // Directory.CreateDirectory(BaseDir);
            Directory.CreateDirectory(ConfigManager.CabinetsDB);
        }
        if (!Directory.Exists(ConfigManager.SystemDir)) {
            Directory.CreateDirectory(ConfigManager.SystemDir);
            Directory.CreateDirectory(ConfigManager.RomsDir);
            Directory.CreateDirectory(ConfigManager.GameSaveDir);
        }

    }
    public static void WriteConsole(string st) {
        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, st);
    }

    public static void SignalToStopVideos() {
        GameVideosStopped = true;
    }
}
