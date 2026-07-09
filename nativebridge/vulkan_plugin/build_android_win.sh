#!/usr/bin/env bash
# Build libpdvk.so (arm64-v8a) on WINDOWS (git-bash) — the isolated Vulkan context plugin.
# Windows-only port of the original macOS build_android.sh. See PDDocs/vulkancontext.md.
#
# Steps: glslc compiles the two GLSL shaders -> SPIR-V -> xxd embeds them as C headers,
# then clang++ links a shared lib against the NDK's libvulkan stub. Output is copied
# straight into PDUnity/Assets/Plugins/Android/pdvk/.
#
# Defaults to the Unity-bundled NDK (r23b). Override with ANDROID_NDK_HOME / API.
set -euo pipefail

NDK_DEFAULT="/c/Program Files/Unity/Hub/Editor/2022.3.18f1/Editor/Data/PlaybackEngines/AndroidPlayer/NDK"
NDK="${ANDROID_NDK_HOME:-$NDK_DEFAULT}"
API="${API:-29}"   # link level; Quest runs higher. AHardwareBuffer (later) needs >=26.

TOOLS="$NDK/toolchains/llvm/prebuilt/windows-x86_64"
CLANGXX="$TOOLS/bin/clang++.exe"
GLSLC="$NDK/shader-tools/windows-x86_64/glslc.exe"
SYSLIB="$TOOLS/sysroot/usr/lib/aarch64-linux-android/$API"

# Unity native plugin API headers (IUnityGraphicsVulkan.h etc.) — ship with the editor. Override
# with UNITY_PLUGINAPI if your editor lives elsewhere.
UNITY_PLUGINAPI_DEFAULT="/c/Program Files/Unity/Hub/Editor/2022.3.18f1/Editor/Data/PluginAPI"
UNITY_PLUGINAPI="${UNITY_PLUGINAPI:-$UNITY_PLUGINAPI_DEFAULT}"

for t in "$CLANGXX" "$GLSLC"; do
  [ -x "$t" ] || { echo "ERROR: missing toolchain tool: $t" >&2; exit 1; }
done
[ -f "$SYSLIB/libvulkan.so" ] || { echo "ERROR: no libvulkan.so at API $API ($SYSLIB)" >&2; exit 1; }
[ -f "$UNITY_PLUGINAPI/IUnityGraphicsVulkan.h" ] || { echo "ERROR: no Unity PluginAPI headers at $UNITY_PLUGINAPI" >&2; exit 1; }
command -v xxd >/dev/null || { echo "ERROR: xxd not found (git-bash should provide it)" >&2; exit 1; }

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO="$(cd "$HERE/../.." && pwd)"
OUT="$REPO/PDUnity/Assets/Plugins/Android/pdvk"
BUILD="$HERE/build"
mkdir -p "$BUILD" "$OUT"

echo ">>> NDK: $NDK"
echo ">>> compiling shaders -> SPIR-V -> embedded headers"
emit_spv() {  # <glsl-stage> <src> <symbol>
  local stage="$1" src="$2" sym="$3"
  "$GLSLC" -fshader-stage="$stage" -O "$HERE/$src" -o "$BUILD/$sym.spv"
  ( cd "$BUILD" && xxd -i "$sym.spv" ) \
    | sed "s/unsigned char ${sym}_spv\[\]/const unsigned char ${sym}_spv[]/; \
           s/unsigned int ${sym}_spv_len/const unsigned int ${sym}_spv_len/" \
    > "$BUILD/$sym.spv.h"
}
emit_spv vert checker.vert checker_vert
emit_spv frag checker.frag checker_frag

echo ">>> compiling libpdvk.so (arm64-v8a, API $API)"
# -static-libstdc++ folds the (tiny, no-exceptions/RTTI) C++ runtime in so the plugin is
# self-contained -- no libc++_shared.so to ship/collide with other plugins' copy.
"$CLANGXX" \
  --target=aarch64-linux-android$API \
  -fPIC -shared -O2 -std=c++17 -fvisibility=hidden \
  -fno-exceptions -fno-rtti -static-libstdc++ \
  -I"$HERE" -I"$BUILD" -I"$UNITY_PLUGINAPI" \
  "$HERE/pdvk.cpp" \
  -L"$SYSLIB" -lvulkan -llog -landroid \
  -Wl,-soname,libpdvk.so \
  -o "$OUT/libpdvk.so"

echo ">>> wrote $OUT/libpdvk.so ($(du -h "$OUT/libpdvk.so" | cut -f1))"
echo ">>> exported symbols:"
"$TOOLS/bin/llvm-nm.exe" -D "$OUT/libpdvk.so" | grep pdvk_ || echo "    (llvm-nm found no pdvk_ symbols!)"
