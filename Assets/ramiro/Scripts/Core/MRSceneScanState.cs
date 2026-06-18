/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using Meta.XR.MRUtilityKit;

/// <summary>True when MRUK loaded a scanned room with usable anchors (not physics/Y=0 fallback).</summary>
public static class MRSceneScanState
{
    public const string ScanRoomMenuOption = "SCAN ROOM";

    public static bool IsRoomScanned()
    {
        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        MRUKRoom room = surfaces != null ? surfaces.CurrentRoom : null;
        if (room == null && MRUK.Instance != null)
            room = MRUK.Instance.GetCurrentRoom();

        return IsScannedRoom(room, surfaces);
    }

    /// <summary>Floor + at least one wall — enough for placement and EffectMesh.</summary>
    public static bool HasUsableRoom(MRUKRoom room)
    {
        if (room == null || room.Anchors == null || room.Anchors.Count == 0)
            return false;

        if (room.FloorAnchor == null)
            return false;

        return room.WallAnchors != null && room.WallAnchors.Count > 0;
    }

    public static bool IsScannedRoom(MRUKRoom room, MREnvironmentSurfaces surfaces)
    {
        if (!HasUsableRoom(room))
            return false;

        return surfaces == null || surfaces.UsesMrukAnchors;
    }
}
