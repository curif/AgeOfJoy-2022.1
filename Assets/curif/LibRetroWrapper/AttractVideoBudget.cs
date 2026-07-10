/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global budget of simultaneous video decoders for cabinet attraction videos.
/// Each GameVideoPlayer must own a slot before preparing/playing its Unity
/// VideoPlayer. When the budget is full the closest cabinets to the player win;
/// losers stop and show their cached first-frame texture instead.
/// A pinned player (AGEBasic game session paid with a coin) is always granted
/// and never evicted.
/// </summary>
public static class AttractVideoBudget
{
    public const int MAX_ACTIVE_DEFAULT = 5;
    public const int MAX_ACTIVE_DEFAULT_Q3 = 7;
    // a requester only evicts a holder when it is this much closer to the player
    public const float MinDistanceAdvantage = 0.75f;
    // a holder keeps its slot at least this long before it can be evicted
    public const float MinHoldSeconds = 3f;

    private class Slot
    {
        public GameVideoPlayer holder;
        public float grantedAt;
    }

    private static readonly List<Slot> slots = new List<Slot>();
    // pinning is independent of the slot so it survives Release/Request cycles
    // (e.g. an AGEBasic VIDEOLOAD releases the slot and Play() re-requests it)
    private static readonly HashSet<GameVideoPlayer> pinned = new HashSet<GameVideoPlayer>();
    private static int maxActive = 0; // 0 = not configured yet, resolve by device
    private static Transform playerTransform;

    public static int MaxActive
    {
        get
        {
            if (maxActive <= 0)
                maxActive = DeviceController.IsQ3 ? MAX_ACTIVE_DEFAULT_Q3 : MAX_ACTIVE_DEFAULT;
            return maxActive;
        }
    }

    public static int ActiveCount => slots.Count;

    /// <summary>Set the cap from the global configuration. 0 = auto by device.</summary>
    public static void Configure(int maxFromConfig)
    {
        if (maxFromConfig <= 0)
            maxActive = DeviceController.IsQ3 ? MAX_ACTIVE_DEFAULT_Q3 : MAX_ACTIVE_DEFAULT;
        else
            maxActive = Mathf.Clamp(maxFromConfig, 1, 10);
    }

    private static Transform PlayerTransform()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("OVRPlayerControllerGalery");
            if (player != null)
                playerTransform = player.transform;
        }
        return playerTransform;
    }

    private static float DistanceToPlayer(GameVideoPlayer p)
    {
        Transform player = PlayerTransform();
        if (player == null || p == null)
            return 0f; // no player found (editor/desktop): everything is "close", never evict
        return Vector3.Distance(p.transform.position, player.position);
    }

    private static void Prune()
    {
        slots.RemoveAll(s => s.holder == null || !s.holder.isActiveAndEnabled);
        pinned.RemoveWhere(p => p == null);
    }

    private static Slot FindSlot(GameVideoPlayer p)
    {
        foreach (Slot s in slots)
            if (s.holder == p)
                return s;
        return null;
    }

    /// <summary>
    /// Ask for a decoder slot. Returns true when the player may prepare/play.
    /// Holders are re-granted for free, so callers can retry every tick.
    /// </summary>
    public static bool RequestSlot(GameVideoPlayer p)
    {
        if (p == null)
            return false;

        Prune();

        Slot existing = FindSlot(p);
        if (existing != null)
            return true;

        if (pinned.Contains(p))
        {
            // an AGEBasic session owns this screen: always granted, may exceed the cap
            slots.Add(new Slot { holder = p, grantedAt = Time.time });
            ConfigManager.WriteConsole($"[AttractVideoBudget] grant pinned {p.name} ({slots.Count}/{MaxActive})");
            return true;
        }

        if (slots.Count < MaxActive)
        {
            slots.Add(new Slot { holder = p, grantedAt = Time.time });
            ConfigManager.WriteConsole($"[AttractVideoBudget] grant {p.name} ({slots.Count}/{MaxActive})");
            return true;
        }

        // budget full: try to evict the worst non-pinned holder
        // (paused before playing, then farthest from the player)
        Slot victim = null;
        float victimDistance = 0f;
        foreach (Slot s in slots)
        {
            if (pinned.Contains(s.holder))
                continue;
            if (Time.time - s.grantedAt < MinHoldSeconds)
                continue;
            float distance = DistanceToPlayer(s.holder);
            bool worse;
            if (victim == null)
                worse = true;
            else if (victim.holder.IsActuallyPlaying != s.holder.IsActuallyPlaying)
                worse = !s.holder.IsActuallyPlaying; // paused holders are evicted first
            else
                worse = distance > victimDistance;
            if (worse)
            {
                victim = s;
                victimDistance = distance;
            }
        }

        float requesterDistance = DistanceToPlayer(p);
        if (victim != null && victimDistance > requesterDistance + MinDistanceAdvantage)
        {
            // remove the slot before stopping so the victim's ReleaseSlot is a no-op
            slots.Remove(victim);
            ConfigManager.WriteConsole($"[AttractVideoBudget] evict {victim.holder.name} (d:{victimDistance:F1}) for {p.name} (d:{requesterDistance:F1})");
            victim.holder.Stop();
            slots.Add(new Slot { holder = p, grantedAt = Time.time });
            return true;
        }

        return false;
    }

    /// <summary>Release a slot. Safe to call when no slot is owned.</summary>
    public static void ReleaseSlot(GameVideoPlayer p)
    {
        if (p == null)
            return;
        Slot s = FindSlot(p);
        if (s != null)
        {
            slots.Remove(s);
            ConfigManager.WriteConsole($"[AttractVideoBudget] release {p.name} ({slots.Count}/{MaxActive})");
        }
    }

    /// <summary>
    /// Pin a player: always granted (may exceed the cap) and never evicted.
    /// Used while an AGEBasic game session controls the screen.
    /// </summary>
    public static void Pin(GameVideoPlayer p)
    {
        if (p == null)
            return;
        Prune();
        if (pinned.Add(p))
            ConfigManager.WriteConsole($"[AttractVideoBudget] pin {p.name}");
    }

    public static void Unpin(GameVideoPlayer p)
    {
        if (p == null)
            return;
        if (pinned.Remove(p))
            ConfigManager.WriteConsole($"[AttractVideoBudget] unpin {p.name}");
    }

    public static void Status()
    {
        ConfigManager.WriteConsole($"[AttractVideoBudget] active: {slots.Count}/{MaxActive} pinned: {pinned.Count}");
    }
}
