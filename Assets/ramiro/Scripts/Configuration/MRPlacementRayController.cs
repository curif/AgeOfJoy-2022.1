/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Runtime placement ray used to reposition placed MR objects.
/// Supports Floor, Wall, Ceiling, and Table surfaces.
/// </summary>
public class MRPlacementRayController : MonoBehaviour
{
    const string LogPrefix = "[MRPlacementRayController]";

    [SerializeField] float maxDistanceMeters = 5f;
    [Tooltip("Wall-only fine adjust after auto-facing fix. Keep near zero.")]
    [Range(-30f, 30f)]
    [SerializeField] float wallMountYawOffsetDegrees = 0f;
    [SerializeField] float defaultStickRotationSpeed = 90f;
    [SerializeField] float stickDeadZone = 0.15f;
    [SerializeField] Color validColor = new Color(0.25f, 1f, 0.55f, 1f);
    [SerializeField] Color invalidColor = new Color(1f, 0.3f, 0.2f, 1f);

    [Header("Ray visual (Built-in RP)")]
    [SerializeField] float rayWidthAtOrigin = 0.004f;
    [SerializeField] float rayWidthAtHit = 0.022f;
    [SerializeField] int rayTubeSides = 12;
    [SerializeField] float rayOriginAlpha = 0.12f;
    [SerializeField] float haloWidthMultiplier = 2.4f;
    [SerializeField] float haloAlpha = 0.28f;
    [SerializeField] float invalidPulseSpeed = 4f;
    [Tooltip("Minimum reticle radius when object bounds are unavailable (e.g. small props).")]
    [SerializeField] float hitReticleRadiusMin = 0.06f;
    [Tooltip("Scale for game cabinets (circumscribed footprint).")]
    [SerializeField] float hitReticleCabinetPadding = 1.2f;
    [Tooltip("Scale for environment props with MRPlacementProfile (tighter footprint).")]
    [SerializeField] float hitReticlePropPadding = 0.92f;
    [SerializeField] int hitReticleSegments = 28;
    [SerializeField] float hitReticleSurfaceOffset = 0.002f;

    [Tooltip("Ignore cancel/confirm briefly after begin (phone-booth explosion / menu confirm edges).")]
    [SerializeField] float inputGraceSeconds = 1.5f;
    [SerializeField] float confirmGraceSeconds = 0.35f;

    GameObject movingTarget;
    PlacementSurfaceType surfaceType;
    PlacementFacingAxis facingAxis = PlacementFacingAxis.PositiveZ;
    bool allowStickRotation;
    PlacementStickRotationAxis stickRotationAxis = PlacementStickRotationAxis.WorldYaw;
    float stickRotationSpeed;
    float initialYawDegrees;
    float userYawOffsetDegrees;
    Action<Vector3, Quaternion, Guid> onConfirmPose;
    Action onCancel;

    MeshRenderer beamRenderer;
    Mesh beamMesh;
    MeshRenderer haloRenderer;
    Mesh haloMesh;
    LineRenderer hitReticle;
    Material rayMaterial;
    bool isActive;
    static int s_activeCount;

    /// <summary>True while any placement ray is moving an object (shared singleton instance).</summary>
    public static bool AnyActive => s_activeCount > 0;

    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 previewPosition;
    Quaternion previewRotation;
    Vector3 previewSurfaceNormal = Vector3.up;
    bool hasValidPreview;
    Guid previewAnchorUuid = Guid.Empty;
    float ignoreCancelUntilUnscaledTime;
    float ignoreConfirmUntilUnscaledTime;
    float nextInvalidPreviewLogTime;
    bool hadValidPreviewThisSession;

    public bool IsActive => isActive;
    public GameObject MovingTarget => movingTarget;

