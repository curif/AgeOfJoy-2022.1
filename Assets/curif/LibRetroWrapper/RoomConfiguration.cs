using UnityEngine;
using System.IO;
using UnityEngine.Events;

//[RequireComponent(typeof(FileMonitor))]
public class RoomConfiguration : MonoBehaviour
{
    public UnityEvent OnRoomConfigChanged;
    public string Room;

    public string yamlPath;
    public FileMonitor fileMonitor;

    private GlobalConfiguration globalConfiguration;
    private bool initialized = false;
    private ConfigInformation configuration;
    private bool isListenerAdded = false;


    public ConfigInformation Configuration
    {
        get {
            init();
            return configuration; 
        }
        set
        {
            configuration = value;
            ConfigManager.WriteConsole($"[RoomConfiguration] invoke calls...");

            OnRoomConfigChanged?.Invoke();
        }
    }

    void Start()
    {
        init();
    }

    void init()
    {
        if ( initialized )
            return;

        if (globalConfiguration == null)
        {
            GameObject globalGO = GameObject.Find("FixedGlobalConfiguration");
            globalConfiguration = globalGO.GetComponent<GlobalConfiguration>();
        }

        if (globalConfiguration == null)
        {
            ConfigManager.WriteConsoleError($"[RoomConfiguration.Start] Global Configuration isn't assigned, (check if IntroGallery is loaded) can't continue");
            initialized = true;
            return;
        }

        initialized = true;
        if (fileMonitor == null)
            fileMonitor = GetComponent<FileMonitor>();
        yamlPath = ConfigManager.ConfigDir + "/" + fileMonitor.FileName;

        OnEnable();
        Load();

    }

    private void mergeWithGlobalAndAssign(ConfigInformation config)
    {
        //merge with global
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] merge with global configuration");
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] config to be merged: {config.ToString()}");
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] global to merge to: {globalConfiguration.Configuration?.ToString()}");
        Configuration = (ConfigInformation)ConfigInformation.Merge(globalConfiguration.Configuration, config);
        ConfigManager.WriteConsole($"[RoomConfiguration.mergeWithGlobalAndAssign] final configuration: {Configuration.ToString()}");
    }

    public string GetName()
    {
        return Path.GetFileNameWithoutExtension(fileMonitor?.FileName);
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

    private void Load()
    {
        ConfigInformation config;

        ConfigManager.WriteConsole($"[RoomConfiguration.Load] load: {yamlPath}");
        if (File.Exists(yamlPath))
        {
            fileMonitor.fileLock();
            config = ConfigInformation.fromYaml(yamlPath);
            fileMonitor.fileUnlock();
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
        fileMonitor.fileLock();
        configuration.ToYaml(yamlPath);
        fileMonitor.fileUnlock();
    }

    void addListener()
    {
        if (isListenerAdded) return;
        fileMonitor?.OnFileChanged.AddListener(OnFileChanged);
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        fileMonitor?.OnFileChanged.RemoveListener(OnFileChanged);
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
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

    void OnGlobalConfigChanged()
    {
        ConfigManager.WriteConsole($"[RoomConfiguration] room: {Room} global config changed, reload: {yamlPath}");
        Load();
    }

    void OnFileChanged()
    {
        ConfigManager.WriteConsole($"[RoomConfiguration] room: {Room} file changed, reload: {yamlPath}");
        Load();
    }
}
