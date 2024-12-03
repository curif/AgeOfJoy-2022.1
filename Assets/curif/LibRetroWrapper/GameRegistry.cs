/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;

///<summary> Cabinet associated to its possition on a room </summary>
[Serializable]
public class CabinetPosition //: IEquatable<Part>
{
    public string CabinetDBName; //in DB struct
    public string Rom; //mame rom
    public string Room; //place in the arcade gallery
    public int Position; //cabinet inside the Room
    [NonSerialized]
    [YamlIgnore]
    public CabinetInformation CabInfo;

    public override string ToString()
    {
        return $"cab: {CabinetDBName} Pos: {Position} Room: {Room} rom: {Rom} hasInfo: {CabInfo != null}";
    }
}

///<summary> cabinet database, contains and control the registry </sumary>
[Serializable]
public class CabinetsPosition
{
    public List<CabinetPosition> Registry = new();
    private System.Object registryLock = new object();

    
    //modifications needs to be saved.
    bool dirty = false;
    public CabinetPosition Add(CabinetPosition g)
    {
        lock (registryLock)
        {
            Registry.Add(g);
            dirty = true;
        }
        return g;
    }

    public CabinetPosition Remove(CabinetPosition g)
    {
        lock (registryLock)
        {
            Registry.Remove(g);
            dirty = true;
        }
        return g;
    }

    public bool NeedsSave()
    {
        return dirty;
    }


    public CabinetsPosition SaveAsYaml(string fileName)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml;
        lock (registryLock)
        {
            Registry.Sort((x, y) => string.Compare(x.Room, y.Room, StringComparison.OrdinalIgnoreCase) * 1000 + x.Position - y.Position);
            yaml = serializer.Serialize(this);
        }
        File.WriteAllText(fileName, yaml);
        dirty = false;

        return this;
    }
    /// <summary> loads cabinetsdb.yaml </summary> 
    public static CabinetsPosition LoadFromYaml(string fileName)
    {
        if (!File.Exists(fileName))
            return null;

        ConfigManager.WriteConsole($"[CabinetsPosition.LoadFromYaml] {fileName}");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml = File.ReadAllText(fileName);
        CabinetsPosition ret = deserializer.Deserialize<CabinetsPosition>(yaml);
        return ret;
    }
    public CabinetsPosition Persist()
    {
        //json is deprecated
        // string serializedJson = JsonUtility.ToJson(this);
        // System.IO.File.WriteAllText(JsonFile, serializedJson);
        return SaveAsYaml(ConfigManager.CabinetsDB + "/registry.yaml");
    }

    public static CabinetsPosition ReadFromFile()
    {
        
        string YamlFile = ConfigManager.CabinetsDB + "/registry.yaml";

        if (File.Exists(YamlFile))
            return LoadFromYaml(YamlFile);
        
        string JsonFile = ConfigManager.CabinetsDB + "/registry.json";

        if (File.Exists(JsonFile))
        {
            ConfigManager.WriteConsole($"[CabinetsPosition.ReadFromFile] from {JsonFile}");
            string json = File.ReadAllText(JsonFile);
            CabinetsPosition cabinetsPosition = JsonUtility.FromJson<CabinetsPosition>(json);
            cabinetsPosition.Persist();
            File.Move(JsonFile, JsonFile + ".bak");
            return cabinetsPosition;
        }

        return new CabinetsPosition();
    }
}


//https://blog.unity.com/technology/serialization-in-unity
public class GameRegistry : MonoBehaviour
{

    public CabinetsPosition cabinetsPosition;
    public List<string> UnassignedCabinets = new List<string>();
    public static string[] cabinetDirectories = Array.Empty<string>();

    static GameRegistry()
    {
        loadCabinetsFromDirectory();
    }

    public int CabinetsInRegistry
    {
        get
        {
            return cabinetsPosition.Registry.Count;
        }
    }

    void Start()
    {
        //Init.OnRuntimeMethodLoad runs before

        Recover();
        ConfigManager.WriteConsole($"[GameRegistry.Start] {cabinetsPosition.Registry.Count} cabinets from registry, {UnassignedCabinets.Count} unassigned");

    }