    public void BeginMove(
        GameObject target,
        PlacementSurfaceType placementSurfaceType,
        PlacementFacingAxis objectFacingAxis,
        Action<Vector3, Quaternion, Guid> confirmCallback,
        Action cancelCallback = null)
    {
        if (target == null || confirmCallback == null)
            return;

        MRPlacementProfile profile = MRPlacementProfile.Resolve(target);

        movingTarget = target;
        surfaceType = placementSurfaceType;
        facingAxis = objectFacingAxis;
        ResolveStickRotation(profile, placementSurfaceType, out allowStickRotation, out stickRotationAxis, out stickRotationSpeed);
        onConfirmPose = confirmCallback;
        onCancel = cancelCallback;

        startPosition = target.transform.position;
        startRotation = target.transform.rotation;
        previewPosition = startPosition;
        previewRotation = startRotation;
        hasValidPreview = false;
        previewAnchorUuid = Guid.Empty;
        initialYawDegrees = NormalizeYaw(startRotation.eulerAngles.y);
        userYawOffsetDegrees = 0f;
        hadValidPreviewThisSession = false;
        nextInvalidPreviewLogTime = 0f;
        float now = Time.unscaledTime;
        ignoreCancelUntilUnscaledTime = now + inputGraceSeconds;
        ignoreConfirmUntilUnscaledTime = now + confirmGraceSeconds;

        EnsureRayVisuals();
        SetRayVisualVisible(true);
        isActive = true;
        s_activeCount++;

        if (placementSurfaceType == PlacementSurfaceType.Wall && allowStickRotation)
            SeedWallStickOffsetFromStartRotation();

        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        MRTransitionLog.LogStep("PlacementRay", $"begin {target.name} surface={surfaceType}");
        ConfigManager.WriteConsole(
            $"{LogPrefix} begin move target={target.name} surface={surfaceType} facing={facingAxis} " +
            $"stickRot={allowStickRotation} stickAxis={stickRotationAxis} " +
            $"mrukReady={(surfaces != null && surfaces.IsReady)} mrukAnchors={(surfaces != null && surfaces.UsesMrukAnchors)}");
    }

    public bool AllowsStickRotation => isActive && allowStickRotation;

    public void CancelActive()
    {
        if (!isActive)
            return;

        if (movingTarget != null)
            movingTarget.transform.SetPositionAndRotation(startPosition, startRotation);

        StopMove(cancelled: true, reason: "external");
    }

    /// <summary>
    /// Prefab profile wins when present. Floor objects without profile (game cabinets) default to world-Y stick rotation.
    /// </summary>
    void ResolveStickRotation(
        MRPlacementProfile profile,
        PlacementSurfaceType placementSurface,
        out bool stickEnabled,
        out PlacementStickRotationAxis rotationAxis,
        out float rotationSpeed)
    {
        if (profile != null)
        {
            stickEnabled = profile.allowStickRotation;
            rotationAxis = profile.stickRotationAxis;
            rotationSpeed = profile.stickRotationSpeed > 0f
                ? profile.stickRotationSpeed
                : defaultStickRotationSpeed;
            return;
        }

        // Game cabinets are not prefabs — floor/ceiling placement defaults to yaw on world Y.
        stickEnabled = placementSurface == PlacementSurfaceType.Floor
            || placementSurface == PlacementSurfaceType.Ceiling
            || placementSurface == PlacementSurfaceType.Table;
        rotationAxis = PlacementStickRotationAxis.WorldYaw;
        rotationSpeed = defaultStickRotationSpeed;
    }

    public static bool ExpectsStickRotationHint(MRPlacementProfile profile, PlacementSurfaceType placementSurface)
    {
        if (profile != null)
            return profile.allowStickRotation;
        return placementSurface == PlacementSurfaceType.Floor
            || placementSurface == PlacementSurfaceType.Ceiling
            || placementSurface == PlacementSurfaceType.Table;
    }

    void Update()
    {
        if (!isActive || movingTarget == null)
            return;

        if (allowStickRotation)
            ApplyStickRotationInput();

        UpdatePreviewPose();
        DrawRay();

        if (Time.unscaledTime >= ignoreCancelUntilUnscaledTime && WasCancelPressed())
        {
            movingTarget.transform.SetPositionAndRotation(startPosition, startRotation);
            StopMove(cancelled: true, reason: "grip");
            return;
        }

        if (!hasValidPreview && Time.unscaledTime >= nextInvalidPreviewLogTime)
        {
            nextInvalidPreviewLogTime = Time.unscaledTime + 3f;
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no {surfaceType} hit — aim mouse at surface; confirm=RMB cancel=Esc");
            MRTransitionLog.LogWarning($"PlacementRay no {surfaceType} preview for {movingTarget.name}");
        }

        if (Time.unscaledTime >= ignoreConfirmUntilUnscaledTime
            && WasConfirmPressed()
            && hasValidPreview)
        {
            movingTarget.transform.SetPositionAndRotation(previewPosition, previewRotation);
            MRTransitionLog.LogStep(
                "PlacementRay",
                $"confirm {movingTarget.name} pos={previewPosition} rotY={previewRotation.eulerAngles.y:F1} anchor={previewAnchorUuid}");
            onConfirmPose?.Invoke(previewPosition, previewRotation, previewAnchorUuid);
            StopMove(cancelled: false, reason: "confirm");
        }
    }

