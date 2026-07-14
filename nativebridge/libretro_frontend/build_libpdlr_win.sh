#!/usr/bin/env bash
# Build libpdlr.so (arm64-v8a) on WINDOWS (git-bash) — the embedded libretro frontend.
# Self-contained (system libs + static C++ runtime), like libpdvk. See libpdlr.h.
#
# Defaults to the Unity-bundled r23b NDK. Override with ANDROID_NDK_HOME / API / LIBRETRO_INC.
set -euo pipefail

NDK_DEFAULT="/c/Program Files/Unity/Hub/Editor/2022.3.18f1/Editor/Data/PlaybackEngines/AndroidPlayer/NDK"
NDK="${ANDROID_NDK_HOME:-$NDK_DEFAULT}"
API="${API:-29}"

TOOLS="$NDK/toolchains/llvm/prebuilt/windows-x86_64"
CLANGXX="$TOOLS/bin/clang++.exe"
SYSLIB="$TOOLS/sysroot/usr/lib/aarch64-linux-android/$API"

# libretro.h / libretro_vulkan.h live in AoJ's cwrapper. Override with LIBRETRO_INC if needed.
LIBRETRO_INC_DEFAULT="/e/AgeOfJoy-2022.1_curif/Assets/curif/LibRetroWrapper/cwrapper"
LIBRETRO_INC="${LIBRETRO_INC:-$LIBRETRO_INC_DEFAULT}"

# Unity native plugin API headers (IUnityGraphicsVulkan.h etc.) — for the Step B zero-copy import.
# Ship with the editor. Override with UNITY_PLUGINAPI if your editor lives elsewhere.
UNITY_PLUGINAPI_DEFAULT="/c/Program Files/Unity/Hub/Editor/2022.3.18f1/Editor/Data/PluginAPI"
UNITY_PLUGINAPI="${UNITY_PLUGINAPI:-$UNITY_PLUGINAPI_DEFAULT}"

[ -x "$CLANGXX" ] || { echo "ERROR: missing clang++: $CLANGXX" >&2; exit 1; }
[ -f "$SYSLIB/libvulkan.so" ] || { echo "ERROR: no libvulkan.so at API $API ($SYSLIB)" >&2; exit 1; }
[ -f "$LIBRETRO_INC/libretro.h" ] || { echo "ERROR: no libretro.h at $LIBRETRO_INC" >&2; exit 1; }
[ -f "$UNITY_PLUGINAPI/IUnityGraphicsVulkan.h" ] || { echo "ERROR: no Unity PluginAPI headers at $UNITY_PLUGINAPI" >&2; exit 1; }

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD="$HERE/build"
mkdir -p "$BUILD"

echo ">>> NDK: $NDK"
echo ">>> compiling libpdlr.so (arm64-v8a, API $API)"
"$CLANGXX" \
  --target=aarch64-linux-android$API \
  -fPIC -shared -O2 -std=c++17 -fvisibility=hidden \
  -fno-exceptions -fno-rtti -static-libstdc++ \
  -I"$HERE" -I"$LIBRETRO_INC" -I"$UNITY_PLUGINAPI" \
  "$HERE/libpdlr.cpp" \
  -L"$SYSLIB" -lvulkan -llog -landroid -ldl \
  -Wl,-soname,libpdlr.so \
  -o "$BUILD/libpdlr.so"

echo ">>> wrote $BUILD/libpdlr.so ($(du -h "$BUILD/libpdlr.so" | cut -f1))"
echo ">>> exported symbols:"
"$TOOLS/bin/llvm-nm.exe" -D "$BUILD/libpdlr.so" | grep pdlr_ || echo "    (no pdlr_ symbols!)"
