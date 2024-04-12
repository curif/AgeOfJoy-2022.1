using System;
using System.IO;

public class Core
{
    public string Name { get; private set; }
    public string Library { get; private set; }
    public CoreEnvironment GlobalEnvironment { get; set; }

    public Core(string _name, string _library, CoreEnvironment _coreEnvironment)
    {
        Name = _name;
        Library = _library;
        GlobalEnvironment = _coreEnvironment;
    }

    public Core(string name, string library) : this(name, library, null)
    {
    }

    public CoreEnvironment ReadCoreEnvironment()
    {
        string configFile = getConfigFile();
        if (File.Exists(configFile))
        {
            try
            {
                // Append configuration from yaml file, if it exists
                CoreConfig fileConfig = YamlUtils.ParseOptional<CoreConfig>(configFile);
                if (fileConfig != null)
                {
                    ConfigManager.WriteConsole($"[Core]: Using configuration for {Name} from {configFile}");
                    return fileConfig.environment;
                }
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsole($"[Core]: Error reading configuration for {Name} in {configFile}: {e.Message}");
            }
        }
        return new CoreEnvironment();
    }

    private string getConfigFile()
    {
        return Path.Combine(ConfigManager.ConfigCoresDir, Name + ".yaml");
    }
}
