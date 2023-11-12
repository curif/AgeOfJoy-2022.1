using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BasicInterpreter
{
    Dictionary<string, BasicValue> vars = new();

    void run()
    {
    }
}