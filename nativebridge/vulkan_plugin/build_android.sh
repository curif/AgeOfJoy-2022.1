#!/usr/bin/env bash
# Build libpdvk.so (arm64-v8a) — the isolated Vulkan context plugin.
# See PDDocs/vulkancontext.md. Quest-only; there is no host build (macOS is Metal).
#
# Steps: glslc compiles the two GLSL shaders → SPIR-V → xxd embeds them as C headers,
# then clang++ links a shared lib against the NDK's libvulkan stub. Output is copied
# straight into PDUnity/Assets/Plugins/Android/pdvk/.
set -euo pipefail

NDK_DEFAULT="$HOME/Library/Android/ndk/android-ndk-r27c"
export ANDROID_NDK_HOME="${ANDROID_NDK_HOME:-$NDK_DEFAULT}"
API="${API:-29}"   # link level; Quest runs higher. AHardwareBuffer (later) needs >=26.

TOOLS="$ANDROID_NDK_HOME/toolchains/llvm/prebuilt/darwin-x86_64"
CLANGXX="$TOOLS/bin/clang++"
GLSLC="$ANDROID_NDK_HOME/shader-tools/darwin-x86_64/glslc"
SYSLIB="$TOOLS/sysroot/usr/lib/aarch64-linux-android/$API"

for t in "$CLANGXX" "$GLSLC"; do
  [ -x "$t" ] || { echo "ERROR: missing toolchain tool: $t" >&2; exit 1; }
done
[ -f "$SYSLIB/libvulkan.so" ] || { echo "ERROR: no libvulkan.so at API $API ($SYSLIB)" >&2; exit 1; }

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO="$(cd "$HERE/../.." && pwd)"
OUT="$REPO/PDUnity/Assets/Plugins/Android/pdvk"
BUILD="$HERE/build"
mkdir -p "$BUILD" "$OUT"

echo ">>> compiling shaders → SPIR-V → embedded headers"
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

echo ">>> compiling libpdvk.so (arm64-v8a, API $API, clang $("$CLANGXX" -dumpversion))"
# -static-libstdc++ folds the (tiny, no-exceptions/RTTI) C++ runtime in so the plugin is
# self-contained — no libc++_shared.so to ship/collide with libpdmame's copy.
"$CLANGXX" \
  --target=aarch64-linux-android$API \
  -fPIC -shared -O2 -std=c++17 -fvisibility=hidden \
  -fno-exceptions -fno-rtti -static-libstdc++ \
  -I"$HERE" -I"$BUILD" \
  "$HERE/pdvk.cpp" \
  -L"$SYSLIB" -lvulkan -llog -landroid \
  -Wl,-soname,libpdvk.so \
  -o "$OUT/libpdvk.so"

echo ">>> wrote $OUT/libpdvk.so ($(du -h "$OUT/libpdvk.so" | cut -f1))"
echo "    (Unity import settings: Android / arm64 only — verify the generated .meta.)"
