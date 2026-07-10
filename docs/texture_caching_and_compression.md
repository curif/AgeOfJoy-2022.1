# Age of Joy: Texture Pipeline, Compression, and Caching

This document outlines the internal workings of the texture management system in Age of Joy. Given the VR nature of the project (specifically targeting standalone hardware like Meta Quest), efficiently loading, compressing, and caching custom arcade cabinet artwork is critical to maintaining high frame rates and preventing out-of-memory (OOM) crashes.

---

## 1. Texture Formats and Memory
When users provide custom artwork (PNGs, JPGs) for cabinets, they are initially loaded into memory using Unity's `UnityWebRequest` and `DownloadHandlerTexture`.

By default, Unity decodes these raw image files into **Uncompressed 32-bit Formats** such as `TextureFormat.RGBA32` or `TextureFormat.ARGB32` (often referred to generically as ARGB8 or RGBA8, meaning 8 bits per channel).
*   **The Problem:** Uncompressed textures are massive. A single 2048x2048 uncompressed RGBA32 texture consumes **16MB** of VRAM. A room full of cabinets with marquees, bezels, and side art can easily exceed the memory limits of a mobile VR headset.
*   **The Solution:** We utilize Unity's hardware compression (`Texture2D.Compress()`) to convert these raw formats into heavily optimized block-compressed formats like **ETC2** or **ASTC**. This reduces the memory footprint by up to 75% (e.g., from 16MB down to 2-4MB) with minimal visual degradation.

## 2. The "Multiple of 4" Constraint and GPU Resizing
Hardware compression algorithms (ETC2, ASTC, DXT) operate on blocks of pixels, typically 4x4. Therefore, **Unity can only compress a texture if its width and height are exact multiples of 4.**

If a user provides an image with dimensions like `1001 x 1000`:
1.  Unity's `Compress()` function will silently fail or be skipped.
2.  The texture will remain in memory as an uncompressed `RGBA32` file, drastically hurting performance.

### The GPU Resize Solution
To guarantee all textures can be compressed without enforcing strict rules on the end-user, the `CabinetTextureCache` implements an automatic **Runtime GPU Resize**:
1.  When an image is loaded, its dimensions are checked (`width % 4 == 0 && height % 4 == 0`).
2.  If invalid, the dimensions are rounded up to the nearest multiple of 4.
3.  A temporary `RenderTexture` is created, and `Graphics.Blit` is used to stretch the original image to the new dimensions using the GPU's hardware bilinear filter (preserving colors, alpha, and quality perfectly).
4.  **Crucially**, we use `AsyncGPUReadback.Request` to pull the resized image data back from the GPU. This prevents the main thread from blocking. If standard CPU-based resizing (`GetPixels`/`SetPixels`) was used, the VR headset would stutter violently, causing nausea.
5.  Once read back, the now-valid texture is sent to the compression step.

## 3. The Caching Lifecycle
To avoid repeating expensive downloads, resizing, and compression steps, the game uses a robust two-tier caching system (`CabinetTextureCache.cs` and `TextureDiskCache.cs`).

Here is the exact lifecycle of an artwork file:

### Phase A: Disk Cache Check (`.aojv1`)
1.  The system checks if a pre-compiled cache file exists (e.g., `image.png.aojv1`).
2.  **Stale Check:** It compares the "Last Modified" timestamp of the original image with the `.aojv1` file. If the user updated their art, the cache is ignored and rebuilt.
3.  **Fast Load:** If valid, the raw ETC2/ASTC byte array is read directly from the `.aojv1` file using a `BinaryReader`, pushed into a new `Texture2D` via `LoadRawTextureData()`, and sent straight to the GPU via `Apply(false, makeNoLongerReadable: true)`. This bypasses the heavy PNG/JPG decoding entirely.

### Phase B: Processing & Caching
If no valid disk cache exists:
1.  The image is downloaded via `UnityWebRequest`.
2.  The dimensions are checked and **resized via GPU** if necessary (ensuring multiples of 4).
3.  The raw texture is compressed in memory: `texTmp.Compress(false)`.
4.  **Save to Disk:** Immediately after compression, `TextureDiskCache.SaveToDisk` extracts the compressed bytes using `GetRawTextureData()` and writes them to the `.aojv1` binary file alongside the original image.
5.  **Free System RAM:** The system calls `texTmp.Apply(false, true)`. The `true` flag makes the texture "no longer readable" by the CPU, uploading it permanently to the GPU and freeing the duplicate system RAM.

