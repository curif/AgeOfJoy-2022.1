using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MaterialsConfiguration
{
    public List<MaterialConfiguration> materials = new List<MaterialConfiguration>();

    public class MaterialConfiguration
    {
        public string name;
        public Dictionary<string, object> properties = new Dictionary<string, object>();
    }
}
