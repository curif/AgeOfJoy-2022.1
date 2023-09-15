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

public class TeleportationController : MonoBehaviour
{
    private SceneReference[] ScenesToUnload;

    private SceneDocument teleportTo;

    public void Teleport(SceneDocument teleportTo, SceneReference[] scenesToUnload)
    {
        if (string.IsNullOrEmpty(teleportTo.PlayerSpawnGameObjectName))
            return;

        ConfigManager.WriteConsole($"[TeleportationController.Teleport] starting teleportation");

        this.teleportTo = teleportTo;
        this.ScenesToUnload = scenesToUnload;
        StartCoroutine(TeleportLoop());
    }

    IEnumerator TeleportLoop()
    {
        GameObject player = GameObject.Find("OVRPlayerControllerGalery");

        if (teleportTo != null /*&& teleportTo.Scene.IsSafeToUse*/)
        {
            if (SceneManager.GetSceneByName(teleportTo.Scene.Name).isLoaded)
            {
                ConfigManager.WriteConsole($"[TeleportLoop] LOADED SCENE (previously): {teleportTo.Scene.Name}");
            }
            else
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(teleportTo.Scene.Name,
                                                                          LoadSceneMode.Additive);
                while (!asyncLoad.isDone)
                    yield return null;

                ConfigManager.WriteConsole($"[TeleportLoop] LOADED SCENE: {teleportTo.Scene.Name}");
            }
        }
        else
        {
            ConfigManager.WriteConsole($"[TeleportLoop] scene isn't ready to be loaded, cancel: {teleportTo.Scene.Name}");
            yield break;
        }

        //get player position
        GameObject spawnPosition = GameObject.Find(teleportTo.PlayerSpawnGameObjectName);
        if (spawnPosition != null)
        {
            ConfigManager.WriteConsole($"[TeleportLoop] player spawn in {spawnPosition}");
            Vector3 sourcePosition = spawnPosition.transform.position;
            Vector3 destinationPosition = player.transform.position;
            destinationPosition.x = sourcePosition.x;
            destinationPosition.y = sourcePosition.y + 0.5f;
            destinationPosition.z = sourcePosition.z;
            //moves the player
            player.transform.position = destinationPosition;
        }
        else
        {
            ConfigManager.WriteConsoleError($"[TeleportLoop] can't spawn, position {teleportTo.PlayerSpawnGameObjectName} not found, check scene database in room init or the gameobject spawn in the destination room.");
            SceneManager.UnloadSceneAsync(teleportTo.Scene.Name);
            yield break;
        }

        //unload unused scenes
        if (ScenesToUnload.Length > 0)
        {
            bool unloadUnusedAssets = false;
            foreach (SceneReference controledSceneToUnLoad in ScenesToUnload)
            {
                if (controledSceneToUnLoad != null /*&& controledSceneToUnLoad.IsSafeToUse*/)
                {
                    if (SceneManager.GetSceneByName(controledSceneToUnLoad.Name).isLoaded)
                    {
                        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(controledSceneToUnLoad.Name);
                        while (!asyncLoad.isDone)
                            yield return null;

                        ConfigManager.WriteConsole($"[TeleportLoop] UNLOADED SCENE: {controledSceneToUnLoad.Name} ******.");
                        unloadUnusedAssets = true;
                    }
                    else
                    {
                        ConfigManager.WriteConsoleError($"[TeleportLoop] scene isn't ready to be unloaded {controledSceneToUnLoad}");
                    }
                }
            }
            if (unloadUnusedAssets)
            {
                AsyncOperation resourceUnloadOp = Resources.UnloadUnusedAssets();
                while (!resourceUnloadOp.isDone)
                    yield return null;
            }
        }

        LightProbes.TetrahedralizeAsync();
        yield break;
    }

}