    void UpdatePreviewPose()
    {
        ResolvePointer(out Vector3 rayOrigin, out Vector3 rayDir, out Vector3 viewerPosition);
        Transform viewpoint = Camera.main != null ? Camera.main.transform : null;

        bool ok = false;
        Guid hitAnchorUuid = Guid.Empty;
        Vector3 worldPos = movingTarget.transform.position;
        Quaternion worldRot = movingTarget.transform.rotation;

        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        switch (surfaceType)
        {
            case PlacementSurfaceType.Wall:
                if (surfaces != null)
                {
                    Meta.XR.MRUtilityKit.MRUKAnchor wallAnchor = null;
                    float wallDepth = ResolveWallMountDepthMeters();
                    ok = surfaces.TryGetWallMountedFramePoseForPlacementRay(
                        rayOrigin,
                        rayDir,
                        viewpoint,
                        maxDistanceMeters,
                        wallDepth,
                        out worldPos,
                        out worldRot,
                        out wallAnchor,
                        facingAxis);
                    if (ok)
                    {
                        MRAnchorPoseResolver.TryGetUuid(wallAnchor, out hitAnchorUuid);
                        worldRot = PlacementOrientation.EnsureFacingViewer(
                            worldRot, facingAxis, worldPos, viewerPosition);
                        worldRot *= Quaternion.Euler(0f, wallMountYawOffsetDegrees, 0f);
                        if (allowStickRotation)
                        {
                            worldRot = PlacementOrientation.ApplyStickRotationOffset(
                                worldRot, stickRotationAxis, userYawOffsetDegrees);
                        }
                    }
                }
                break;

            case PlacementSurfaceType.Ceiling:
                if (surfaces != null && surfaces.TryGetCeilingPointFromRay(
                        rayOrigin, rayDir, maxDistanceMeters, out Vector3 ceilingPoint, out Meta.XR.MRUtilityKit.MRUKAnchor ceilingAnchor))
                {
                    MRAnchorPoseResolver.TryGetUuid(ceilingAnchor, out hitAnchorUuid);
                    worldPos = ceilingPoint;
                    if (allowStickRotation)
                    {
                        Quaternion baseYaw = Quaternion.Euler(0f, initialYawDegrees, 0f);
                        worldRot = PlacementOrientation.ApplyStickRotationOffset(
                            baseYaw, stickRotationAxis, userYawOffsetDegrees);
                    }
                    else
                    {
                        Vector3 look = viewerPosition - worldPos;
                        look.y = 0f;
                        if (look.sqrMagnitude < 0.001f)
                            look = Vector3.forward;
                        worldRot = PlacementOrientation.LookRotationWithFacing(look, facingAxis, Vector3.up);
                        worldRot = PlacementOrientation.EnsureFacingViewer(
                            worldRot, facingAxis, worldPos, viewerPosition);
                    }

                    ApplyCeilingPivotOffset(movingTarget, ref worldPos, worldRot);
                    ok = true;
                }
                break;

            case PlacementSurfaceType.Table:
                if (surfaces != null && surfaces.TryGetTablePointFromRay(
                        rayOrigin, rayDir, maxDistanceMeters, out Vector3 tablePoint, out Meta.XR.MRUtilityKit.MRUKAnchor tableAnchor))
                {
                    MRAnchorPoseResolver.TryGetUuid(tableAnchor, out hitAnchorUuid);
                    worldPos = tablePoint;
                    if (allowStickRotation)
                    {
                        Quaternion baseYaw = Quaternion.Euler(0f, initialYawDegrees, 0f);
                        worldRot = PlacementOrientation.ApplyStickRotationOffset(
                            baseYaw, stickRotationAxis, userYawOffsetDegrees);
                    }
                    else
                    {
                        Vector3 look = viewerPosition - worldPos;
                        look.y = 0f;
                        if (look.sqrMagnitude < 0.001f)
                            look = Vector3.forward;
                        worldRot = PlacementOrientation.LookRotationWithFacing(look, facingAxis, Vector3.up);
                        worldRot = PlacementOrientation.EnsureFacingViewer(
                            worldRot, facingAxis, worldPos, viewerPosition);
                    }

                    ApplyFloorPivotOffset(movingTarget, ref worldPos, worldRot);
                    ok = true;
                }
                break;

            case PlacementSurfaceType.Floor:
            default:
                if (surfaces != null && surfaces.TryGetFloorPointFromRay(
                        rayOrigin, rayDir, maxDistanceMeters, out Vector3 floorPoint, out Meta.XR.MRUtilityKit.MRUKAnchor floorAnchor))
                {
                    MRAnchorPoseResolver.TryGetUuid(floorAnchor, out hitAnchorUuid);
                    worldPos = floorPoint;
                    if (allowStickRotation)
                    {
                        Quaternion baseYaw = Quaternion.Euler(0f, initialYawDegrees, 0f);
                        worldRot = PlacementOrientation.ApplyStickRotationOffset(
                            baseYaw, stickRotationAxis, userYawOffsetDegrees);
                    }
                    else
                    {
                        Vector3 look = viewerPosition - worldPos;
                        look.y = 0f;
                        if (look.sqrMagnitude < 0.001f)
                            look = Vector3.forward;
                        worldRot = PlacementOrientation.LookRotationWithFacing(look, facingAxis, Vector3.up);
                        worldRot = PlacementOrientation.EnsureFacingViewer(
                            worldRot, facingAxis, worldPos, viewerPosition);
                    }

                    ApplyFloorPivotOffset(movingTarget, ref worldPos, worldRot);
                    MRLayoutRegistry.ApplyFloorCabinetDisplayOffset(PlacementSurfaceType.Floor, ref worldPos);
                    ok = true;
                }
                break;
        }

        hasValidPreview = ok;
        if (ok)
            hadValidPreviewThisSession = true;
        previewAnchorUuid = hitAnchorUuid;
        previewPosition = worldPos;
        previewRotation = worldRot;
        if (ok)
            previewSurfaceNormal = ResolvePreviewSurfaceNormal(worldRot);

        if (ok)
            movingTarget.transform.SetPositionAndRotation(previewPosition, previewRotation);
    }

