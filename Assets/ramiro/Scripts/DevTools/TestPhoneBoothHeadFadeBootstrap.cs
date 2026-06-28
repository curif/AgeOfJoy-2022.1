/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TestPhoneBoothHeadFade scene: head fade by booth interior volume (no immersive travel).
/// Auto-installs when the scene loads.
/// </summary>
[DefaultExecutionOrder(-150)]
public class TestPhoneBoothHeadFadeBootstrap : MonoBehaviour
{
    public const string TestSceneName = "TestPhoneBoothHeadFade";

    const string LogPrefix = "[TestPhoneBoothHeadFade]";
    const string PayphoneObjectName = "PF_Payphone";

    [SerializeField] MRPhoneBoothPortal portal;
    [SerializeField] bool beginOnStart = true;

    MRPhoneBoothTravelHeadFade headFade;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallIfTestScene()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        if (Object.FindObjectOfType<TestPhoneBoothHeadFadeBootstrap>() != null)
            return;

        var root = new GameObject(nameof(TestPhoneBoothHeadFadeBootstrap));
        root.AddComponent<TestPhoneBoothHeadFadeBootstrap>();
    }

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        TestPhoneBoothHeadFadeEditorCamera.EnsureOnMainCamera();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        if (!beginOnStart)
            return;

        if (!TryResolvePortal(out MRPhoneBoothPortal resolvedPortal))
            return;

        portal = resolvedPortal;
        BoxCollider fadeZone = portal.FadeZoneCollider;
        if (fadeZone == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} fade zone missing on portal");
            return;
        }

        headFade = portal.GetComponent<MRPhoneBoothTravelHeadFade>();
        if (headFade == null)
            headFade = portal.gameObject.AddComponent<MRPhoneBoothTravelHeadFade>();

        headFade.BeginMonitoring(fadeZone);
        ConfigManager.WriteConsole(
            $"{LogPrefix} head fade ON — WASD move in/out of booth | RMB+mouse look");
    }

    void OnDestroy()
    {
        if (headFade != null && (headFade.IsMonitoring || headFade.IsForcedTravelBlackout))
            headFade.EndMonitoring();
    }

    bool TryResolvePortal(out MRPhoneBoothPortal resolvedPortal)
    {
        resolvedPortal = portal;
        if (resolvedPortal != null)
            return true;

        resolvedPortal = Object.FindObjectOfType<MRPhoneBoothPortal>();
        if (resolvedPortal != null)
            return true;

        GameObject booth = GameObject.Find(PayphoneObjectName);
        if (booth == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} {PayphoneObjectName} missing in scene");
            return false;
        }

        resolvedPortal = MRPhoneBoothPortal.EnsureOn(booth);
        return resolvedPortal != null;
    }
}
