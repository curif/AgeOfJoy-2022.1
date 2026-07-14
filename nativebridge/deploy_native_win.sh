#!/usr/bin/env bash
# Stage the built native binaries into the Unity tree (arm64-v8a), on WINDOWS (git-bash).
#
#   libflycast_libretro_android.so  <- flycast-aoj  build/flycast_libretro.so   (renamed)
#   libpdlr.so                      <- libretro_frontend/build/libpdlr.so
#
# The rename is required: Android only extracts lib*.so from the APK into nativeLibraryDir,
# while libretro cores are conventionally unprefixed (flycast's CMake clears the lib prefix
# on purpose). Renaming here keeps the fork byte-identical to a stock libretro build, so the
# same .so still loads in RetroArch.
#
# Override FLYCAST_SRC if your clone lives elsewhere.  Args: [core|pdlr]  (default: both)
set -euo pipefail

FLYCAST_SRC="${FLYCAST_SRC:-/e/flycast-aoj}"

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$HERE/.." && pwd)"
PLUGIN="$ROOT/Assets/Plugins/Android64"

CORE_SRC="$FLYCAST_SRC/build/flycast_libretro.so"
CORE_DST="$PLUGIN/libflycast_libretro_android.so"
PDLR_SRC="$HERE/libretro_frontend/build/libpdlr.so"
PDLR_DST="$PLUGIN/libpdlr.so"

what="${1:-both}"
[ -d "$PLUGIN" ] || { echo "ERROR: no Unity plugin dir: $PLUGIN" >&2; exit 1; }

stage() { # src dst label
  [ -f "$1" ] || { echo "ERROR: $3 not built: $1" >&2; exit 1; }
  cp -f "$1" "$2"
  echo ">>> $3  $(du -h "$2" | cut -f1)  $(sha256sum "$2" | cut -c1-16)"
  echo "    -> ${2#$ROOT/}"
}

if [ "$what" = both ] || [ "$what" = core ]; then
  stage "$CORE_SRC" "$CORE_DST" "flycast core"
  # GPL source-correspondence: the shipped .so must match a PUBLIC flycast-aoj commit.
  if git -C "$FLYCAST_SRC" rev-parse --short HEAD >/dev/null 2>&1; then
    rev="$(git -C "$FLYCAST_SRC" rev-parse --short HEAD)"
    git -C "$FLYCAST_SRC" diff --quiet HEAD 2>/dev/null || rev="$rev-DIRTY"
    echo "    built from flycast-aoj $rev"
    case "$rev" in *-DIRTY) echo "    WARNING: uncommitted core source — do not ship this build" >&2;; esac
  fi
fi

if [ "$what" = both ] || [ "$what" = pdlr ]; then
  stage "$PDLR_SRC" "$PDLR_DST" "libpdlr     "
fi

echo ">>> staged. Binaries are gitignored (.gitignore:133) — nothing to commit."
