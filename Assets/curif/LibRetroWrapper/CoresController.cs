/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

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

        private static Dictionary<string, string> Cores = new Dictionary<string, string>();

        private void Start()
        {
            AddEmbeddedCores();
            SyncCores();
            ScanForUserCores();
        }

        private void AddEmbeddedCores()
        {
            Cores.Add("mame2003+", "libmame2003_plus_libretro_android.so");
            Cores.Add("fbneo", "libfbneo_libretro_android.so");
            Cores.Add("mame2010", "libmame2010_libretro_android.so");
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
            string[] cores = Directory.GetFiles(ConfigManager.InternalCoresDir, "*" + CORE_FILE_EXTENSION);
            foreach (string core in cores)
            {
                string coreName = ExtractCoreName(Path.GetFileName(core));
                if (coreName != null)
                {
                    ConfigManager.WriteConsole($"[CoresController] Using core: {coreName}");
                    Cores.Add(coreName, core);
                }
                else
                {
                    ConfigManager.WriteConsole($"[CoresController] Invalid core name: {core}");
                }
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
