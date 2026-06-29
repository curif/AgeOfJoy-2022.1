# Hardware-Accelerated Cores — In-Project Test & Execution Plan

A staged plan to bring Vulkan hardware-rendering libretro cores (Flycast / Dreamcast / NAOMI
the primary target) into Age of Joy, **tested incrementally inside the running game** using
debug-only GameObjects placed in the first room the player spawns into.

This is the execution companion to [libretro_video_pipeline.md §9](libretro_video_pipeline.md),
which describes the architecture and the remaining native stubs. Read that first. This document
is about *how to land and verify each phase on a Quest 3 without breaking the shipping arcade.*

---

## 0. Guiding constraints

These come from [CLAUDE.md](../CLAUDE.md) and shape every choice below.

- **Preserve the simulation.** The test rig is **debug-only** and must never appear in a release
  build. No 2D menus or overlays survive into the player-facing arcade. Test GameObjects are gated
  behind `ConfigManager.DEBUG_ACTIVE` (`FORCE_DEBUG`) and a dedicated
  `HWRenderTest` compile/runtime flag, and are disabled by default.
- **VR perf is the hard constraint.** Target Quest 3 at 90 Hz. The emulator's GPU work shares one
  Adreno 740 and (effectively) one graphics queue with Unity's stereo rendering. Every phase has a
  frame-time / thermal acceptance gate, not just a "does it render" gate.
- **No editor shortcut for the native path.** The libretro `.so` only loads on Android, and the
  upload path is wrapped in `#if !UNITY_EDITOR`
  ([LibretroMameCore.cs:987](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L987)). **All
  validation is on-device** (sideload APK + `logcat`). Editor playmode only helps for wiring/compile
  checks, not for frames.
- **One game at a time, globally.** `LibretroMameCore` is static with a single `GameTexture`. The
  test rig drives that same singleton — it does not introduce a parallel emulator instance.

---

## 1. Current state (verified)

What works today (negotiation + image hand-off are live):

- `FixedScene.unity` is the persistent boot scene — it carries `Init` and `CoresController` as
  session singletons. This is the correct home for a persistent Vulkan bootstrap object.
- The Vulkan negotiation path in [cwrapper/vulkan.c](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c)
  creates the device on Unity's `VkPhysicalDevice`, calls `context_reset`, and the core delivers a
  `VkImage` each frame via `vulkan_set_image` → `vulcanImageCB` → C#.

What is **broken or missing** (the work):

| # | Gap | Where | Effect |
|---|---|---|---|
| **0** | `UnityVulkan` MonoBehaviour is **not attached to any GameObject / prefab** | (no scene references its GUID) | `LibretroVulkan.Init()` never runs → `WrapperInit()` at [LibretroMameCore.cs:542](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L542) sends **zeroed** `VkInstance`/device to native. The HW path is dead on arrival. |
| 1 | Queue sync callbacks are no-ops | [vulkan.c:86-130](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L86) | Concurrent `vkQueueSubmit` from the worker thread = Vulkan UB / crashes. |
| 2 | HW frame signal dropped | [image.c:157](../Assets/curif/LibRetroWrapper/cwrapper/image.c) | Main thread never learns a frame arrived. |
| 3 | `LoadVulkanTextureData` body commented out | [LibretroMameCore.cs:1001](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L1001) | `VkImage` never wrapped as a `Texture2D`. |
| 4 | No `get_image_info` accessor | — | C# lacks width/height/`VkFormat` for `CreateExternalTexture`. |

**Phase 0 is non-negotiable and comes first** — nothing downstream can be tested until Unity's real
Vulkan handles reach native.

---

## 2. Test-rig philosophy

Rather than wiring HW rendering straight into the real cabinet + coin-slot pipeline (which couples a
hard native bring-up to the full UGC cabinet lifecycle), we build a **minimal, debug-gated test rig**
that exercises one slice of the pipeline at a time, in isolation, in the spawn room. Each phase has a
self-contained GameObject you can enable, point at, and read pass/fail from logs or a clearly-fake
debug quad. Only after the rig proves the pipeline end-to-end do we promote it to the real cabinet.

### 2.1 Where the rig lives