    private static void loadCabinetsFromDirectory()
    {
        if (!Directory.Exists(ConfigManager.CabinetsDB))
        {
            ConfigManager.WriteConsoleWarning("[GameRegistry.loadCabinetsFromDirectory] Game is initializing directories, not possible to load any cabinet. Load some cabinets and restart the game");
            return;
        }

        // Get all cabinet directories sorted in alphabetical order
        cabinetDirectories = System.IO.Directory.GetDirectories(ConfigManager.CabinetsDB)
                                                .OrderBy(path => path)
                                                .Select(path => System.IO.Path.GetFileName(path))
                                                .ToArray();
    }
    public void AddNewCabinetDirectory(string newDirectory)
    {
        if (string.IsNullOrWhiteSpace(newDirectory))
            throw new ArgumentException("Directory name cannot be null or empty.", nameof(newDirectory));

        // Check if the directory already exists (case-insensitive)
        if (!cabinetDirectories.Contains(newDirectory, StringComparer.OrdinalIgnoreCase))
        {
            // Add the new directory while maintaining sorted order
            var newDirectories = new List<string>(cabinetDirectories) { newDirectory };
            cabinetDirectories = newDirectories
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }


    public bool NeedsSave()
    {
        return cabinetsPosition.NeedsSave();
    }

    public CabinetPosition Add(string cabinetDBName = null, string rom = null,
                                string room = null, int position = 0, CabinetInformation cabInfo = null)
    {
        if (room != null)
        {
            CabinetPosition cabpos = GetCabinetPositionInRoom(position, room);
            if (cabpos != null)
                throw new Exception($"Can't add position taken in {room}-{position}");
        }

        CabinetPosition g = new();
        g.Room = room;
        g.CabinetDBName = cabinetDBName;
        g.Rom = rom;
        g.Position = position;
        g.CabInfo = cabInfo;

        return Add(g);
    }

    public CabinetPosition Add(CabinetPosition g)
    {
        return cabinetsPosition.Add(g);
    }

    public CabinetPosition Remove(CabinetPosition g)
    {
        return cabinetsPosition.Remove(g);
    }

    public void Replace(CabinetPosition g, CabinetPosition by)
    {
        if (g != null)
            Remove(g);
        Add(by);
        Persist();
    }

    public CabinetPosition GetCabinetPositionInRoom(int position, string room)
    {
        CabinetPosition cabPos = cabinetsPosition.Registry.FirstOrDefault(
            g => g.Position == position &&
            string.Equals(g.Room, room, StringComparison.OrdinalIgnoreCase)
        );
        return cabPos;
    }

    public CabinetPosition DeleteCabinetPositionInRoom(int position, string room)
    {
        CabinetPosition cabPos = GetCabinetPositionInRoom(position, room);
        if (cabPos != null)
            Remove(cabPos);
        return cabPos;
    }

    public int GetCabinetsCountInRoom(string room)
    {
        // Use LINQ to count elements that match the condition
        int count = cabinetsPosition.Registry.Count(cabPos =>
            string.Equals(cabPos.Room, room, StringComparison.OrdinalIgnoreCase));

        return count;
    }

    public int CountRegistry()
    {
        return cabinetsPosition.Registry.Count;
    }
    public int CountCabinets()
    {
        return cabinetDirectories.Length;
    }

    public GameRegistry Persist()
    {
        //save to disk
        cabinetsPosition.Persist();
        return this;
    }
    public CabinetPosition AssignOrAddCabinet(string room, int position, string cabinetDBName)
    {
        // Check if the cabinet already exists in the specified room and position
        CabinetPosition existingCabinet = cabinetsPosition.Registry.FirstOrDefault(cabPos =>
            string.Equals(cabPos.Room, room, StringComparison.OrdinalIgnoreCase) &&
            cabPos.Position == position);

        if (existingCabinet != null)
        {
            // Update the CabinetDBName of the existing cabinet
            existingCabinet.CabinetDBName = cabinetDBName;
            return existingCabinet;
        }
        else
        {
            // If the cabinet doesn't exist, create a new one and add it to the list
            CabinetPosition newCabinet = new CabinetPosition
            {
                CabinetDBName = cabinetDBName,
                Room = room,
                Position = position
            };

            cabinetsPosition.Add(newCabinet);
            DeleteUnassigned(cabinetDBName);
            return newCabinet;
        }
    }

    public bool RomInRoom(string rom)
    {
        return cabinetsPosition.Registry.Exists(game => game.Rom == rom);
    }
    public bool CabinetInRoom(string cab)
    {
        return cabinetsPosition.Registry.Exists(game => game.CabinetDBName == cab);
    }

    public GameRegistry Show()
    {
        ConfigManager.WriteConsole($"[GameRegistry] {cabinetsPosition.Registry.Count}----------------");
        foreach (CabinetPosition g in cabinetsPosition.Registry)
        {
            ConfigManager.WriteConsole($"cab: {g.CabinetDBName} (rom:{g.Rom}) asigned to: {g.Room} pos #{g.Position}");
        }

        ConfigManager.WriteConsole($"[GameRegistry] Unassigned cabinets: {UnassignedCabinets.Count}");
        return this;
    }

    public GameRegistry LoadUnnasigned()
    {
        UnassignedCabinets = (from cab in cabinetDirectories
                              where !CabinetInRoom(cab)
                              select cab).ToList();
        return this;
    }

    public GameRegistry DeleteUnassigned(string cabinetName)
    {
        if (UnassignedCabinets.Contains(cabinetName))
            UnassignedCabinets.Remove(cabinetName);

        return this;
    }

    public string GetCabinetNameByPosition(int position)
    {
        if (position < 0)
            throw new ArgumentException("Position must be a non-negative integer.");
        if (position > cabinetDirectories.Length - 1)
            throw new ArgumentException($"Position index out of range {position}: 0 - {cabinetDirectories.Length - 1}.");

        return cabinetDirectories[position];
    }

    private GameRegistry DeleteMissingCabinets()
    {
        List<string> cabsInDB = GetAllCabinetsName();
        List<CabinetPosition> cabsToDelete = cabinetsPosition.Registry
                                                .Where(cab => !cabsInDB.Contains(cab.CabinetDBName))
                                                .ToList();

        foreach (CabinetPosition cab in cabsToDelete)
        {
            ConfigManager.WriteConsole($"[GameRegistry.DeleteMissingCabinets] cabinet {cab.CabinetDBName} removed because someone deleted it from cabinetsDB");
            Remove(cab);
        }

        return this;
    }


    //get all from disk
    public List<string> GetAllCabinetsName()
    {
        return cabinetDirectories.ToList();
    }
    public List<string> GetRandomizedAllCabinetNames()
    {
        // Use LINQ to order the cabinet names randomly
        List<string> randomizedCabinetNames = cabinetDirectories.OrderBy(x => UnityEngine.Random.value).ToList();

        return randomizedCabinetNames;
    }

    public List<string> GetAllPrefixMatches(string val)
    {
        var result = new List<string>();

        foreach (string dir in cabinetDirectories)
        {
            // Special case: if val is "#", include all entries less than "0"
            if (val == "0" && string.Compare(dir, "0", StringComparison.OrdinalIgnoreCase) < 0)
            {
                result.Add(dir);
            }
            // Regular case: match entries that start with the prefix val
            else if (val != "0" && dir.StartsWith(val, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(dir);
            }
            // Early termination: if sorted order ensures no further matches
            else if (val != "0" && string.Compare(dir, val, StringComparison.OrdinalIgnoreCase) > 0)
            {
                break;
            }
        }

        return result;
    }


    public bool CabinetExists(string cabinetName)
    {
        return Array.Exists(cabinetDirectories, d => d.Equals(cabinetName, StringComparison.OrdinalIgnoreCase));
    }

    public string GetRandomUnassignedCabinetDBName()
    {
        // Return null if UnassignedCabinets is null or empty, else return a random CabinetDBName
        return UnassignedCabinets?.Count > 0 ? UnassignedCabinets[UnityEngine.Random.Range(0, UnassignedCabinets.Count)] : null;
    }


    /// <summary> 
    /// load cabinets from file and store unnasigned, also delete cabinets that doesn't exists on disk
    /// </summary>
    public GameRegistry Recover()
    {
        cabinetsPosition = CabinetsPosition.ReadFromFile();
        ConfigManager.WriteConsole($"[Gameregistry.Recover] recovered: {cabinetsPosition.Registry.Count} cabinets from registry file");
        LoadUnnasigned().DeleteMissingCabinets();
        ConfigManager.WriteConsole($"[Gameregistry.Recover] Unnasigned: {UnassignedCabinets.Count} cabinets");

        return this;
    }

    public List<string> GetRomsAssignedToRoom(string room)
    {
        if (String.IsNullOrEmpty(room))
            return new List<string>();

        List<string> roms = new List<string>(
             from cabPos in cabinetsPosition.Registry
             where string.Equals(cabPos.Room, room, StringComparison.OrdinalIgnoreCase)
             select cabPos.Rom
           ).ToList();
        return roms;
    }
    public List<string> GetCabinetsNamesAssignedToRoom(string room)
    {
        if (String.IsNullOrEmpty(room))
            return new List<string>();

        List<string> roms = new List<string>(
                            from cabPos in cabinetsPosition.Registry
                            where string.Equals(cabPos.Room, room, StringComparison.OrdinalIgnoreCase)
                            select cabPos.CabinetDBName
                        ).ToList();
        return roms;
    }
    public List<CabinetPosition> GetCabinetsAndPositionsAssignedToRoom(string room)
    {
        if (String.IsNullOrEmpty(room))
            return new List<CabinetPosition>();

        if (cabinetsPosition.Registry.Count == 0)
        {
            ConfigManager.WriteConsoleWarning("[GameRegistry.GetCabinetsAndPositionsAssignedToRoom] no cabinets in regitry");
            return new List<CabinetPosition>();
        }

        List<CabinetPosition> cabs = new List<CabinetPosition>(
                            from cabPos in cabinetsPosition.Registry
                            where string.Equals(cabPos.Room, room, StringComparison.OrdinalIgnoreCase)
                            orderby cabPos.Position
                            select cabPos
                        ).ToList();
        return cabs;
    }
    public List<int> GetFreePositions(List<CabinetPosition> cabsPosition, int quantity)
    {
        HashSet<int> occupiedPositions = new HashSet<int>(cabsPosition.Select(cab => cab.Position));
        List<int> freePositions = Enumerable.Range(0, quantity).Except(occupiedPositions).ToList();
        return freePositions;
    }

    public List<CabinetPosition> GetSetCabinetsAssignedToRoom(string room, int quantity)
    {
        if (String.IsNullOrEmpty(room))
            return null;
        room = room.ToUpper();

        Recover(); //user can modify it.

        List<CabinetPosition> cabsPosition = GetCabinetsAndPositionsAssignedToRoom(room);
        if (cabsPosition == null)
            cabsPosition = new();

        ConfigManager.WriteConsole($"[GetSetCabinetsAssignedToRoom] {cabsPosition.Count} cabinets from Registry in room {room}");

        //remove exceeded cabinets (because the position disapear from room)
        if (cabsPosition.Count > quantity)
        {
            ConfigManager.WriteConsole($"[GetSetCabinetsAssignedToRoom] Room: {room} - {cabsPosition.Count} > space in room: {quantity} there are more cabinets in the list than in the room. Developer remove some cabinets? adjusting.");
            foreach (CabinetPosition cab in cabsPosition.Where(g => g.Position >= quantity))
            {
                Remove(cab); //from registry
                ConfigManager.WriteConsole($"[GetSetCabinetsAssignedToRoom] Room: {room} - removed #{cab.Position} {cab.CabinetDBName}");
            }
            //reload
            cabsPosition = GetCabinetsAndPositionsAssignedToRoom(room);
        }
        /*
                // add cabinets in free positions:
                // LoadUnnasigned(); //done in Recover()
                if (UnassignedCabinets.Count > 0)
                {
                    List<int> freePos = GetFreePositions(cabsPosition, quantity);
                    ConfigManager.WriteConsole($"[GetSetCabinetsAssignedToRoom] there are {freePos.Count} positions free in {room}");
                    foreach (int free in freePos)
                    {
                        cabsPosition.Add(
                                           Add(cabinetDBName: UnassignedCabinets[0],
                                               room: room,
                                               position: free
                                            )
                                        );
                        UnassignedCabinets.RemoveAt(0);
                        dirty = true;
                        if (UnassignedCabinets.Count == 0)
                            break;
                    }
                }
                if (dirty)
                    Persist();
                Show();
        */
        return cabsPosition;
    }

    public string GetUnassignedCabinet(int pos)
    {
        if (pos > UnassignedCabinets.Count() - 1)
            return null;

        return UnassignedCabinets[pos];
    }

    public List<string> GetUnassignedCabinets()
    {
        return new List<string>(UnassignedCabinets);
    }

}