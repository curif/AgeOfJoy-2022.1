/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.curif.LibRetroWrapper
{
    public class CoresController : MonoBehaviour
    {
        private static string CORE_FILE_EXTENSION = ".so";
        private static string ARCH = "android";

        private static Dictionary<string, string> Cores = new Dictionary<string, string>();

        private void Start()
        {
            AddEmbeddedCores();
            ScanForUserCores();
        }

        private void AddEmbeddedCores()
        {
            Cores.Add("mame2003+", "libmame2003_plus_libretro_android.so");
            Cores.Add("fbneo", "libfbneo_libretro_android.so");
            Cores.Add("mame2010", "libmame2010_libretro_android.so");
        }

        private void ScanForUserCores()
        {
            PurgeExistingCores();

            string[] cores = Directory.GetFiles(ConfigManager.CoresDir, "*" + CORE_FILE_EXTENSION);
            foreach (string core in cores)
            {
                string coreName = ExtractCoreName(Path.GetFileName(core));
                if (coreName != null)
                {
                    ConfigManager.WriteConsole($"[CoresController] Found core: {coreName}");
                    string internalCore = Path.Combine(ConfigManager.InternalCoresDir, Path.GetFileName(core));
                    File.Copy(core, internalCore);
                    ConfigManager.WriteConsole($"[CoresController] Copied to: {internalCore}");
                    Cores.Add(coreName, internalCore);
                }
                else
                {
                    ConfigManager.WriteConsole($"[CoresController] Invalid core name: {core}");
                }
            }
        }

        private void PurgeExistingCores()
        {
            ConfigManager.WriteConsole($"[CoresController] Purging existing cores in: {ConfigManager.InternalCoresDir}");
            string[] files = Directory.GetFiles(ConfigManager.InternalCoresDir, "*" + CORE_FILE_EXTENSION);
                foreach (string file in files)
                {
                    File.Delete(file);
                ConfigManager.WriteConsole($"[CoresController] Existing internal core file has been deleted: {file}");
                }
        }

        private string ExtractCoreName(string core)
        {
            int index = core.IndexOf("_libretro_" + ARCH + CORE_FILE_EXTENSION);
            return index > 0 ? core.Substring(0, index) : null;
        }

        public static string GetCorePath(string coreName)
        {
            return Cores.ContainsKey(coreName) ? Cores[coreName] : null;
        }

        public static bool Contains(string core)
        {
            return Cores.ContainsKey(core);
        }
    }
}
