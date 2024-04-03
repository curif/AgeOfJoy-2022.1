using System.Collections.Generic;

public class CoreEnvironment
{
    public string prefix { get; set; }
    public Dictionary<string, string> properties { get; set; }

    public CoreEnvironment()
    {
        properties = new Dictionary<string, string>();
    }

    public CoreEnvironment(string _prefix, Dictionary<string, string> _properties)
    {
        prefix = _prefix;
        properties = _properties;
    }

    public CoreEnvironment Copy()
    {
        return new CoreEnvironment(prefix, new Dictionary<string, string>(properties));
    }

    public void Merge(CoreEnvironment other)
    {
        foreach (var pair in other.properties)
        {
            properties[pair.Key] = pair.Value;
        }
    }
}