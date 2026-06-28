/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Links a spawned MR environment prop to its MR/objects-layout.yaml entry.</summary>
public class MRPlacedEnvironment : MonoBehaviour
{
    public string PlacementId { get; private set; }
    public string PrefabName { get; private set; }
    public string PackageName { get; private set; }
    public MREnvironmentObjectSource Source { get; private set; }

    public void Initialize(string placementId, string prefabName)
    {
        PlacementId = placementId;
        PrefabName = prefabName;
        PackageName = null;
        Source = MREnvironmentObjectSource.Build;
    }

    public void Initialize(string placementId, MREnvironmentCatalogEntry entry)
    {
        PlacementId = placementId;
        Source = entry.Source;
        if (entry.Source == MREnvironmentObjectSource.Custom
            || entry.Source == MREnvironmentObjectSource.RoomSkin)
        {
            PackageName = entry.Key;
            PrefabName = null;
        }
        else
        {
            PrefabName = entry.Key;
            PackageName = null;
        }
    }
}