```
FixedScene.unity  (persistent singletons)
  └── [VulkanBootstrap]      ← UnityVulkan MonoBehaviour (Phase 0). Persistent, always on.

<spawn room>.unity  (Room001 / workshop — wherever the player first lands)
  └── [HWRenderTestRig]      ← root, disabled unless DEBUG_ACTIVE && hwRenderTestEnabled
        ├── TestScreenQuad   ← a plain MeshRenderer quad, NOT a real cabinet. Binds GameTexture.
        ├── TestController    ← drives LibretroMameCore.Start() on a test core+ROM, no coin needed.
        └── TestHUDLogger     ← logs FPS / frame-time / image handles to logcat (no on-screen UI).
```

The rig root reads the flag in `Awake()` and `SetActive(false)`s itself in any non-debug build, so it
is inert in release regardless of whether someone leaves it in the scene. **It is removed entirely
before a production tag** — the gate is a safety net, not the ship plan.

### 2.2 Why a test quad instead of a real cabinet

A real cabinet pulls in YAML parsing, GLB loading, the CRT prefab/shader chain, coin-slot grab
detection, and the model cache. For native Vulkan bring-up those are noise that can mask (or be
blamed for) a black screen. `TestScreenQuad` is a bare quad with an unlit material whose `_MainTex`
is bound directly to `LibretroMameCore.GameTexture` — the smallest possible consumer of the texture.
Once frames are correct on the quad, swapping in the real `ShaderCRT` chain is a known-good step.

### 2.3 Driving emulation without a coin

`TestController` calls `LibretroMameCore.Start(screenName, gameFile, core, ...)` directly on a debug
keybind / controller button, bypassing `CoinSlotController`. This keeps the test independent of the
VR interaction layer and lets you start/stop the core repeatedly while watching logcat. It targets a
small, known ROM (see §4 Phase 4) placed in the user cores/ROMs dirs.

---

## 3. Tooling & validation harness

| Need | Tool | Notes |
|---|---|---|
| Native logs | `adb logcat -s Unity` (+ the `[VULCAN_HANDLERS]`, `[LoadVulkanTextureData]`, `[wrapper_init_vulkan]` tags already in the code) | Primary signal source for every phase. |
| Frame-time / dropped frames / GPU & CPU level | **OVR Metrics Tool** (overlay) or `adb shell logcat -s VrApi` | The 90 Hz acceptance gate. Watch `Stale`/`Early` frame counts and GPU level. |
| Thermals | OVR Metrics Tool thermal readout; sustained 15-20 min runs | Dual-load (emu + VR) is a throttle scenario, not an instantaneous-fps one. |
| GPU capture / layout & sync validation | **RenderDoc for Oculus** / Snapdragon Profiler; enable **Vulkan validation layers** in a debug build | Catches queue races, wrong image layouts, and the sync-index bugs of Phase 1. |
| Frame correctness dump | The commented `EncodeToPNG` path in [LoadVulkanTextureData](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L1008) writing to `ConfigManager.DebugDir` | Pull with `adb pull` to eyeball the first wrapped frame off the headset. |

Build/iterate loop: `build.cmd` → sideload → launch → `logcat`. Keep `build.cmd -customScenes=` scoped
to `FixedScene` + the spawn room during bring-up to shorten builds.

---

## 4. Phased execution & test plan

Each phase lands a **single verifiable behavior change** and has an explicit gate. The order is
load-bearing: sync safety (Phase 1) precedes anything visual so a queue race can't masquerade as a
wrapping bug.

### Phase 0 — Bootstrap the real Vulkan handles
**Goal:** Unity's actual `VkInstance`/`VkPhysicalDevice`/`VkDevice` reach native instead of zeros.

- Build `libVulkanPlugin` for `arm64-v8a` (see [§9.9](libretro_video_pipeline.md)) and place the `.so`
  where Unity packages it.
- Add a `[VulkanBootstrap]` GameObject to `FixedScene` carrying the `UnityVulkan` MonoBehaviour
  ([UnityVulkan.cs](../Assets/curif/LibRetroWrapper/UnityVulkan.cs)). Confirm its `Start()` runs at
  boot (it pulls the 7 handles and calls `LibretroVulkan.Init`).
- Verify ordering: `LibretroVulkan.Init` (boot) must run before any `WrapperInit()`
  ([LibretroMameCore.cs:542](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L542)). `UnityVulkan`
  uses the engine's `Start`; `LibretroMameCore.Start` only runs when a game launches, so ordering holds,
  but assert non-zero handles defensively.

**Pass gate (logcat):** `[UnityVulkan]` logs non-zero `vkInstance`/`vkPhysicalDevice`/`vkDevice`, and
`[wrapper_init_vulkan]` echoes the *same* non-zero values. No functional rendering yet.

