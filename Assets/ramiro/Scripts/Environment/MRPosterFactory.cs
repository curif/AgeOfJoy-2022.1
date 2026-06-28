/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>Spawns MR wall posters from Resources/ramiro/Poster.fbx.</summary>
public static class MRPosterFactory
{
    const string LogPrefix = "[MRPosterFactory]";

    public const string PosterPrefabId = "MRWallPoster";
    public const string PosterMeshResourcePath = "ramiro/Poster";
    public const float WallMountDepthMeters = 0.002f;

    static GameObject cachedPosterMeshPrefab;

    public static bool TryInstantiate(
        string textureRelativePath,
        Vector3 worldPosition,
        Quaternion worldRotation,
        out GameObject root)
    {
        root = null;
        if (string.IsNullOrEmpty(textureRelativePath))
            return false;

        GameObject prefab = LoadPosterMeshPrefab();
        if (prefab == null)
        {
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} missing mesh at Resources/{PosterMeshResourcePath}");
            MRDebugLog.LogError($"Poster mesh missing: Resources/{PosterMeshResourcePath}");
            return false;
        }

        root = Object.Instantiate(prefab);
        root.name = PosterPrefabId;
        root.transform.SetPositionAndRotation(worldPosition, worldRotation);
        root.transform.SetParent(null, true);

        StripColliders(root);

        MRPlacementProfile profile = root.GetComponent<MRPlacementProfile>();
        if (profile == null)
            profile = root.AddComponent<MRPlacementProfile>();

        profile.surfaceType = PlacementSurfaceType.Wall;
        profile.facingAxis = PlacementFacingAxis.PositiveZ;
        profile.displayName = "Wall poster";
        profile.wallMountDepthMeters = WallMountDepthMeters;
        profile.allowStickRotation = true;
        profile.stickRotationAxis = PlacementStickRotationAxis.WorldYaw;
        profile.stickRotationSpeed = 90f;

        MRWallPoster poster = root.GetComponent<MRWallPoster>();
        if (poster == null)
            poster = root.AddComponent<MRWallPoster>();

        poster.Configure(textureRelativePath);
        return true;
    }

    static GameObject LoadPosterMeshPrefab()
    {
        if (cachedPosterMeshPrefab == null)
            cachedPosterMeshPrefab = Resources.Load<GameObject>(PosterMeshResourcePath);

        return cachedPosterMeshPrefab;
    }

    static void StripColliders(GameObject root)
    {
        if (root == null)
            return;

        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            Object.Destroy(collider);
    }
}
