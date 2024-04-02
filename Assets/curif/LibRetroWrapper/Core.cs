using System;
using System.IO;

public class Core
{
    public string name { get; private set; }
    public string library { get; private set; }

    private CoreConfig config;

    public Core(string name, string library, CoreConfig coreConfig)
    {
        this.name = name;
        this.library = library;
        this.config = coreConfig;
    }

    public Core(string name, string library) : this(name, library, null)
    {
    }

    public CoreConfig GetConfig()
    {
        if (config != null)
        {
            return config;
        }

        string configFile = Path.Combine(ConfigManager.ConfigCoresDir, name + ".yaml");
        try
        {
            CoreConfig config = YamlUtils.ParseOptional<CoreConfig>(configFile);
            if (config != null)
            {
                ConfigManager.WriteConsole($"[Core]: Using configuration for {name} from {configFile}");
                return config;
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsole($"[Core]: Error reading configuration for {name} in {configFile}: {e.Message}");
        }
        ConfigManager.WriteConsole($"[Core]: Using default configuration for {name}");
        return new CoreConfig();
    }
}
