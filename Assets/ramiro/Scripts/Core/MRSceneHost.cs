/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>
/// FixedScene "MR" root — disabled children are enabled on demand instead of runtime Instantiate.
/// </summary>
public static class MRSceneHost
{
    const string LogPrefix = "[MRSceneHost]";

    public const string RootObjectName = "MR";
    public const string OvrManagerChildName = "OVRManager";
    public const string MrukChildName = "MRUK";
    public const string PassthroughLayerChildName = "OVRPassthroughLayer";
    public const string EffectMeshChildName = "EffectMesh";
    public const string EffectMeshGlobalChildName = "EffectMeshGlobalMesh";

    static Transform cachedRoot;

    public static bool HasSceneRoot => Root != null;

    public static Transform Root
    {
        get
        {
            if (cachedRoot == null)
                cachedRoot = FindRootTransform();
            return cachedRoot;
        }
    }

    public static void InvalidateCache() => cachedRoot = null;

    /// <summary>Enable MR infrastructure before passthrough / MRUK load.</summary>
    public static void PrepareForMr()
    {
        if (Root == null)
            return;

        SetChildActive(OvrManagerChildName, true);
        SetChildActive(MrukChildName, true);
        ConfigManager.WriteConsole($"{LogPrefix} PrepareForMr — OVRManager + MRUK enabled");
    }

    /// <summary>VR→MR phone booth: scan gate needs OVR + MRUK before immersive travel.</summary>
    public static void PrepareForPhoneBoothTravel()
    {
        PrepareForMr();
    }

    /// <summary>Disable MR-only scene objects when leaving MR.</summary>
    public static void SuspendForVr()
    {
        if (Root == null)
            return;

        SetChildActive(MrukChildName, false);
        SetChildActive(PassthroughLayerChildName, false);
        SetChildActive(EffectMeshChildName, false);
        SetChildActive(EffectMeshGlobalChildName, false);
        SetChildActive(OvrManagerChildName, false);
        ConfigManager.WriteConsole($"{LogPrefix} SuspendForVr — MR children disabled");
    }

    /// <summary>Gallery zone passthrough may need OVRManager before MR entry.</summary>
    public static bool EnsureOvrManagerActive()
    {
        GameObject ovrGo = GetChild(OvrManagerChildName);
        if (ovrGo != null && !ovrGo.activeSelf)
        {
            ovrGo.SetActive(true);
            ConfigManager.WriteConsole($"{LogPrefix} OVRManager enabled (on demand)");
        }

        return OVRManager.instance != null;
    }

    public static GameObject GetChild(string childName)
    {
        Transform root = Root;
        if (root == null)
            return null;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == childName)
                return child.gameObject;
        }

        return null;
    }

    public static T GetChildComponent<T>(string childName) where T : Component
    {
        GameObject go = GetChild(childName);
        return go != null ? go.GetComponent<T>() : null;
    }

    public static MRUK GetSceneMruk(bool activate = true)
    {
        GameObject mrukGo = GetChild(MrukChildName);
        if (mrukGo == null)
            return null;

        if (activate && !mrukGo.activeSelf)
            mrukGo.SetActive(true);

        return mrukGo.GetComponent<MRUK>();
    }

    public static EffectMesh GetSceneEffectMesh(string childName, bool activate = false)
    {
        GameObject meshGo = GetChild(childName);
        if (meshGo == null)
            return null;

        if (activate && !meshGo.activeSelf)
            meshGo.SetActive(true);

        return meshGo.GetComponent<EffectMesh>();
    }

    public static OVRPassthroughLayer GetPassthroughLayerTemplate()
    {
        return GetChildComponent<OVRPassthroughLayer>(PassthroughLayerChildName);
    }

    public static void StashEffectMeshUnderRoot(GameObject meshRoot)
    {
        if (meshRoot == null || Root == null)
            return;

        meshRoot.transform.SetParent(Root, false);
        meshRoot.transform.localPosition = Vector3.zero;
        meshRoot.transform.localRotation = Quaternion.identity;
        meshRoot.SetActive(false);
    }

    static void SetChildActive(string childName, bool active)
    {
        GameObject go = GetChild(childName);
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }

    static Transform FindRootTransform()
    {
        var scenes = UnityEngine.SceneManagement.SceneManager.GetAllScenes();
        for (int i = 0; i < scenes.Length; i++)
        {
            if (!scenes[i].isLoaded)
                continue;

            foreach (GameObject root in scenes[i].GetRootGameObjects())
            {
                if (root.name == RootObjectName)
                    return root.transform;
            }
        }

        return null;
    }
}
