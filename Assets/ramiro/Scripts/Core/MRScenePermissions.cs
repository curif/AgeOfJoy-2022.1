/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// <summary>Requests Meta spatial scene permission (com.oculus.permission.USE_SCENE) before MRUK LoadSceneFromDevice.</summary>
public static class MRScenePermissions
{
    const string LogPrefix = "[MRScenePermissions]";

    public static bool IsGranted { get; private set; }
    public static bool WasRequested { get; private set; }
    public static string StatusText { get; private set; } = "";

    public static void Reset()
    {
        IsGranted = false;
        WasRequested = false;
        StatusText = "";
    }

    public static IEnumerator EnsureGranted()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string permissionId = OVRPermissionsRequester.ScenePermission;

        yield return null;
        yield return new WaitForSeconds(0.35f);

        if (Permission.HasUserAuthorizedPermission(permissionId))
        {
            IsGranted = true;
            StatusText = "permissão espacial: OK";
            ConfigManager.WriteConsole($"{LogPrefix} USE_SCENE already granted");
            yield break;
        }

        WasRequested = true;
        StatusText = "aguardando permissão da sala...";
        ConfigManager.WriteConsole($"{LogPrefix} requesting USE_SCENE — system dialog should appear");

        bool dialogSettled = false;
        bool callbackGranted = false;

        void Settle(bool granted)
        {
            callbackGranted = granted;
            dialogSettled = true;
        }

        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += _ => Settle(true);
        callbacks.PermissionDenied += _ => Settle(false);
        callbacks.PermissionDeniedAndDontAskAgain += _ => Settle(false);

        Action<string> ovrGrantedHandler = id =>
        {
            if (id == permissionId)
                Settle(true);
        };
        OVRPermissionsRequester.PermissionGranted += ovrGrantedHandler;

        Permission.RequestUserPermission(permissionId, callbacks);

        const float timeoutSeconds = 60f;
        float remaining = timeoutSeconds;
        while (!dialogSettled && remaining > 0f)
        {
            if (Permission.HasUserAuthorizedPermission(permissionId))
            {
                Settle(true);
                break;
            }

            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        OVRPermissionsRequester.PermissionGranted -= ovrGrantedHandler;

        IsGranted = Permission.HasUserAuthorizedPermission(permissionId) || callbackGranted;
        StatusText = IsGranted
            ? "permissão espacial: OK"
            : "permissão espacial negada — Ajustes > Privacidade > Dados espaciais";

        ConfigManager.WriteConsole($"{LogPrefix} result granted={IsGranted} settled={dialogSettled} ({StatusText})");
#else
        IsGranted = true;
        StatusText = "editor";
        yield break;
#endif
    }
}
