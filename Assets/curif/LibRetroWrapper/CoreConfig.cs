using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreConfig
{
    public CoreEnvironment environment { get; set; }

    public CoreConfig()
    {
        environment = new CoreEnvironment();
    }

    public CoreConfig(string prefix, Dictionary<string, string> properties)
    {
        environment = new CoreEnvironment(prefix, properties);
    }

    public  class CoreEnvironment
    {
        public string prefix { get; set; }
        public Dictionary<string, string> properties { get; set; }

        public CoreEnvironment()
        {
            prefix = "";
            properties = new Dictionary<string, string>();
        }

        public CoreEnvironment(string prefix, Dictionary<string, string> properties)
        {
            this.prefix = prefix;
            this.properties = properties;
        }
    }
}
