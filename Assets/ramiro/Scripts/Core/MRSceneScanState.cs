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

    public static bool IsScannedRoom(MRUKRoom room, MREnvironmentSurfaces surfaces)
    {
        if (room == null)
            return false;

        if (room.Anchors == null || room.Anchors.Count == 0)
            return false;

        if (surfaces != null && surfaces.UsesMrukAnchors)
            return true;

        return room.FloorAnchor != null
            || room.CeilingAnchor != null
            || (room.WallAnchors != null && room.WallAnchors.Count > 0);
    }
}
