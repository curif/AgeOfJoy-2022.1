# Vulkan Support

Implementing Vulkan has long been a goal for the AOJ team. The original plan
was to implement a one-size-fits-all Vulkan wrapper, similar to the way that
AOJ can run most if not all software-rendered Retroarch cores in .so format.

Unfortunately, Vulkan cores come with a large number of gotchas that require
a tighter integration with AOJ and Unity, so for our first core integration,
Flycast, we need to build a special binary.


# Vulkan HW cores — native source & build

The Flycast-based Dreamcast/NAOMI/Atomiswave cores that drive the experimental Vulkan cabinets are **not** built from this repository. The C# side lives here and is tracked on the `fix/v0.5-2` branch: `LibretroFlycastCore.cs` (the runtime driver for Vulkan cabinets), `LibretroHWBridge.cs` (P/Invoke into `libpdlr.so`), `cwrapper/vulkan.*` and `libVulkanPlugin/`, plus the debug-only helpers `PdFlycast.cs` (flat-quad test driver) and `PdLibretroProbe.cs`. The libretro host frontend source under `nativebridge/libretro_frontend/` (builds `libpdlr.so`) is tracked here too. The **native core source** (the Flycast emulator itself) lives in a separate repository so we can easily update it when Flycast itself is updated.



## Native source repository

- **Repo:** `https://github.com/mcwild77/flycast-aoj`
- **Branch:** `age-of-joy`
- **AoJ modifications:** three commits atop the upstream base — `8ed21550f` (ARM7/AICA audio-race throttle for Dreamcast/NAOMI BGM, plus CMake linking `log`/`android` so `ASharedMemory_create` resolves), `6d6c7c222` (GPLv2 §2(a) change notices + `AGE_OF_JOY.md`), `53a874ec7` (build-doc tweak; branch tip)
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
cmake -S . -B build -G Ninja \
  -DCMAKE_BUILD_TYPE=Release \
  -DLIBRETRO=ON \
  -DANDROID_ABI=arm64-v8a \
  -DANDROID_PLATFORM=android-29 \
  -DCMAKE_TOOLCHAIN_FILE="<path-to>/AndroidPlayer/NDK/build/cmake/android.toolchain.cmake"
cmake --build build
# output: build/flycast_libretro.so
```

Then copy into place and rename:

```
build/flycast_libretro.so  ->  Assets/Plugins/Android64/libflycast_libretro_android.so
```

The companion `libpdlr.so` bridge binary is built from
`nativebridge/libretro_frontend/` (in this repo) via `build_libpdlr_win.sh`;
`libVulkanPlugin.so` is built from `libVulkanPlugin/` and the `cwrapper/` sources.

## Binaries

`*.so` and `*.dll` are gitignored (see `.gitignore`), with one exception: `Assets/Plugins/Android64/libVulkanPlugin.so` (and its `.meta`) is tracked in git. A clean clone is therefore missing `libflycast_libretro_android.so` and `libpdlr.so` and cannot run the Vulkan cabinets until both are rebuilt — the core from `flycast-aoj` (above), the bridge via `nativebridge/libretro_frontend/build_libpdlr_win.sh` — and dropped into `Assets/Plugins/Android64/`.