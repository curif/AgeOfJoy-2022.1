/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MR game cabinets miss VR slot wiring: AgentScenePosition (attract video) and CabinetReplace
/// (control-map lookup when a coin starts Libretro).
/// </summary>
public static class MRGameCabinetAttractSetup
{
    const string LogPrefix = "[MRGameCabinetAttractSetup]";
    const string ZoneObjectName = "MRAgentPlayerPosition";

    static readonly Vector3 ZoneSize = new Vector3(3.6f, 2.6f, 3.6f);
    const float MinVideoDistanceMeters = 4f;
    const float PresenceRadiusMeters = 2.5f;

    public static void AttachAttractZone(
        GameObject cabinetRoot,
        Cabinet cabinet,
        CabinetInformation cabInfo,
        string cabinetDBName,
        int slotIndex,
        PlacementFacingAxis facingAxis = PlacementFacingAxis.PositiveZ)
    {
        if (cabinetRoot == null)
            return;

        Transform existing = cabinetRoot.transform.Find(ZoneObjectName);
        if (existing != null)
            Object.Destroy(existing.gameObject);

        MRPlacementProfile profile = cabinetRoot.GetComponentInChildren<MRPlacementProfile>(true);
        if (profile != null)
            facingAxis = profile.facingAxis;

        Vector3 playSide = -PlacementOrientation.WorldForward(cabinetRoot.transform.rotation, facingAxis);
        if (playSide.sqrMagnitude < 0.001f)
            playSide = -cabinetRoot.transform.forward;

        GameObject zoneGo = new GameObject(ZoneObjectName);
        zoneGo.transform.SetParent(cabinetRoot.transform, false);
        Vector3 localPlayOffset = cabinetRoot.transform.InverseTransformDirection(playSide.normalized);
        zoneGo.transform.localPosition = localPlayOffset * 0.75f + Vector3.up * 1.05f;
        zoneGo.transform.localRotation = Quaternion.identity;

        int agentLayer = LayerMask.NameToLayer("AgentPlayerPosition");
        if (agentLayer < 0)
            agentLayer = 7;
        zoneGo.layer = agentLayer;

        BoxCollider box = zoneGo.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = ZoneSize;
        box.center = Vector3.zero;

        Rigidbody body = zoneGo.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        AgentScenePosition agent = zoneGo.AddComponent<AgentScenePosition>();
        agent.BoxColliderHeight = ZoneSize.y;
        agent.playerStayDurationTimeSecs = 0f;

        MRAgentPlayerPresence presence = zoneGo.AddComponent<MRAgentPlayerPresence>();
        presence.Configure(cabinetRoot.transform, PresenceRadiusMeters);

        List<AgentScenePosition> positions = new List<AgentScenePosition> { agent };
        WireScreenControllers(cabinetRoot, positions);
        WireCabinetReplace(cabinetRoot, cabinet, cabInfo, cabinetDBName, slotIndex, positions);

        ConfigManager.WriteConsole($"{LogPrefix} gameplay context on {cabinetRoot.name} ({cabinetDBName}) facing={facingAxis}");
    }

    static void WireCabinetReplace(
        GameObject cabinetRoot,
        Cabinet cabinet,
        CabinetInformation cabInfo,
        string cabinetDBName,
        int slotIndex,
        List<AgentScenePosition> positions)
    {
        if (cabinet == null || string.IsNullOrEmpty(cabinetDBName))
            return;

        CabinetReplace replace = cabinetRoot.GetComponent<CabinetReplace>();
        if (replace == null)
            replace = cabinetRoot.AddComponent<CabinetReplace>();

        replace.cabinet = cabinet;
        replace.game = new CabinetPosition
        {
            CabinetDBName = cabinetDBName,
            Rom = cabInfo?.rom,
            Room = MixedRealityManager.MrRoomName,
            Position = slotIndex,
            CabInfo = cabInfo
        };
        replace.AgentPlayerPositionComponents = positions;
        replace.AgentPlayerPositionComponentsToLoad = positions;
    }

    static void WireScreenControllers(GameObject cabinetRoot, List<AgentScenePosition> positions)
    {
        LibretroScreenController[] libretroScreens =
            cabinetRoot.GetComponentsInChildren<LibretroScreenController>(true);
        foreach (LibretroScreenController screen in libretroScreens)
        {
            screen.AgentPlayerPositions = positions;
            if (screen.DistanceMaxToPlayerToActivateVideo < MinVideoDistanceMeters)
                screen.DistanceMaxToPlayerToActivateVideo = MinVideoDistanceMeters;
            if (screen.DistanceMaxToPlayerToActivateAudio < MinVideoDistanceMeters)
                screen.DistanceMaxToPlayerToActivateAudio = MinVideoDistanceMeters;
            screen.EnsureAttractLoopRunning();
        }

        AGEBasicScreenController[] ageScreens =
            cabinetRoot.GetComponentsInChildren<AGEBasicScreenController>(true);
        foreach (AGEBasicScreenController screen in ageScreens)
        {
            screen.AgentPlayerPositions = positions;
            if (screen.DistanceMinToPlayerToActivate < MinVideoDistanceMeters)
                screen.DistanceMinToPlayerToActivate = MinVideoDistanceMeters;
        }
    }
}
