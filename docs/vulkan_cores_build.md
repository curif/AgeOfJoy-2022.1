# Vulkan HW cores — native source & build

**Status:** R&D. Not shipped, not runnable from a clean clone (see [Binaries](#binaries)).

The Flycast-based Dreamcast/NAOMI/Atomiswave cores that drive the experimental
Vulkan cabinets are **not** built from this repository. The C# bridge glue
(`PdFlycast.cs`, `LibretroHWBridge.cs`, `PdLibretroProbe.cs`,
`cwrapper/vulkan.*`, `libVulkanPlugin/`) lives here and is tracked on the
`fix/v0.5-2` branch, as does the libretro host frontend source under
`nativebridge/libretro_frontend/` (builds `libpdlr.so`). The **native core
source** (the Flycast emulator itself) lives in a separate repository.

## Native source repository

- **Repo:** `https://github.com/mcwild77/flycast-aoj`
- **Branch:** `age-of-joy`
- **AoJ modifications:** `8f7a90bc` — ARM7/AICA audio-race throttle (Dreamcast/NAOMI BGM)
- **Upstream base:** flycast `7ec978e8521f75427ad38eb8f8f4f3cabaa891d0`
  (`flyinghead/flycast`, kept as the `upstream` remote for pulling fixes)

It is a **public copy**, not a GitHub fork (it began life private, and fork
visibility can't be made private). Because Age of Joy ships a Flycast-derived
`.so` to end users, GPL-2 requires that the corresponding source of the modified
core be offered to those users — this repository is that offer, and it must stay
public and in sync with whatever binary we distribute. The core is a
separately-built `.so` loaded at runtime via P/Invoke — the standard libretro
aggregation, which keeps GPL-2 (core) and GPL-3 (AoJ) compatible; do not
statically link them into one binary.

## Building `libflycast_libretro_android.so`

Built from the `flycast-aoj` tree with CMake + Ninja against Unity's bundled
Android NDK. Cache values from the working build tree:

| Setting | Value |
|---|---|
| Generator | Ninja |
| `CMAKE_BUILD_TYPE` | `Release` |
| `LIBRETRO` | `ON` |
| `ANDROID_ABI` | `arm64-v8a` |
| `ANDROID_PLATFORM` | `android-29` |
| `CMAKE_TOOLCHAIN_FILE` | `<UnityHub>/Editor/2022.3.18f1/Editor/Data/PlaybackEngines/AndroidPlayer/NDK/build/cmake/android.toolchain.cmake` |

```sh
# from the flycast-aoj checkout (branch age-of-joy)
cmake -S . -B build-android -G Ninja \
  -DCMAKE_BUILD_TYPE=Release \
  -DLIBRETRO=ON \
  -DANDROID_ABI=arm64-v8a \
  -DANDROID_PLATFORM=android-29 \
  -DCMAKE_TOOLCHAIN_FILE="<path-to>/AndroidPlayer/NDK/build/cmake/android.toolchain.cmake"
cmake --build build-android
# output: build-android/flycast_libretro.so
```

Then copy into place and rename:

```
build-android/flycast_libretro.so  ->  Assets/Plugins/Android64/libflycast_libretro_android.so
```

The companion `libpdlr.so` bridge binary is built from
`nativebridge/libretro_frontend/` (in this repo); `libVulkanPlugin.so` is built
from `libVulkanPlugin/` and the `cwrapper/` sources.

## Binaries

`*.so` and `*.dll` are gitignored (see `.gitignore`), so the compiled cores in
`Assets/Plugins/Android64/` are **not** in version control and a clean clone
cannot run the Vulkan cabinets. Rebuild them from `flycast-aoj` + `libVulkanPlugin`
and drop them into `Assets/Plugins/Android64/`. This is intentional while the
feature is R&D; revisit (e.g. git-lfs or a release artifact) if/when it ships.

## Never commit

BIOS, ROMs, and CHDs stay out of git entirely. During R&D they live under the
gitignored `claudedocs/` (`claudedocs/bios`, `claudedocs/rom`, …) alongside the
untracked working notes. Do not add them to the repo or to the `flycast-aoj`
repo.
