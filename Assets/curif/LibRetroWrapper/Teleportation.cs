/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Eflatun.SceneReference;

public class Teleportation : MonoBehaviour
{
    [Tooltip("List of scenes to unload when teleport")]
    public SceneReference[] ScenesToUnload;

    private SceneDocument teleportTo;

    public void Teleport(SceneDocument teleportTo)
    {
        if (string.IsNullOrEmpty(teleportTo.PlayerSpawnGameObjectName))
            return;

        GameObject roomInit = GameObject.Find("RoomInit");
        if (roomInit != null)
        {
            TeleportationController controller = roomInit.GetComponent<TeleportationController>();
            controller.Teleport(teleportTo, ScenesToUnload);
        }
        else
        {
            ConfigManager.WriteConsoleError($"[Teleport] can't teleport, telleportationController not found.");
        }
    }
}
