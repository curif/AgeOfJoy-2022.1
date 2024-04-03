using System;
using System.IO;

public class Core
{
    public string name { get; private set; }
    public string library { get; private set; }

    private CoreConfig baseConfig;

    public Core(string _name, string _library, CoreConfig _coreConfig)
    {
        name = _name;
        library = _library;
        baseConfig = _coreConfig;
    }

    public Core(string name, string library) : this(name, library, null)
    {
    }

    public CoreConfig GetConfig()
    {
        // Optional global baseConfig serves as a template for the final configuration
        CoreConfig resultConfig = baseConfig != null ? baseConfig.Copy() : new CoreConfig();

        string configFile = getConfigFile();
        if (File.Exists(configFile))
        {
            try
            {
                // Append configuration from yaml file, if it exists
                CoreConfig fileConfig = YamlUtils.ParseOptional<CoreConfig>(configFile);
                if (fileConfig != null)
                {
                    ConfigManager.WriteConsole($"[Core]: Using configuration for {name} from {configFile}");
                    resultConfig.Merge(fileConfig);
                }
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsole($"[Core]: Error reading configuration for {name} in {configFile}: {e.Message}");
            }
        }
        else
        {
            ConfigManager.WriteConsole($"[Core]: Using default configuration for {name}");
        }

        return resultConfig;
    }

    private string getConfigFile()
    {
        return Path.Combine(ConfigManager.ConfigCoresDir, name + ".yaml");
    }
}
