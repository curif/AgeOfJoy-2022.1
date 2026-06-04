/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Payphone handset grab for the IntroGalleryExterior setup:
///   SM_Payphone_Handset (parent) = Rigidbody + Collider + XRGrabInteractable + this script
///   child mesh(es) = visible geometry (follow parent automatically)
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)]
public class PayphoneHandsetGrab : MonoBehaviour
{
    const string LogPrefix = "[PayphoneHandsetGrab]";
    const string HandsetObjectName = "SM_Payphone_Handset";
    const string CradleObjectName = "PF_Grabbable_Phone";
    const string BoothObjectName = "PF_Payphone";
    static readonly string[] GrabInteractionLayers = { "InteractablePart" };
    const string GrabPhysicsLayerName = "InteractablePart";

    [SerializeField] bool hideHandOnGrab = true;
    [Tooltip("Seconds to ease back to the cradle. 0 = instant snap.")]
    [SerializeField] float returnDurationSeconds;
    [SerializeField] bool logDebug;

    Transform grabRoot;
    XRGrabInteractable grabInteractable;
    Rigidbody body;

    Transform homeParent;
    Vector3 homeLocalPosition;
    Quaternion homeLocalRotation;
    Vector3 homeLocalScale;
    Vector3 lockedWorldScale;

    readonly List<Renderer> hiddenRenderers = new List<Renderer>();
    Transform followTransform;
    Vector3 followLocalGrabOffset;
    Quaternion followLocalGrabRotationOffset;
    bool isGrabbed;
    bool virtualGrabFromTravel;
    bool handHideTransferred;
    float virtualGrabReleaseAllowedTime;
    MRPhoneBoothPortal phoneBoothPortal;
    Coroutine snapHomeWhenReadyCoroutine;

    void Awake()
    {
        SetupGrab();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void Start()
    {
        EnsurePhoneBoothPortal();
        CaptureHomePose();
        SetDockedPhysics(true);
        LogSetup();
    }

    void EnsurePhoneBoothPortal()
    {
        MRPhoneBoothPortal mrTraveler = ResolveMrTravelerPortal();
        if (mrTraveler != null)
        {
            phoneBoothPortal = mrTraveler;
            return;
        }

        MRPhoneBoothPortal scenePortal = ResolveScenePortalFromHierarchy();
        if (scenePortal != null)
        {
            phoneBoothPortal = scenePortal;
            return;
        }

        phoneBoothPortal = ResolvePhoneBoothPortalInHierarchy();
        if (phoneBoothPortal == null)
            phoneBoothPortal = MRPhoneBoothPortal.FindSceneBoothPortal();

        if (phoneBoothPortal == null)
            ConfigManager.WriteConsoleWarning($"{LogPrefix} MRPhoneBoothPortal not found on {name}");
    }

    static MRPhoneBoothPortal ResolveMrTravelerPortal()
    {
        MixedRealityManager manager = MixedRealityManager.Instance;
        if (manager == null || !manager.IsMrEnvironmentActive())
            return null;

        return MRPhoneBoothPortal.FindMrTravelerInstance(includeInactive: true);
    }

    MRPhoneBoothPortal ResolveScenePortalFromHierarchy()
    {
        Transform node = grabRoot != null ? grabRoot : transform;
        while (node != null)
        {
            MRPhoneBoothPortal portal = node.GetComponent<MRPhoneBoothPortal>();
            if (portal != null && !portal.IsTravelerInstance)
                return portal;

            node = node.parent;
        }

        return null;
    }

    public void BindToScenePortal(MRPhoneBoothPortal scenePortal)
    {
        if (scenePortal == null || scenePortal.IsTravelerInstance)
            return;

        phoneBoothPortal = scenePortal;
    }

    MRPhoneBoothPortal ResolvePhoneBoothPortalInHierarchy()
    {
        Transform node = transform;
        while (node != null)
        {
            if (node.name == "PF_Payphone")
                return MRPhoneBoothPortal.EnsureOn(node.gameObject);

            MRPhoneBoothPortal portal = node.GetComponent<MRPhoneBoothPortal>();
            if (portal != null)
                return portal;

            node = node.parent;
        }

        return null;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
            return;

        Transform root = ResolveGrabRoot();
        if (root != null)
            ClearStaticFlags(root);
    }
#endif

    void Update()
    {
        if (virtualGrabFromTravel && !IsVirtualGrabInputHeld())
        {
            EndVirtualGrabFromTravel();
            return;
        }

        if (!isGrabbed || followTransform == null || grabRoot == null)
            return;

        if (grabInteractable != null && !grabInteractable.isSelected && !virtualGrabFromTravel)
        {
            EndGrabImmediate();
            return;
        }

        FollowHand();
    }

    void OnDisable()
    {
        RestoreHiddenHand();
        StopAllCoroutines();
        isGrabbed = false;
        followTransform = null;
    }

    void OnDestroy()
    {
        if (!handHideTransferred)
            RestoreHiddenHand();

        if (grabInteractable == null)
            return;

        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    public void MarkHandHideTransferred()
    {
        handHideTransferred = true;
    }

    void SetupGrab()
    {
        grabRoot = ResolveGrabRoot();
        EnsureNotStatic();

        body = EnsureRigidbody();
        grabInteractable = EnsureGrabInteractable();
        DisableMisplacedGrabComponents();

        ConfigureGrabInteractable();
        ConfigureGrabLayers();
        RebuildGrabColliders();

        ConfigManager.WriteConsole(
            $"{LogPrefix} grabRoot='{grabRoot.name}' script='{name}' meshChildren={CountMeshChildren(grabRoot)}");
    }

    /// <summary>
    /// Grab the Rigidbody owner (SM_Payphone_Handset), not the mesh child.
    /// </summary>
    Transform ResolveGrabRoot()
    {
        if (GetComponent<Rigidbody>() != null)
            return transform;

        Transform handset = FindDeepChild(transform, HandsetObjectName);
        if (handset != null && handset.GetComponent<Rigidbody>() != null)
            return handset;

        Rigidbody rb = GetComponentInChildren<Rigidbody>(includeInactive: false);
        if (rb != null)
            return rb.transform;

        return transform;
    }

    static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (root.name == childName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeepChild(root.GetChild(i), childName);
            if (found != null)
                return found;
        }

        return null;
    }

    static int CountMeshChildren(Transform root)
    {
        int count = 0;
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(includeInactive: false))
        {
            if (r.transform != root)
                count++;
        }

        return count;
    }

