#!/usr/bin/env python3
"""Add libandroid.so to a libretro core's DT_NEEDED so its WEAK ASharedMemory_create resolves.

Why: Flycast (and similar cores) reference ASharedMemory_create as a weak undefined symbol but do
NOT link libandroid. Run standalone via RetroArch they get it from the frontend's scope; embedded
in another process (AoJ/Unity) Android's linker-namespace isolation leaves the weak symbol NULL, so
the core falls back to legacy /dev/ashmem (SELinux-blocked on Quest, API 31) -> nvmem disabled ->
fastmem crash. Adding libandroid as a direct NEEDED makes the weak symbol resolve in the core's own
dependency closure. This edits ONLY the dynamic dependency list -- no code is changed.

Usage:
    python patch_core_add_libandroid.py <input_core.so> <output_core.so>
    python patch_core_add_libandroid.py                 # uses the AoJ defaults below

Requires: pip install lief
Re-run this on any newer Flycast core you drop in (it's a no-op if libandroid is already NEEDED).
"""
import sys
import lief

# Defaults for this project: patch the pristine original into the AoJ plugin slot.
DEFAULT_SRC = r"E:\AgeOfJoy-2022.1_curif\claudedocs\SO cores\flycast_libretro_android.so"
DEFAULT_DST = r"E:\AgeOfJoy-2022.1_curif\Assets\Plugins\Android64\libflycast_libretro_android.so"

LIB = "libandroid.so"


def needed(binary):
    tag = lief.ELF.DynamicEntry.TAG.NEEDED
    return [e.name for e in binary.dynamic_entries if e.tag == tag]


def main():
    src = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_SRC
    dst = sys.argv[2] if len(sys.argv) > 2 else DEFAULT_DST

    b = lief.parse(src)
    if b is None:
        print(f"ERROR: could not parse {src}", file=sys.stderr)
        return 1

    before = needed(b)
    print("NEEDED before:", before)
    if LIB in before:
        print(f"{LIB} already present — writing unchanged copy")
    else:
        b.add_library(LIB)
        print(f"added {LIB}")
    b.write(dst)

    after = needed(lief.parse(dst))
    print("NEEDED after :", after)
    ok = LIB in after
    print("OK" if ok else "FAILED")
    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
