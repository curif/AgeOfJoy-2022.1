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
using System.Linq;


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
    [Tooltip("Delay between check if the player is present.")]
    public float IdleTimeCheck = 0.2f;

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

    private HashSet<string> scenesToLoadSet;

    // Cache for SliderDoorController components to avoid GetComponent calls in the loop
    private Dictionary<GameObject, SliderDoorController> doorControllerCache;

    // Cache for scene loaded status to avoid redundant SceneManager.GetSceneByName calls
    private Dictionary<string, bool> sceneLoadedStateCache;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneLoadedStateCache != null && sceneLoadedStateCache.ContainsKey(scene.name))
        {
            sceneLoadedStateCache[scene.name] = true;
            RefreshBlockersForScene(scene.name);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (sceneLoadedStateCache != null && sceneLoadedStateCache.ContainsKey(scene.name))
        {
            sceneLoadedStateCache[scene.name] = false;
            RefreshBlockersForScene(scene.name);
        }
    }

    private void RefreshBlockersForScene(string sceneName)
    {
        foreach (var gate in SceneBlockers)
        {
            if (gate.SceneRef != null && gate.SceneRef.Name == sceneName)
            {
                UpdateBlockers(gate, !sceneLoadedStateCache[sceneName]);
            }
        }
    }

    void Start()
    {
        player = GameObject.Find("OVRPlayerControllerGalery");

        scenesToLoadSet = new HashSet<string>();
        foreach (var sceneReference in ScenesToLoad)
        {
            scenesToLoadSet.Add(sceneReference.Name);
        }

        // Initialize and populate the door controller cache
        doorControllerCache = new Dictionary<GameObject, SliderDoorController>();
        sceneLoadedStateCache = new Dictionary<string, bool>();

        if (SceneBlockers != null)
        {
            foreach (SceneGate scn in SceneBlockers)
            {
                if (scn.SceneRef != null)
                {
                    sceneLoadedStateCache[scn.SceneRef.Name] = SceneManager.GetSceneByName(scn.SceneRef.Name).isLoaded;
                }

                if (scn.Blockers != null)
                {
                    foreach (GameObject blocker in scn.Blockers)
                    {
                        if (blocker != null && !doorControllerCache.ContainsKey(blocker))
                        {
                            doorControllerCache[blocker] = blocker.GetComponent<SliderDoorController>();
                        }
                    }
                }
            }
        }

        // Initial sync of all blockers
        LockGate();

        StartCoroutine(gateControlLoop());
    }

    bool IsSceneToLoad(string sceneName)
    {
        return scenesToLoadSet.Contains(sceneName);
    }
    IEnumerator gateControlLoop()
    {
        while (true)
        {
            if (playerIsOnTheGate)
            {
                bool scenesLoadedOrUnloaded = false;
                if (ScenesToUnload.Length > 0)
                {
                    //ConfigManager.WriteConsole($"[GateController] gate activated, unloading rooms...");
                    //bool unloadUnusedAssets = false;
                    foreach (SceneReference controledSceneToUnLoad in ScenesToUnload)
                    {
                        if (controledSceneToUnLoad != null &&
                            !IsSceneToLoad(controledSceneToUnLoad.Name) &&
                            controledSceneToUnLoad.IsSafeToUse &&
                            SceneManager.GetSceneByName(controledSceneToUnLoad.Name).isLoaded)
                        {
                            AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(controledSceneToUnLoad.Name);
                            while (!asyncLoad.isDone)
                                yield return null;
                            // yield return null;
                            ConfigManager.WriteConsole($"[GateController] UNLOADED SCENE: {controledSceneToUnLoad.Name} ******.");
                            scenesLoadedOrUnloaded = true;
                        }
                    }
                    if (scenesLoadedOrUnloaded)
                    {
                        // --- AGGRESSIVE CLEANUP ---
                        
                        yield return Resources.UnloadUnusedAssets();
                        System.GC.Collect();
                    
                        // LOG THE MEMORY TO ADB
                        long mem = System.GC.GetTotalMemory(false) / 1024 / 1024;
                        ConfigManager.WriteConsole($"[Memory Check] RAM after unload: {mem}MB");

                        if (mem > 2500)
                        {
                            ConfigManager.WriteConsole("****WARNING***** Memory still too high! New load will likely crash.");
                        }
                        yield return new WaitForSecondsRealtime(0.5f);
                    }
                }

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
                                yield return null;
                            scenesLoadedOrUnloaded = true;
                            ConfigManager.WriteConsole($"[GateController] LOADED SCENE: {controledSceneToLoad.Name}");
                        }
                    }
                }

                if (scenesLoadedOrUnloaded)
                {
                    // Only call this ONCE at the very end of all operations.

                    // time to calculate blend probes teselation
                    // Force Unity to asynchronously regenerate the tetrahedral tesselation for all loaded Scenes
                    // https://docs.unity3d.com/Manual/light-probes-and-scene-loading.html  
                    LightProbes.TetrahedralizeAsync();
                }
            }

            yield return new WaitForSeconds(IdleTimeCheck);
        }
    }

    void UpdateBlockers(SceneGate scn, bool blocked)
    {
        foreach (GameObject blocker in scn.Blockers)
        {
            if (blocker != null)
            {
                if (doorControllerCache.TryGetValue(blocker, out SliderDoorController ctrl) && ctrl != null)
                {
                    ctrl.SetDoorState(!blocked);
                }
                else
                {
                    blocker.SetActive(blocked);
                }
            }
        }
    }

    void LockGate(bool? blocked = null)
    {
        if (SceneBlockers.Length > 0)
        {
            foreach (var gate in SceneBlockers)
            {
                if (gate.SceneRef != null)
                {
                    bool isBlocked = (blocked != null) ? (bool)blocked : !sceneLoadedStateCache[gate.SceneRef.Name];
                    UpdateBlockers(gate, isBlocked);
                }
            }
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
