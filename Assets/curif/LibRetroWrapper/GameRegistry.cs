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

  // public Game(string cabinetDBName = null, string rom = null, string room = null, int position = 0, CabinetInformation cabInfo = null)
  // {
  //   CabinetDBName = cabinetDBName;
  //   Rom = rom;
  //   Room = room;
  //   Position = position;
  //   CabInfo = cabInfo;
  // }

  // public override bool Equals(object obj) {
  //   if (obj == null) return false;
  //   Game g = obj as Game;
  //   return g.Rom = Rom && g.Position == Position && g.Room == Room && CabinetDBName = g.CabinetDBName;
  // }


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

  // List<string> UnasignedRoms;
  List<string> UnassignedCabinets;

  void Start()
  {
    //Init.OnRuntimeMethodLoad runs before


    // Scene[] rooms = SceneManager.GetAllScenes().Where(room => room.name.StartsWith("Room") && room.buildIndex != -1);
    // ConfigManager.WriteConsole($"[GameRegistry] Rooms in build: {rooms.Length} ----------------");

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
    ConfigManager.WriteConsole("[GameRegistry] ----------------");
    foreach (CabinetPosition g in cabinetsPosition.Registry)
    {
      ConfigManager.WriteConsole($"{g.Rom} asigned to: {g.Room} cab: {g.CabinetDBName} pos #{g.Position}");
    }

    // ConfigManager.WriteConsole("[GameRegistry] Unassigned roms");
    // foreach (string rom in UnasignedRoms)
    // {
    //   ConfigManager.WriteConsole($"{rom}");
    // }

    ConfigManager.WriteConsole("[GameRegistry] Unassigned cabinets");
    foreach (string cab in UnassignedCabinets)
    {
      ConfigManager.WriteConsole($"{cab}");
    }

    return this;
  }

  public GameRegistry Recover()
  {
    cabinetsPosition = CabinetsPosition.ReadFromFile();
    // UnasignedRoms = from dir in System.IO.Directory.GetDirectories(ConfigManager.RomsDir) where !Registry.Contains(new Game(rom: dir)) select dir;

    //roms that hasn't been assigned to any cabinet/Room yet.
    // UnasignedRoms = System.IO.Directory.GetFiles(ConfigManager.RomsDir).
    //                           Select(rom => System.IO.Path.GetFileName(rom)).
    //                           Where(rom => !RomInRoom(rom)).
    //                           ToList();

    //check for new cabinets not assigned to any Room
    // UnassignedCabinets = System.IO.Directory.GetDirectories(ConfigManager.CabinetsDB).
    //                           Where(cab => !CabinetInRoom(cab)).
    //                           ToList();
    UnassignedCabinets = (from path in System.IO.Directory.GetDirectories(ConfigManager.CabinetsDB)
                          let cab = CabinetDBAdmin.GetNameFromPath(path)
                          where !CabinetInRoom(cab)
                          select cab).ToList();
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

    ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] {cabs.Count} cabinets obtained from Registry for room {room}");
    while (cabs.Count < quantity && UnassignedCabinets.Count > 0)
    {
      cabs.Add(Add(cabinetDBName: UnassignedCabinets[0], room: room, position: cabs.Count));
      UnassignedCabinets.RemoveAt(0);
      dirty = true;
    }
    /*
        This is not possible, we can analyze all the cabinets to know witch room is not assigned.
        //todo: assign unnasigned roms.
        while (cabs.Count < quantity && UnasignedRoms.Count > 0)
        {
          string cabName = CabinetDBAdmin.CreateGenericForUnnasignedRom(UnasignedRoms[0]);

          //if something weird happens, is better to not continue
          if (String.IsNullOrEmpty(cabName))
            break;

          cabs.Add(Add(cabinetDBName: cabName, room: UnasignedRoms[0], position: cabs.Count));
          ConfigManager.WriteConsole($"[GetCabinetsAssignedToRoom] Created generic cabinet for {UnasignedRoms[0]} at #{cabs.Count} for room {room}");
          UnasignedRoms.RemoveAt(0);
          dirty = true;
        }
    */
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
