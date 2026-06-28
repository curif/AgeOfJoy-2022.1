/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;

/// <summary>Anchor reference captured when a placement ray is confirmed (MRUK UUID or another placed object).</summary>
public readonly struct MRPlacementConfirmAnchor
{
    public Guid MrukUuid { get; }
    public string ObjectPlacementId { get; }
    public string ObjectAnchorPoint { get; }

    public bool IsObjectAnchor => !string.IsNullOrEmpty(ObjectPlacementId);
    public bool HasMrukUuid => MrukUuid != Guid.Empty;

    public MRPlacementConfirmAnchor(Guid mrukUuid, string objectPlacementId, string objectAnchorPoint)
    {
        MrukUuid = mrukUuid;
        ObjectPlacementId = objectPlacementId;
        ObjectAnchorPoint = objectAnchorPoint;
    }

    public static MRPlacementConfirmAnchor FromMruk(Guid uuid) =>
        new MRPlacementConfirmAnchor(uuid, null, null);

    public static MRPlacementConfirmAnchor FromObject(string placementId, string anchorPoint) =>
        new MRPlacementConfirmAnchor(Guid.Empty, placementId, anchorPoint ?? string.Empty);
}
