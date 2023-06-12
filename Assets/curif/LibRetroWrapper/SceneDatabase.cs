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
using System.Linq;


[Serializable]
public class SceneDocument
{
    public SceneReference Scene;
    public string SceneName;
    public string Description;

    [Tooltip("The name of the gameobject where the player spawn on teleportation")]

    public string PlayerSpawnGameObjectName;
}
public class SceneDatabase : MonoBehaviour
{
    public SceneDocument[] Scenes;

    public SceneDocument FindByName(string sceneName)
    {
        return Scenes.FirstOrDefault(scene => scene.SceneName == sceneName);
    }
    public SceneDocument FindByDescription(string sceneDescription)
    {
        return Scenes.FirstOrDefault(scene => scene.Description == sceneDescription);
    }
    public List<SceneDocument> GetTeleportationDestinationRooms()
    {
        return Scenes.Where(scene => !string.IsNullOrEmpty(scene.PlayerSpawnGameObjectName)).ToList();
    }
    public List<string> GetTeleportationDestinationRoomNames()
    {
        return Scenes
            .Where(scene => !string.IsNullOrEmpty(scene.PlayerSpawnGameObjectName))
            .Select(scene => scene.SceneName)
            .ToList();
    }
    public List<string> GetTeleportationDestinationRoomDescritions()
    {
        return Scenes
            .Where(scene => !string.IsNullOrEmpty(scene.PlayerSpawnGameObjectName))
            .Select(scene => scene.Description)
            .ToList();
    }
    
}

