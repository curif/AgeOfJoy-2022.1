/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>Bookshelf magazine visual proxy that spawns the real readable magazine in the player's hand.</summary>
[DisallowMultipleComponent]
public class MRBookshelfMagazineProxy : MonoBehaviour
{
    const string LogPrefix = "[MRBookshelfMagazineProxy]";
    public const float ShelfReturnRadiusMeters = 0.25f;
    static readonly string[] GrabInteractionLayers = { "InteractablePart" };
    const string GrabPhysicsLayerName = "InteractablePart";
    static readonly Quaternion ShelfDockLocalRotation = Quaternion.Euler(-80f, 0f, 0f);

    string issueName;
    XRSimpleInteractable interactable;
    readonly List<Renderer> cachedRenderers = new List<Renderer>();
    readonly List<Collider> cachedColliders = new List<Collider>();
    readonly List<Material> runtimeMaterials = new List<Material>();
    Texture2D frontCoverTexture;
    Texture2D backCoverTexture;
    GameObject preparedMagazineRoot;
    XRGrabInteractable preparedGrabInteractable;
    MagazineGrab preparedMagazineGrab;
    MRSpawnedShelfMagazine preparedShelfMagazine;
    bool proxyHidden;
    bool spawnPending;
    bool magazineLooseInWorld;

    public void Configure(string configuredIssueName)
    {
        issueName = configuredIssueName;
        CacheVisualsAndColliders();
        ApplyProxyCoverTextures();
        DisableProxyInteraction();
        EnsurePreparedMagazine();
        RestoreProxyVisuals();
    }

    public void RestoreProxyVisuals()
    {
        ShowProxyRenderers();
        ReturnPreparedMagazineToShelf();
    }

    public Vector3 GetDockWorldPosition() => transform.position;

    public bool IsMagazineLooseInWorld() => magazineLooseInWorld;

    public void ShowProxyAndLeaveMagazineInWorld()
    {
        magazineLooseInWorld = true;
        ShowProxyRenderers();

        if (preparedMagazineRoot == null)
            return;

        SetMagazineVisualState(show: true);

#if UNITY_EDITOR
        preparedMagazineGrab?.SetEditorSimulateGrab(true);
#endif
    }

    public void ReturnPreparedMagazineToShelf()
    {
        magazineLooseInWorld = false;
        ShowProxyRenderers();
        DockPreparedMagazine();
    }

