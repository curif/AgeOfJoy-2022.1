using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;

//[RequireComponent(typeof(FileMonitor))]
public class RoomConfiguration : MonoBehaviour
{
    public GameObject FileMonitorGameObject;
    public GameObject GlobalConfigurationGameObject;
    public UnityEvent OnRoomConfigChanged;

    public string yamlPath;
    private FileMonitor fileMonitor;
    private GlobalConfiguration globalConfiguration;

    private ConfigInformation configuration;
    public ConfigInformation Configuration
    {
        get { return configuration; }
        set
        {
            configuration = value;
            ConfigManager.WriteConsole($"[RoomConfiguration] invoke calls...");

            OnRoomConfigChanged?.Invoke();
        }
    }

    void Start()
    {
        if (GlobalConfigurationGameObject == null)
        {
            GlobalConfigurationGameObject = GameObject.Find("GlobalConfiguration");
        }
        fileMonitor = FileMonitorGameObject.GetComponent<FileMonitor>();
        yamlPath = ConfigManager.ConfigDir + "/" + fileMonitor.ConfigFileName;
        globalConfiguration = GlobalConfigurationGameObject.GetComponent<GlobalConfiguration>();
        OnEnable();
        Load();
    }

    private void mergeWithGlobalAndAssign(ConfigInformation config)
    {
        //merge with global
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] merge with global configuration");
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] config to be merged: {config.ToString()}");
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] global to merge to: {globalConfiguration.Configuration.ToString()}");
        Configuration = (ConfigInformation)ConfigInformation.Merge(globalConfiguration.Configuration, config);
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] final configuration: {Configuration.ToString()}");
    }

    public string GetName()
    {
        return Path.GetFileNameWithoutExtension(fileMonitor?.ConfigFileName);
    }

    public bool ExistsRoomConfiguration()
    {
        return File.Exists(yamlPath);
    }

    public void Reset()
    {
        try
        {
            if (File.Exists(yamlPath))
            {
                File.Delete(yamlPath);
            }
            Load();
        }
        catch (IOException e)
        {
            ConfigManager.WriteConsoleError($"[RoomConfiguration.Delete] {yamlPath} - {e}");
        }
    }

    private void Load()
    {
        ConfigInformation config;

        ConfigManager.WriteConsole($"[RoomConfiguration.Load] load: {yamlPath}");
        if (File.Exists(yamlPath))
        {
            config = ConfigInformation.fromYaml(yamlPath);
            if (config == null)
            {
                ConfigManager.WriteConsoleError($"[RoomConfiguration.Load] can't load existent file, default to global: {yamlPath}");
                mergeWithGlobalAndAssign(new()); //set a new configuration merged with global configuration.
            }
            else
            {
                ConfigManager.WriteConsole($"[RoomConfiguration.Load] loaded from file, merge with global {yamlPath}");
                mergeWithGlobalAndAssign(config);
            }
        }
        else
        {
            ConfigManager.WriteConsole($"[RoomConfiguration.Load] file doesn't exists, using global. {yamlPath}");
            mergeWithGlobalAndAssign(new()); //set a new configuration merged with global configuration.
        }

        ConfigManager.WriteConsole($"[RoomConfiguration.Load] configuration established: ");
        ConfigManager.WriteConsole(configuration.ToString());
    }

    public void Save()
    {
        ConfigManager.WriteConsole($"[RoomConfiguration] writing configuration: {yamlPath}");
        configuration.ToYaml(yamlPath);
    }

    void OnEnable()
    {
        // Listen for the config reload message
        fileMonitor?.OnFileChanged.AddListener(OnFileChanged);
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        fileMonitor?.OnFileChanged.RemoveListener(OnFileChanged);
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
    }

    void OnGlobalConfigChanged()
    {
        ConfigManager.WriteConsole($"[RoomConfiguration] global config changed, reload: {yamlPath}");
        Load();
    }

    void OnFileChanged()
    {
        ConfigManager.WriteConsole($"[RoomConfiguration] file changed, reload: {yamlPath}");
        Load();
    }
}
