/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;



public class GameCompatibilityList : MonoBehaviour
{
    public TextAsset CompatibilityTextList; //https://docs.unity3d.com/ScriptReference/TextAsset.html

    // private const string compatibilityListTextFile = "CompatibilityList.txt"; // in Assets/StreamingAssets/CompatibilityList.txt
    private HashSet<string> compatibilityList; //https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1?view=net-6.0

  void Start()
    {
        //compatibilityList = File.ReadAllLines(compatibilityListTextFile).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
        // compatibilityList = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, compatibilityListTextFile)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
        compatibilityList = new HashSet<string>(CompatibilityTextList.text.Split('\n'));
        ConfigManager.WriteConsole($"[GameCompatibilityList] {compatibilityList.Count} roms are compatible.");
    }

    public bool IsCompatible(string rom) {
        return compatibilityList.Contains(rom);
    }
}
