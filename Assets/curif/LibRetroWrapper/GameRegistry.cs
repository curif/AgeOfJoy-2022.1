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

[Serializable]
public class CabinetPosition //: IEquatable<Part>
{
  public string CabinetDBName; //in DB struct
  public string Rom; //mame rom
  public string Room; //place in the arcade gallery
  public int Position; //cabinet inside the Room
  [NonSerialized]
  public CabinetInformation CabInfo;

}

[Serializable]
public class CabinetsPosition
{
  public List<CabinetPosition> Registry = new();

  static string file = ConfigManager.CabinetsDB + "/registry.json";

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

  public CabinetsPosition Persist()
  {
    //save to disk
    string serializedJson = JsonUtility.ToJson(this);
    System.IO.File.WriteAllText(file, serializedJson);
    // ConfigManager.WriteConsole($"[Persist] --> {serializedJson}");

    return this;
  }

  public static CabinetsPosition ReadFromFile()
  {
    if (System.IO.File.Exists(file))
    {
      string json = System.IO.File.ReadAllText(file);
      return JsonUtility.FromJson<CabinetsPosition>(json);
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
  public GameRegistry Recover()
  {
    cabinetsPosition = CabinetsPosition.ReadFromFile();
    LoadUnnasigned();
    return this;
  }

  public List<CabinetPosition> GetCabinetsAssignedToRoom(string room, int quantity)
  {
    bool dirty = false;
    List<CabinetPosition> cabs = (
                      from game in cabinetsPosition.Registry
                      where game.Room == room
                      select game).
                      ToList();
    if (cabs == null)
      cabs = new();

    ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] {cabs.Count} cabinets from Registry in room {room}");

    if (cabs.Count > quantity)
    {
      ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] Room: {room} - {cabs.Count} > space in room: {quantity} there are more cabinets in the list than in the room. Developer remove some cabinets? adjusting.");
      foreach (CabinetPosition cab in cabs.Where(g => g.Position >= quantity))
      {
        Remove(cab); //from registry
        ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] Room: {room} - removed #{cab.Position} {cab.CabinetDBName}");
      }
      cabs = (
            from game in cabinetsPosition.Registry
            where game.Room == room
            select game
      ).ToList();
      LoadUnnasigned();
      dirty = true;
    }

    while (cabs.Count < quantity && UnassignedCabinets.Count > 0)
    {
      cabs.Add(Add(cabinetDBName: UnassignedCabinets[0], room: room, position: cabs.Count));
      UnassignedCabinets.RemoveAt(0);
      dirty = true;
    }
    
    if (dirty)
    {
      Persist();
    }
    Show();

    //load cabinets information
    foreach (CabinetPosition cab in cabs.Where(g => g.CabInfo == null))
    {
      cab.CabInfo = CabinetInformation.fromName(cab.CabinetDBName);
      if (cab.CabInfo != null)
        cab.Rom = cab.CabInfo.rom;
    }

    return cabs;
  }

}
