
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

    [Tooltip("Optional: root transform to auto-discover additional room NPCs by 'NPC' tag. " +
             "Use when some NPCs live under a different parent and are not in CharacterList.")]
    public Transform NPCsRoot;

    private List<GameObject> effectiveNPCList = new List<GameObject>();
    private bool isListenerAdded = false;

    private void Start()
    {
        buildEffectiveList();
        addListener();
    }

    void buildEffectiveList()
    {
        effectiveNPCList = new List<GameObject>(CharacterList ?? new GameObject[0]);

        // If NPCsRoot is assigned use it; otherwise fall back to this GameObject's own children.
        Transform root = NPCsRoot != null ? NPCsRoot : transform;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("NPC") && !effectiveNPCList.Contains(child.gameObject))
                effectiveNPCList.Add(child.gameObject);
        }
    }

    void OnRoomConfigChanged()
    {
        bool isActive = true;
        bool isStatic = false;
        if (roomConfiguration.Configuration?.npc != null)
        {
            isActive = roomConfiguration.Configuration.npc.status != "disabled";
            isStatic = roomConfiguration.Configuration.npc.status == "static";
        }
        foreach (GameObject npc in effectiveNPCList)
        {
            npc.SetActive(isActive);
            if (npc.activeSelf)
            {
                ArcadeRoomBehavior behavior = npc.GetComponent<ArcadeRoomBehavior>();
                if (behavior != null)
                    behavior.IsStatic = isStatic;
            }
        }
    }


    void addListener()
    {
        if (isListenerAdded) return;
        roomConfiguration?.OnRoomConfigChanged.AddListener(OnRoomConfigChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        roomConfiguration?.OnRoomConfigChanged.RemoveListener(OnRoomConfigChanged);
        isListenerAdded = false;
    }

    void OnEnable()
    {
        // Listen for the config reload message
        addListener();

    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        removeListener();
    }
}