### Phase C: Memory Cache (LRU)
Once loaded (either from disk cache or freshly processed), the `Texture2D` object is stored in an active Memory Cache (`ResourceCacheManager`).
*   This cache tracks the active MB footprint.
*   It uses a Least Recently Used (LRU) policy. When the cache hits its limit (e.g., 1024MB on Quest 2, 1536MB on Quest 3), it automatically unloads the oldest, unused textures to make room for new ones.

---

## 3b. Pinning: Protecting On-Screen Textures From Eviction

### The Bug: Cabinets Going Dark
Cabinet artwork is bound to a `Material` **once**, at cabinet-build time, via `CabinetPart.SetTextureFromFile()` / `SetEmissionTextureFromFile()`. The `Material` then holds a raw C# reference to the `Texture2D` and the cache is never queried again for that texture while the cabinet stays loaded — the LRU only refreshes an entry's recency on `Add`/`Get`, and nothing ever calls `Get` again for an already-displayed texture.

Because of this, once enough *other* cabinets loaded and pushed the total past the 1024MB/1536MB budget, the LRU could pick an **actively on-screen** texture as the eviction victim (it looked "oldest" purely because nothing had touched it since load), destroy the underlying `Texture2D`, and leave the `Material` pointing at a destroyed object — rendering that cabinet dark/black, even though a player might be looking straight at it.

### The Fix: Ref-Counted Pinning
`ResourceCache<K,V>` (`Assets/curif/LibRetroWrapper/ResourceCache.cs`) now supports **pinning** an entry to protect it from automatic LRU eviction:
*   `Pin(key)` / `Unpin(key)` maintain a ref count per key (multiple owners can pin the same texture, e.g. shared artwork reused across cabinets).
*   `makeSpaceFor()` and `FreeHalfResources()` (the two eviction paths) walk the LRU list from least- to most-recently-used and **skip any pinned key**, only destroying genuinely idle (unpinned) textures.
*   If every entry in the cache happens to be pinned and the budget is still exceeded, eviction gives up gracefully (logs a warning once) rather than destroying something currently in use — the cache simply runs over budget until something becomes eligible again.

`CabinetTextureCache` exposes this as `PinTexture(path)` / `UnpinTexture(path)`.

`CabinetPart` calls `PinTexture` immediately after binding a loaded texture to a material (in both `SetTextureFromFile` and `SetEmissionTextureFromFile`), and tracks every path it pinned in a local list. Its new `OnDestroy()` unpins all of them, so pins are released correctly when a cabinet is torn down (e.g. during a marketplace `CabinetReplace` swap) and don't leak over time.

**Scope note:** this pinning mechanism currently only covers cabinet-part textures (marquee/bezel/side-art/emission). The thumbnail flow (`TextureCache.cs`) and UI sprite cache (`ScreenGenerator.LoadSprite`) still rely on plain LRU — lower risk since those textures are more transient — but can adopt the same `Pin`/`Unpin` primitive later if needed.

### The Real Culprit: `OnLowMemory` Bypassed Pinning Entirely

The pinning fix above did **not** fully solve the "cabinets going dark" bug, because there is a *third* eviction path that ran alongside `makeSpaceFor`/`FreeHalfResources` and originally ignored pin status completely: Unity's OS-level low-memory callback.

`Assets/curif/LibRetroWrapper/Init.cs` subscribes to `Application.lowMemory` in `Awake()`:
```csharp
Application.lowMemory += OnLowMemory;
```
`OnLowMemory()` calls `ResourceCacheManager.FreeResourcesAsync()`, which calls `FreeResources()` on **every** registered `ResourceCache` (textures, `ConfigManager.CabinetCache`, `ConfigManager.CabinetInformationCache`). The original `ResourceCache<K,V>.FreeResources()` did an **unconditional full wipe** — destroying every entry regardless of pin status — because it predates the pinning work and was never updated alongside `makeSpaceFor`/`FreeHalfResources`.

This explains why the bug persisted after the first fix, and why disabling texture compression (`DeviceController.originalTextures = true`) made it *worse*: uncompressed textures are up to 4x larger, so the memory budget is exhausted faster, which means the Android/Quest OS fires `Application.lowMemory` more often — and every firing nuked every visible cabinet's textures at once, pinned or not.

