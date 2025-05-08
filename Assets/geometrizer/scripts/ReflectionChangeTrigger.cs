using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


//set the box collider to layer "Player" to accept collisions only for player gameobject
//to avoid calls.
public class ReflectionChangeTrigger : MonoBehaviour
{
    public Cubemap newReflectionCubemap; // Assign this in the Inspector
    public float lightTransitionDuration = 1f;
    public UserLightManager userLightManager;
    [SerializeField] public Color RoomStartingColor;
    public float intensity = 1;
    public RoomConfiguration roomConfiguration;


    private bool isListenerAdded = false;

    // Start is called before the first frame update
    void Start()
    {
        if (userLightManager == null)
        {
            GameObject userLightGO = GameObject.Find("UserLightManager");
            if (userLightGO != null)
                userLightManager = userLightGO.GetComponent<UserLightManager>();
        }

        //OnEnable();  If the GameObject is active on startup, OnEnable() will be called before the first Start() call

        if (roomConfiguration?.Configuration?.light == null)
        {
            Scene triggerScene = gameObject.scene;
            RGBColor c = new RGBColor(RoomStartingColor, 0);
            ConfigManager.WriteConsole($"[ReflectionChangeTrigger] '{triggerScene.name}' applying dev configuration color: {c}");
            userLightManager.ApplyUserLightSettings(c, intensity);
        }
        else
        {
            change();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            //    ConfigManager.WriteConsole($"[ReflectionChangeTrigger] Triggered in scene: '{triggerScene.name}', object: '{other}'");
            return;
        }
        
        Scene triggerScene = gameObject.scene;

        // Log the scene asset path and name
        ConfigManager.WriteConsole($"[ReflectionChangeTrigger] Triggered in scene: '{triggerScene.name}', path: '{triggerScene.path}'");

        // Update reflection if needed
        if (newReflectionCubemap != null && RenderSettings.customReflection != newReflectionCubemap)
        {
            RenderSettings.customReflection = newReflectionCubemap;
            DynamicGI.UpdateEnvironment();
        }

        //initial ligth color and intensity
        if (roomConfiguration != null && userLightManager != null)
        {
            if (roomConfiguration?.Configuration?.light?.color != null)
            {
                ConfigManager.WriteConsole($"[ReflectionChangeTrigger] '{triggerScene.name}' using room yaml configuration {roomConfiguration.Configuration.light.color}");
                //force user intesity to 0:
                roomConfiguration.Configuration.light.color.intensity = 0;
                userLightManager.ApplyUserLightSettings(roomConfiguration.Configuration.light.color, roomConfiguration.Configuration.light.intensity);
            }
            /*
            else
            {
                RGBColor c = new RGBColor(RoomStartingColor, 0);
                ConfigManager.WriteConsole($"[ReflectionChangeTrigger] '{triggerScene.name}' using dev configuration color: {c}");
                userLightManager.ApplyUserLightSettings(c, intensity);
            }
            */
        }

        /*
        GameObject userLightGO;
        if (userLightManager == null)
        {
            userLightGO = GameObject.Find("UserLightManager");
            if (userLightGO != null)
                userLightManager = userLightGO.GetComponent<UserLightManager>();
        }
        if (userLightManager == null) return;

        // Search for PF_UserLight recursively in this scene
        // pfUserLight must be deactivated on every scene
        if (pfUserLight == null)
        {
            foreach (GameObject rootObj in triggerScene.GetRootGameObjects())
            {
                pfUserLight = FindChildRecursiveByName(rootObj.transform, "PF_UserLight");
                if (pfUserLight != null) break;
            }
        }

        // Default values if not found
        Color finalColor = Color.black;
        float finalIntensity = 0f;
        Vector3 finalEulerRotation = Vector3.zero;

        if (pfUserLight != null)
        {
            Light lightComponent = pfUserLight.GetComponent<Light>();
            if (lightComponent != null)
            {
                finalColor = lightComponent.color;
                finalIntensity = lightComponent.intensity;
            }

            finalEulerRotation = pfUserLight.transform.eulerAngles;
        }

        ConfigManager.WriteConsole($"[ReflectionChangeTrigger] Found PF_UserLight in scene '{triggerScene.name}'. " +
                                $"Color: {finalColor}, Intensity: {finalIntensity}, Rotation: {finalEulerRotation}");
    
        // Apply to UserLightManager
        UserLightSettings newSettings = new UserLightSettings
        {
            color = finalColor,
            intensity = finalIntensity,
            eulerRotation = finalEulerRotation
        };
        userLightManager.ApplyUserLightSettings(newSettings, lightTransitionDuration);
        
        ConfigManager.WriteConsole($"[ReflectionChangeTrigger] Applying to UserLightManager: " +
                                $"Color = {finalColor}, Intensity = {finalIntensity}, Rotation = {finalEulerRotation}");
        */
    }
    /*
    // Recursively search for child named PF_UserLight
    private GameObject FindChildRecursiveByName(Transform parent, string targetName)
    {
        if (parent.name == targetName)
            return parent.gameObject;

        foreach (Transform child in parent)
        {
            GameObject result = FindChildRecursiveByName(child, targetName);
            if (result != null)
                return result;
        }

        return null;
    }
    */

    void change()
    {
        if (userLightManager != null &&
            roomConfiguration.Configuration?.light != null)
        {
            userLightManager.ApplyUserLightSettings(roomConfiguration.Configuration.light.color, roomConfiguration.Configuration.light.intensity);
        }

    }
    void OnRoomConfigChanged()
    {
        change();
    }

    void addListener()
    {
        if (roomConfiguration == null) return;
        if (isListenerAdded) return;
        roomConfiguration.OnRoomConfigChanged.AddListener(OnRoomConfigChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (roomConfiguration == null) return;
        if (!isListenerAdded) return;
        roomConfiguration.OnRoomConfigChanged.RemoveListener(OnRoomConfigChanged);
        isListenerAdded = false;
    }

    void OnEnable()
    {
        addListener();
    }

    void OnDisable()
    {
        removeListener();
    }

    /*
    public RoomConfiguration GetRoomConfiguration()
    {
        if (roomConfiguration != null)
            return roomConfiguration;

        Scene currentScene = gameObject.scene;

        GameObject[] rootGameObjects = currentScene.GetRootGameObjects();

        foreach (GameObject rootGameObject in rootGameObjects)
        {
            if (rootGameObject.name == "Configuration")
            {
                Transform roomConfigurationTransform = rootGameObject.transform.Find("RoomConfiguration");
                if (roomConfigurationTransform != null)
                {
                    RoomConfiguration roomConfigurationComponent = roomConfigurationTransform.GetComponent<RoomConfiguration>();
                    if (roomConfigurationComponent != null)
                    {
                        return roomConfigurationComponent;
                    }
                    else
                    {
                        ConfigManager.WriteConsoleWarning("[ReflectionChangeTrigger] The 'RoomConfiguration' GameObject does not have a RoomConfiguration component attached in scene: " + currentScene.name);
                        return null;
                    }
                }
                else
                {
                    ConfigManager.WriteConsoleWarning("[ReflectionChangeTrigger] Could not find the 'RoomConfiguration' GameObject under 'Configuration' in scene: " + currentScene.name);
                    return null;
                }
            }
        }
        return null;
    }
    */
}
