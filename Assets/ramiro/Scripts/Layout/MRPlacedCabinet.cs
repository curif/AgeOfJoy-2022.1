/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Links a spawned MR cabinet instance to its mr-layout.yaml entry.</summary>
public class MRPlacedCabinet : MonoBehaviour
{
    public string PlacementId { get; private set; }
    public string CabinetDBName { get; private set; }

    public void Initialize(string placementId, string cabinetDbName)
    {
        PlacementId = placementId;
        CabinetDBName = cabinetDbName;
    }
}
