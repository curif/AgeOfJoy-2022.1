using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace PixelCrushers.SceneStreamer
{

    /// <summary>
    /// SceneStreamer is a singleton MonoBehavior used to load and unload scenes that contain
    /// pieces of the game world. You can use it to implement continuous worlds. The piece
    /// of the world containing the player is called the "current scene." SceneStreamer 
    /// automatically loads neighboring scenes up to a distance you specify and unloads 
    /// scenes beyond that distance.
    /// 
    /// In Unity versions below 5.3, all scenes are loaded into a single containing GameObject. 
    /// As the player moves through the world, scenes are added and removed from this container.
    /// 
    /// You'll usually only call the static method SetCurrentScene(), and often only through 
    /// the SetStartScene script.
    /// 
    /// You can manually load and unload scenes using LoadScene() and UnloadScene(), and
    /// check whether a scene is loaded with IsSceneLoaded().
    /// 
    /// You can also hook into the events On Loading and On Loaded to get notification when a
    /// scene begins loading and when it's done loading.
    /// </summary>
    [AddComponentMenu("Scene Streamer/Scene Streamer")]
    public class SceneStreamer : MonoBehaviour
    {

        /// <summary>
        /// The max number of neighbors to load out from the current scene.
        /// </summary>
        [Tooltip("Max number of neighbors to load out from the current scene.")]
        public int maxNeighborDistance = 1;

        /// <summary>
        /// A failsafe in case loading hangs. After this many seconds, the SceneStreamer
        /// will stop waiting for the scene to load.
        /// </summary>
        [Tooltip("(Failsafe) If scene doesn't load after this many seconds, stop waiting.")]
        public float maxLoadWaitTime = 10f;

        [System.Serializable]
        public class StringEvent : UnityEvent<string> { }
        [System.Serializable]
        public class StringAsyncEvent : UnityEvent<string, AsyncOperation> { }

        public StringAsyncEvent onLoading = new StringAsyncEvent();

        public StringEvent onLoaded = new StringEvent();

        [Tooltip("Tick to log debug info to the Console window.")]
        public bool debug = false;

        public bool logDebugInfo { get { return debug && Debug.isDebugBuild; } }

        /// <summary>
        /// The name of the player's current scene.
        /// </summary>
        private string m_currentSceneName = null;

        /// <summary>
        /// The names of all loaded scenes.
        /// </summary>
        private HashSet<string> m_loaded = new HashSet<string>();

        /// <summary>
        /// The names of all scenes that are in the process of being loaded.
        /// </summary>
        private HashSet<string> m_loading = new HashSet<string>();

        /// <summary>
        /// The names of all scenes within maxNeighborDistance of the current scene.
        /// This is used when determining which neighboring scenes to load or unload.
        /// </summary>
        private HashSet<string> m_near = new HashSet<string>();

        private static object s_lock = new object();

        private static SceneStreamer s_instance = null;

        private static SceneStreamer instance
        {
            get
            {
                lock (s_lock)
                {
                    if (s_instance == null)
                    {
                        s_instance = FindObjectOfType(typeof(SceneStreamer)) as SceneStreamer;
                        if (s_instance == null)
                        {
                            s_instance = new GameObject("Scene Loader", typeof(SceneStreamer)).GetComponent<SceneStreamer>();
                        }
                    }
                    return s_instance;
                }
            }
            set
            {
                s_instance = value;
            }
        }

        public void Awake()
        {
            if (s_instance)
            {
                Destroy(this);
            }
            else
            {
                s_instance = this;
                Object.DontDestroyOnLoad(this.gameObject);
            }
        }

        /// <summary>
        /// Sets the current scene, loads it, and manages neighbors. The scene must be in your
        /// project's build settings.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public void SetCurrent(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || string.Equals(sceneName, m_currentSceneName)) return;
            if (logDebugInfo) Debug.Log("Scene Streamer: Setting current scene to " + sceneName + ".");
            StartCoroutine(LoadCurrentScene(sceneName));
        }

        /// <summary>
        /// Loads a scene as the current scene and manages neighbors, loading scenes
        /// within maxNeighborDistance and unloading scenes beyond it.
        /// </summary>
        /// <returns>The current scene.</returns>
        /// <param name="sceneName">Scene name.</param>
        private IEnumerator LoadCurrentScene(string sceneName)
        {
            // First load the current scene:
            m_currentSceneName = sceneName;
            if (!IsLoaded(m_currentSceneName)) Load(sceneName);
            float failsafeTime = Time.realtimeSinceStartup + maxLoadWaitTime;
            while ((m_loading.Count > 0) && (Time.realtimeSinceStartup < failsafeTime))
            {
                yield return null;
            }
            if (Time.realtimeSinceStartup >= failsafeTime && Debug.isDebugBuild) Debug.LogWarning("Scene Streamer: Timed out waiting to load " + sceneName + ".");

            // Next load neighbors up to maxNeighborDistance, keeping track
            // of them in the near list:
            if (logDebugInfo) Debug.Log("Scene Streamer: Loading " + maxNeighborDistance + " closest neighbors of " + sceneName + ".");
            m_near.Clear();
            LoadNeighbors(sceneName, 0);
            failsafeTime = Time.realtimeSinceStartup + maxLoadWaitTime;
            while ((m_loading.Count > 0) && (Time.realtimeSinceStartup < failsafeTime))
            {
                yield return null;
            }
            if (Time.realtimeSinceStartup >= failsafeTime && Debug.isDebugBuild) Debug.LogWarning("Scene Streamer: Timed out waiting to load neighbors of " + sceneName + ".");

            // Finally unload any scenes not in the near list:
            UnloadFarScenes();
        }

        /// <summary>
        /// Loads neighbor scenes within maxNeighborDistance, adding them to the near list.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        /// <param name="distance">Distance.</param>
        private void LoadNeighbors(string sceneName, int distance)
        {
            if (m_near.Contains(sceneName)) return;
            m_near.Add(sceneName);
            if (distance >= maxNeighborDistance) return;
            GameObject scene = GameObject.Find(sceneName);
            NeighboringScenes neighboringScenes = (scene) ? scene.GetComponent<NeighboringScenes>() : null;
            if (!neighboringScenes) neighboringScenes = CreateNeighboringScenesList(scene);
            if (!neighboringScenes) return;
            for (int i = 0; i < neighboringScenes.sceneNames.Length; i++)
            {
                Load(neighboringScenes.sceneNames[i], LoadNeighbors, distance + 1);
            }
        }

        /// <summary>
        /// Creates the neighboring scenes list. It's faster to manually add a
        /// NeighboringScenes script to your scene's root object; this method
        /// builds it manually if it's missing, but requires the scene to have
        /// SceneEdge components.
        /// </summary>
        /// <returns>The neighboring scenes list.</returns>
        /// <param name="scene">Scene.</param>
        private NeighboringScenes CreateNeighboringScenesList(GameObject scene)
        {
            if (!scene) return null;
            NeighboringScenes neighboringScenes = scene.AddComponent<NeighboringScenes>();
            HashSet<string> neighbors = new HashSet<string>();
            var sceneEdges = scene.GetComponentsInChildren<SceneEdge>();
            for (int i = 0; i < sceneEdges.Length; i++)
            {
                neighbors.Add(sceneEdges[i].nextSceneName);
            }
            neighboringScenes.sceneNames = new string[neighbors.Count];
            neighbors.CopyTo(neighboringScenes.sceneNames);
            return neighboringScenes;
        }

        /// <summary>
        /// Determines whether a scene is loaded.
        /// </summary>
        /// <returns><c>true</c> if loaded; otherwise, <c>false</c>.</returns>
        /// <param name="sceneName">Scene name.</param>
        public bool IsLoaded(string sceneName)
        {
            return m_loaded.Contains(sceneName);
        }

        /// <summary>
        /// Loads a scene.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public void Load(string sceneName)
        {
            Load(m_currentSceneName, null, 0);
        }

        private delegate void InternalLoadedHandler(string sceneName, int distance);

        /// <summary>
        /// Loads a scene and calls an internal delegate when done. The delegate is
        /// used by the LoadNeighbors() method.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        /// <param name="loadedHandler">Loaded handler.</param>
        /// <param name="distance">Distance from the current scene.</param>
        private void Load(string sceneName, InternalLoadedHandler loadedHandler, int distance)
        {
            if (IsLoaded(sceneName))
            {
                if (loadedHandler != null) loadedHandler(sceneName, distance);
                return;
            }
            m_loading.Add(sceneName);
            if (logDebugInfo && distance > 0) Debug.Log("Scene Streamer: Loading " + sceneName + ".");
            StartCoroutine(LoadAdditiveAsync(sceneName, loadedHandler, distance));
        }

        /// <summary>
        /// (Unity Pro) Runs Application.LoadLevelAdditiveAsync() and calls FinishLoad() when done.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        /// <param name="loadedHandler">Loaded handler.</param>
        /// <param name="distance">Distance.</param>
        private IEnumerator LoadAdditiveAsync(string sceneName, InternalLoadedHandler loadedHandler, int distance)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            onLoading.Invoke(sceneName, asyncOperation);
            yield return asyncOperation;
            FinishLoad(sceneName, loadedHandler, distance);
        }

        /// <summary>
        /// (Unity) Runs Application.LoadLevelAdditive() and calls FinishLoad() when done.
        /// This coroutine waits two frames to wait for the load to complete.
        /// </summary>
        /// <returns>The additive.</returns>
        /// <param name="sceneName">Scene name.</param>
        /// <param name="loadedHandler">Loaded handler.</param>
        /// <param name="distance">Distance.</param>
        private IEnumerator LoadAdditive(string sceneName, InternalLoadedHandler loadedHandler, int distance)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            onLoading.Invoke(sceneName, null);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            FinishLoad(sceneName, loadedHandler, distance);
        }

        /// <summary>
        /// Called when a level is done loading. Updates the loaded and loading lists, and 
        /// calls the loaded handler.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        /// <param name="loadedHandler">Loaded handler.</param>
        /// <param name="distance">Distance.</param>
        private void FinishLoad(string sceneName, InternalLoadedHandler loadedHandler, int distance)
        {
            GameObject scene = GameObject.Find(sceneName);
            if (scene == null && Debug.isDebugBuild) Debug.LogWarning("Scene Streamer: Can't find loaded scene named '" + sceneName + "'.");
            m_loading.Remove(sceneName);
            m_loaded.Add(sceneName);
            onLoaded.Invoke(sceneName);
            if (loadedHandler != null) loadedHandler(sceneName, distance);
        }

        /// <summary>
        /// Unloads scenes beyond maxNeighborDistance. Assumes the near list has already been populated.
        /// </summary>
        private void UnloadFarScenes()
        {
            HashSet<string> far = new HashSet<string>(m_loaded);
            far.ExceptWith(m_near);
            if (logDebugInfo && far.Count > 0) Debug.Log("Scene Streamer: Unloading scenes more than " + maxNeighborDistance + " away from current scene " + m_currentSceneName + ".");
            foreach (var sceneName in far)
            {
                Unload(sceneName);
            }
        }

        /// <summary>
        /// Unloads a scene.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public void Unload(string sceneName)
        {
            if (logDebugInfo) Debug.Log("Scene Streamer: Unloading scene " + sceneName + ".");
            Destroy(GameObject.Find(sceneName));
            m_loaded.Remove(sceneName);
            SceneManager.UnloadSceneAsync(sceneName);
        }

        /// <summary>
        /// Sets the current scene.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public static void SetCurrentScene(string sceneName)
        {
            instance.SetCurrent(sceneName);
        }

        /// <summary>
        /// Determines if a scene is loaded.
        /// </summary>
        /// <returns><c>true</c> if loaded; otherwise, <c>false</c>.</returns>
        /// <param name="sceneName">Scene name.</param>
        public static bool IsSceneLoaded(string sceneName)
        {
            return instance.IsLoaded(sceneName);
        }

        /// <summary>
        /// Loads a scene.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public static void LoadScene(string sceneName)
        {
            instance.Load(sceneName);
        }

        /// <summary>
        /// Unloads a scene.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public static void UnloadScene(string sceneName)
        {
            instance.Unload(sceneName);
        }

    }

}