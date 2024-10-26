using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * NOTE: The FileMonitor Unity function didn't work bcz runs out the main thread
 */ 


[RequireComponent(typeof(FileMonitor))]
public class GlobalConfiguration : MonoBehaviour
{
    public UnityEvent OnGlobalConfigChanged;

    [Tooltip("Global Configuration should use the first File Monitor in attached to the gameobject if there are more than one.")]
    public FileMonitor fileMonitor;
    public string yamlPath;
    private ConfigInformation configuration;
    private bool initialized = false;
    private bool isListenerAdded = false;


    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    public ConfigInformation Configuration
    {
        get {
            init();
            return configuration; 
        }
        set
        {
            configuration = value;
            ConfigManager.WriteConsole($"[GlobalConfiguration] new config asigned, invoke calls");
            try
            {
                OnGlobalConfigChanged?.Invoke();
            }
            catch (Exception ex)
            {
                ConfigManager.WriteConsoleException($"[GlobalConfiguration] error in invocation call", ex);
            }
        }
    }

    void init()
    {
        if ( initialized )
            return;

        // Get the first FileMonitor component in the array
        initialized = true;

        if (fileMonitor == null)
            fileMonitor = GetComponent<FileMonitor>();

        yamlPath = ConfigManager.ConfigDir + "/" + fileMonitor.FileName;
        
        OnEnable();
        Load();
    }

    public void InvokeOnGlobalConfigChanged()
    {
        OnGlobalConfigChanged?.Invoke();
        ConfigManager.WriteConsole($"[GlobalConfiguration] Manual event invoke called.");
    }

    private void Load()
    {
        ConfigInformation config;

        ConfigManager.WriteConsole($"[GlobalConfiguration] loadConfiguration: {yamlPath}");
        if (File.Exists(yamlPath))
        {
            fileMonitor.fileLock();
            config = ConfigInformation.fromYaml(yamlPath);
            fileMonitor.fileUnlock();

            if (config == null)
            {
                ConfigManager.WriteConsole($"[GlobalConfiguration] ERROR can't read, back to default: {yamlPath}");
                Configuration = new();
            }
            else
            {
                Configuration = config;
            }
        }
        else
        {
            ConfigManager.WriteConsole($"[GlobalConfiguration] file doesn't exists, create default: {yamlPath}");
            Configuration = ConfigInformation.newDefault();
            Save();
            ConfigManager.WriteConsole($"[GlobalConfiguration] ");
            ConfigManager.WriteConsole(configuration.ToString());
        }
    }

    public void Reset()
    {
        try
        {
            if (File.Exists(yamlPath))
            {
                fileMonitor.fileLock();
                File.Delete(yamlPath);
                fileMonitor.fileUnlock();
            }
            Load();
        }
        catch (IOException e)
        {
            ConfigManager.WriteConsoleError($"[RoomConfiguration.Delete] {yamlPath} - {e}");
        }
    }

    public void Save()
    {
        ConfigManager.WriteConsole($"[GlobalConfiguration] writing configuration: {yamlPath}");
        fileMonitor.fileLock();
        configuration.ToYaml(yamlPath);
        fileMonitor.fileUnlock();
    }

    private void OnFileChanged()
    {
        Load();
    }

    private void addListener()
    {
        fileMonitor?.OnFileChanged.AddListener(OnFileChanged);
        isListenerAdded = true;
    }

    void OnEnable()
    {
        // Listen for the config reload message
        addListener();
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        fileMonitor?.OnFileChanged.RemoveListener(OnFileChanged);
        isListenerAdded = false;
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(GlobalConfiguration))]
public class GlobalConfigurationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        GlobalConfiguration config = (GlobalConfiguration)target;

        // Add a button to invoke the event
        if (GUILayout.Button("Invoke OnGlobalConfigChanged"))
        {
            config.InvokeOnGlobalConfigChanged();
        }
    }
}
#endif
