/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>Prepared shelf magazine that is shown while held and docked back into the proxy on release.</summary>
[DisallowMultipleComponent]
public class MRSpawnedShelfMagazine : MonoBehaviour
{
    XRGrabInteractable grabInteractable;
    MagazineGrab magazineGrab;
    MRBookshelfMagazineProxy sourceProxy;
    bool releaseHandled;
    bool releaseCheckPending;
    bool wasHeld;

    public void Configure(MRBookshelfMagazineProxy proxy)
    {
        sourceProxy = proxy;
    }

    public void BeginHeldSession()
    {
        releaseHandled = false;
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
        if (!releaseHandled)
        {
            sourceProxy?.RestoreProxyVisuals();
            sourceProxy?.DockPreparedMagazine();
        }
    }

    void Update()
    {
        if (releaseHandled || magazineGrab == null)
            return;

        bool heldNow = magazineGrab.IsHeldNow();
        if (wasHeld && !heldNow)
        {
            releaseHandled = true;
            StartCoroutine(RestoreProxyAndDock());
            return;
        }

        wasHeld = heldNow;
    }

    void OnGrabbed(SelectEnterEventArgs _)
    {
        releaseHandled = false;
        releaseCheckPending = false;
        wasHeld = true;
        sourceProxy?.OnPreparedMagazineGrabbed();
    }

    void OnReleased(SelectExitEventArgs _)
    {
        if (releaseHandled || releaseCheckPending)
            return;

        releaseCheckPending = true;
        StartCoroutine(HandleReleaseAfterFrame());
    }

    IEnumerator HandleReleaseAfterFrame()
    {
        yield return null;
        releaseCheckPending = false;

        if (releaseHandled)
            yield break;

        if (magazineGrab != null && magazineGrab.IsHeldNow())
            yield break;

        releaseHandled = true;
        yield return RestoreProxyAndDock();
    }

    IEnumerator RestoreProxyAndDock()
    {
        yield return null;
        sourceProxy?.RestoreProxyVisuals();
        sourceProxy?.DockPreparedMagazine();
    }
}
