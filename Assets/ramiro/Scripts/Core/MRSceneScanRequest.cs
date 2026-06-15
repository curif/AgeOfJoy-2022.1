/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>Launches Meta Quest Space Setup and reloads MRUK scene data.</summary>
public static class MRSceneScanRequest
{
    const string LogPrefix = "[MRSceneScanRequest]";

    public static IEnumerator RunSpaceSetupAndReload(Transform player)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        yield return MRScenePermissions.EnsureGranted();
        if (!MRScenePermissions.IsGranted)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} aborted — USE_SCENE not granted");
            yield break;
        }

        ConfigManager.WriteConsole($"{LogPrefix} opening Quest Space Setup");
        OVRTask<bool> setupTask = OVRScene.RequestSpaceSetup();

        yield return new WaitUntil(() => setupTask.IsCompleted);

        if (!setupTask.GetResult())
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} Space Setup returned false (unexpected failure)");
            yield break;
        }

        ConfigManager.WriteConsole($"{LogPrefix} Space Setup closed — reloading MRUK room");

        if (MREnvironmentSurfaces.Instance != null)
        {
            yield return MREnvironmentSurfaces.Instance.ProbeWhenReady(player);
        }
        else if (MRUK.Instance != null)
        {
            Task<MRUK.LoadDeviceResult> loadTask = MRUK.Instance.LoadSceneFromDevice(
                requestSceneCaptureIfNoDataFound: false,
                removeMissingRooms: true);

            while (!loadTask.IsCompleted)
                yield return null;
        }

        if (MixedRealityManager.Instance != null)
            yield return MixedRealityManager.Instance.RefreshEnvironmentAfterRoomScan(player);

        MRRoomInfoUI.Instance?.RefreshContent();
        ConfigManager.WriteConsole(
            $"{LogPrefix} reload done scanned={MRSceneScanState.IsRoomScanned()}");
#else
        ConfigManager.WriteConsoleWarning($"{LogPrefix} Space Setup runs on Quest device builds only");
        yield break;
#endif
    }
}
