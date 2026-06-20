/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Reflection;
using Meta.XR.MRUtilityKit;
using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// MRUK SDK 72 requires a referenced OVRCameraRig to:
///   - convert anchor poses to world-space (pose.ComputeWorldPosition(trackingSpace));
///   - resolve the current room via _cameraRig.centerEyeAnchor.position.
/// Age of Joy uses XROrigin (XRI), not OVRCameraRig. We subclass OVRCameraRig and disable
/// its lifecycle so no MainCamera is spawned, then keep trackingSpace + centerEyeAnchor
/// aligned with the XROrigin every frame.
/// </summary>
public static class MRCameraRigShim
{
    const string LogPrefix = "[MRCameraRigShim]";
    const string ShimRootName = "MRUKCameraRigShim";

    static MRUKCameraRigStub shimRig;

    public static Transform ResolveXrTrackingOrigin(XROrigin origin)
    {
        if (origin == null)
            return null;
        if (origin.CameraFloorOffsetObject != null)
            return origin.CameraFloorOffsetObject.transform;
        if (origin.Camera != null && origin.Camera.transform.parent != null)
            return origin.Camera.transform.parent;
        return origin.transform;
    }

    public static bool EnsureAttachedTo(MRUK target)
    {
        if (target == null)
            return false;

        MRCameraRigAlignLog.EnsureSession("EnsureAttachedTo");

        OVRCameraRig existing = Object.FindObjectOfType<OVRCameraRig>();
        if (existing != null && !(existing is MRUKCameraRigStub))
        {
            AssignCameraRig(target, existing);
            MRCameraRigAlignLog.LogEvent("attached-existing-ovr-rig");
            return true;
        }

        if (shimRig == null)
            CreateShim();

        if (shimRig == null)
            return false;

        shimRig.SyncWithXrOrigin();
        AssignCameraRig(target, shimRig);
        MRCameraRigAlignLog.LogEvent("attached-shim");
        return true;
    }

    public static void Align()
    {
        if (shimRig != null)
            shimRig.SyncWithXrOrigin();
    }

    const float MinSnapDeltaMeters = 0.002f;

    /// <summary>
    /// Moves XROrigin so CameraFloorOffsetObject Y matches MRUK floor after VR scene unload
    /// recentering (phone booth → MR).
    /// </summary>
    public static bool TrySnapTrackingOriginY(float targetFloorWorldY, out float appliedDeltaY)
    {
        appliedDeltaY = 0f;
        XROrigin origin = Object.FindObjectOfType<XROrigin>();
        if (origin == null)
            return false;

        Transform tracking = ResolveXrTrackingOrigin(origin);
        if (tracking == null)
            return false;

        float deltaY = targetFloorWorldY - tracking.position.y;
        if (Mathf.Abs(deltaY) < MinSnapDeltaMeters)
            return false;

        origin.transform.position += Vector3.up * deltaY;
        appliedDeltaY = deltaY;
        Align();
        ConfigManager.WriteConsole(
            $"{LogPrefix} snapped tracking origin Y by {deltaY:F3}m (target floor Y={targetFloorWorldY:F3})");
        return true;
    }

    static void CreateShim()
    {
        GameObject rigGo = new GameObject(ShimRootName);
        Object.DontDestroyOnLoad(rigGo);
        shimRig = rigGo.AddComponent<MRUKCameraRigStub>();
        shimRig.BuildAnchors();
        ConfigManager.WriteConsole($"{LogPrefix} stub created (overrides OVRCameraRig lifecycle)");
        MRCameraRigAlignLog.LogEvent("shim-created");
    }

    static void AssignCameraRig(MRUK target, OVRCameraRig rig)
    {
        PropertyInfo prop = typeof(MRUK).GetProperty(
            "_cameraRig",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} MRUK._cameraRig property not found (SDK changed)");
            return;
        }