    Vector3 ResolvePreviewSurfaceNormal(Quaternion worldRot)
    {
        switch (surfaceType)
        {
            case PlacementSurfaceType.Ceiling:
                return Vector3.down;
            case PlacementSurfaceType.Wall:
                return worldRot * PlacementOrientation.LocalForward(facingAxis);
            case PlacementSurfaceType.Table:
            case PlacementSurfaceType.Floor:
            default:
                return Vector3.up;
        }
    }

    void DrawRay()
    {
        if (beamRenderer == null)
            return;

        ResolvePointer(out Vector3 rayOrigin, out Vector3 rayDir, out _);
        Vector3 rayEnd = hasValidPreview ? previewPosition : rayOrigin + rayDir * maxDistanceMeters;

        Color beamColor = hasValidPreview ? validColor : invalidColor;
        float endAlpha = beamColor.a;
        if (!hasValidPreview)
            endAlpha *= 0.55f + 0.45f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * invalidPulseSpeed));

        Color startBeam = beamColor;
        startBeam.a = rayOriginAlpha;
        Color endBeam = beamColor;
        endBeam.a = endAlpha;

        UpdateTubeMesh(
            beamMesh,
            rayOrigin,
            rayEnd,
            rayWidthAtOrigin,
            rayWidthAtHit,
            rayTubeSides,
            startBeam,
            endBeam);

        if (haloRenderer != null && haloMesh != null)
        {
            Color startHalo = startBeam;
            startHalo.a *= haloAlpha;
            Color endHalo = endBeam;
            endHalo.a *= haloAlpha;
            UpdateTubeMesh(
                haloMesh,
                rayOrigin,
                rayEnd,
                rayWidthAtOrigin * haloWidthMultiplier,
                rayWidthAtHit * haloWidthMultiplier,
                rayTubeSides,
                startHalo,
                endHalo);
        }

        if (hitReticle != null)
        {
            bool showReticle = hasValidPreview;
            hitReticle.enabled = showReticle;
            if (showReticle)
            {
                float reticleRadius = ResolveHitReticleRadius();
                DrawHitReticle(previewPosition, previewSurfaceNormal, beamColor, reticleRadius);
            }
        }
    }

    void ResolvePointer(out Vector3 rayOrigin, out Vector3 rayDirection, out Vector3 viewerPosition)
    {
#if UNITY_EDITOR
        if (Application.isEditor && TryResolveEditorMousePointer(out rayOrigin, out rayDirection, out viewerPosition))
            return;
#endif

        Transform pointer = ResolveRightControllerTransform();
        Transform viewer = Camera.main != null ? Camera.main.transform : pointer;

        if (pointer != null)
        {
            rayOrigin = pointer.position;
            rayDirection = pointer.forward;
        }
        else if (viewer != null)
        {
            rayOrigin = viewer.position;
            rayDirection = viewer.forward;
        }
        else
        {
            rayOrigin = transform.position;
            rayDirection = transform.forward;
        }

        viewerPosition = viewer != null ? viewer.position : rayOrigin;
        if (rayDirection.sqrMagnitude < 0.001f)
            rayDirection = Vector3.forward;
        rayDirection.Normalize();
    }

