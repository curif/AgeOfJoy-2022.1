using System.Collections.Generic;

public class CoreConfig
{
    public CoreEnvironment environment { get; set; }

    public CoreConfig()
    {
    }

    public CoreConfig(CoreEnvironment _environment)
    {
        environment = _environment;
    }
}