    /// <summary>
    /// If XRGrabInteractable was left on a mesh child by mistake, disable it.
    /// </summary>
    void DisableMisplacedGrabComponents()
    {
        foreach (XRGrabInteractable grab in GetComponentsInChildren<XRGrabInteractable>(includeInactive: true))
        {
            if (grab == null || grab == grabInteractable)
                continue;

            grab.enabled = false;
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} disabled extra XRGrabInteractable on '{grab.name}' — grab stays on '{grabRoot.name}'");
        }
    }

    XRGrabInteractable EnsureGrabInteractable()
    {
        XRGrabInteractable onRoot = grabRoot.GetComponent<XRGrabInteractable>();
        if (onRoot != null)
            return onRoot;

        if (transform == grabRoot)
        {
            XRGrabInteractable onSelf = GetComponent<XRGrabInteractable>();
            if (onSelf != null)
                return onSelf;
        }

        return grabRoot.gameObject.AddComponent<XRGrabInteractable>();
    }

    Rigidbody EnsureRigidbody()
    {
        Rigidbody rb = grabRoot.GetComponent<Rigidbody>();
        if (rb == null)
            rb = grabRoot.gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.detectCollisions = true;
        return rb;
    }

    void EnsureNotStatic()
    {
        ClearStaticFlags(grabRoot);

        Transform parent = grabRoot.parent;
        while (parent != null)
        {
            ClearStaticFlags(parent);
            if (parent.name == "PF_Grabbable_Phone" || parent.name == "PF_Payphone")
                break;
            parent = parent.parent;
        }
    }

    static void ClearStaticFlags(Transform root)
    {
        if (root == null)
            return;

        if (root.gameObject.isStatic)
            root.gameObject.isStatic = false;

        for (int i = 0; i < root.childCount; i++)
            ClearStaticFlags(root.GetChild(i));
    }

    void ConfigureGrabInteractable()
    {
        grabInteractable.throwOnDetach = false;
        grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.trackScale = false;
        grabInteractable.retainTransformParent = false;
    }

    void ConfigureGrabLayers()
    {
        int physicsLayer = LayerMask.NameToLayer(GrabPhysicsLayerName);
        if (physicsLayer >= 0)
            grabRoot.gameObject.layer = physicsLayer;

        grabInteractable.interactionLayers = InteractionLayerMask.GetMask(GrabInteractionLayers);
    }

    void RebuildGrabColliders()
    {
        grabInteractable.colliders.Clear();

        Collider col = grabRoot.GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.enabled = true;
            grabInteractable.colliders.Add(col);
        }

        if (grabInteractable.colliders.Count == 0)
        {
            BoxCollider box = grabRoot.gameObject.AddComponent<BoxCollider>();
            FitBoxColliderToRenderers(box, grabRoot);
            grabInteractable.colliders.Add(box);
        }
    }

    static void FitBoxColliderToRenderers(BoxCollider box, Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: false);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        box.center = root.InverseTransformPoint(bounds.center);
        Vector3 lossy = root.lossyScale;
        box.size = new Vector3(
            bounds.size.x / Mathf.Max(0.0001f, lossy.x),
            bounds.size.y / Mathf.Max(0.0001f, lossy.y),
            bounds.size.z / Mathf.Max(0.0001f, lossy.z));
    }

    void CaptureHomePose()
    {
        if (grabRoot == null || grabRoot.parent == null)
            return;

        homeParent = grabRoot.parent;
        homeLocalPosition = grabRoot.localPosition;
        homeLocalRotation = grabRoot.localRotation;
        homeLocalScale = grabRoot.localScale;
    }

    Transform GetBoothRootTransform() => ResolveBoothRootFromHierarchy(grabRoot);

    static Transform ResolveBoothRootFromHierarchy(Transform start)
    {
        Transform node = start;
        while (node != null)
        {
            if (node.name == BoothObjectName)
                return node;

            node = node.parent;
        }

        return null;
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        virtualGrabFromTravel = false;
        EnsureNotStatic();

        EnsurePhoneBoothPortal();

        NotifyPhoneBoothPortalTravel();
        DetachFromCradleForGrab();

        followTransform = ResolveFollowTransform(args.interactorObject);
        CacheFollowOffset();

        if (body != null)
        {
            body.isKinematic = true;
            body.WakeUp();
        }

        Log($"grabbed '{grabRoot.name}' follow='{followTransform?.name}'");

        if (hideHandOnGrab)
            StartCoroutine(HideHandVisualsNextFrame(args.interactorObject));
    }

    void NotifyPhoneBoothPortalTravel()
    {
        if (phoneBoothPortal == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} travel skipped — no phone booth portal");
            return;
        }

        if (MixedRealityManager.Instance == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} travel skipped — MixedRealityManager missing (start from FixedScene?)");
            return;
        }

        phoneBoothPortal.NotifyHandsetGrabbedForTravel();
    }

    /// <summary>Editor Play Mode: same path as XR grab — detach handset and start immersive booth travel.</summary>
    public void SimulateEditorGrabForTravel()
    {
        if (!Application.isEditor)
            return;

        isGrabbed = true;
        // Without virtual grab, Update() sees !grabInteractable.isSelected and EndGrabImmediate() cancels travel.
        virtualGrabFromTravel = true;
        virtualGrabReleaseAllowedTime = float.MaxValue;
        EnsureNotStatic();
        EnsurePhoneBoothPortal();

        NotifyPhoneBoothPortalTravel();
        DetachFromCradleForGrab();

        followTransform = ResolveDefaultHandFollowTransform();
        CacheFollowOffset();

        if (body != null)
        {
            body.isKinematic = true;
            body.WakeUp();
        }

        RefreshHandHideIfGrabbed();
        ConfigManager.WriteConsole($"{LogPrefix} editor simulated handset grab for booth travel");
    }

    public static PayphoneHandsetGrab FindOnPortal(MRPhoneBoothPortal portal) => FindGrabOnPortal(portal);

    /// <summary>
    /// Leave the cradle but stay under the traveling booth so UnloadVrScenes does not destroy the handset.
    /// </summary>
    void DetachFromCradleForGrab()
    {
        lockedWorldScale = grabRoot.lossyScale;

        Transform boothRoot = GetBoothRootTransform();
        if (boothRoot != null)
        {
            grabRoot.SetParent(boothRoot, worldPositionStays: true);
            return;
        }

        grabRoot.SetParent(null, worldPositionStays: true);
        DontDestroyOnLoad(grabRoot.gameObject);
        ConfigManager.WriteConsoleWarning($"{LogPrefix} grab detached without booth root — DontDestroyOnLoad fallback on '{grabRoot.name}'");
    }

    /// <summary>Re-parent loose handsets before the VR scene unloads.</summary>
    public static void AttachLooseHandsetsToBooth(MRPhoneBoothPortal portal)
    {
        if (portal == null)
            return;

        Transform boothRoot = portal.transform;
        PayphoneHandsetGrab[] grabs = FindObjectsOfType<PayphoneHandsetGrab>();
        foreach (PayphoneHandsetGrab grab in grabs)
            grab.EnsureUnderBoothForTravel(boothRoot);
    }

    void EnsureUnderBoothForTravel(Transform boothRoot)
    {
        if (grabRoot == null || boothRoot == null || !IsAwayFromCradle())
            return;

        if (grabRoot.IsChildOf(boothRoot))
            return;

        grabRoot.SetParent(boothRoot, worldPositionStays: true);
        ConfigManager.WriteConsole($"{LogPrefix} attached loose handset '{grabRoot.name}' to booth '{boothRoot.name}' before scene unload");
    }

    bool ShouldDeferHandsetReturn()
    {
        return MixedRealityManager.Instance != null && MixedRealityManager.Instance.TransitionInProgress;
    }

    IEnumerator HideHandVisualsNextFrame(IXRSelectInteractor interactor)
    {
        yield return null;
        if (!isGrabbed)
            yield break;

        HideHandVisuals(ResolveHandModel(interactor));
    }

    void CacheFollowOffset()
    {
        if (followTransform == null)
            return;

        followLocalGrabOffset = followTransform.InverseTransformPoint(grabRoot.position);
        followLocalGrabRotationOffset = Quaternion.Inverse(followTransform.rotation) * grabRoot.rotation;
    }

    void EndVirtualGrabFromTravel()
    {
        isGrabbed = false;
        virtualGrabFromTravel = false;
        followTransform = null;
        ShowPlayerHandsAfterRelease();
        RefreshCradleHomeFromHierarchy();
        QueueReturnHandsetToCradle();
    }

    bool IsVirtualGrabInputHeld()
    {
        if (Time.unscaledTime < virtualGrabReleaseAllowedTime)
            return true;

        if (followTransform == null)
            return false;

        ActionBasedController controller = followTransform.GetComponentInParent<ActionBasedController>();
        if (controller == null || controller.selectAction.action == null)
            return true;

        return controller.selectAction.action.IsPressed();
    }

    void ShowPlayerHandsAfterRelease()
    {
        ForceShowPlayerHands();
    }

    void FollowHand()
    {
        if (followTransform == null)
            return;

        Vector3 worldPos = followTransform.TransformPoint(followLocalGrabOffset);
        Quaternion worldRot = followTransform.rotation * followLocalGrabRotationOffset;
        grabRoot.SetPositionAndRotation(worldPos, worldRot);
        ApplyLockedWorldScale();
    }

    void ApplyLockedWorldScale()
    {
        if (grabRoot.parent == null)
        {
            grabRoot.localScale = lockedWorldScale;
            return;
        }

        Vector3 parentScale = grabRoot.parent.lossyScale;
        grabRoot.localScale = new Vector3(
            lockedWorldScale.x / Mathf.Max(0.0001f, parentScale.x),
            lockedWorldScale.y / Mathf.Max(0.0001f, parentScale.y),
            lockedWorldScale.z / Mathf.Max(0.0001f, parentScale.z));
    }

    void OnReleased(SelectExitEventArgs _)
    {
        EndGrabAndReturnToCradle();
    }

    void EndGrabImmediate()
    {
        EndGrabAndReturnToCradle();
    }

    void EndGrabAndReturnToCradle()
    {
        isGrabbed = false;
        virtualGrabFromTravel = false;
        followTransform = null;
        ShowPlayerHandsAfterRelease();
        NotifyPhoneBoothHandsetReleased();
        RefreshCradleHomeFromHierarchy();
        QueueReturnHandsetToCradle();
    }

    void NotifyPhoneBoothHandsetReleased()
    {
        MRPhoneBoothPortal portal = ResolvePhoneBoothPortalInHierarchy();
        if (portal == null)
            portal = phoneBoothPortal;

        portal?.NotifyHandsetReleasedDuringTravel();
    }

    void QueueReturnHandsetToCradle()
    {
        if (snapHomeWhenReadyCoroutine != null)
            StopCoroutine(snapHomeWhenReadyCoroutine);

        snapHomeWhenReadyCoroutine = StartCoroutine(ReturnHandsetWhenReady());
    }

    IEnumerator ReturnHandsetWhenReady()
    {
        while (ShouldDeferHandsetReturn())
            yield return null;

        yield return null;
        snapHomeWhenReadyCoroutine = null;

        if (isGrabbed || grabRoot == null)
            yield break;

        ReturnHandsetToCradle();
    }

    void ReturnHandsetToCradle()
    {
        EnsureValidHomeParent();
        if (homeParent == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} cannot return handset — home parent missing on '{name}'");
            return;
        }

        if (returnDurationSeconds > 0f)
            StartCoroutine(ReturnHomeSmooth());
        else
            SnapHome();
    }

    void RefreshCradleHomeFromHierarchy()
    {
        Transform boothRoot = ResolveBoothRootFromHierarchy(grabRoot);
        if (boothRoot == null)
            return;

        Transform cradle = FindDeepChild(boothRoot, CradleObjectName);
        if (cradle == null)
            return;

        homeParent = cradle;

        MRPhoneBoothPortal portalOnBooth = boothRoot.GetComponent<MRPhoneBoothPortal>();
        if (portalOnBooth != null && !portalOnBooth.IsTravelerInstance)
            phoneBoothPortal = portalOnBooth;
    }

    void EnsureValidHomeParent()
    {
        RefreshCradleHomeFromHierarchy();

        if (homeParent == null)
            return;

        if (grabRoot != null && grabRoot.parent == homeParent)
            CaptureHomePose();
    }

    /// <summary>After immersive travel — release from hand and snap handset back to the cradle.</summary>
    public void NotifyBoothTravelComplete()
    {
        ForceReleaseAndReturnToCradleAfterTravel();
    }

    /// <summary>Ends XR/virtual grab and returns the handset to PF_Grabbable_Phone (used when travel finishes).</summary>
    public void ForceReleaseAndReturnToCradleAfterTravel()
    {
        EnsurePhoneBoothPortal();

        if (snapHomeWhenReadyCoroutine != null)
        {
            StopCoroutine(snapHomeWhenReadyCoroutine);
            snapHomeWhenReadyCoroutine = null;
        }

        ForceEndXrSelection();

        isGrabbed = false;
        virtualGrabFromTravel = false;
        followTransform = null;
        ShowPlayerHandsAfterRelease();
        RefreshCradleHomeFromHierarchy();

        if (grabRoot == null)
            return;

        ReturnHandsetToCradle();
        Log("force release after travel — returned to cradle");
    }

    void ForceEndXrSelection()
    {
        if (grabInteractable == null || !grabInteractable.isSelected)
            return;

        if (grabInteractable.interactorsSelecting.Count == 0)
            return;

        XRInteractionManager manager = grabInteractable.interactionManager;
        if (manager == null)
            manager = FindObjectOfType<XRInteractionManager>();

        if (manager == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} ForceEndXrSelection skipped — no XRInteractionManager");
            return;
        }

        IXRSelectInteractor interactor = grabInteractable.interactorsSelecting[0];
        manager.SelectExit(interactor, grabInteractable);
    }

    /// <summary>Removes handset objects orphaned at the scene root after MR booth travel.</summary>
    public static void DestroyDetachedInstances()
    {
        PayphoneHandsetGrab[] grabs = FindObjectsOfType<PayphoneHandsetGrab>();
        foreach (PayphoneHandsetGrab grab in grabs)
        {
            if (grab == null || !grab.IsRootOrphan())
                continue;

            Destroy(grab.gameObject);
        }
    }

    bool IsAwayFromCradle()
    {
        EnsureValidHomeParent();
        if (grabRoot == null)
            return false;

        if (homeParent == null)
            return grabRoot.parent == null;

        return grabRoot.parent != homeParent;
    }

    bool IsRootOrphan() => grabRoot != null && grabRoot.parent == null;

    public struct TravelerHandsetSnapshot
    {
        public bool WasGrabbed;
        public Vector3 WorldPosition;
        public Quaternion WorldRotation;
        public Vector3 LockedWorldScale;
        public Transform FollowTransform;
        public Vector3 FollowLocalGrabOffset;
        public Quaternion FollowLocalGrabRotationOffset;
    }

    TravelerHandsetSnapshot CaptureTravelerSnapshot()
    {
        return new TravelerHandsetSnapshot
        {
            WasGrabbed = isGrabbed,
            WorldPosition = grabRoot != null ? grabRoot.position : Vector3.zero,
            WorldRotation = grabRoot != null ? grabRoot.rotation : Quaternion.identity,
            LockedWorldScale = lockedWorldScale,
            FollowTransform = followTransform,
            FollowLocalGrabOffset = followLocalGrabOffset,
            FollowLocalGrabRotationOffset = followLocalGrabRotationOffset
        };
    }

    void ApplyTravelerSnapshot(TravelerHandsetSnapshot snapshot, bool forceContinueGrab = false)
    {
        if (grabRoot == null)
            return;

        if (!snapshot.WasGrabbed && !forceContinueGrab)
        {
            NotifyBoothTravelComplete();
            return;
        }

        isGrabbed = true;
        virtualGrabFromTravel = true;
        DetachFromCradleForGrab();
        lockedWorldScale = snapshot.LockedWorldScale.sqrMagnitude > 0.0001f
            ? snapshot.LockedWorldScale
            : grabRoot.lossyScale;
        followTransform = snapshot.FollowTransform ?? ResolveDefaultHandFollowTransform();
        followLocalGrabOffset = snapshot.FollowLocalGrabOffset;
        followLocalGrabRotationOffset = snapshot.FollowLocalGrabRotationOffset;

        if (followTransform != null && followLocalGrabOffset == Vector3.zero)
            CacheFollowOffset();
        else if (snapshot.WorldPosition != Vector3.zero)
            grabRoot.SetPositionAndRotation(snapshot.WorldPosition, snapshot.WorldRotation);
        else
            FollowHand();

        ApplyLockedWorldScale();

        if (body != null)
        {
            body.isKinematic = true;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        virtualGrabReleaseAllowedTime = Time.unscaledTime + 1f;
        RefreshHandHideIfGrabbed();

        ConfigManager.WriteConsole($"{LogPrefix} continued grab on scene handset '{grabRoot.name}'");
    }

    void RefreshHandHideIfGrabbed()
    {
        if (!isGrabbed || !hideHandOnGrab || followTransform == null)
            return;

        GameObject handModel = ResolveHandModelFromFollowTransform(followTransform);
        if (handModel != null)
            HideHandVisuals(handModel);
    }

    static GameObject ResolveHandModelFromFollowTransform(Transform followTransform)
    {
        if (followTransform == null)
            return null;

        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls == null)
            return null;

        if (controls.rightHandXRControl != null
            && followTransform.IsChildOf(controls.rightHandXRControl.transform))
            return controls.RightHand;

        if (controls.leftHandXRControl != null
            && followTransform.IsChildOf(controls.leftHandXRControl.transform))
            return controls.LeftHand;

        return null;
    }

    public static void RefreshGrabbedHandVisibility(MRPhoneBoothPortal scenePortal)
    {
        PayphoneHandsetGrab grab = FindGrabOnPortal(scenePortal);
        grab?.RefreshHandHideIfGrabbed();
    }

    static Transform ResolveDefaultHandFollowTransform()
    {
        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls == null)
            return null;

        if (controls.rightHandXRControl != null)
            return controls.rightHandXRControl.transform;

        if (controls.leftHandXRControl != null)
            return controls.leftHandXRControl.transform;

        return null;
    }

    public void ReleaseHiddenHandVisuals()
    {
        RestoreHiddenHand();
    }

    public static void ReleaseAllHiddenHandVisuals()
    {
        PayphoneHandsetGrab[] grabs = FindObjectsOfType<PayphoneHandsetGrab>();
        foreach (PayphoneHandsetGrab grab in grabs)
        {
            if (grab != null)
                grab.RestoreHiddenHand();
        }
    }

    public static void ForceShowPlayerHands()
    {
        ReleaseAllHiddenHandVisuals();

        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls == null)
            return;

        controls.PlayerMode(false);
        ForceShowHandObject(controls.LeftHand);
        ForceShowHandObject(controls.RightHand);
        ForceShowHandObject(controls.leftHandXRControl);
        ForceShowHandObject(controls.rightHandXRControl);
    }

    static void ForceShowHandObject(GameObject hand)
    {
        if (hand == null)
            return;

        hand.SetActive(true);
        foreach (Renderer renderer in hand.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
                renderer.enabled = true;
        }
    }

    public struct VrReturnHandsetPlan
    {
        public TravelerHandsetSnapshot Snapshot;
        public bool ContinueGrab;
    }

    public static VrReturnHandsetPlan CaptureVrReturnPlan(
        MRPhoneBoothPortal travelerPortal,
        PhoneBoothTravelState travelState)
    {
        VrReturnHandsetPlan plan = default;
        PayphoneHandsetGrab travelerGrab = FindGrabOnPortal(travelerPortal);
        if (travelerGrab != null)
            plan.Snapshot = travelerGrab.CaptureTravelerSnapshot();

        plan.ContinueGrab = plan.Snapshot.WasGrabbed
            || (travelState != null && travelState.HandsetGrabbed);

        ConfigManager.WriteConsole(
            $"{LogPrefix} CaptureVrReturnPlan continueGrab={plan.ContinueGrab} wasGrabbed={plan.Snapshot.WasGrabbed}");

        return plan;
    }

    /// <summary>Moves the visible handset from the MR traveler booth onto the reloaded VR scene booth.</summary>
    public static void FinalizeForVrSceneReturn(
        MRPhoneBoothPortal travelerPortal,
        MRPhoneBoothPortal scenePortal,
        VrReturnHandsetPlan plan)
    {
        PayphoneHandsetGrab travelerGrab = FindGrabOnPortal(travelerPortal);

        if (scenePortal == null)
        {
            travelerGrab?.ForceReleaseAndReturnToCradleAfterTravel();
            PayphoneHandsetGrab.ReleaseAllHiddenHandVisuals();
            MRPhoneBoothPortal.DestroyTravelerInstance();
            return;
        }

        PayphoneHandsetGrab sceneGrab = EnsureSceneHandsetGrab(scenePortal);

        if (sceneGrab == null)
        {
            travelerGrab?.ForceReleaseAndReturnToCradleAfterTravel();
            PayphoneHandsetGrab.ReleaseAllHiddenHandVisuals();
            MRPhoneBoothPortal.DestroyTravelerInstance();
            ConfigManager.WriteConsoleWarning($"{LogPrefix} no scene handset grab on '{scenePortal.name}'");
            return;
        }

        sceneGrab.BindToScenePortal(scenePortal);

        // Release XR on the MR traveler grab before the booth is destroyed.
        travelerGrab?.ForceReleaseAndReturnToCradleAfterTravel();
        PayphoneHandsetGrab.ReleaseAllHiddenHandVisuals();
        MRPhoneBoothPortal.DestroyTravelerInstance();
        sceneGrab.ForceReleaseAndReturnToCradleAfterTravel();
    }

    static PayphoneHandsetGrab FindGrabOnPortal(MRPhoneBoothPortal portal)
    {
        if (portal == null)
            return null;

        return portal.GetComponentInChildren<PayphoneHandsetGrab>(true);
    }

    static PayphoneHandsetGrab EnsureSceneHandsetGrab(MRPhoneBoothPortal scenePortal)
    {
        PayphoneHandsetGrab grab = FindGrabOnPortal(scenePortal);
        if (grab != null)
            return grab;

        Transform handset = FindDeepChild(scenePortal.transform, HandsetObjectName);
        if (handset == null)
            return null;

        grab = handset.GetComponent<PayphoneHandsetGrab>();
        if (grab == null)
            grab = handset.gameObject.AddComponent<PayphoneHandsetGrab>();

        return grab;
    }

    public bool BelongsToTravelerBooth()
    {
        EnsurePhoneBoothPortal();
        return phoneBoothPortal != null && phoneBoothPortal.IsTravelerInstance;
    }

    void SnapHome()
    {
        EnsureValidHomeParent();
        if (homeParent == null || grabRoot == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} SnapHome failed — missing grabRoot or homeParent on '{name}'");
            return;
        }

        grabRoot.gameObject.SetActive(true);
        grabRoot.SetParent(homeParent, worldPositionStays: false);
        grabRoot.localPosition = homeLocalPosition;
        grabRoot.localRotation = homeLocalRotation;
        grabRoot.localScale = homeLocalScale;
        SetDockedPhysics(true);

        foreach (Renderer renderer in grabRoot.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
                renderer.enabled = true;
        }

        ConfigManager.WriteConsole($"{LogPrefix} SnapHome '{grabRoot.name}' parent='{homeParent.name}' localPos={homeLocalPosition}");
    }

    IEnumerator ReturnHomeSmooth()
    {
        Vector3 startPos = grabRoot.position;
        Quaternion startRot = grabRoot.rotation;

        Vector3 endPos = homeParent != null
            ? homeParent.TransformPoint(homeLocalPosition)
            : homeLocalPosition;
        Quaternion endRot = homeParent != null
            ? homeParent.rotation * homeLocalRotation
            : homeLocalRotation;

        grabRoot.SetParent(homeParent, worldPositionStays: true);

        float duration = returnDurationSeconds;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            grabRoot.position = Vector3.Lerp(startPos, endPos, t);
            grabRoot.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        SnapHome();
    }

    void HideHandVisuals(GameObject handRoot)
    {
        RestoreHiddenHand();
        if (handRoot == null)
            return;

        foreach (Renderer renderer in handRoot.GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            if (renderer == null || !renderer.enabled)
                continue;

            renderer.enabled = false;
            hiddenRenderers.Add(renderer);
        }
    }

    void RestoreHiddenHand()
    {
        for (int i = 0; i < hiddenRenderers.Count; i++)
        {
            Renderer renderer = hiddenRenderers[i];
            if (renderer != null)
                renderer.enabled = true;
        }

        hiddenRenderers.Clear();
    }

    void SetDockedPhysics(bool docked)
    {
        if (body == null)
            return;

        body.useGravity = false;
        body.isKinematic = true;

        if (docked)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    static Transform ResolveFollowTransform(IXRSelectInteractor interactor)
    {
        var interactorBehaviour = interactor as MonoBehaviour;
        return interactorBehaviour != null ? interactorBehaviour.transform : null;
    }

    static GameObject ResolveHandModel(IXRSelectInteractor interactor)
    {
        var interactorBehaviour = interactor as MonoBehaviour;
        if (interactorBehaviour == null)
            return null;

        ActionBasedController controller = interactorBehaviour.GetComponentInParent<ActionBasedController>();
        if (controller != null && controller.model != null)
            return controller.model.gameObject;

        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls == null)
            return null;

        Transform interactorTransform = interactorBehaviour.transform;
        if (controls.leftHandXRControl != null
            && interactorTransform.IsChildOf(controls.leftHandXRControl.transform))
            return controls.LeftHand;

        if (controls.rightHandXRControl != null
            && interactorTransform.IsChildOf(controls.rightHandXRControl.transform))
            return controls.RightHand;

        return null;
    }

    void LogSetup()
    {
        if (!logDebug)
            return;

        Log($"setup grabRoot='{grabRoot.name}' colliders={grabInteractable.colliders.Count}");
    }

    void Log(string message)
    {
        if (!logDebug)
            return;

        ConfigManager.WriteConsole($"{LogPrefix} {message}");
    }

}