### Phase 1 — Synchronization safety (no UB, no crashes)
**Goal:** the core can submit to the shared queue without racing Unity. **Most dangerous phase — do
it before anything visual.**

- Implement `vulkan_lock_queue`/`unlock_queue` against a mutex shared with the plugin (coordinate with
  Unity via `kUnityVulkanGraphicsQueueAccess_Allow` / `AccessQueue`).
- Implement a minimal 2-frame sync ring: `get_sync_index = frame % 2`, `get_sync_index_mask = 0b11`,
  two `VkFence`s, real `wait_sync_index`, and `set_signal_semaphore`. See
  [§9.7](libretro_video_pipeline.md) for the recommended starting points.
- Run a HW core through `wrapper_run()` on the worker thread **with the texture-wrap step still
  stubbed** — we are only validating that submission is safe.

**Test rig:** `TestController` starts the core; no `TestScreenQuad` binding yet. Run with **Vulkan
validation layers on**.

**Pass gate:** 10+ minutes of `retro_run` with zero validation-layer errors, zero queue-submit
warnings in RenderDoc, no crash. VR stays at 90 Hz (core GPU work is still trivial here).

### Phase 2 — Image wrapping (first frame visible)
**Goal:** the core's `VkImage` shows up on `TestScreenQuad`.

- Add `wrapper_vulkan_get_image_info` (width/height/`VkFormat`) pulling from the cached `create_info`
  in `vulkan_set_image`; expose via `DllImport`.
- Replace the [image.c:157](../Assets/curif/LibRetroWrapper/cwrapper/image.c) early-return with a call
  to the existing `TextureSemAvailableCB()` so the main thread's `UpdateTexture` dispatch fires.
- Fill in [LoadVulkanTextureData](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L1001):
  `CreateExternalTexture` on first frame / `UpdateExternalTexture` on subsequent frames, map
  `VkFormat` → `TextureFormat`, then `Shader.Refresh(GameTexture)`.
- `TestScreenQuad` binds its material `_MainTex` to `GameTexture`.

**Pass gate:** a recognizable (possibly mis-oriented / wrong-colored) frame appears on the quad **and**
the `EncodeToPNG` dump in `DebugDir` shows core content. Orientation/format correctness is Phase 3.

### Phase 3 — Layout & format correctness
**Goal:** the frame is right, not corrupt.

- Ensure the sampled image is in `SHADER_READ_ONLY_OPTIMAL` / `GENERAL`; insert a pipeline barrier via
  Unity's command buffer if the core doesn't deliver it that way.
