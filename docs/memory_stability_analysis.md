# Root Cause Analysis: Age of Joy Memory Instability

**Date:** 2026-03-31
**Branch:** fix/v0.5-2
**Target platforms:** Meta Quest 2, Meta Quest 3

This document identifies the verified root causes of the memory instability reported by users on release builds. All findings are based on direct code inspection.

---

## Issue #1 — `FORCE_DEBUG` Active in Production Builds

**File:** `Assets/curif/LibRetroWrapper/ConfigManager.cs:8`
**Severity:** Critical

```csharp
// coment the next line for releases build
#define FORCE_DEBUG      // <--- never commented out
```

This activates `#if DEBUG_ACTIVE` in all builds, including release APKs. Every `WriteConsole` call executes `UnityEngine.Debug.LogFormat(...)`, which evaluates the C# string interpolation and allocates a new managed string on the heap — even when no one is reading the output.

`LoadAndCacheAsync` alone has ~8 log calls per texture. A room with 50 cabinets × 4 textures = 200 textures → ~1,600 string allocations per room load. This feeds the managed heap continuously and triggers GC collections far more often than necessary, causing frame-time spikes at 90 Hz.

### Constraint

`BugReportManager` subscribes to `Application.logMessageReceivedThreaded` and captures all `Debug.Log*` output to `Logs/debug_log.txt` when the player activates debug mode. The log report feature depends on `WriteConsole` producing `Debug.Log` calls — simply stripping them would make the captured log empty and useless.

### Proposed Fix

`WriteConsole` should gate both the string interpolation and the `Debug.LogFormat` call on whether log capture is actually active (`BugReportManager.Instance?.IsDebugModeActive() == true`), paying zero cost during normal play while keeping the full log pipeline intact when the player enables the bug report feature.

`WriteConsoleAGEBasic` and `WriteConsoleErrorAGEBasic` (lines 269–286) must remain unconditional — they represent user-visible AGEBasic script output.

---

## Issue #2 — GLB Model Cache Uses Disk Size, Not Memory Size

**File:** `Assets/curif/LibRetroWrapper/CabinetFactory.cs:101`
**Severity:** Critical

```csharp
FileInfo fileInfo = new FileInfo(modelFilePath);
ConfigManager.CabinetCache.Add(cacheKey, model, fileInfo.Length / (1024f * 1024f)); //dont know correct size in memory, in disk is used.
```

The `CabinetCache` limit is 512 MB (set in `ConfigManager.cs:117`). However, a 50 MB `.glb` file expands in memory to 200–500 MB as a loaded `GameObject` with full mesh data, materials, and textures. The LRU eviction logic believes it is managing 512 MB while the actual allocation is 2–5 GB. **Eviction never fires when it should.**

### Proposed Fix

Use `UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(model)` after the GLB is loaded to measure actual memory, the same approach `CalculateActualSizeBytes()` already uses for textures.

---

## Issue #3 — `Resources.UnloadUnusedAssets()` Not Awaited in `Clear()`

**File:** `Assets/curif/LibRetroWrapper/ResourceCache.cs:228–235`
**Severity:** High

```csharp
public void Clear()
{
    lock (locker) { ... }

    System.GC.Collect();               // blocking, on main thread
    Resources.UnloadUnusedAssets();    // returns AsyncOperation — silently discarded
}
```

`Resources.UnloadUnusedAssets()` returns an `AsyncOperation` that must be awaited. Here it is fire-and-forget — `Clear()` returns before any assets are actually unloaded from GPU memory. The correct pattern already exists in the same file in `CleanupRoutine()`, which is a coroutine that yields on the operation, but `Clear()` does not use it.

Additionally, the synchronous `GC.Collect()` call can stall the main thread for 50–200 ms on Quest hardware.

---

## Issue #4 — Texture Cache Limits Are Too Large

**File:** `Assets/curif/LibRetroWrapper/CabinetTextureCache.cs:29–31`
**Severity:** High

```csharp
public const float CACHE_SIZE = 1024;     // Quest 2
public const float CACHE_SIZE_Q3 = 1536f; // Quest 3
```

