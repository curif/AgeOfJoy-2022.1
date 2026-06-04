/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TestMRmanager only: spawn one game cabinet on Play to validate materials/skinning without MR menu flow.
/// </summary>
public class MRTestGameCabinetSpawn : MonoBehaviour
{
    const string LogPrefix = "[MRTestGameCabinetSpawn]";
    const string TestSceneName = "TestMRmanager";

    [Tooltip("Empty = first folder in cabinetsdb.")]
    [SerializeField] string cabinetDBName = "umk3";
    [SerializeField] float spawnDistanceMeters = 2.2f;
    [SerializeField] float startDelaySeconds = 0.35f;
    [SerializeField] float spawnHeightMeters = 0f;

    void Start()
    {
        if (SceneManager.GetActiveScene().name != TestSceneName)
            return;

        StartCoroutine(SpawnValidationCabinetWhenReady());
    }

    IEnumerator SpawnValidationCabinetWhenReady()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        MRCatalogBootstrap.PrepareCatalog(seedExampleInEditor: true);

        while (MRLayoutRegistry.Instance == null)
            yield return null;

        string cabName = ResolveCabinetName();
        if (string.IsNullOrEmpty(cabName))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} no cabinet in cabinetsdb — add a folder or example seed");
            yield break;
        }

        Transform origin = EnsureSpawnOrigin();
        ResolveSpawnPose(origin, out Vector3 worldPos, out Quaternion worldRot);

        if (!MRLayoutRegistry.Instance.TrySpawnValidationCabinet(
                cabName, origin, worldPos, worldRot, out GameObject spawnedRoot))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} spawn failed for {cabName}");
            yield break;
        }

        ConfigManager.WriteConsole(
            $"{LogPrefix} validation spawn '{cabName}' at {spawnedRoot.transform.position} (check materials in Scene/Game view)");
    }

    string ResolveCabinetName()
    {
        if (!string.IsNullOrWhiteSpace(cabinetDBName))
            return cabinetDBName.Trim();

        var names = MRLayoutRegistry.GetCatalogCabinetNames();
        return names.Count > 0 ? names[0] : null;
    }

    static Transform EnsureSpawnOrigin()
    {
        if (MixedRealityManager.Instance != null && MixedRealityManager.Instance.MRSpaceOrigin != null)
            return MixedRealityManager.Instance.MRSpaceOrigin;

        GameObject existing = GameObject.Find("MRTestSpawnOrigin");
        if (existing != null)
            return existing.transform;

        var originGo = new GameObject("MRTestSpawnOrigin");
        originGo.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        return originGo.transform;
    }

    void ResolveSpawnPose(Transform origin, out Vector3 worldPos, out Quaternion worldRot)
    {
        Transform view = Camera.main != null ? Camera.main.transform : null;
        if (view == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null && pc.xrorigin != null && pc.xrorigin.Camera != null)
                view = pc.xrorigin.Camera.transform;
        }

        if (view != null)
        {
            Vector3 forward = view.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();

            worldPos = view.position + forward * spawnDistanceMeters;
            worldPos.y = spawnHeightMeters;
            worldRot = Quaternion.LookRotation(-forward, Vector3.up);
            return;
        }

        worldPos = origin.position + origin.forward * spawnDistanceMeters;
        worldPos.y = spawnHeightMeters;
        worldRot = origin.rotation;
    }
}