- Resolve `VkFormat` → `TextureFormat` mapping (e.g. `R8G8B8A8_UNORM` → `RGBA32`); fix any
  vertical-flip vs the software path's expectations (the `Invert` hooks in
  [LibretroScreenController](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs#L447)).
- Validate `wait_sync_index` actually gates reuse — Flycast corrupts its texture cache within seconds
  if this is a no-op.

**Pass gate:** stable, correctly-oriented, correctly-colored frames for 5+ minutes, no tearing/cache
corruption under RenderDoc.

### Phase 4 — Real core integration (Flycast / NAOMI)
**Goal:** a real Dreamcast/NAOMI game runs through the rig.

- Drop `libflycast_libretro_android.so` into `ConfigManager.CoresDir` — auto-registered as `flycast`
  by [CoresController.ScanForUserCores](../Assets/curif/LibRetroWrapper/CoresController.cs#L131) /
  `SyncCores`. No code change to register.
- Place BIOS (`dc_boot.bin`, `dc_flash.bin`; `naomi.zip`/`awbios.zip` for NAOMI) in `SystemDir`.
- Push `flycast_renderer = "Vulkan"` so it doesn't fall back to GL — add a `flycast` core config
  alongside the `Mame2003PlusConfig()`-style entries in
  [CoresController](../Assets/curif/LibRetroWrapper/CoresController.cs#L60), surfaced through
  `EnvironmentHandlerCB`.
- `TestController` targets a small known-good NAOMI/DC ROM (native 640×480 — comfortably inside any
  `Texture2D`; the 1024×768 software ceiling does not apply to the HW path).

**Pass gate:** the game boots and is playable on the quad; start/stop the core 5+ times without leak
or crash (watch `OnMemoryUsageChanged` / `OnLowMemory` in [Init.cs](../Assets/curif/LibRetroWrapper/Init.cs#L68)).

### Phase 5 — Promote to a real cabinet + perf hardening
**Goal:** swap the test quad for the real CRT cabinet and meet the VR perf bar.

- Point a real cabinet's screen at the HW path: a cabinet YAML with `core: flycast`, going through the
  normal `ShaderCRT` chain and coin slot. The test quad is retired here.
- Wire Unity's `VkPipelineCache` (already fetched, never used in `vulkan.c`) to cut Flycast's
  first-boot shader-compile stall.
- **Perf & thermal acceptance (the real bar):**
  - Sustained **90 Hz** in-room with the cabinet running; measure stereo frame-time headroom with the
    emulator submitting on the shared queue.
  - **15-20 min** continuous play with OVR Metrics Tool: no thermal throttle, stale-frame count near
    zero.
  - Memory budget holds alongside the 512 MB GLB cache + texture caches on 8 GB.
  - Try Flycast "threaded rendering" **off first** (easier to debug sync), then on as an optimization.

---

## 5. Fallback: copy-blit instead of zero-copy

If `CreateExternalTexture`/`UpdateExternalTexture` fights Flycast's per-frame image recycling or
layout expectations, fall back to a **GPU blit** from the core's `VkImage` into the existing
`GameTexture` (or a `RenderTexture`). One extra copy, slightly slower, but it decouples image
lifetimes and sidesteps format/layout mismatch. It's the lower-risk way to prove Phases 2-4 before
chasing true zero-copy. Keep it as a runtime-selectable path in `LoadVulkanTextureData` during
bring-up.

---

## 6. Safety, rollback, and not breaking the arcade

- **Flag-gated, default off.** `HWRenderTestRig` and the HW dispatch are behind `DEBUG_ACTIVE` + an
  explicit `hwRenderTest` flag. Software cores (mame2003-plus, fbneo) are unaffected — they never set
  `hardware_rendering`, so `UpdateTexture` keeps routing to `LoadTextureData`.
- **No simulation pollution.** The rig logs to logcat and an optional PNG dump in `DebugDir` — **no
  in-VR overlay**. The debug quad is visibly a test object, not dressed as a cabinet, and is deleted
  before any player-facing tag.
- **Reversibility.** Phase 0's `UnityVulkan` object and the native sync code are additive; reverting
  is removing the GameObject and restoring the stubs. The software path is the untouched default.
- **Per-phase gates are hard stops.** Do not advance on a black screen "that probably works" — a
  missing wrap and a silent sync race look identical from the couch. Confirm each gate in logcat /
  RenderDoc first.

---

## 7. Open risks specific to in-game testing

- **Queue-access timing with Unity's render thread.** `AccessQueue` is callback-driven; if it can't
  give synchronous mutex-style access, the coordination in Phase 1 may need to hook `vkQueueSubmit`
  via `InterceptInitialization` — more invasive. Surfaces first as Phase 1 validation errors.
- **Worker-thread vs render-thread vs main-thread** all touch Vulkan. The core renders on the
  `Task.Run` worker; Unity submits on its render thread; the texture wrap happens on the main thread.
  The Phase 1 mutex must cover all three.
- **Thermal masking.** A 30-second smoke test passes; a 20-minute session throttles. The Phase 5 gate
  must be the sustained run, not the smoke test.
- **GPU-handle honoring.** If a future core ignores Unity's `VkPhysicalDevice`
  ([§9.5](libretro_video_pipeline.md)), the wrapped image is unsampleable and no stub fix helps. Check
  the `context.gpu` log line on first boot of any new core.

---

## 8. Quick checklist

- [ ] Phase 0: `UnityVulkan` on `[VulkanBootstrap]` in `FixedScene`; logcat shows matching non-zero handles.
- [ ] Phase 1: sync callbacks implemented; 10 min run, zero validation errors, 90 Hz held.
- [ ] Phase 2: `get_image_info` + `LoadVulkanTextureData` + image.c signal; first frame on `TestScreenQuad` + PNG dump.
- [ ] Phase 3: layout/format/orientation correct; `wait_sync_index` real; 5 min clean under RenderDoc.
- [ ] Phase 4: Flycast registered, BIOS in place, `flycast_renderer=Vulkan`; game playable; 5x start/stop clean.
- [ ] Phase 5: real CRT cabinet; pipeline cache wired; 15-20 min @ 90 Hz, no throttle, memory in budget.
- [ ] Rig removed / disabled for release; software cores verified unaffected.
</content>
</invoke>
