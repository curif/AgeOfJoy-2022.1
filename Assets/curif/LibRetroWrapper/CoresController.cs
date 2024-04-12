/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

#define DEBUG_CONFIG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Assets.curif.LibRetroWrapper
{
    public class CoresController : MonoBehaviour
    {
        private static string CORE_FILE_EXTENSION = ".so";
        private static string ARCH = "android";

        private static Dictionary<string, Core> Cores = new Dictionary<string, Core>();

        private void Start()
        {
            SyncCores();
            ScanForUserCores();
            AddEmbeddedCores();

#if DEBUG_CONFIG
            foreach (var core in Cores)
            {
                CoreEnvironment coreEnvironment = core.Value.ReadCoreEnvironment();
                ConfigManager.WriteConsole($"[CoresController] Core {core.Key} configuration: {coreEnvironment.prefix}");
                foreach (var prop in coreEnvironment.properties)
                {
                    ConfigManager.WriteConsole($"[CoresController] {prop.Key}: {prop.Value}");
                }
            }
#endif
        }

        private void AddEmbeddedCores()
        {
            AddInternalCore("mame2003+", "libmame2003_plus_libretro_android.so", Mame2003PlusConfig());
            AddInternalCore("mame2010", "libmame2010_libretro_android.so", Mame2010Config());
            AddInternalCore("fbneo", "libfbneo_libretro_android.so", FbNeoConfig());
        }

        public static CoreEnvironment Mame2003PlusConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("skip_disclaimer", "enabled");
            properties.Add("skip_warnings", "enabled");
            properties.Add("mame_remapping", "enabled");
            properties.Add("use_samples", "enabled");
            properties.Add("vector_vector_translusency", "disabled");
            properties.Add("vector_intensity", "2.0");
            CoreEnvironment coreEnvironment = new CoreEnvironment("mame2003-plus", properties);
            return coreEnvironment;
        }

        public static CoreEnvironment Mame2010Config()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            CoreEnvironment coreEnvironment = new CoreEnvironment("mame2010", properties);
            return coreEnvironment;
        }

        public static CoreEnvironment FbNeoConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("lightgun-crosshair-emulation", "enabled");
            CoreEnvironment coreEnvironment = new CoreEnvironment("fbneo", properties);
            return coreEnvironment;
        }

        private void SyncCores()
        {
            var sourceFiles = new DirectoryInfo(ConfigManager.CoresDir).GetFiles();
            var targetFiles = new DirectoryInfo(ConfigManager.InternalCoresDir).GetFiles();
            
            var targetFilesDict = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in targetFiles)
            {
                targetFilesDict[file.Name] = file;
            }

            // Copy or update files from source to target
            foreach (var sourceFile in sourceFiles)
            {
                FileInfo targetFile;
                if (targetFilesDict.TryGetValue(sourceFile.Name, out targetFile))
                {
                    // Check if the source file is different (date or size)
                    if (sourceFile.LastWriteTimeUtc != targetFile.LastWriteTimeUtc || sourceFile.Length != targetFile.Length)
                    {
                        sourceFile.CopyTo(targetFile.FullName, true);
                        ConfigManager.WriteConsole($"[CoresController] New version of {sourceFile.Name} copied to: {ConfigManager.InternalCoresDir}");
                    }
                }
                else
                {
                    // File does not exist in target, so copy it
                    sourceFile.CopyTo(Path.Combine(ConfigManager.InternalCoresDir, sourceFile.Name), false);
                    ConfigManager.WriteConsole($"[CoresController] New file {sourceFile.Name} copied to: {ConfigManager.InternalCoresDir}");
                }
            }

            // Delete files in target that don't exist in source
            foreach (var targetFile in targetFiles)
            {
                if (!sourceFiles.Any(sf => sf.Name.Equals(targetFile.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    targetFile.Delete();
                    ConfigManager.WriteConsole($"[CoresController] Unused file: {targetFile.Name} deleted");
                }
            }
        }

        private void ScanForUserCores()
        {
            string[] coreLibs = Directory.GetFiles(ConfigManager.InternalCoresDir, "*" + CORE_FILE_EXTENSION);
            foreach (string coreLib in coreLibs)
            {
                string coreName = ExtractCoreName(Path.GetFileName(coreLib));
                if (coreName != null)
                {
                    ConfigManager.WriteConsole($"[CoresController] Adding user core: {coreName}");
                    AddUserCore(coreName, coreLib);
                }
                else
                {
                    ConfigManager.WriteConsole($"[CoresController] Invalid core name: {coreLib}");
                }
            }
        }

        private void AddUserCore(string coreName, string coreLib)
        {
            Cores.Add(coreName, new Core(coreName, coreLib));
        }

        private void AddInternalCore(string coreName, string coreLib, CoreEnvironment coreEnvironment)
        {
            if (Cores.ContainsKey(coreName))
            {
                ConfigManager.WriteConsole($"[CoresController] Internal core {coreName} upgraded as a user core");
                Cores[coreName].GlobalEnvironment = coreEnvironment;
            }
            else
            {
                ConfigManager.WriteConsole($"[CoresController] Adding internal core {coreName}");
                Cores.Add(coreName, new Core(coreName, coreLib, coreEnvironment));
            }
        }

        private string ExtractCoreName(string core)
        {
            int index = core.IndexOf("_libretro_" + ARCH + CORE_FILE_EXTENSION);
            return index > 0 ? core.Substring(0, index) : null;
        }

        public static Core GetCore(string coreName)
        {
            return CoreExists(coreName) ? Cores[coreName] : null;
        }

        public static bool CoreExists(string core)
        {
            return Cores.ContainsKey(core);
        }
    }
}
