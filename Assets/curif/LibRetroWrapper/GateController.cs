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
using UnityEngine.XR.Interaction.Toolkit;

// https://github.com/starikcetin/Eflatun.SceneReference

[Serializable]
public class SceneGate
{
    [Tooltip("Room blocked by the gate")]
    public SceneReference SceneRef;
    [Tooltip("Objects that block the gate, the object should be in the actual room, not in the blocked one.")]
    public GameObject[] Blockers;
}

[RequireComponent(typeof(BoxCollider))]
//[RequireComponent(typeof(MeshCollider))]
//[RequireComponent(typeof(TeleportationArea))]
public class GateController : MonoBehaviour
{

    /*
      [Tooltip("The minimal distance between the player and the gate to load/unload scenes.")]
      [SerializeField]
      public float MinimalDistance = 0.9f;
    */
    [Header("Scene Load settings")]
    [Tooltip("Names of the scenes to load.")]
    [SerializeField]
    public SceneReference[] ScenesToLoad;

    [Header("Scene Unload settings")]
    [SerializeField]
    public SceneReference[] ScenesToUnload;

    [Header("Scene Blockers")]
    [SerializeField]
    public SceneGate[] SceneBlockers;

    private GameObject player;
    private bool playerIsOnTheGate = false;

    void Start()
    {
        player = GameObject.Find("OVRPlayerControllerGalery");
        StartCoroutine(gateControlLoop());
    }

    IEnumerator gateControlLoop()
    {
        while (true)
        {
            if (playerIsOnTheGate)
            {

                if (ScenesToLoad.Length > 0)
                {
                    //ConfigManager.WriteConsole($"[GateController] gate activated, loading rooms...");
                    foreach (SceneReference controledSceneToLoad in ScenesToLoad)
                    {
                        if (controledSceneToLoad != null &&
                            controledSceneToLoad.IsSafeToUse &&
                            !SceneManager.GetSceneByName(controledSceneToLoad.Name).isLoaded)
                        {
                            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(controledSceneToLoad.Name, LoadSceneMode.Additive);
                            while (!asyncLoad.isDone)
                                yield return new WaitForSeconds(0.1f);
                            // yield return null;
                            ConfigManager.WriteConsole($"[GateController] LOADED SCENE: {controledSceneToLoad.Name}");
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    // time to calculate blend probes teselation
                    // Force Unity to asynchronously regenerate the tetrahedral tesselation for all loaded Scenes
                    // https://docs.unity3d.com/Manual/light-probes-and-scene-loading.html  
                    LightProbes.TetrahedralizeAsync();
                    yield return new WaitForSeconds(0.1f);
                }
                if (ScenesToUnload.Length > 0)
                {
                    //ConfigManager.WriteConsole($"[GateController] gate activated, unloading rooms...");
                    bool unloadUnusedAssets = false;
                    foreach (SceneReference controledSceneToUnLoad in ScenesToUnload)
                    {
                        if (controledSceneToUnLoad != null && controledSceneToUnLoad.IsSafeToUse &&
                            SceneManager.GetSceneByName(controledSceneToUnLoad.Name).isLoaded)
                        {
                            AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(controledSceneToUnLoad.Name);
                            while (!asyncLoad.isDone)
                                yield return new WaitForSeconds(0.1f);
                            // yield return null;
                            ConfigManager.WriteConsole($"[GateController] UNLOADED SCENE: {controledSceneToUnLoad.Name} ******.");
                            unloadUnusedAssets = true;
                            LightProbes.TetrahedralizeAsync();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    if (unloadUnusedAssets)
                    {
                        AsyncOperation resourceUnloadOp = Resources.UnloadUnusedAssets();
                        while (!resourceUnloadOp.isDone)
                            yield return new WaitForSeconds(0.1f);
                        // yield return null;
                    }
                }
            }
            if (SceneBlockers.Length > 0)
            {
                int idx;
                for (idx = 0; idx < SceneBlockers.Length; idx++)
                {
                    SceneReference controledSceneBlocker = SceneBlockers[idx].SceneRef;
                    if (controledSceneBlocker != null)
                        LockGate(SceneBlockers[idx],
                                !SceneManager.GetSceneByName(controledSceneBlocker.Name).isLoaded);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
    void LockGate(SceneGate scn, bool block)
    {
        foreach (GameObject blocker in scn.Blockers)
        {
            if (blocker != null)
                blocker.SetActive(block);
        }
    }

    //box trigger must to be true
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            playerIsOnTheGate = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerIsOnTheGate = false;
        }
    }
    /*
      void OnCollisionEnter(Collision collision)
    {
        // Code to handle collision goes here
        ConfigManager.WriteConsole("collision");
        if (collision.gameObject  == player)
        {
            playerIsOnTheGate = true; 
        }
    }
     private void OnCollisionExit(Collision collision)
      {
        ConfigManager.WriteConsole("uncollision");
        if (collision.gameObject == player)
        {
          playerIsOnTheGate = false; 
        }
      }
      */
}