#if UNITY_EDITOR
    static bool TryResolveEditorMousePointer(
        out Vector3 rayOrigin,
        out Vector3 rayDirection,
        out Vector3 viewerPosition)
    {
        rayOrigin = default;
        rayDirection = default;
        viewerPosition = default;

        if (ResolveRightControllerTransform() != null)
            return false;

        Camera cam = Camera.main;
        if (cam == null)
            return false;

        viewerPosition = cam.transform.position;

        if (MREditorInput.TryGetMouseScreenRay(cam, out Ray mouseRay))
        {
            rayOrigin = mouseRay.origin;
            rayDirection = mouseRay.direction.normalized;
            return true;
        }

        rayOrigin = cam.transform.position;
        rayDirection = cam.transform.forward;
        return true;
    }
#endif

    static Transform ResolveRightControllerTransform()
    {
        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls != null && controls.RightHand != null)
            return controls.RightHand.transform;

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.xrorigin != null && pc.xrorigin.transform != null)
        {
            Transform fallback = FindDeepChildByNameContains(pc.xrorigin.transform, "right");
            if (fallback != null)
                return fallback;
        }

        return null;
    }

    static Transform FindDeepChildByNameContains(Transform root, string token)
    {
        if (root == null || string.IsNullOrEmpty(token))
            return null;

        string normalized = token.ToLowerInvariant();
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            string name = t.name.ToLowerInvariant();
            if (name.Contains(normalized) && (name.Contains("controller") || name.Contains("hand")))
                return t;
        }

        return null;
    }

    void StopMove(bool cancelled, string reason)
    {
        if (isActive && s_activeCount > 0)
            s_activeCount--;

        if (cancelled)
            onCancel?.Invoke();

        SetRayVisualVisible(false);
        isActive = false;
        movingTarget = null;
        onConfirmPose = null;
        onCancel = null;
        hasValidPreview = false;
        previewAnchorUuid = Guid.Empty;

        MRTransitionLog.LogStep(
            "PlacementRay",
            $"end cancelled={cancelled} reason={reason} hadPreview={hadValidPreviewThisSession}");
        ConfigManager.WriteConsole(
            $"{LogPrefix} end move cancelled={cancelled} reason={reason} hadPreview={hadValidPreviewThisSession}");
    }

    static void ApplyFloorPivotOffset(GameObject target, ref Vector3 floorPoint, Quaternion worldRotation)
    {
        if (target == null)
            return;

        BoxCollider box = target.GetComponentInChildren<BoxCollider>();
        if (box == null)
            return;

        Transform t = target.transform;
        t.SetPositionAndRotation(
            new Vector3(floorPoint.x, floorPoint.y, floorPoint.z),
            worldRotation);

        float bottomY = PlaceOnFloorFromBoxCollider.CalculateLowerPointY(t, box);
        float pivotToBottom = t.position.y - bottomY;
        floorPoint = new Vector3(floorPoint.x, floorPoint.y + pivotToBottom, floorPoint.z);
    }

    static void ApplyCeilingPivotOffset(GameObject target, ref Vector3 ceilingPoint, Quaternion worldRotation)
    {
        if (target == null)
            return;

        BoxCollider box = target.GetComponentInChildren<BoxCollider>();
        if (box == null)
            return;

        Transform t = target.transform;
        t.SetPositionAndRotation(
            new Vector3(ceilingPoint.x, ceilingPoint.y, ceilingPoint.z),
            worldRotation);

        float topY = CalculateUpperPointY(t, box);
        float pivotToTop = topY - t.position.y;
        ceilingPoint = new Vector3(ceilingPoint.x, ceilingPoint.y - pivotToTop, ceilingPoint.z);
    }

    static float CalculateUpperPointY(Transform transform, BoxCollider boxCollider)
    {
        Vector3 colliderCenter = transform.TransformPoint(boxCollider.center);
        Vector3 colliderHalfSize = Vector3.Scale(boxCollider.size * 0.5f, transform.lossyScale);
        return colliderCenter.y + colliderHalfSize.y;
    }

    void ApplyStickRotationInput()
    {
        float stickX = ReadRightStickX();
        if (Mathf.Abs(stickX) <= stickDeadZone)
            return;

        userYawOffsetDegrees += stickX * stickRotationSpeed * Time.deltaTime;
    }

    static float ReadRightStickX()
    {
#if UNITY_EDITOR
        if (MREditorInput.IsHeld(KeyCode.A) || MREditorInput.IsHeld(KeyCode.LeftArrow))
            return -1f;
        if (MREditorInput.IsHeld(KeyCode.D) || MREditorInput.IsHeld(KeyCode.RightArrow))
            return 1f;
        return MREditorInput.GamepadStickX();
#else
        return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;
#endif
    }

    static float NormalizeYaw(float yaw)
    {
        yaw %= 360f;
        if (yaw < 0f)
            yaw += 360f;
        return yaw;
    }

    void EnsureRayVisuals()
    {
        if (beamRenderer == null)
            CreateTubeVisual("PlacementRayBeam", out beamRenderer, out beamMesh);

        if (haloRenderer == null)
            CreateTubeVisual("PlacementRayHalo", out haloRenderer, out haloMesh);

        if (hitReticle == null)
        {
            GameObject reticleGo = new GameObject("PlacementRayReticle");
            reticleGo.transform.SetParent(transform, false);
            hitReticle = reticleGo.AddComponent<LineRenderer>();
            ConfigureReticleLine(hitReticle);
            hitReticle.loop = true;
            hitReticle.positionCount = Mathf.Max(4, hitReticleSegments + 1);
        }
    }

    void CreateTubeVisual(string objectName, out MeshRenderer renderer, out Mesh mesh)
    {
        GameObject tubeGo = new GameObject(objectName);
        tubeGo.transform.SetParent(transform, false);

        MeshFilter meshFilter = tubeGo.AddComponent<MeshFilter>();
        mesh = new Mesh { name = objectName + "Mesh" };
        mesh.MarkDynamic();
        meshFilter.sharedMesh = mesh;

        renderer = tubeGo.AddComponent<MeshRenderer>();
        ConfigureTubeRenderer(renderer);
    }

    void ConfigureTubeRenderer(MeshRenderer renderer)
    {
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.material = ResolveRayMaterial();
    }

    void ConfigureReticleLine(LineRenderer beam)
    {
        beam.useWorldSpace = true;
        beam.shadowCastingMode = ShadowCastingMode.Off;
        beam.receiveShadows = false;
        beam.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        beam.textureMode = LineTextureMode.Stretch;
        beam.alignment = LineAlignment.View;
        beam.numCapVertices = 4;
        beam.numCornerVertices = 4;
        beam.widthMultiplier = 1f;
        beam.widthCurve = AnimationCurve.Linear(0f, rayWidthAtHit * 0.35f, 1f, rayWidthAtHit * 0.35f);
        beam.material = ResolveRayMaterial();
        beam.positionCount = 0;
    }

    static void BuildPerpendicularBasis(Vector3 forward, out Vector3 right, out Vector3 up)
    {
        Vector3 reference = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) < 0.99f
            ? Vector3.up
            : Vector3.right;
        right = Vector3.Cross(reference, forward).normalized;
        up = Vector3.Cross(forward, right);
    }

    static void UpdateTubeMesh(
        Mesh mesh,
        Vector3 start,
        Vector3 end,
        float radiusStart,
        float radiusEnd,
        int sides,
        Color colorStart,
        Color colorEnd)
    {
        if (mesh == null)
            return;

        sides = Mathf.Max(3, sides);
        Vector3 axis = end - start;
        float length = axis.magnitude;
        if (length < 0.0001f)
        {
            mesh.Clear();
            return;
        }

        Vector3 forward = axis / length;
        BuildPerpendicularBasis(forward, out Vector3 right, out Vector3 up);

        int vertexCount = sides * 2;
        var vertices = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        var colors = new Color[vertexCount];
        var triangles = new int[sides * 6];

        for (int i = 0; i < sides; i++)
        {
            float angle = (i / (float)sides) * Mathf.PI * 2f;
            Vector3 radial = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);

            vertices[i] = start + radial * radiusStart;
            vertices[i + sides] = end + radial * radiusEnd;
            normals[i] = radial;
            normals[i + sides] = radial;
            colors[i] = colorStart;
            colors[i + sides] = colorEnd;

            int next = (i + 1) % sides;
            int bottom0 = i;
            int bottom1 = next;
            int top0 = i + sides;
            int top1 = next + sides;

            int tri = i * 6;
            triangles[tri + 0] = bottom0;
            triangles[tri + 1] = top0;
            triangles[tri + 2] = bottom1;
            triangles[tri + 3] = bottom1;
            triangles[tri + 4] = top0;
            triangles[tri + 5] = top1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    Material ResolveRayMaterial()
    {
        if (rayMaterial != null)
            return rayMaterial;

        Shader shader = Shader.Find("Particles/Additive");
        if (shader == null)
            shader = Shader.Find("Mobile/Particles/Additive");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        rayMaterial = new Material(shader);
        return rayMaterial;
    }

    static Gradient BuildRayGradient(Color baseColor, float startAlpha, float endAlpha)
    {
        Color start = baseColor;
        start.a = startAlpha;
        Color end = baseColor;
        end.a = endAlpha;

        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(start, 0f),
                new GradientColorKey(end, 1f),
            },
            new[]
            {
                new GradientAlphaKey(start.a, 0f),
                new GradientAlphaKey(end.a, 1f),
            });
        return gradient;
    }

    float ResolveHitReticleRadius()
    {
        if (movingTarget == null || !hasValidPreview)
            return hitReticleRadiusMin;

        bool isGameCabinet = IsGameCabinet(movingTarget);
        if (!TryGetPlacementBounds(movingTarget, isGameCabinet, out Bounds bounds))
            return hitReticleRadiusMin;

        Vector3 normal = previewSurfaceNormal;
        if (normal.sqrMagnitude < 0.001f)
            normal = Vector3.up;
        normal.Normalize();

        float footprint = ComputePlanarFootprintRadius(
            bounds,
            previewPosition,
            normal,
            useCircumscribed: isGameCabinet);
        float padding = isGameCabinet ? hitReticleCabinetPadding : hitReticlePropPadding;
        return Mathf.Max(hitReticleRadiusMin, footprint * padding);
    }

    static bool IsGameCabinet(GameObject target) =>
        target != null && target.GetComponentInChildren<Cabinet>(true) != null;

    static float ComputePlanarFootprintRadius(
        Bounds bounds,
        Vector3 anchor,
        Vector3 normal,
        bool useCircumscribed)
    {
        BuildPerpendicularBasis(normal, out Vector3 tangent, out Vector3 bitangent);

        float maxAbsU = 0f;
        float maxAbsV = 0f;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        for (int ix = -1; ix <= 1; ix += 2)
        {
            for (int iy = -1; iy <= 1; iy += 2)
            {
                for (int iz = -1; iz <= 1; iz += 2)
                {
                    Vector3 corner = center + new Vector3(extents.x * ix, extents.y * iy, extents.z * iz);
                    Vector3 delta = corner - anchor;
                    delta -= normal * Vector3.Dot(delta, normal);
                    maxAbsU = Mathf.Max(maxAbsU, Mathf.Abs(Vector3.Dot(delta, tangent)));
                    maxAbsV = Mathf.Max(maxAbsV, Mathf.Abs(Vector3.Dot(delta, bitangent)));
                }
            }
        }

        if (useCircumscribed)
            return Mathf.Sqrt(maxAbsU * maxAbsU + maxAbsV * maxAbsV);

        return Mathf.Max(maxAbsU, maxAbsV);
    }

    static bool TryGetPlacementBounds(GameObject target, bool isGameCabinet, out Bounds bounds)
    {
        bounds = default;
        if (target == null)
            return false;

        bool hasCollider = TryGetSolidColliderBounds(target, out Bounds colliderBounds);
        bool hasRenderer = TryGetVisualRendererBounds(target, out Bounds rendererBounds);

        if (isGameCabinet)
        {
            if (hasCollider && hasRenderer)
            {
                bounds = colliderBounds;
                bounds.Encapsulate(rendererBounds);
                return true;
            }

            if (hasRenderer)
            {
                bounds = rendererBounds;
                return true;
            }

            if (hasCollider)
            {
                bounds = colliderBounds;
                return true;
            }

            return false;
        }

        if (hasRenderer)
        {
            bounds = rendererBounds;
            return true;
        }

        if (hasCollider)
        {
            bounds = colliderBounds;
            return true;
        }

        return false;
    }

    static bool TryGetSolidColliderBounds(GameObject target, out Bounds bounds)
    {
        bounds = default;
        if (target == null)
            return false;

        bool hasBounds = false;
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !collider.enabled || collider.isTrigger)
                continue;

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }

    static bool TryGetVisualRendererBounds(GameObject target, out Bounds bounds)
    {
        bounds = default;
        if (target == null)
            return false;

        bool hasBounds = false;
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled || !IsVisualRenderer(renderer))
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

        return hasBounds;
    }

    static bool IsVisualRenderer(Renderer renderer) =>
        renderer is MeshRenderer
        || renderer is SkinnedMeshRenderer
        || renderer is SpriteRenderer;

    void DrawHitReticle(Vector3 center, Vector3 normal, Color color, float radius)
    {
        if (hitReticle == null)
            return;

        int segments = Mathf.Max(4, hitReticleSegments);
        if (hitReticle.positionCount != segments + 1)
            hitReticle.positionCount = segments + 1;

        if (normal.sqrMagnitude < 0.001f)
            normal = Vector3.up;
        normal.Normalize();

        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.001f)
            tangent = Vector3.Cross(normal, Vector3.right);
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(normal, tangent);

        Vector3 reticleCenter = center + normal * hitReticleSurfaceOffset;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 offset = (Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent) * radius;
            hitReticle.SetPosition(i, reticleCenter + offset);
        }

        hitReticle.colorGradient = BuildRayGradient(color, color.a, color.a);
    }

    void SetRayVisualVisible(bool visible)
    {
        if (beamRenderer != null)
            beamRenderer.enabled = visible;
        if (haloRenderer != null)
            haloRenderer.enabled = visible;
        if (hitReticle != null)
            hitReticle.enabled = visible && hasValidPreview;
    }

    void OnDestroy()
    {
        if (beamMesh != null)
            Destroy(beamMesh);
        if (haloMesh != null)
            Destroy(haloMesh);
        if (rayMaterial != null)
            Destroy(rayMaterial);
    }

    static bool WasConfirmPressed()
    {
#if UNITY_EDITOR
        return MREditorInput.WasMouseRightPressed();
#else
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
#endif
    }

    static bool WasCancelPressed()
    {
#if UNITY_EDITOR
        return MREditorInput.WasAnyPressed(KeyCode.Escape, KeyCode.Backspace);
#else
        // Right Y (Button.Two) is CRT back / menu — grip only avoids accidental cancel.
        return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
#endif
    }

    float ResolveWallMountDepthMeters()
    {
        MRPlacementProfile profile = MRPlacementProfile.Resolve(movingTarget);
        return profile != null ? profile.GetWallMountDepthMeters() : 0.25f;
    }

    void SeedWallStickOffsetFromStartRotation()
    {
        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        if (surfaces == null)
            return;

        Transform viewpoint = Camera.main != null ? Camera.main.transform : null;
        Vector3 eye = viewpoint != null ? viewpoint.position : startPosition + Vector3.forward;
        Vector3 toPoster = startPosition - eye;
        toPoster.y = 0f;
        if (toPoster.sqrMagnitude < 0.001f)
            toPoster = Vector3.forward;

        float depth = ResolveWallMountDepthMeters();
        if (!surfaces.TryGetWallMountedFramePoseForPlacementRay(
                eye,
                toPoster.normalized,
                viewpoint,
                maxDistanceMeters,
                depth,
                out _,
                out Quaternion autoRot,
                out _,
                facingAxis))
            return;

        autoRot = PlacementOrientation.EnsureFacingViewer(autoRot, facingAxis, startPosition, eye);
        userYawOffsetDegrees = Mathf.DeltaAngle(autoRot.eulerAngles.y, startRotation.eulerAngles.y);
    }
}
