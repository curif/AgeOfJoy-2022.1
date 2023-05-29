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
  public  ConfigInformation Configuration
  {
    get { return configuration; }
    set 
    { 
      configuration = value; 
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

  private void mergeWithGlobal(ConfigInformation config)
  {
    //merge with global
    ConfigManager.WriteConsole($"[RoomConfiguration] merge with global configuration:");
    ConfigManager.WriteConsole($"[RoomConfiguration] actual: {config.ToString()}");
    ConfigManager.WriteConsole($"[RoomConfiguration] global to merge to: {globalConfiguration.Configuration.ToString()}");
    Configuration = (ConfigInformation)ConfigInformation.Merge(globalConfiguration.Configuration, config);
    ConfigManager.WriteConsole($"[RoomConfiguration] final configuration: {Configuration.ToString()}");
  }

  public string GetName()
  {
    return Path.GetFileNameWithoutExtension(fileMonitor.ConfigFileName);
  }

  public void Reset()
  {
    try
    {
        if (File.Exists(yamlPath))
        {
            File.Delete(yamlPath);
            Load();
        }
    }
    catch (IOException e)
    {
      ConfigManager.WriteConsoleError($"[RoomConfiguration.Delete] {yamlPath} - {e}");
    }
  }

  private void Load()
  {
    ConfigInformation config;

    ConfigManager.WriteConsole($"[RoomConfiguration] load: {yamlPath}");
    if (File.Exists(yamlPath))
    {
      config = ConfigInformation.fromYaml(yamlPath);
      if (config == null) 
      {
        ConfigManager.WriteConsoleError($"[RoomConfiguration] can't read, using global: {yamlPath}");
        Configuration = globalConfiguration.Configuration; 
      }
      else {
        mergeWithGlobal(config);
      }
    }
    else {
      ConfigManager.WriteConsole($"[RoomConfiguration] file doesn't exists using global: {yamlPath}");
      Configuration = globalConfiguration.Configuration; 
    }

    ConfigManager.WriteConsole($"[RoomConfiguration] configuration established: ");
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