**Fix:** `ResourceCache<K,V>.FreeResources()` now walks the cache the same way `makeSpaceFor` does — it destroys only **unpinned** entries and leaves pinned (actively displayed) ones untouched, logging how many were freed vs. kept. `Remove()` (used by `CabinetTextureCache.InvalidateCachedTexture`) was given the same guard: it now refuses to destroy a pinned entry and logs a warning instead.

### Diagnostics Added For Cache-Size Decisions

Because the previous logging wasn't enough to see *why* cabinets were going dark, the following was added:

*   **`ResourceCache<K,V>.Status()`** now reports `size / maxSize (%)`, entry count, and pinned-entry count in one line, instead of just size and count.
*   **`ResourceCacheManager.LogAllCacheStatus(label)`** dumps `Status()` for every registered cache (textures, cabinet GameObjects, cabinet info) in one call.
*   **`FreeResourcesAsync()`** now logs a full `LogAllCacheStatus("BEFORE")` / `LogAllCacheStatus("AFTER")` snapshot around every free, so you can see exactly what was freed vs. kept per cache.
*   **`Pin`/`Unpin`** log the resulting ref count on every call, so a leak (a pin count that never returns to 0) is visible directly in the logs.
*   **`Init.cs`** now logs a combined snapshot — engine-level memory counters (see below), all cache statuses, and the list of currently-loaded (additive) scenes — on every `OnLowMemory` and `OnMemoryUsageChanged` event, and also on a 15-second repeating timer (`InvokeRepeating(nameof(LogPeriodicMemorySnapshot), 10f, 15f)`), so real memory/cache trends over a full play session are visible in the logs, not just at crisis moments.

### What The Diagnostics Revealed: The Cache Budget Isn't The Bottleneck

Real-device logs captured after the pinning fix confirmed pinning now works correctly (e.g. `texturesCache: FreeResources freed 5/92 entries. Kept 87 pinned entries in use`), but exposed the actual constraint: **the OS-level "Critical" memory event can fire while the texture cache is nowhere near its budget.** One capture showed `texturesCache | size: 492.74MB / 1536.00MB (32.1%)` with only **two** scenes loaded (`FixedScene`, `Room005`) — so this isn't a room-unload leak either; `GateController`/`TeleportationController`'s per-gate `ScenesToUnload` mechanism is working as designed.

The real issue: a **single room's** worth of cabinet art, in `originalTextures` (uncompressed) mode, is already enough to push total device memory into critical territory — independent of our own cache bookkeeping. The gap between Unity's `GetTotalReservedMemoryLong()` (~1.5GB) and our tracked cache size (~0.5GB) is roughly ~1GB of overhead our texture cache never accounts for, because `GetTotalReservedMemoryLong()`/`GetTotalAllocatedMemoryLong()` only track native + managed heaps, **not GPU-only allocations** — and every cabinet texture ends up GPU-only after `Apply(false, makeNoLongerReadable: true)` (see section 4 below for the same GPU-visibility gotcha in `CalculateActualSizeBytes`).

To pin down where that gap actually lives, `Init.LogMemorySnapshot` now also logs:
*   `Profiler.GetAllocatedMemoryForGraphicsDriver()` — GPU/graphics-driver memory, separate from the native/managed counters above; if this tracks closely with the "missing" memory, the overhead is GPU-side (consistent with uncompressed cabinet textures).
*   `Profiler.GetMonoUsedSizeLong()` / `GetMonoHeapSizeLong()` — managed heap usage, to rule out a C#-side leak as the source of the gap.
*   `Profiler.GetTotalUnusedReservedMemoryLong()` — reserved-but-idle native memory (fragmentation), logged for completeness.
*   `SystemInfo.graphicsMemorySize` / `SystemInfo.systemMemorySize` — logged once at startup as a static reference point for the device's total capacity.

**Decision so far:** compression policy (whether to cap/restrict `originalTextures` mode) is deliberately left unchanged for now — ship the pinning fix and the expanded diagnostics first, and let the graphics-driver/mono breakdown from real play sessions confirm where the ~1GB baseline actually comes from before deciding whether to touch the 1024MB/1536MB cache budgets or the compression policy itself.

### Confirmed: The Gap Is GPU Memory, and It's Bigger Than The Texture Cache Itself

