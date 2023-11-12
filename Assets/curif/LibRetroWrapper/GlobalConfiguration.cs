using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.IO;
using System.Reflection;

//[RequireComponent(typeof(FileMonitor))]
public class GlobalConfiguration : MonoBehaviour
{
    public GameObject FileMonitorGameObject;
    public UnityEvent OnGlobalConfigChanged;

    //[Tooltip("Global Configuration should use the first File Monitor in attached to the gameobject if there are more than one.")]

    private FileMonitor fileMonitor;
    public string yamlPath;
    private ConfigInformation configuration;
    public ConfigInformation Configuration
    {
        get { return configuration; }
        set
        {
            configuration = value;
            ConfigManager.WriteConsole($"[GlobalConfiguration] new config asigned, invoke calls");
            OnGlobalConfigChanged?.Invoke();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get all FileMonitor components attached to the GameObject
        //FileMonitor[] fileMonitors = GetComponents<FileMonitor>();

        // Get the first FileMonitor component in the array
        fileMonitor = FileMonitorGameObject.GetComponent<FileMonitor>();
        yamlPath = ConfigManager.ConfigDir + "/" + fileMonitor.ConfigFileName;
        OnEnable();
        Load();
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

    void OnEnable()
    {
        // Listen for the config reload message
        fileMonitor?.OnFileChanged.AddListener(OnFileChanged);
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        fileMonitor?.OnFileChanged.RemoveListener(OnFileChanged);
    }

}