        MethodInfo setter = prop.GetSetMethod(true);
        if (setter == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} MRUK._cameraRig has no setter (SDK changed)");
            return;
        }

        setter.Invoke(target, new object[] { rig });
        ConfigManager.WriteConsole($"{LogPrefix} MRUK._cameraRig assigned ({rig.name})");
    }
}

/// <summary>
/// OVRCameraRig with disabled lifecycle (no auto-spawned cameras, no anchor setup).
/// We assemble trackingSpace + centerEyeAnchor manually and sync them with the active XROrigin.
/// </summary>
public class MRUKCameraRigStub : OVRCameraRig
{
    const string LogPrefix = "[MRUKCameraRigStub]";
    const string TrackingSpaceName = "TrackingSpace";
    const string CenterEyeAnchorName = "CenterEyeAnchor";
    const string LeftEyeAnchorName = "LeftEyeAnchor";
    const string RightEyeAnchorName = "RightEyeAnchor";

    XROrigin cachedOrigin;

    protected override void Awake() { /* no-op: skip OVR auto camera setup */ }
    protected override void Start() { /* no-op */ }
    protected override void FixedUpdate() { /* no-op */ }
    protected override void OnDestroy() { /* no-op */ }
    public override void EnsureGameObjectIntegrity() { /* no-op */ }

    protected override void Update()
    {
        SyncWithXrOrigin();
    }

    public void BuildAnchors()
    {
        Transform ts = CreateChild(transform, TrackingSpaceName);
        Transform center = CreateChild(ts, CenterEyeAnchorName);
        Transform left = CreateChild(ts, LeftEyeAnchorName);
        Transform right = CreateChild(ts, RightEyeAnchorName);

        AssignAnchorProperty(nameof(trackingSpace), ts);
        AssignAnchorProperty(nameof(centerEyeAnchor), center);
        AssignAnchorProperty(nameof(leftEyeAnchor), left);
        AssignAnchorProperty(nameof(rightEyeAnchor), right);
    }

    public void SyncWithXrOrigin()
    {
        if (cachedOrigin == null)
            cachedOrigin = Object.FindObjectOfType<XROrigin>();

        if (cachedOrigin != null)
        {
            Transform originTracking = ResolveTrackingTransform(cachedOrigin);
            if (originTracking != null && trackingSpace != null)
                trackingSpace.SetPositionAndRotation(originTracking.position, originTracking.rotation);

            if (cachedOrigin.Camera != null && centerEyeAnchor != null)
            {
                centerEyeAnchor.SetPositionAndRotation(
                    cachedOrigin.Camera.transform.position,
                    cachedOrigin.Camera.transform.rotation);
            }

            MRCameraRigAlignLog.LogSnapshot("SyncWithXrOrigin", cachedOrigin, this);
            return;
        }

        Camera main = Camera.main;
        if (main == null || centerEyeAnchor == null)
            return;

        if (trackingSpace != null)
            trackingSpace.SetPositionAndRotation(main.transform.position, main.transform.rotation);

        centerEyeAnchor.SetPositionAndRotation(main.transform.position, main.transform.rotation);
        MRCameraRigAlignLog.LogSnapshot("SyncWithXrOrigin-fallback", null, this, force: true);
    }

    static Transform ResolveTrackingTransform(XROrigin origin)
    {
        if (origin == null)
            return null;
        if (origin.CameraFloorOffsetObject != null)
            return origin.CameraFloorOffsetObject.transform;
        if (origin.Camera != null && origin.Camera.transform.parent != null)
            return origin.Camera.transform.parent;
        return origin.transform;
    }

    static Transform CreateChild(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    void AssignAnchorProperty(string propertyName, Transform value)
    {
        PropertyInfo prop = typeof(OVRCameraRig).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        if (prop == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} OVRCameraRig.{propertyName} not found");
            return;
        }

        MethodInfo setter = prop.GetSetMethod(true);
        if (setter == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} OVRCameraRig.{propertyName} has no setter");
            return;
        }

        setter.Invoke(this, new object[] { value });
    }
}