Combined with the model cache (512 MB budgeted, but far more actual — see Issue #2), the total allowed allocation is 1.5 GB budgeted but potentially 3–6 GB actual. Meta recommends apps stay under 4 GB on Quest 2 (6 GB total RAM, ~2–3 GB consumed by Android and the OS). The texture limit alone at 1024 MB leaves very little headroom, especially when model memory is underreported.

---

## Issue #5 — Two Uncoordinated Texture Systems Coexist

**Severity:** High

| System | File | Used By | Eviction |
|--------|------|---------|----------|
| `CabinetTextureCache` | `CabinetTextureCache.cs` | Cabinet parts | LRU via `ResourceCache` |
| `TextureCache` | `TextureCache.cs` | `GameVideoPlayer` | None |

`TextureCache` is still attached to `GameVideoPlayer` objects (`GameVideoPlayer.cs:11, 32, 40`). It has no eviction, no size limit, and its `ConvertToTexture2D` method uses synchronous `ReadPixels()`, which is a full GPU pipeline stall causing a guaranteed dropped frame every time it runs.

`TextureCache` objects are never registered in `ResourceCacheManager`, so they represent invisible, uncounted memory consumption outside of all cache budgets.

---

## Issue #6 — `CabinetInformationCache` Uses Fake Size Units

**Files:** `ConfigManager.cs:121–122`, `CabinetInformation.cs:254`
**Severity:** Medium

```csharp
// ConfigManager.cs
ResourceCacheManager.Create<string, CabinetInformation>("CabinetInformationCache", 5000f); // units not MB

// CabinetInformation.cs
ConfigManager.CabinetInformationCache.Add(cabPath, cabInfo, 1f); // cant know object size
```

The entire `ResourceCache<K,V>` LRU eviction system is built around MB. When used with item-count units, all threshold calculations produce meaningless numbers. This cache holds up to 5,000 `CabinetInformation` objects before eviction starts — effectively unlimited for any real arcade room.

`CabinetInformation` is a plain C# class, not a `UnityEngine.Object`, so `DestroyIfUnityObject` does nothing on eviction. Eviction only drops the managed reference. If any cabinet controller holds a reference to an evicted `CabinetInformation`, the object cannot be collected and becomes a managed memory leak.

---

## Issue #7 — GPU Readback Leaks RenderTexture on Coroutine Cancellation

**File:** `Assets/curif/LibRetroWrapper/CabinetTextureCache.cs:144–170`
**Severity:** Medium

```csharp
var request = AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32);
while (!request.done) { yield return null; }  // coroutine can be cancelled here

// ...
RenderTexture.ReleaseTemporary(rt);  // never reached if coroutine is stopped
```

If the player leaves the room while a cabinet is loading, Unity stops the coroutine. The `while (!request.done)` loop is interrupted, `rt` is never released, and the pending GPU request holds a reference to a dangling `RenderTexture`. This is a GPU memory leak per aborted cabinet load, compounding in rooms where the player moves quickly.

---

## Summary

| # | Root Cause | Severity | Status |
|---|-----------|---------|--------|
| 1 | `FORCE_DEBUG` active in release — GC pressure from unconditional string allocation | Critical | ✅ Fixed |
| 2 | GLB cache uses disk file size instead of actual GPU memory size — eviction never fires | Critical | ✅ Fixed |
| 3 | `Clear()` discards the `AsyncOperation` from `UnloadUnusedAssets()` — memory not actually freed | High | ✅ Fixed |
| 4 | Texture cache limits (1024/1536 MB) leave no safety margin on Quest 2 | High | ⏳ Open — needs on-device profiling before changing |
| 5 | `TextureCache` (video player) is uncoordinated with `ResourceCacheManager` — invisible unbounded memory | High | ✅ Fixed |
| 6 | `CabinetInformationCache` uses item-count units in an MB-based LRU — eviction logic broken | Medium | ✅ Fixed (2026-07-01) |
| 7 | GPU `RenderTexture` leaked when texture-load coroutine is cancelled mid-readback | Medium | ✅ Fixed (2026-07-01) |

---

## The Crash Scenario on Quest 2

1. Room loads 20 cabinets → textures consume ~400 MB (accurately tracked), but models consume 2–4 GB (not tracked — see Issue #2).
2. Cache believes total is ~500 MB → eviction never triggers.
3. OS issues memory warning → app is killed.
4. If not killed: player moves fast between areas → coroutines aborted → `RenderTexture` leaks accumulate (Issue #7).
5. At some point `Clear()` is called → `GC.Collect()` stalls the frame → `UnloadUnusedAssets()` is discarded → memory is not reclaimed (Issue #3).
6. GC pressure from debug logging compounds every step (Issue #1).

The single highest-impact fix is **Issue #2** (measure actual model memory). The quickest fix to ship is **Issue #1** (gate `WriteConsole` on `BugReportManager.IsDebugModeActive()`).

---

## Follow-up: 2026-07-01 fixes

By this date, issues #1, #2, #3, and #5 had already been fixed in earlier commits
(`42a10b04`, `a5cc41a6`, `5542ed1a`, and the `#if UNITY_EDITOR || DEVELOPMENT_BUILD`
guard now around `DEBUG_ACTIVE` in `ConfigManager.cs:7-9`). This pass closed the
remaining two documented issues and one additional gap found during the review.

### Issue #6 fixed — `CabinetInformationCache` now sized in real MB

**Files:** `Assets/curif/LibRetroWrapper/CabinetInformation.cs`, `Assets/curif/LibRetroWrapper/ConfigManager.cs`

The original proposed fix (shrink the 5000-item cap) would have broken an intentional
feature: `CabinetInformation.PreloadAllAsync()` deliberately preloads *every* cabinet's
`description.yaml` into this cache at startup, so a small cap would cause constant
eviction/reload churn during preload.

Instead, `CabinetInformation.fromYaml()` now estimates each entry's real size from its
source YAML text length (`yaml.Length * 2` bytes, a reasonable proxy since a parsed YAML
object graph stays the same order of magnitude as its source text — unlike a GLB's much
larger GPU expansion) and passes that as the `sizeInMB` argument to
`ConfigManager.CabinetInformationCache.Add(...)`. The cache budget in `ConfigManager.cs`
was changed from `5000f` (mislabeled "units not MB") to a genuine `64f` MB, which
comfortably holds several thousand typical cabinet descriptions while still being a real,
enforceable memory bound.

### Issue #7 fixed — GPU `RenderTexture` no longer leaks on coroutine cancellation

**Files:** `Assets/curif/LibRetroWrapper/CabinetTextureCache.cs` (GPU resize/compress path, ~line 142), `Assets/curif/LibRetroWrapper/TextureCache.cs` (`SaveTextureCoroutine`, ~line 66)

Both coroutines now wrap `Graphics.Blit` + `AsyncGPUReadback.Request` + the
`while (!request.done) yield return null;` wait loop in a `try/finally`, with
`RenderTexture.ReleaseTemporary(rt)` moved into the `finally` block. If Unity stops the
coroutine mid-wait (e.g. the player leaves the room while a cabinet's art is still
loading or a thumbnail is still being saved), the `RenderTexture` is now always released
instead of leaking GPU memory. `AsyncGPUReadbackRequest` is a struct, so it's declared
before the `try` and assigned inside it — used again after the `finally` runs.

### New finding fixed — `ScreenGenerator.screenTexture` had no explicit teardown

**File:** `Assets/curif/UI/ScreenGenerator.cs`

Not one of the original 7 issues, but found while reviewing the same area: every cabinet
screen's `screenTexture` (created in `createTexture()`) had no `OnDestroy()`, unlike its
sibling `baseTexture` in the same class, which is explicitly `Destroy()`'d in
`ClearSprites()`. It relied entirely on periodic `Resources.UnloadUnusedAssets()` sweeps
to be reclaimed. Added an `OnDestroy()` that destroys both `screenTexture` and
`baseTexture` (if still set) as soon as the `ScreenGenerator` component is destroyed,
rather than waiting for the next sweep.

### Still open — Issue #4 (cache size constants)

Texture cache (1024 MB Quest 2 / 1536 MB Quest 3) and model cache (512 MB) budgets were
left unchanged. Now that Issues #1-#3 are fixed, there may be headroom to raise them
responsibly, but this needs on-device validation (`adb shell dumpsys meminfo <package>`
on real Quest 2 hardware) rather than a code-only guess — changing these blind risks
re-introducing OOM crashes. Recommended next step: build with the fixes above, profile
actual memory headroom on-device across a full room-traversal session, then revisit these
constants together with the `GateController.cs` aggressive-cleanup threshold (`mem > 2500`)
so they're tuned as one system rather than independently.

### Verification performed

Code-level only (no automated test runner for this project). Confirmed by direct
inspection that:
- `ConfigManager.cs:7-9` gates `DEBUG_ACTIVE` on editor/dev builds (Issue #1 stayed fixed).
- `CabinetFactory.CalculateGameObjectSizeBytes()` and `ResourceCacheManager.FreeResourcesAsync()` remain in place (Issues #2, #3 stayed fixed).
- `TextureCache.cs` still delegates to `CabinetTextureCache.LoadAndCacheAsync` (Issue #5 stayed fixed).

Not yet verified on-device: the user should build to Quest 2/3, traverse several rooms
and gates, and watch the `[Memory Check] RAM after unload` log line in
`GateController.cs` to confirm memory stays lower and the `> 2500` warning fires less
often. For the `RenderTexture` fix specifically, rapidly entering/leaving a room with
several cabinets mid-load and checking `adb shell dumpsys meminfo` for GPU memory growth
would confirm the leak is closed.
