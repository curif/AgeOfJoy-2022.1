/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// MR cabinets: keep AgentScenePosition.IsPlayerPresent in sync by distance.
/// Unity CharacterController + runtime triggers are unreliable for dynamically spawned zones.
/// </summary>
public class MRAgentPlayerPresence : MonoBehaviour
{
    [SerializeField] Transform presenceOrigin;
    [SerializeField] float presenceRadiusMeters = 2.5f;

    AgentScenePosition agent;
    Transform player;

    public void Configure(Transform origin, float radiusMeters)
    {
        presenceOrigin = origin;
        presenceRadiusMeters = radiusMeters;
    }

    void Awake()
    {
        agent = GetComponent<AgentScenePosition>();
    }

    void Start()
    {
        ResolvePlayer();
    }

    void Update()
    {
        if (agent == null)
            return;

        if (player == null)
        {
            ResolvePlayer();
            if (player == null)
                return;
        }

        Vector3 origin = presenceOrigin != null ? presenceOrigin.position : transform.position;
        Vector3 playerPos = player.position;
        origin.y = playerPos.y = 0f;

        bool near = (playerPos - origin).sqrMagnitude <= presenceRadiusMeters * presenceRadiusMeters;
        agent.IsPlayerPresent = near;
        agent.IsPlayerColliding = near;
    }

    void ResolvePlayer()
    {
        if (player != null)
            return;

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
        {
            player = pc.PlayerControllerGameObject.transform;
            return;
        }

        GameObject tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged != null)
            player = tagged.transform;
    }
}
