/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using UnityEngine;

/// <summary>
/// Runtime placement ray used to reposition placed MR objects.
/// Supports Floor, Wall, and Ceiling surfaces.
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
    [SerializeField] Color validColor = new Color(0.2f, 1f, 0.35f, 1f);
    [SerializeField] Color invalidColor = new Color(1f, 0.25f, 0.25f, 1f);

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

    LineRenderer line;
    bool isActive;
    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 previewPosition;
    Quaternion previewRotation;
    bool hasValidPreview;
    Guid previewAnchorUuid = Guid.Empty;

    public bool IsActive => isActive;

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

        EnsureLineRenderer();
        SetLineVisible(true);
        isActive = true;

        ConfigManager.WriteConsole(
            $"{LogPrefix} begin move target={target.name} surface={surfaceType} facing={facingAxis} " +
            $"stickRot={allowStickRotation} stickAxis={stickRotationAxis}");
    }

    public bool AllowsStickRotation => isActive && allowStickRotation;

    public void CancelActive()
    {
        if (!isActive)
            return;

        if (movingTarget != null)
            movingTarget.transform.SetPositionAndRotation(startPosition, startRotation);

        StopMove(cancelled: true);
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
            || placementSurface == PlacementSurfaceType.Ceiling;
        rotationAxis = PlacementStickRotationAxis.WorldYaw;
        rotationSpeed = defaultStickRotationSpeed;
    }

    public static bool ExpectsStickRotationHint(MRPlacementProfile profile, PlacementSurfaceType placementSurface)
    {
        if (profile != null)
            return profile.allowStickRotation;
        return placementSurface == PlacementSurfaceType.Floor
            || placementSurface == PlacementSurfaceType.Ceiling;
    }

    void Update()
    {
        if (!isActive || movingTarget == null)
            return;

        if (allowStickRotation)
            ApplyStickRotationInput();

        UpdatePreviewPose();
        DrawRay();

        if (WasCancelPressed())
        {
            movingTarget.transform.SetPositionAndRotation(startPosition, startRotation);
            StopMove(cancelled: true);
            return;
        }

        if (WasConfirmPressed() && hasValidPreview)
        {
            movingTarget.transform.SetPositionAndRotation(previewPosition, previewRotation);
            onConfirmPose?.Invoke(previewPosition, previewRotation, previewAnchorUuid);
            StopMove(cancelled: false);
        }
    }

    void UpdatePreviewPose()
    {
        ResolvePointer(out Vector3 rayOrigin, out Vector3 rayDir, out Vector3 viewerPosition);

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
                    ok = surfaces.TryGetWallMountedFramePoseFromRay(
                        rayOrigin,
                        rayDir,
                        maxDistanceMeters,
                        0.25f,
                        out worldPos,
                        out worldRot,
                        out wallAnchor,
                        facingAxis);
                    if (ok)
                    {
                        MRAnchorPoseResolver.TryGetUuid(wallAnchor, out hitAnchorUuid);
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
        previewAnchorUuid = hitAnchorUuid;
        previewPosition = worldPos;
        previewRotation = worldRot;

        if (ok)
            movingTarget.transform.SetPositionAndRotation(previewPosition, previewRotation);
    }

    void DrawRay()
    {
        if (line == null)
            return;

        ResolvePointer(out Vector3 rayOrigin, out Vector3 rayDir, out _);
        line.positionCount = 2;
        line.SetPosition(0, rayOrigin);
        line.SetPosition(1, hasValidPreview ? previewPosition : rayOrigin + rayDir * maxDistanceMeters);
        line.startColor = hasValidPreview ? validColor : invalidColor;
        line.endColor = hasValidPreview ? validColor : invalidColor;
    }

    void ResolvePointer(out Vector3 rayOrigin, out Vector3 rayDirection, out Vector3 viewerPosition)
    {
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

    void StopMove(bool cancelled)
    {
        if (cancelled)
            onCancel?.Invoke();

        SetLineVisible(false);
        isActive = false;
        movingTarget = null;
        onConfirmPose = null;
        onCancel = null;
        hasValidPreview = false;
        previewAnchorUuid = Guid.Empty;

        ConfigManager.WriteConsole($"{LogPrefix} end move cancelled={cancelled}");
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
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            return -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            return 1f;
        return Input.GetAxisRaw("Horizontal");
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

    void EnsureLineRenderer()
    {
        if (line != null)
            return;

        line = GetComponent<LineRenderer>();
        if (line == null)
            line = gameObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.widthMultiplier = 0.008f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 0;
    }

    void SetLineVisible(bool visible)
    {
        if (line != null)
            line.enabled = visible;
    }

    static bool WasConfirmPressed()
    {
#if UNITY_EDITOR
        return Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0);
#else
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
#endif
    }

    static bool WasCancelPressed()
    {
#if UNITY_EDITOR
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace);
#else
        return OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)
            || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
#endif
    }
}
