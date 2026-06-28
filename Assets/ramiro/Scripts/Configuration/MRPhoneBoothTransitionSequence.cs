/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configurable step order for phone booth immersive travel and MR→VR return.
/// Defaults mirror the shipped sequence; reorder in MRRuntimeSettings (FixedScene).
/// </summary>
public static class MRPhoneBoothTransitionSequence
{
    public enum ImmersiveTravelStep
    {
        HandsetAudioCue,
        BeginJourneyVisuals,
        FadeSphereIn,
        SpaceshipEngine,
        EndJourneyVisuals,
    }

    public enum MrToVrReturnStep
    {
        ReloadVrScenes,
        CacheArrivalExplosionClip,
        RestoreGalleryPlayerPose,
        RefreshCameraOffset,
        ApplyTravelStateFallback,
        FinalizeHandsetOnSceneBooth,
        WaitFramesBeforeArrivalEffects,
        ArrivalExplosionAndSmoke,
        EnableVrModeAndLocomotion,
        MrEnvironmentCleanup,
        FinalPassthroughRebind,
    }

    static readonly ImmersiveTravelStep[] DefaultImmersive =
    {
        ImmersiveTravelStep.HandsetAudioCue,
        ImmersiveTravelStep.BeginJourneyVisuals,
        ImmersiveTravelStep.FadeSphereIn,
        ImmersiveTravelStep.SpaceshipEngine,
        ImmersiveTravelStep.EndJourneyVisuals,
    };

    static readonly MrToVrReturnStep[] DefaultMrToVr =
    {
        MrToVrReturnStep.CacheArrivalExplosionClip,
        MrToVrReturnStep.RestoreGalleryPlayerPose,
        MrToVrReturnStep.RefreshCameraOffset,
        MrToVrReturnStep.ApplyTravelStateFallback,
        MrToVrReturnStep.FinalizeHandsetOnSceneBooth,
        MrToVrReturnStep.WaitFramesBeforeArrivalEffects,
        MrToVrReturnStep.ArrivalExplosionAndSmoke,
        MrToVrReturnStep.MrEnvironmentCleanup,
        MrToVrReturnStep.ReloadVrScenes,
        MrToVrReturnStep.EnableVrModeAndLocomotion,
        MrToVrReturnStep.FinalPassthroughRebind,
    };

    public static IReadOnlyList<ImmersiveTravelStep> DefaultImmersiveSteps => DefaultImmersive;

    public static IReadOnlyList<MrToVrReturnStep> DefaultMrToVrReturnSteps => DefaultMrToVr;

    public static List<ImmersiveTravelStep> CopyDefaultImmersive() =>
        new List<ImmersiveTravelStep>(DefaultImmersive);

    public static List<MrToVrReturnStep> CopyDefaultMrToVr() =>
        new List<MrToVrReturnStep>(DefaultMrToVr);

    public static IReadOnlyList<ImmersiveTravelStep> ResolveImmersive(List<ImmersiveTravelStep> configured) =>
        configured != null && configured.Count > 0 ? configured : DefaultImmersive;

    public static IReadOnlyList<MrToVrReturnStep> ResolveMrToVr(List<MrToVrReturnStep> configured) =>
        configured != null && configured.Count > 0 ? configured : DefaultMrToVr;

    public static bool ValidateImmersive(IList<ImmersiveTravelStep> steps, out string warning)
    {
        warning = null;
        if (steps == null || steps.Count == 0)
            return true;

        int handsetIdx = IndexOf(steps, ImmersiveTravelStep.HandsetAudioCue);
        if (handsetIdx >= 0 && handsetIdx != 0)
        {
            warning = "HandsetAudioCue should be first (cancel-before-commit depends on it).";
            return false;
        }

        int engineIdx = IndexOf(steps, ImmersiveTravelStep.SpaceshipEngine);
        int visualsIdx = IndexOf(steps, ImmersiveTravelStep.BeginJourneyVisuals);
        if (engineIdx >= 0 && visualsIdx >= 0 && engineIdx < visualsIdx)
        {
            warning = "SpaceshipEngine should run after BeginJourneyVisuals.";
            return false;
        }

        int endIdx = IndexOf(steps, ImmersiveTravelStep.EndJourneyVisuals);
        if (endIdx >= 0 && endIdx != steps.Count - 1)
        {
            warning = "EndJourneyVisuals should be last in the immersive list.";
            return false;
        }

        return true;
    }

    public static bool ValidateMrToVr(IList<MrToVrReturnStep> steps, out string warning)
    {
        warning = null;
        if (steps == null || steps.Count == 0)
            return true;

        int reloadIdx = IndexOf(steps, MrToVrReturnStep.ReloadVrScenes);
        int cleanupIdx = IndexOf(steps, MrToVrReturnStep.MrEnvironmentCleanup);
        if (reloadIdx >= 0 && cleanupIdx >= 0 && reloadIdx < cleanupIdx)
        {
            warning = "ReloadVrScenes should run after MrEnvironmentCleanup (keeps gallery cabinet audio off during travel).";
            return false;
        }

        int cacheIdx = IndexOf(steps, MrToVrReturnStep.CacheArrivalExplosionClip);
        int finalizeIdx = IndexOf(steps, MrToVrReturnStep.FinalizeHandsetOnSceneBooth);
        int explosionIdx = IndexOf(steps, MrToVrReturnStep.ArrivalExplosionAndSmoke);

        if (cacheIdx >= 0 && finalizeIdx >= 0 && cacheIdx > finalizeIdx)
        {
            warning = "CacheArrivalExplosionClip must run before FinalizeHandsetOnSceneBooth (traveler is destroyed).";
            return false;
        }

        if (finalizeIdx >= 0 && explosionIdx >= 0 && finalizeIdx > explosionIdx)
        {
            warning = "FinalizeHandsetOnSceneBooth must run before ArrivalExplosionAndSmoke.";
            return false;
        }

        int restoreIdx = IndexOf(steps, MrToVrReturnStep.RestoreGalleryPlayerPose);
        int fallbackIdx = IndexOf(steps, MrToVrReturnStep.ApplyTravelStateFallback);
        if (restoreIdx >= 0 && fallbackIdx >= 0 && fallbackIdx < restoreIdx)
        {
            warning = "ApplyTravelStateFallback should run after RestoreGalleryPlayerPose.";
            return false;
        }

        return true;
    }

    static int IndexOf<T>(IList<T> list, T value)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(list[i], value))
                return i;
        }

        return -1;
    }
}
