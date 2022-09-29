using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[ExecuteInEditMode]
public class CabinetTestCreation : MonoBehaviour
{
    public string RomsPath;
    public string PathDest;

    void OnGUI()
    {
        if (GUILayout.Button("Generate Cabinets"))
        {
            ConfigManager.WriteConsole($"Generate cabinets in dir {PathDest} from {RomsPath}");
        
            if (!Directory.Exists(PathDest))
                 Directory.CreateDirectory(PathDest);
            
            string[] roms = Directory.GetFiles(RomsPath, "*.zip");
            foreach (string rom in roms)
            {
                if (File.Exists(rom)) 
                    CabinetDBAdmin.CreateGenericForUnnasignedRom(rom, PathDest);
            }
        }

    }

}
