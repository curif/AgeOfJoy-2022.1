# Third-Party Notices

Age of Joy is licensed under the [GNU General Public License v3.0](./gpl-3.0.md).

This file lists third-party software distributed **in the Age of Joy application binary (APK)** or
included in this source repository, together with the notices their licenses require us to carry.
Several of these components are gitignored in this repository but are nonetheless linked into or
packaged with the shipped APK — the obligations attach to what we distribute, not to what we
commit.

---

## Emulation cores (native, `lib/arm64-v8a/` in the APK)

The libretro cores below are **separate programs**. Age of Joy loads them at runtime with
`dlopen()` and communicates with them exclusively across the libretro C ABI. They are not linked
into, and do not form a single work with, the Age of Joy application.

### Flycast (`libflycast_libretro_android.so`)

- **Copyright** © flyinghead and the Flycast contributors; © the Reicast contributors.
- **License:** GNU General Public License, either version 2 of the License, or (at your option)
  any later version (**GPL-2.0-or-later**). *(The libretro core database records Flycast simply as
  "GPLv2". The per-file headers throughout `core/` say "either version 2 … or any later version",
  and no GPLv2-only header exists outside `core/deps/`; the source headers govern.)*
- **Upstream:** https://github.com/flyinghead/flycast
- **Base commit:** `7ec978e85` (upstream `v2.6`, 269 commits back from our head).
- **MODIFIED BY US.** Yes. One additive code commit, `8ed21550f`, touching five files:
  `CMakeLists.txt`, `core/hw/aica/aica_mem.cpp`, `core/hw/aica/aica_mem.h`,
  `core/hw/aica/aica_if.cpp`, `core/hw/arm7/arm_mem.cpp`. The change adds an AICA init-window
  register-write throttle that wins an ARM7 sound-driver startup race under our in-process
  libretro frontend, and links `libandroid` on Android so a weak `ASharedMemory_create` import
  resolves correctly in the `dlopen` namespace. A follow-up commit, `6d6c7c222`, adds
  `AGE_OF_JOY.md` and the per-file GPLv2 §2(a) change notices (documentation only).
- **The shipped binary was built from `6d6c7c222`.** It self-reports
  `v2.6-269-g6d6c7c222` (visible via `retro_get_system_info`), and its SHA-256 is
  `711dc806baf3bdf17d792a9ee940a7871fff09f80c9ce134a531c6bd266780ba`. Rebuilding that commit with
  the recipe in `AGE_OF_JOY.md` reproduces it.
- **Complete corresponding source** for the modified binary we distribute:
  **TO VERIFY — https://github.com/mcwild77/flycast-aoj (branch `age-of-joy`, commit `6d6c7c222`).
  This repository is currently PRIVATE and MUST be made public, or an equivalent written offer
  made, before release.**

Flycast bundles its own third-party dependencies under `core/deps/` (libchdr, vixl, oboe, imgui,
glslang, VulkanMemoryAllocator, and others). Their licenses and notices travel with the Flycast
source linked above.

---

## Native code written for Age of Joy

### libpdlr (`libpdlr.so`)

Source: [`nativebridge/libretro_frontend/`](./nativebridge/libretro_frontend/).

Our own embedded libretro frontend — it is the process that `dlopen()`s a hardware (Vulkan) core.
Licensed **GPL-2.0-or-later** rather than GPL-3.0, deliberately: it shares an address space with a
GPL-2.0-or-later core, and v2-or-later keeps that combination unambiguous while still folding
cleanly into the GPL-3 Age of Joy work as a whole.

### libVulkanPlugin (`libVulkanPlugin.so`)

Source: [`libVulkanPlugin/`](./libVulkanPlugin/). Part of Age of Joy, GPL-3.0.

---

## libretro API headers

Files: [`Assets/curif/LibRetroWrapper/cwrapper/libretro.h`](./Assets/curif/LibRetroWrapper/cwrapper/libretro.h),
[`libretro_vulkan.h`](./Assets/curif/LibRetroWrapper/cwrapper/libretro_vulkan.h)

Copyright (C) 2010-2020 The RetroArch team

> Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
> associated documentation files (the "Software"), to deal in the Software without restriction,
> including without limitation the rights to use, copy, modify, merge, publish, distribute,
> sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all copies or
> substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
> NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
> NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
> DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*(These permissive terms apply only to the libretro API headers themselves, not to any libretro
core.)*

---

## Managed dependencies

| Component | Version | License | Notes |
|---|---|---|---|
| YamlDotNet | bundled | MIT — © 2008-2014 Antoine Aubry and contributors | Full text: [`Licences/YamlDotNet.txt`](./Licences/YamlDotNet.txt) |
| GLTFUtility (Siccity) | git `dfdc5083` | MIT | `com.siccity.gltfutility` |
| Fluid Behavior Tree | 2.2.3 | MIT | `com.fluid.behavior-tree` |
| KtxUnity (atteneder) | 2.2.3 | Apache-2.0 | `com.atteneder.ktx` |
| Eflatun.SceneReference | 1.3.0 | MIT | git dependency |
| Meta XR SDK (All) | 76.0.1 | Proprietary — Oculus SDK License | See note below |
| Unity Engine + packages | 2022.3.18f1 | Proprietary — Unity Companion / Unity EULA | Not redistributed as source |

Licenses above were read from the resolved packages under `Library/PackageCache/`. Apache-2.0
(KtxUnity) is GPL-3-compatible but **not** GPL-2-compatible — another reason Age of Joy as a whole
is GPL-3 and only `libpdlr` is v2-or-later.

### Note on the Meta XR SDK

The shipped APK links Meta's proprietary XR SDK. There is a well-known and unresolved tension
between the GPL and proprietary platform SDKs on Meta Quest; this is inherited from Age of Joy
upstream and is not introduced by the Flycast work. It is recorded here for transparency and is
not resolved by this document.

---

## Editor-only tools (not redistributed)

**Amplify Shader Editor** is used to author shaders in the Unity editor. It is a commercial Unity
Asset Store product and is **not** committed to this repository or redistributed. The shader files
it generates (`Assets/SHA_*.shader` and similar) are our own output and are covered by Age of
Joy's GPL-3.0 license.

---

## Art, models, audio, and other assets

Per-asset licenses and attributions live in [`Licences/`](./Licences/): skybox, low-poly cars,
street lamps, pool table, construction barrier, Star Trek assets, Creative Commons attributions,
and others. Each file names its source and terms.

---

## What we do **not** distribute

Age of Joy ships **no ROMs, no CHDs, and no BIOS images**. Dreamcast/NAOMI BIOS files, arcade
ROMs, and disc images must be supplied by the user and placed on the device. Nothing in this
repository or in the APK contains such content.
