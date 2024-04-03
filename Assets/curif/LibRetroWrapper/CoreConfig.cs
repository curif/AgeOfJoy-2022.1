using System.Collections.Generic;

public class CoreConfig
{
    public CoreEnvironment environment { get; set; }

    public CoreConfig(CoreEnvironment _environment)
    {
        environment = _environment;
    }

    public CoreConfig() : this(new CoreEnvironment())
    {
    }

    public CoreConfig(string prefix, Dictionary<string, string> properties) : this(new CoreEnvironment(prefix, properties))
    {
    }

    public CoreConfig Copy()
    {
        return new CoreConfig(environment.Copy());
    }

    public void Merge(CoreConfig other)
    {
        environment.Merge(other?.environment);
    }
}
