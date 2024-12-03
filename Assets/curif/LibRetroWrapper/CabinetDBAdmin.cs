/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class CabinetDBAdmin : MonoBehaviour
{
    private Coroutine loadCabsCoroutine = null;
    public GameRegistry GameRegistry;

    private static void emptyDir(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        DirectoryInfo di = new DirectoryInfo(path);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }

        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
        }

        return;
    }

    
    // create a new cabinet from an unnasigned rom and return its name (not used)
    public static string CreateGenericForUnnasignedRom(string rom, string path = "", string cabinetModel = "galaga",
        bool invertx = false, bool inverty = false, string orientation = "vertical")
    {
        path = String.IsNullOrEmpty(path) ? ConfigManager.CabinetsDB : path;

        string cabName = Path.GetFileNameWithoutExtension(rom);
        string pathDest = Path.Combine(path, cabName); //$"{path}/{cabName}/";

        if (!Directory.Exists(pathDest))
            Directory.CreateDirectory(pathDest);

        var cbinfo = new CabinetInformation
        {
            rom = Path.GetFileName(rom),
            name = cabName,
            style = cabinetModel,
            timetoload = 8,
            year = 1980,
            material = "black",
            crt = new CabinetInformation.CRT()
            {
                type = "19i",
                orientation = orientation,
                screen = new CabinetInformation.Screen()
                {
                    damage = "low",
                    invertx = invertx,
                    inverty = inverty,
                }
            },
            Parts = new List<CabinetInformation.Part>()
            {
                new CabinetInformation.Part()
                {
                    name = "marquee",
                    type = "marquee",
                    art = new CabinetInformation.Art()
                    {
                        file = "marquee.png"
                    },
                    color = new CabinetInformation.RGBColor() { r = 238, g = 232, b = 176, intensity = -2 }
                }
            }
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(cbinfo);
        File.WriteAllText($"{pathDest}/description.yaml", yaml);

        return cabName;
    }
    

    private static void DecompressFile(string path, string destPath)
    {
        ConfigManager.WriteConsole($"[DecompressFile] from {path} to {destPath}");
        // using FileStream compressedFileStream = File.Open(path, FileMode.Open);
        // using FileStream outputFileStream = File.Create(destPath);
        // using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
        // decompressor.CopyTo(outputFileStream);
        ZipFile.ExtractToDirectory(path, destPath, true); //overwrite.
    }

    public static string GetNameFromPath(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    //load the contents of the zip file and move them to the database cabinet directory. Deletes the original zip file.
    public string loadCabinetFromZip(string zipPath)
    {
        string pathDest = "";
        try
        {
            string cabZipFileName = GetNameFromPath(zipPath);
            pathDest = Path.Combine(ConfigManager.CabinetsDB, cabZipFileName);
            if (!Directory.Exists(pathDest))
                Directory.CreateDirectory(pathDest);
            else
                emptyDir(pathDest);

            // Object.ZipUtility.UncompressFromZip(path, null, $"{ConfigManager.CabinetsDB}/{cabZipFileName}");
            DecompressFile(zipPath, pathDest);
            File.Delete(zipPath);

        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[loadCabinetFromZip] ERROR decompressing Cabinet {zipPath}", e);
            return "";
        }
        return pathDest;
    }


    public static bool ZipFileContainsDescriptionYaml(string zipFilePath)
    {
        using (ZipArchive zip = ZipFile.OpenRead(zipFilePath))
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                if (entry.FullName == "description.yaml")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void MoveMameFiles(CabinetInformation cbInfo)
    {
        if (cbInfo.MameFiles == null || cbInfo.MameFiles.Count == 0)
            return;
        
        string destPath = "";

        try
        {
            foreach (CabinetInformation.MameFile mf in cbInfo.MameFiles)
            {
            
                if (!CabinetInformation.MameFileType.IsValid(mf.type))
                    throw new Exception($"invalid MAME file type '{mf.type}' for file ${mf.file}");

                destPath = CabinetInformation.MameFileType.GetAndCreatePath(mf.type, cbInfo.getPath(cbInfo.rom));
                string sourceFile = cbInfo.getPath(mf.file);
                if (File.Exists(sourceFile))
                {
                    string destFilePath = Path.Combine(destPath, mf.file);
                    File.Copy(sourceFile, destFilePath, true); //overwrite
                    File.Delete(sourceFile);
                }
            }
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[MoveMameFiles] ERROR moving MAME files from {destPath}", e);
        }
    }

    // check for new zip files, decompress and storage them into the cabinet DB
    public void loadCabinets()
    {
        ConfigManager.WriteConsole($"[loadCabinets] {ConfigManager.Cabinets}");

        if (loadCabsCoroutine != null )
        {
            ConfigManager.WriteConsoleError($"[loadCabinets] coroutine already started.");
            return;
        }
        loadCabsCoroutine = StartCoroutine(loadCabinetsCoroutine());

        return;
    }

    //called from Init()
    IEnumerator loadCabinetsCoroutine()
    {
        ConfigManager.WriteConsole($"[loadCabinetsCoroutine] {ConfigManager.Cabinets}");
 
        string testfile = Path.Combine(ConfigManager.Cabinets, "ffight.zip");
        ConfigManager.WriteConsole($"[loadCabinetsCoroutine] file: {File.Exists(testfile)}");
        
        //string[] files = Directory.GetFiles(ConfigManager.Cabinets, "*.zip");

        //ConfigManager.WriteConsole($"[loadCabinetsCoroutine] cabinets found: {files.Length}");
        
        DirectoryInfo di = new DirectoryInfo(ConfigManager.Cabinets);
        FileInfo[] files = di.GetFiles("*.zip");
        ConfigManager.WriteConsole($"[loadCabinetsCoroutine] cabinets found: {files.Length}");

        foreach (FileInfo zip in files)
        {
            string cabPath = "";
            string zipFile = zip.FullName;
            ConfigManager.WriteConsole($"[loadCabinetsCoroutine] {zip.Name}");

            if (zip.Name == "test.zip")
                continue;

            bool containsZip = ZipFileContainsDescriptionYaml(zipFile);
            if (!containsZip)
            {
                ConfigManager.WriteConsoleError($"[loadCabinetsCoroutine] Cabinet {zipFile} doesn't have a desccription.yaml");
                continue;
            }
            yield return null;

            ConfigManager.WriteConsole($"[loadCabinetsCoroutine] loadCabinetFromZip {zipFile}");
            cabPath = loadCabinetFromZip(zipFile);
            GameRegistry.AddNewCabinetDirectory(Path.GetFileNameWithoutExtension(zip.Name));

            yield return new WaitForSeconds(0.1f);
            /*
            if (!String.IsNullOrEmpty(cabPath))
            {
                CabinetInformation cbInfo;
                try
                {
                    cbInfo = CabinetInformation.fromYaml(cabPath);
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"[loadCabinetsCoroutine] Cabinet {zipFile} error", e);
                    continue;
                }
                MoveMameFiles(cbInfo);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                ConfigManager.WriteConsoleError($"[loadCabinetsCoroutine] Cabinet {zip.Name} can't parse yaml file");
            }
            */
        }

        ConfigManager.WriteConsole($"[loadCabinetsCoroutine] END loading cabinets");
    }
}
