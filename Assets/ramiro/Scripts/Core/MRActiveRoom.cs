/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;

/// <summary>
/// Phase A: which MRUK room's layout files are bound for load/save/spawn.
/// BoundRoomId null → registries fall back to global MR/*.yaml paths.
/// </summary>
public static class MRActiveRoom
{
    const string LogPrefix = "[MRActiveRoom]";

    /// <summary>Room id currently bound for cabinets/objects layout paths.</summary>
    public static string BoundRoomId { get; private set; }

    public static bool HasBoundRoom => !string.IsNullOrEmpty(BoundRoomId);

    /// <summary>
    /// Bind layout paths to the device current room.
    /// Returns true when BoundRoomId changed (callers should reload/spawn).
    /// </summary>
    public static bool TryBindFromDevice()
    {
        if (!MRRoomIdentity.TryGetCurrentRoomId(out string roomId))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no usable room id — layouts use global fallback (bound={BoundRoomId ?? "none"})");
            return false;
        }

        return Bind(roomId);
    }

    /// <summary>
    /// Bind to <paramref name="roomId"/>. Returns true if the bound room changed.
    /// </summary>
    public static bool Bind(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return false;

        if (string.Equals(BoundRoomId, roomId, StringComparison.OrdinalIgnoreCase))
            return false;

        string previous = BoundRoomId;
        MRPaths.EnsureRoomLayouts(roomId);
        BoundRoomId = roomId;

        MRLayoutRegistry.Instance?.UnloadLayoutMemory();
        MREnvironmentRegistry.Instance?.UnloadLayoutMemory();
        MRSkyboxSettings.OnActiveRoomChanged();

        ConfigManager.WriteConsole(
            $"{LogPrefix} bound {(string.IsNullOrEmpty(previous) ? "(none)" : previous)} -> {roomId}");
        return true;
    }

    public static void Clear()
    {
        if (string.IsNullOrEmpty(BoundRoomId))
            return;

        ConfigManager.WriteConsole($"{LogPrefix} cleared bound room {BoundRoomId}");
        BoundRoomId = null;
        MRLayoutRegistry.Instance?.UnloadLayoutMemory();
        MREnvironmentRegistry.Instance?.UnloadLayoutMemory();
        MRSkyboxSettings.OnActiveRoomChanged();
    }
}
