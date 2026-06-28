/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Applies room skin textures to MRUK EffectMesh anchors (floor, walls, furniture, etc.).
/// </summary>
public static class MRRoomSurfaceSkin
{
    const string LogPrefix = "[MRRoomSurfaceSkin]";
    const string MaterialResourcePath = "ramiro/Materials/MRRoomSkin";

    static readonly HashSet<string> activePackageNames =
        new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
    static readonly Dictionary<MeshRenderer, Material> runtimeMaterials = new Dictionary<MeshRenderer, Material>();
    static readonly List<Texture2D> ownedTextures = new List<Texture2D>();

    public static bool IsActive => runtimeMaterials.Count > 0;

    public static bool TryApplyPackage(string packageName)
    {
        if (!MRRoomSkinDefinition.TryLoad(packageName, out MRRoomSkinDefinition definition))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} package not found: {packageName}");
            return false;
        }

        return TryApplyDefinition(definition);
    }

    public static IEnumerator ApplyPackageWhenReady(string packageName, float timeoutSeconds = 15f)
    {
        if (!MREffectMeshSettings.AnchorMeshEnabled)
            MREffectMeshVisibility.SetAnchorMeshEnabled(true);

        MREffectMeshController controller = ResolveEffectMeshController();
        if (controller != null)
            yield return controller.WaitForAnchorMeshReady(timeoutSeconds);
        else
        {
            float remaining = timeoutSeconds;
            while (remaining > 0f)
            {
                if (IsAnchorMeshReady(controller))
                    break;

                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (!TryApplyPackage(packageName))
            ConfigManager.WriteConsoleWarning($"{LogPrefix} apply failed for {packageName}");
    }

    public static void Clear()
    {
        activePackageNames.Clear();

        foreach (KeyValuePair<MeshRenderer, Material> pair in runtimeMaterials)
        {
            if (pair.Key != null)
                pair.Key.sharedMaterial = null;

            if (pair.Value != null)
                UnityEngine.Object.Destroy(pair.Value);
        }

        runtimeMaterials.Clear();

        foreach (Texture2D texture in ownedTextures)
        {
            if (texture != null)
                UnityEngine.Object.Destroy(texture);
        }

        ownedTextures.Clear();

        MREffectMeshController controller = ResolveEffectMeshController();
        controller?.RestoreDefaultMeshMaterials();
        ConfigManager.WriteConsole($"{LogPrefix} cleared");
    }

    static bool TryApplyDefinition(MRRoomSkinDefinition definition)
    {
        MREffectMeshController controller = ResolveEffectMeshController();
        EffectMesh effectMesh = controller != null ? controller.GetAnchorEffectMesh() : null;
        if (effectMesh == null || effectMesh.EffectMeshObjects.Count == 0)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} anchor EffectMesh not ready");
            return false;
        }

        if (!definition.IsValid)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} invalid room skin definition '{definition.PackageName}'");
            return false;
        }

        return TryApplySingleTextureDefinition(definition, effectMesh);
    }

    static bool TryApplySingleTextureDefinition(MRRoomSkinDefinition definition, EffectMesh effectMesh)
    {
        Texture2D texture = LoadOwnedTexture(definition, definition.ResolvedTexturePath);
        if (texture == null)
            return false;

        IReadOnlyList<MRRoomSkinSurface> surfaces = definition.GetNormalizedSurfaces();
        int configured = 0;

        foreach (KeyValuePair<MRUKAnchor, EffectMesh.EffectMeshObject> pair in effectMesh.EffectMeshObjects)
        {
            MRUKAnchor anchor = pair.Key;
            EffectMesh.EffectMeshObject meshObject = pair.Value;
            if (anchor == null || meshObject?.effectMeshGO == null)
                continue;

            if (!AnchorMatchesSurfaces(anchor, surfaces))
                continue;

            if (!TryApplyTextureToRenderer(meshObject.effectMeshGO, texture))
                continue;

            configured++;
        }

        activePackageNames.Add(definition.PackageName);
        ConfigManager.WriteConsole(
            $"{LogPrefix} applied '{definition.PackageName}' on {configured} surface(s) " +
            $"texture={definition.ResolvedTexturePath} surfaces=[{string.Join(",", surfaces)}]");
        return configured > 0;
    }

    static bool TryApplyTextureToRenderer(GameObject meshObject, Texture2D texture)
    {
        MeshRenderer renderer = meshObject.GetComponent<MeshRenderer>();
        if (renderer == null)
            return false;

        if (runtimeMaterials.TryGetValue(renderer, out Material previousMaterial))
        {
            runtimeMaterials.Remove(renderer);
            if (previousMaterial != null)
                UnityEngine.Object.Destroy(previousMaterial);
        }

        Material material = CreateRuntimeMaterial(texture);
        renderer.sharedMaterial = material;
        renderer.SetPropertyBlock(null);
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        runtimeMaterials[renderer] = material;
        return true;
    }

    static bool AnchorMatchesSurfaces(MRUKAnchor anchor, IReadOnlyList<MRRoomSkinSurface> surfaces)
    {
        if (anchor == null || surfaces == null || surfaces.Count == 0)
            return false;

        foreach (MRRoomSkinSurface surface in surfaces)
        {
            if (SurfaceMatchesAnchor(surface, anchor))
                return true;
        }

        return false;
    }

    static bool SurfaceMatchesAnchor(MRRoomSkinSurface surface, MRUKAnchor anchor)
    {
        if (anchor == null)
            return false;

        switch (surface)
        {
            case MRRoomSkinSurface.Floor:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR);
            case MRRoomSkinSurface.Walls:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE);
            case MRRoomSkinSurface.Ceiling:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.CEILING);
            case MRRoomSkinSurface.Table:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE);
            case MRRoomSkinSurface.Couch:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.COUCH);
            case MRRoomSkinSurface.Bed:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.BED);
            case MRRoomSkinSurface.Door:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.DOOR_FRAME);
            case MRRoomSkinSurface.Window:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WINDOW_FRAME);
            case MRRoomSkinSurface.GlobalMesh:
                return anchor.HasAnyLabel(MRUKAnchor.SceneLabels.GLOBAL_MESH);
            default:
                return false;
        }
    }

    static Texture2D LoadOwnedTexture(MRRoomSkinDefinition definition, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        Texture2D texture = MRRoomSkinCatalog.LoadTexture(definition, relativePath);
        if (texture != null)
            ownedTextures.Add(texture);

        return texture;
    }

    static Material CreateRuntimeMaterial(Texture2D texture)
    {
        Material template = Resources.Load<Material>(MaterialResourcePath);
        Material material = template != null
            ? new Material(template)
            : new Material(Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default"));

        material.mainTexture = texture;
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);

        material.color = Color.white;
        return material;
    }

    static MREffectMeshController ResolveEffectMeshController()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        return manager != null ? manager.GetComponent<MREffectMeshController>() : null;
    }

    static bool IsAnchorMeshReady(MREffectMeshController controller)
    {
        if (controller == null)
            return false;

        EffectMesh effectMesh = controller.GetAnchorEffectMesh();
        return effectMesh != null && effectMesh.EffectMeshObjects.Count > 0;
    }
}
