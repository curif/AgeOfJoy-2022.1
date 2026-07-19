/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// While a Libretro game is running in MR, suspend the global key light (shadows)
/// without touching the CRT Global Light preference. Ambient Flat stays on.
/// </summary>
public class MRGameplayLightingGate : MonoBehaviour
{
    const string LogPrefix = "[MRGameplayLightingGate]";
    const float PollIntervalSeconds = 0.25f;

    MRMrEnvironmentLighting lighting;
    float nextPollTime;
    bool lastGameRunning;

    void Awake()
    {
        lighting = GetComponent<MRMrEnvironmentLighting>();
    }

    void OnEnable()
    {
        nextPollTime = 0f;
        lastGameRunning = false;
    }

    void OnDisable()
    {
        if (lighting != null)
            lighting.SetKeySuspendedForGameplay(false);
    }

    void Update()
    {
        if (Time.unscaledTime < nextPollTime)
            return;

        nextPollTime = Time.unscaledTime + PollIntervalSeconds;
        Evaluate();
    }

    void Evaluate()
    {
        if (lighting == null)
            lighting = GetComponent<MRMrEnvironmentLighting>();
        if (lighting == null)
            return;

        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null || manager.CurrentMode == ExperienceMode.VR)
        {
            if (lastGameRunning || lighting.IsKeySuspendedForGameplay)
            {
                lighting.SetKeySuspendedForGameplay(false);
                lastGameRunning = false;
            }

            return;
        }

        bool gameRunning = LibretroMameCore.GameLoaded || LibretroFlycastCore.GameLoaded;
        if (gameRunning == lastGameRunning && gameRunning == lighting.IsKeySuspendedForGameplay)
            return;

        if (gameRunning != lastGameRunning)
        {
            ConfigManager.WriteConsole(
                $"{LogPrefix} gameRunning={gameRunning} (mame={LibretroMameCore.GameLoaded} flycast={LibretroFlycastCore.GameLoaded})");
            lastGameRunning = gameRunning;
        }

        lighting.SetKeySuspendedForGameplay(gameRunning);
    }
}
