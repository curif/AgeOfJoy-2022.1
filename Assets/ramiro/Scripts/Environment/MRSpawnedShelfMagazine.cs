/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Prepared shelf magazine: while held it can be read; on release near the shelf slot it docks,
/// otherwise it stays in the world closed until picked up and returned manually.
/// </summary>
[DisallowMultipleComponent]
public class MRSpawnedShelfMagazine : MonoBehaviour
{
    XRGrabInteractable grabInteractable;
    MagazineGrab magazineGrab;
    MRBookshelfMagazineProxy sourceProxy;
    bool releaseCheckPending;
    bool wasHeld;
    bool dockedAtShelf = true;

    public void Configure(MRBookshelfMagazineProxy proxy)
    {
        sourceProxy = proxy;
        dockedAtShelf = true;
    }

    public bool IsDockedAtShelf() => dockedAtShelf;

    public void BeginHeldSession()
    {
        releaseCheckPending = false;
        wasHeld = false;
    }

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        magazineGrab = GetComponent<MagazineGrab>();
    }

    void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(OnGrabbed);
        if (grabInteractable != null)
            grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        if (grabInteractable != null)
            grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    void OnDestroy()
    {
        sourceProxy?.ReturnPreparedMagazineToShelf();
    }

    void Update()
    {
        if (magazineGrab == null)
            return;

        bool heldNow = magazineGrab.IsHeldNow();
        if (wasHeld && !heldNow && !releaseCheckPending)
            StartCoroutine(HandleReleaseAfterFrame());

        wasHeld = heldNow;
    }

    void OnGrabbed(SelectEnterEventArgs _)
    {
        releaseCheckPending = false;
        wasHeld = true;
        dockedAtShelf = false;
        sourceProxy?.OnPreparedMagazineGrabbed();
    }

    void OnReleased(SelectExitEventArgs _)
    {
        if (releaseCheckPending)
            return;

        releaseCheckPending = true;
        StartCoroutine(HandleReleaseAfterFrame());
    }

    IEnumerator HandleReleaseAfterFrame()
    {
        yield return null;
        releaseCheckPending = false;

        if (magazineGrab != null && magazineGrab.IsHeldNow())
            yield break;

        if (IsNearShelfDock())
        {
            dockedAtShelf = true;
            sourceProxy?.ReturnPreparedMagazineToShelf();
        }
        else
        {
            dockedAtShelf = false;
            sourceProxy?.ShowProxyAndLeaveMagazineInWorld();
        }

        wasHeld = false;
    }

    bool IsNearShelfDock()
    {
        if (sourceProxy == null)
            return false;

        return Vector3.Distance(transform.position, sourceProxy.GetDockWorldPosition())
            <= MRBookshelfMagazineProxy.ShelfReturnRadiusMeters;
    }
}
