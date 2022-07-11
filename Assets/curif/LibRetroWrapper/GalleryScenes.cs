using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Obsolete("Change strategy for GateController")]
public class Room
{
  public string Name;
  List<string> Doors = new();
  public static Dictionary<string, string> DoorAttachList = new()
  {
    { "s1", "n1" },
    { "s2", "n2" },
    { "o1", "e1" },
    { "o2", "e2" },
    { "n1", "s1" },
    { "n2", "s2" },
    { "e1", "o1" },
    { "e2", "o2" }
  };

  public GameObject GameObject;

  public Room(GameObject gameObject)
  {
    Name = gameObject.name;
    GameObject = gameObject;
    foreach (Transform child in gameObject.transform)
    {
      if (child.tag == "door")
      {
        Doors.Add(child.transform.gameObject.name);
      }
    }
  }

  public bool HasDoor(string door)
  {
    // return Array.Exists(Doors, d => d == door);
    return Doors.Contains(door);
  }

  //can this room be attached to the room passed by param?
  public bool isPosibleToAttachToDoor(string door)
  {
    return HasDoor(Room.DoorAttachList[door]);
  }


  public string InstantiateName
  {
    get
    {
      return "Room-" + Guid.NewGuid();
    }
  }
}
public class RoomNode
{
  public Room Room;
  // public Room[] attachedRooms = new();
  public GameObject Space;
  public string Name;
}

[Obsolete("Change strategy for GateController")]
public static class RoomFactory
{
  public static Room[] Rooms = {
        new Room(Resources.Load<GameObject>($"Rooms/Room001")),
        new Room(Resources.Load<GameObject>($"Rooms/Room002")),
        new Room(Resources.Load<GameObject>($"Rooms/Room003")),
        new Room(Resources.Load<GameObject>($"Rooms/Room004"))
    };

  //load the list with the Rooms who can be attached to the door.
  public static List<Room> GetAtachables(string door)
  {
    List<Room> possibleRooms = new();

    foreach (Room _room in RoomFactory.Rooms)
    {
      ConfigManager.WriteConsole($"[GetAtachables] {_room.Name}");
      if (_room.isPosibleToAttachToDoor(door))
      {
        ConfigManager.WriteConsole($"[GetAtachables] {_room.Name} is attachable");
        possibleRooms.Add(_room);
      }
    }
    ConfigManager.WriteConsole($"[GetAtachables] {possibleRooms.Count} rooms attachables to the door {door}");

    return possibleRooms;
  }

  //create a room that can be attached to the room and door in parameters
  //the Room must be instantiated previously
  public static Room Create(Room attachToRoom, string door)
  {
    Debug.Assert(attachToRoom.GameObject != null);

    List<Room> possibleRooms;
    var random = new System.Random(DateTime.Now.Second);

    GameObject goDoorToAttach = attachToRoom.GameObject.transform.Find(door).gameObject;
    if (goDoorToAttach == null)
    {
      ConfigManager.WriteConsole($"[RoomFactory.Create] ERROR GameObject door {door} not found in room {attachToRoom.Name}");
      return null;
    }

    ConfigManager.WriteConsole($"[RoomFactory.Create] obtain atachables to {attachToRoom.Name} at the door {door}");
    possibleRooms = RoomFactory.GetAtachables(door);
    if (possibleRooms.Count == 0)
    {
      ConfigManager.WriteConsole($"[RoomFactory.Create] ERROR no rooms atachables found for the door {door} in the room {attachToRoom.Name}");
      return null;
    }

    int index = random.Next(possibleRooms.Count);
    Room toCreate = possibleRooms[index];
    string newRoomName = toCreate.InstantiateName;
    ConfigManager.WriteConsole($"[RoomFactory.Create] room base: {toCreate.Name} selected for creation {newRoomName} (random index #{index}), load room...");
    toCreate.GameObject = GameObject.Instantiate<GameObject>(toCreate.GameObject, goDoorToAttach.transform.position, goDoorToAttach.transform.rotation);
    return toCreate;
  }
}

[Obsolete("Change strategy for GateController")]
public class GalleryScenes : MonoBehaviour
{
  void Start()
  {
    //test room creation
    ConfigManager.WriteConsole("test room creation");
    GameObject roomInit = GameObject.Find("RoomInit");
    Room initGallery = new Room(roomInit);

    RoomFactory.Create(initGallery, "o2");
  }

}