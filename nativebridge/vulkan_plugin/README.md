# vulkan_plugin — `libpdvk.so`

Isolated Vulkan context plugin for the Quest. Owns its **own** `VkInstance`/`VkDevice`
(approach B in `PDDocs/vulkancontext.md`), renders a **rotating checkerboard** offscreen, and
reads it back to a CPU buffer Unity copies onto a quad. Not emulation-related.

## Layout
- `checker.vert` / `checker.frag` — GLSL; compiled to SPIR-V and embedded at build time.
- `pdvk.h` / `pdvk.cpp` — the C ABI + isolated-context implementation.
- `build_android.sh` — glslc → SPIR-V → xxd headers → clang++ → `libpdvk.so`.
- `build/` — generated SPIR-V + embedded headers (gitignored).

## Build
```sh
./build_android.sh         # NDK r27c by default; override with ANDROID_NDK_HOME / API
```
Output lands at `PDUnity/Assets/Plugins/Android/pdvk/libpdvk.so` (arm64-v8a). Self-contained:
links only system libs (`libvulkan`, `liblog`, `libandroid`, libc) — C++ runtime is static, so
there is **no** `libc++_shared.so` to ship or collide with `libpdmame`'s copy.

Verify after building:
```sh
NDK=$HOME/Library/Android/ndk/android-ndk-r27c
$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm -D \
  ../../PDUnity/Assets/Plugins/Android/pdvk/libpdvk.so | grep pdvk_
```
should list `pdvk_init / pdvk_render / pdvk_get_pixels / pdvk_shutdown`.

## Unity wiring
- `PDUnity/Assets/Script/PdVk.cs` — P/Invoke binding (no-op in the macOS editor; Quest-only).
- `PDUnity/Assets/Script/PdVkQuad.cs` — attach to a quad in `VRScene_VulkanContext`; drives
  render → read-back → `Texture2D` each frame.
- The `.so`'s import settings: Unity defaults a file under `Plugins/Android/` to the Android
  platform and reads arm64 from the ELF — confirm the generated `.meta` targets Android/arm64.

## Status
Milestone 1 (CPU read-back) built and symbol-verified on the host. **Not yet run on device** —
next step is deploy + confirm the checkerboard appears and spins (`pdvk_status.txt` in the
app's `persistentDataPath`, pulled via `adb`).
