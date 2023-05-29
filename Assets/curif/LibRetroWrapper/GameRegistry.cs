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
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;


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

}

[Serializable]
public class CabinetsPosition
{
    public List<CabinetPosition> Registry = new();

    static string JsonFile = ConfigManager.CabinetsDB + "/registry.json";
    static string YamlFile = ConfigManager.CabinetsDB + "/registry.yaml";
    public CabinetPosition Add(CabinetPosition g)
    {
        Registry.Add(g);
        return g;
    }

    public CabinetPosition Remove(CabinetPosition g)
    {
        Registry.Remove(g);
        return g;
    }

    public CabinetsPosition SaveAsYaml(string fileName)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml = serializer.Serialize(this);

        File.WriteAllText(fileName, yaml);
        return this;
    }
    public static CabinetsPosition LoadFromYaml(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }
        ConfigManager.WriteConsole($"[CabinetsPosition.LoadFromYaml] {fileName}");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml = File.ReadAllText(fileName);

        return deserializer.Deserialize<CabinetsPosition>(yaml);
    }
    public CabinetsPosition Persist()
    {
        //json is deprecated
        // string serializedJson = JsonUtility.ToJson(this);
        // System.IO.File.WriteAllText(JsonFile, serializedJson);
        return SaveAsYaml(YamlFile);
    }

    public static CabinetsPosition ReadFromFile()
    {
        if (File.Exists(JsonFile))
        {
            string json = File.ReadAllText(JsonFile);
            CabinetsPosition cabinetsPosition = JsonUtility.FromJson<CabinetsPosition>(json);
            cabinetsPosition.Persist();
            File.Move(JsonFile, JsonFile + ".bak");
            return cabinetsPosition;
        }
        else if (File.Exists(YamlFile))
        {
            return LoadFromYaml(YamlFile);
        }
        return new CabinetsPosition();
    }
}


//https://blog.unity.com/technology/serialization-in-unity
public class GameRegistry : MonoBehaviour
{

    CabinetsPosition cabinetsPosition;
    List<string> UnassignedCabinets;

    void Start()
    {
        //Init.OnRuntimeMethodLoad runs before

        Recover().Show();
    }

    public CabinetPosition Add(string cabinetDBName = null, string rom = null, string room = null, int position = 0, CabinetInformation cabInfo = null)
    {
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

    public GameRegistry Persist()
    {
        //save to disk
        cabinetsPosition.Persist();
        return this;
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
            ConfigManager.WriteConsole($"{g.Rom} asigned to: {g.Room} cab: {g.CabinetDBName} pos #{g.Position}");
        }

        ConfigManager.WriteConsole("[GameRegistry] Unassigned cabinets");
        foreach (string cab in UnassignedCabinets)
        {
            ConfigManager.WriteConsole($"{cab}");
        }
        return this;
    }

    public GameRegistry LoadUnnasigned()
    {
        UnassignedCabinets = (from path in System.IO.Directory.GetDirectories(ConfigManager.CabinetsDB)
                              let cab = CabinetDBAdmin.GetNameFromPath(path)
                              where !CabinetInRoom(cab)
                              select cab).ToList();
        return this;
    }

    public List<string> GetAllCabinetsName()
    {
        List<string> cabs = (from path in System.IO.Directory.GetDirectories(ConfigManager.CabinetsDB)
                              let cab = CabinetDBAdmin.GetNameFromPath(path)
                              select cab).ToList();
        return cabs;
    }
    public GameRegistry Recover()
    {
        cabinetsPosition = CabinetsPosition.ReadFromFile();
        LoadUnnasigned();
        return this;
    }


    public List<string> GetRomsAssignedToRoom(string room)
    {
        List<string> roms = new List<string>(
            from cabPos in cabinetsPosition.Registry
            where string.Equals(cabPos.Room, room)
            select cabPos.Rom
          ).ToList();
        return roms;
    }
    public List<string> GetCabinetsNamesAssignedToRoom(string room)
    {
        if (String.IsNullOrEmpty(room))
            return null;

        List<string> roms = new List<string>(
                            from cabPos in cabinetsPosition.Registry
                            where string.Equals(cabPos.Room, room)
                            select cabPos.CabinetDBName
                        ).ToList();
        return roms;
    }

    public List<CabinetPosition> GetCabinetsAssignedToRoom(string room, int quantity)
    {
        if (String.IsNullOrEmpty(room))
            return null;

        bool dirty = false;
        List<CabinetPosition> cabsPosition = (
                          from game in cabinetsPosition.Registry
                          where string.Equals(game.Room, room)
                          select game).
                          ToList();
        if (cabsPosition == null)
            cabsPosition = new();

        ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] {cabsPosition.Count} cabinets from Registry in room {room}");

        if (cabsPosition.Count > quantity)
        {
            ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] Room: {room} - {cabsPosition.Count} > space in room: {quantity} there are more cabinets in the list than in the room. Developer remove some cabinets? adjusting.");
            foreach (CabinetPosition cab in cabsPosition.Where(g => g.Position >= quantity))
            {
                Remove(cab); //from registry
                ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] Room: {room} - removed #{cab.Position} {cab.CabinetDBName}");
            }
            cabsPosition = (
                  from game in cabinetsPosition.Registry
                  where game.Room == room
                  select game
            ).ToList();
            LoadUnnasigned();
            dirty = true;
        }

        while (cabsPosition.Count < quantity && UnassignedCabinets.Count > 0)
        {
            cabsPosition.Add(
                            Add(cabinetDBName: UnassignedCabinets[0],
                                room: room,
                                position: cabsPosition.Count)
                                );
            UnassignedCabinets.RemoveAt(0);
            dirty = true;
        }

        if (dirty)
        {
            Persist();
        }
        Show();

        //load cabinets information
        foreach (CabinetPosition cab in cabsPosition.Where(g => g.CabInfo == null))
        {
            cab.CabInfo = CabinetInformation.fromName(cab.CabinetDBName);
            if (cab.CabInfo != null)
                cab.Rom = cab.CabInfo.rom;
        }

        return cabsPosition;
    }

}