    void ShowProxyRenderers()
    {
        proxyHidden = false;
        spawnPending = false;

        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }
    }

    public bool TrySpawnHeldMagazineForEditor()
    {
#if UNITY_EDITOR
        if (spawnPending || proxyHidden || magazineLooseInWorld || string.IsNullOrEmpty(issueName))
            return false;

        if (!TrySpawnHeldMagazine(interactor: null, out GameObject heldMagazine))
            return false;

        MagazineGrab magazineGrab = heldMagazine != null ? heldMagazine.GetComponent<MagazineGrab>() : null;
        if (magazineGrab == null)
        {
            MRDebugLog.LogError($"{LogPrefix} editor grab failed: MagazineGrab missing for {issueName}");
            RestoreProxyVisuals();
            if (heldMagazine != null)
                DockPreparedMagazine();
            return false;
        }

        OnPreparedMagazineGrabbed();
        magazineGrab.BeginEditorGrabExternally();
        return true;
#else
        return false;
#endif
    }

    void CacheVisualsAndColliders()
    {
        cachedRenderers.Clear();
        cachedColliders.Clear();

        cachedRenderers.AddRange(GetComponentsInChildren<Renderer>(true));
        foreach (Collider collider in GetComponentsInChildren<Collider>(true))
        {
            if (collider != null && !collider.isTrigger)
                cachedColliders.Add(collider);
        }

        if (cachedColliders.Count == 0)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                box = gameObject.AddComponent<BoxCollider>();
            FitBoxColliderToRenderers(box);
            cachedColliders.Add(box);
        }
    }

    void DisableProxyInteraction()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
            interactable.enabled = false;

        foreach (Collider collider in cachedColliders)
        {
            if (collider != null)
                collider.enabled = false;
        }
    }

    void OnDestroy()
    {
        if (preparedMagazineRoot != null)
            Destroy(preparedMagazineRoot);

        ClearRuntimeResources();
    }

    bool TrySpawnHeldMagazine(IXRSelectInteractor interactor, out GameObject heldMagazine)
    {
        heldMagazine = null;

        if (!MRMagazineCatalog.IssueExists(issueName))
        {
            MRDebugLog.LogError($"{LogPrefix} issue has no page images: {issueName}");
            return false;
        }

        if (!EnsurePreparedMagazine())
            return false;

        preparedMagazineRoot.SetActive(true);
        heldMagazine = preparedMagazineRoot;
        return true;
    }

    public void OnPreparedMagazineGrabbed()
    {
        if (preparedMagazineRoot == null)
            return;

        bool wasLooseInWorld = magazineLooseInWorld;
        magazineLooseInWorld = false;
        preparedMagazineRoot.SetActive(true);
        if (!wasLooseInWorld)
            ApplyDockPose(worldPositionStays: true);
        SetMagazineVisualState(show: true);
        preparedShelfMagazine?.BeginHeldSession();
        HideProxyVisuals();

#if UNITY_EDITOR
        preparedMagazineGrab?.SetEditorSimulateGrab(false);
#endif
    }

    void HideProxyVisuals()
    {
        proxyHidden = true;

        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    void ApplyProxyCoverTextures()
    {
        ClearRuntimeResources();

        if (!MRMagazineIssueDefinition.TryResolve(issueName, out MRMagazineIssueDefinition definition))
        {
            MRDebugLog.LogWarning($"{LogPrefix} could not load issue definition for proxy: {issueName}");
            return;
        }

        frontCoverTexture = MRMagazineCatalog.LoadPageTextureByFileName(issueName, definition.GetFrontCover());
        backCoverTexture = MRMagazineCatalog.LoadPageTextureByFileName(issueName, definition.GetBackCover());

        bool appliedNamedCovers = false;
        Transform frontCover = FindChildByName("FrontCover") ?? FindChildByName("LeftCover");
        Transform backCover = FindChildByName("BackCover") ?? FindChildByName("RightCover");

        if (frontCover != null)
        {
            ApplyTextureToRenderer(frontCover.GetComponentInChildren<Renderer>(true), frontCoverTexture, frontSide: true);
            appliedNamedCovers = true;
        }

        if (backCover != null)
        {
            ApplyTextureToRenderer(backCover.GetComponentInChildren<Renderer>(true), backCoverTexture, frontSide: false);
            appliedNamedCovers = true;
        }

        if (appliedNamedCovers)
            return;

        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.materials;
            if (materials == null || materials.Length < 2)
                continue;

            ApplyTextureToMaterial(materials[0], frontCoverTexture, frontSide: true);
            ApplyTextureToMaterial(materials[1], backCoverTexture, frontSide: false);
            renderer.materials = materials;
            runtimeMaterials.AddRange(materials);
            return;
        }
    }

    void ApplyTextureToRenderer(Renderer renderer, Texture2D texture, bool frontSide)
    {
        if (renderer == null || texture == null)
            return;

        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
            ApplyTextureToMaterial(materials[i], texture, frontSide);
        renderer.materials = materials;
        runtimeMaterials.AddRange(materials);
    }

    static void ApplyTextureToMaterial(Material material, Texture2D texture, bool frontSide)
    {
        if (material == null || texture == null)
            return;

        material.mainTexture = texture;

        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);

        if (frontSide)
        {
            if (material.HasProperty("_FrontTex"))
                material.SetTexture("_FrontTex", texture);
            if (material.HasProperty("_PageFront"))
                material.SetTexture("_PageFront", texture);
        }
        else
        {
            if (material.HasProperty("_BackTex"))
                material.SetTexture("_BackTex", texture);
            if (material.HasProperty("_PageBack"))
                material.SetTexture("_PageBack", texture);
        }
    }

    Transform FindChildByName(string childName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (string.Equals(child.name, childName, StringComparison.Ordinal))
                return child;
        }

        return null;
    }

    void FitBoxColliderToRenderers(BoxCollider box)
    {
        if (box == null || cachedRenderers.Count == 0)
            return;

        bool hasBounds = false;
        Bounds bounds = default;
        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            return;

        box.center = transform.InverseTransformPoint(bounds.center);
        Vector3 lossy = transform.lossyScale;
        box.size = new Vector3(
            bounds.size.x / Mathf.Max(0.0001f, lossy.x),
            bounds.size.y / Mathf.Max(0.0001f, lossy.y),
            bounds.size.z / Mathf.Max(0.0001f, lossy.z));
    }

    void ClearRuntimeResources()
    {
        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            if (runtimeMaterials[i] != null)
                Destroy(runtimeMaterials[i]);
        }
        runtimeMaterials.Clear();

        if (frontCoverTexture != null)
            Destroy(frontCoverTexture);
        if (backCoverTexture != null)
            Destroy(backCoverTexture);
        frontCoverTexture = null;
        backCoverTexture = null;
    }

    bool EnsurePreparedMagazine()
    {
        if (preparedMagazineRoot == null)
        {
            GameObject magazinePrefab = MREnvironmentCatalog.LoadMagazinePrefab();
            if (magazinePrefab == null)
            {
                ConfigManager.WriteConsoleError(
                    $"{LogPrefix} magazine prefab missing: {MREnvironmentCatalog.MagazinePrefabName}");
                MRDebugLog.LogError($"{LogPrefix} magazine prefab missing: {MREnvironmentCatalog.MagazinePrefabName}");
                return false;
            }

            Transform parent = transform.parent != null ? transform.parent : transform;
            preparedMagazineRoot = Instantiate(magazinePrefab, transform.position, transform.rotation, parent);
            preparedMagazineRoot.name = $"{MREnvironmentCatalog.MagazinePrefabName}_{issueName}_Prepared";
        }

        Magazine magazine = preparedMagazineRoot.GetComponent<Magazine>();
        if (magazine == null)
        {
            MRDebugLog.LogError($"{LogPrefix} prepared magazine missing Magazine component for {issueName}");
            return false;
        }

        magazine.ConfigureIssue(issueName);
        magazine.CloseMagazine();

        preparedGrabInteractable = preparedMagazineRoot.GetComponent<XRGrabInteractable>();
        preparedMagazineGrab = preparedMagazineRoot.GetComponent<MagazineGrab>();
        if (preparedMagazineGrab != null)
        {
            preparedMagazineGrab.SetReturnOnRelease(false);
#if UNITY_EDITOR
            preparedMagazineGrab.SetEditorSimulateGrab(false);
#endif
        }

        preparedShelfMagazine = preparedMagazineRoot.GetComponent<MRSpawnedShelfMagazine>();
        if (preparedShelfMagazine == null)
            preparedShelfMagazine = preparedMagazineRoot.AddComponent<MRSpawnedShelfMagazine>();
        preparedShelfMagazine.Configure(this);

        DockPreparedMagazine();
        return preparedGrabInteractable != null;
    }

    public void DockPreparedMagazine()
    {
        if (preparedMagazineRoot == null)
            return;

        Transform parent = transform.parent != null ? transform.parent : transform;
        preparedMagazineRoot.transform.SetParent(parent, worldPositionStays: true);
        ApplyDockPose(worldPositionStays: false);

        Magazine magazine = preparedMagazineRoot.GetComponent<Magazine>();
        magazine?.ResetMagazine();

        SetMagazineVisualState(show: false);
        preparedMagazineGrab?.NotifyPlacementPoseUpdated();
        StartCoroutine(RehidePreparedMagazineNextFrame());
    }

    void SetMagazineVisualState(bool show)
    {
        if (preparedMagazineRoot == null)
            return;

        SetPreparedPageObjectsActive(show);

        foreach (Renderer renderer in preparedMagazineRoot.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
                renderer.enabled = show;
        }
    }

    void SetPreparedPageObjectsActive(bool active)
    {
        if (preparedMagazineRoot == null)
            return;

        foreach (Transform child in preparedMagazineRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == null || child == preparedMagazineRoot.transform)
                continue;

            if (IsPageObjectName(child.name))
                child.gameObject.SetActive(active);
        }
    }

    static bool IsPageObjectName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return false;

        return string.Equals(objectName, "Page", StringComparison.Ordinal)
            || objectName.StartsWith("Page_", StringComparison.Ordinal);
    }

    void ApplyDockPose(bool worldPositionStays)
    {
        if (preparedMagazineRoot == null)
            return;

        if (worldPositionStays || preparedMagazineRoot.transform.parent == null)
        {
            Quaternion worldRotation = transform.rotation * ShelfDockLocalRotation;
            preparedMagazineRoot.transform.SetPositionAndRotation(transform.position, worldRotation);
            return;
        }

        preparedMagazineRoot.transform.localPosition = transform.localPosition;
        preparedMagazineRoot.transform.localRotation = transform.localRotation * ShelfDockLocalRotation;
    }

    IEnumerator RehidePreparedMagazineNextFrame()
    {
        yield return null;

        if (preparedMagazineRoot == null)
            yield break;

        if (preparedMagazineGrab != null && preparedMagazineGrab.IsHeldNow())
            yield break;

        SetMagazineVisualState(show: false);
    }

    static Transform ResolveEditorAttachTransform()
    {
#if UNITY_EDITOR
        if (Camera.main != null)
            return Camera.main.transform;

        Camera anyCamera = FindObjectOfType<Camera>();
        return anyCamera != null ? anyCamera.transform : null;
#else
        return null;
#endif
    }
}
