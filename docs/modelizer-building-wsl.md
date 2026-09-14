# Building the Android core from WSL

This is a supplementary guide for building `libmodelizer_libretro_android.so` from a
**WSL2 (Ubuntu) shell on Windows**, working directly against the repo checkout on the Windows
filesystem (`/mnt/c/...`). It's an alternative to the MSYS2 MINGW64 path in [`BUILDING.md`](BUILDING.md)
— use it if you don't want to install the MSYS2 mingw-w64 toolchain, or if you hit the NDK symlink
issue described in step 2 below.

## 1. Get Android NDK r27d — the Linux build, not the Windows one

Even though the project lives on the Windows filesystem, building from WSL needs the **Linux**
host-toolchain build of the NDK (`linux-x86_64`), not the Windows one — WSL runs Linux ELF binaries,
not Windows `.exe`s.

```sh
curl -L -o android-ndk-r27d-linux.zip \
  https://dl.google.com/android/repository/android-ndk-r27d-linux.zip
```

Expected: ~633 MB, SHA1 `22105e410cf29afcf163760cc95522b9fb981121`.

Older NDKs (e.g. NVPACK's bundled r14b) are too old; this build is written against r27d specifically.

You can put the extracted NDK anywhere with enough free space — it does not need to be near the repo.
If your `C:` drive is tight on space, another drive works fine, e.g. `D:\Android\android-ndk-r27d`.

## 2. Extract it with `unzip`, not Windows Explorer

**This is the step that actually matters.** The NDK zip contains real Unix symlinks (e.g.
`bin/clang -> bin/clang-18`). Windows Explorer's "Extract All" has no concept of Unix symlinks and
silently turns each one into a small plain-text file containing just the link target's name. The
build then fails much later, deep in compilation, with a confusing error like:

```
.../toolchains/llvm/prebuilt/linux-x86_64/bin/clang: 1: clang-18: not found
```

(that's the shell trying to execute the flattened text file's first line as a command).

Always extract from a WSL/Linux shell instead, onto a path WSL can write to (e.g. a Windows drive
mounted at `/mnt/d/...` is fine):

```sh
mkdir -p /mnt/d/Android
cd /mnt/d/Android
unzip ~/android-ndk-r27d-linux.zip
```

Verify the symlink survived before doing anything else:

```sh
ls -la /mnt/d/Android/android-ndk-r27d/toolchains/llvm/prebuilt/linux-x86_64/bin/clang
# expect: ... clang -> clang-18   (note the leading 'l' and the arrow)
```

If `file` on that path reports "ASCII text" instead of "symbolic link", re-extract with `unzip` again
— don't proceed until this check passes.

## 3. Build

From WSL, in the repo root (works fine from `/mnt/c/...`, no need to move the repo):

```sh
export ANDROID_NDK_HOME=/mnt/d/Android/android-ndk-r27d
REGENIE=1 ./build-android.sh       # first build, or after any scripts/ change
./build-android.sh                 # subsequent incremental builds
```

WSL's Ubuntu userland ships `gcc`/`make` out of the box, so no extra host toolchain install is needed
(unlike Git-for-Windows Bash, which has no compiler at all and can't run this script's genie-bootstrap
step — see the note in `BUILDING.md`).

A full build compiles a MAME subtarget cross to arm64 and takes a while; expect several minutes.

## 4. Where to find the output

```
<repo root>/libmodelizer_libretro_android.so
```

This is the **stripped** core — the one to ship or install. It's placed directly in the repo root,
next to `build-android.sh` itself (not under `build/`).

An **unstripped** copy is kept at:

```
<repo root>/build/android/libmodelizer_libretro_android.so
```

Don't ship that one — it carries the full debug info (needed only for symbolising a native crash from
a device with `ndk-stack -sym build/android`).

If you're comparing file sizes against another build (e.g. a colleague's) and the numbers don't match,
double-check you're both looking at the repo-root copy, not the `build/android/` one — the unstripped
copy is meaningfully larger.

`STRIP=0 ./build-android.sh` skips stripping and leaves the root copy unstripped too, if you need that
for local debugging.
