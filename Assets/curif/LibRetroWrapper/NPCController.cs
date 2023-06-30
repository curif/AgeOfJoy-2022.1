
/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Tooltip("Room configuration object")]
    public RoomConfiguration roomConfiguration;

    [Tooltip("List of controlled Characters (NPCs)")]
    public GameObject[] CharacterList;

    void OnRoomConfigChanged()
    {
        bool isActive = true;
        bool isStatic = false;
        if (roomConfiguration.Configuration?.npc != null)
        {
            isActive = roomConfiguration.Configuration.npc.status != "disabled";
            isStatic = roomConfiguration.Configuration.npc.status == "static";
        }
        foreach (GameObject npc in CharacterList)
        {
            npc.SetActive(isActive);
            if (npc.activeSelf)
                npc.GetComponent<ArcadeRoomBehavior>().IsStatic = isStatic;
        }
    }

    void OnEnable()
    {
        // Listen for the config reload message
        roomConfiguration?.OnRoomConfigChanged.AddListener(OnRoomConfigChanged);

    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        roomConfiguration?.OnRoomConfigChanged.RemoveListener(OnRoomConfigChanged);
    }
}
