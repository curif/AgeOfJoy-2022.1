/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

#if UNITY_EDITOR
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Meta XR SDK 76 (UpdateManifestWithCodeSample) scans every AndroidManifest.xml under the
/// Gradle tree and logs an error when manifest/application is missing — common for androidlib
/// plugins (NetworkConfig, Firebase, etc.). Runs before Meta's post-generate hook and adds an
/// empty application node where needed.
/// </summary>
public sealed class MetaUpdateManifestWorkaround : IPostGenerateGradleAndroidProject
{
    const string LogPrefix = "[MetaUpdateManifestWorkaround]";

    public int callbackOrder => -200;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string searchRoot = Path.Combine(path, "..");
        if (!Directory.Exists(searchRoot))
            return;

        string[] manifestFiles = Directory.GetFiles(searchRoot, "AndroidManifest.xml", SearchOption.AllDirectories);
        int patched = 0;

        foreach (string manifestFile in manifestFiles)
        {
            if (EnsureApplicationTag(manifestFile))
                patched++;
        }

        if (patched > 0)
            Debug.Log($"{LogPrefix} added empty <application> to {patched} manifest(s) before Meta XR sample metadata pass");
    }

    static bool EnsureApplicationTag(string manifestPath)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(manifestPath);

        XmlElement manifestElement = doc.SelectSingleNode("/manifest") as XmlElement;
        if (manifestElement == null)
            return false;

        if (doc.SelectSingleNode("/manifest/application") != null)
            return false;

        manifestElement.AppendChild(doc.CreateElement("application"));
        doc.Save(manifestPath);
        Debug.Log($"{LogPrefix} patched {manifestPath}");
        return true;
    }
}
#endif