A subsequent capture with 3 scenes loaded (`FixedScene`, `HallwayToPolybius`, `Room018`) gave the answer the breakdown was added to find:

```
texturesCache | size: 1415.30MB / 1536.00MB (92.1%) | count: 222 | pinned: 204
[Init.Memory] OnLowMemory (CRITICAL) | allocated: 1815.5MB | reserved: 2100.7MB | unusedReserved: 285.2MB | mono: 18.1/26.2MB | graphicsDriver: 3223.0MB | originalTextures: True
```

`graphicsDriver: 3223.0MB` — over 3.2GB of GPU memory in use, while `CabinetTextureCache` accounts for only 1415MB of it. **Roughly 1.8GB of GPU memory is consumed by something entirely outside the texture cache.** This is now a confirmed, quantified finding, not a hypothesis: whatever is eating that 1.8GB cannot be fixed by touching `CabinetTextureCache`, `ResourceCache`, or their budgets — it lives elsewhere in the rendering pipeline.

**This is out of scope for this document/fix.** The "cabinets going dark" bug (destroying pinned/in-use textures) is confirmed resolved — pinning protects 204/222 live textures in the same capture, evicting only the 18 genuinely idle ones. The remaining ~1.8GB GPU consumer is a separate, larger investigation. See `conductor/` for the follow-up task tracking it (likely candidates to check first: baked lightmaps and `OcclusionCullingData` per room — most room scenes had these regenerated recently per git history — `ScreenGenerator`'s per-cabinet CRT screen textures, LibRetro emulator framebuffers for running cabinets, and VR stereo/compositor render targets, none of which route through `CabinetTextureCache`).

---

## 4. Texture Size Calculation (`CalculateActualSizeBytes`)

The LRU cache needs an accurate size in MB for every texture it stores. This is calculated by `CabinetTextureCache.CalculateActualSizeBytes(Texture2D)` using a **manual width × height × bytes-per-pixel formula**, accounting for the texture format and mipmap chain (×1.33 multiplier when mipmaps are present).

### Why not `Profiler.GetRuntimeMemorySizeLong()`?

`Profiler.GetRuntimeMemorySizeLong()` was tried and **must not be used here**. The reason:

After `Apply(false, makeNoLongerReadable: true)` is called (which happens for every texture loaded from the disk cache and after compression), Unity destroys the CPU-side copy of the texture data and retains only the GPU copy. `GetRuntimeMemorySizeLong()` can only measure CPU/native memory — **it returns 0 for any GPU-only texture**.

This was confirmed by log analysis on a release Meta Quest APK (2026-03-31): every disk-cached texture was being stored in the LRU as 0.00 MB, making eviction completely non-functional for those textures and allowing memory to grow unbounded.

The manual formula uses only `tex.width`, `tex.height`, and `tex.format` — all of which remain available on the `Texture2D` object even after the CPU copy is released — so it works correctly in all cases.

---

## 5. Summary of Key Files
*   **`Assets/curif/LibRetroWrapper/CabinetTextureCache.cs`**: The orchestrator. Handles downloading, dimension verification, async GPU resizing, memory caching, and triggering the disk save. Also exposes `PinTexture`/`UnpinTexture`.
*   **`Assets/curif/LibRetroWrapper/TextureDiskCache.cs`**: Handles the low-level binary I/O for saving and loading the `.aojv1` pre-compressed texture files.
*   **`Assets/curif/LibRetroWrapper/ResourceCache.cs`**: The generic LRU memory cache (`ResourceCache<K,V>` / `ResourceCacheManager`) used for textures. Implements the ref-counted pinning described in section 3b.
*   **`Assets/curif/LibRetroWrapper/CabinetPart.cs`**: Binds loaded textures to cabinet materials and pins them for as long as the cabinet part is alive, unpinning in `OnDestroy()`.
*   **`Assets/curif/LibRetroWrapper/Init.cs`**: Subscribes to `Application.lowMemory`/`memoryUsageChanged`, triggers `ResourceCacheManager.FreeResourcesAsync()`, and logs combined engine + cache memory snapshots (on low-memory events and a 15s repeating timer) for cache-size decisions.
*   **`Assets/curif/LibRetroWrapper/GpuRgb565Converter.cs` / `GpuAlphaCheck.cs`**: Auxiliary tools for advanced GPU-based texture manipulation, primarily used when trying to optimize alpha channels or convert to lower-precision 16-bit formats for older hardware.
