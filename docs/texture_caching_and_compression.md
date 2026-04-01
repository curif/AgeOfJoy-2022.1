# Age of Joy: Texture Pipeline, Compression, and Caching

This document outlines the internal workings of the texture management system in Age of Joy. Given the VR nature of the project (specifically targeting standalone hardware like Meta Quest), efficiently loading, compressing, and caching custom arcade cabinet artwork is critical to maintaining high frame rates and preventing out-of-memory (OOM) crashes.

---

## 1. Texture Formats and Memory
When users provide custom artwork (PNGs, JPGs) for cabinets, they are initially loaded into memory using Unity's `UnityWebRequest` and `DownloadHandlerTexture`.

By default, Unity decodes these raw image files into **Uncompressed 32-bit Formats** such as `TextureFormat.RGBA32` or `TextureFormat.ARGB32` (often referred to generically as ARGB8 or RGBA8, meaning 8 bits per channel).
*   **The Problem:** Uncompressed textures are massive. A single 2048x2048 uncompressed RGBA32 texture consumes **16MB** of VRAM. A room full of cabinets with marquees, bezels, and side art can easily exceed the memory limits of a mobile VR headset.
*   **The Solution:** We utilize Unity's hardware compression (`Texture2D.Compress()`) to convert these raw formats into heavily optimized block-compressed formats like **ETC2** or **ASTC**. This reduces the memory footprint by up to 75% (e.g., from 16MB down to 2-4MB) with minimal visual degradation.

## 2. The "Multiple of 4" Constraint and GPU Resizing
Hardware compression algorithms (ETC2, ASTC, DXT) operate on blocks of pixels, typically 4x4. Therefore, **Unity can only compress a texture if its width and height are exact multiples of 4.**

If a user provides an image with dimensions like `1001 x 1000`:
1.  Unity's `Compress()` function will silently fail or be skipped.
2.  The texture will remain in memory as an uncompressed `RGBA32` file, drastically hurting performance.

### The GPU Resize Solution
To guarantee all textures can be compressed without enforcing strict rules on the end-user, the `CabinetTextureCache` implements an automatic **Runtime GPU Resize**:
1.  When an image is loaded, its dimensions are checked (`width % 4 == 0 && height % 4 == 0`).
2.  If invalid, the dimensions are rounded up to the nearest multiple of 4.
3.  A temporary `RenderTexture` is created, and `Graphics.Blit` is used to stretch the original image to the new dimensions using the GPU's hardware bilinear filter (preserving colors, alpha, and quality perfectly).
4.  **Crucially**, we use `AsyncGPUReadback.Request` to pull the resized image data back from the GPU. This prevents the main thread from blocking. If standard CPU-based resizing (`GetPixels`/`SetPixels`) was used, the VR headset would stutter violently, causing nausea.
5.  Once read back, the now-valid texture is sent to the compression step.

## 3. The Caching Lifecycle
To avoid repeating expensive downloads, resizing, and compression steps, the game uses a robust two-tier caching system (`CabinetTextureCache.cs` and `TextureDiskCache.cs`).

Here is the exact lifecycle of an artwork file:

### Phase A: Disk Cache Check (`.aojv1`)
1.  The system checks if a pre-compiled cache file exists (e.g., `image.png.aojv1`).
2.  **Stale Check:** It compares the "Last Modified" timestamp of the original image with the `.aojv1` file. If the user updated their art, the cache is ignored and rebuilt.
3.  **Fast Load:** If valid, the raw ETC2/ASTC byte array is read directly from the `.aojv1` file using a `BinaryReader`, pushed into a new `Texture2D` via `LoadRawTextureData()`, and sent straight to the GPU via `Apply(false, makeNoLongerReadable: true)`. This bypasses the heavy PNG/JPG decoding entirely.

### Phase B: Processing & Caching
If no valid disk cache exists:
1.  The image is downloaded via `UnityWebRequest`.
2.  The dimensions are checked and **resized via GPU** if necessary (ensuring multiples of 4).
3.  The raw texture is compressed in memory: `texTmp.Compress(false)`.
4.  **Save to Disk:** Immediately after compression, `TextureDiskCache.SaveToDisk` extracts the compressed bytes using `GetRawTextureData()` and writes them to the `.aojv1` binary file alongside the original image.
5.  **Free System RAM:** The system calls `texTmp.Apply(false, true)`. The `true` flag makes the texture "no longer readable" by the CPU, uploading it permanently to the GPU and freeing the duplicate system RAM.

### Phase C: Memory Cache (LRU)
Once loaded (either from disk cache or freshly processed), the `Texture2D` object is stored in an active Memory Cache (`ResourceCacheManager`).
*   This cache tracks the active MB footprint.
*   It uses a Least Recently Used (LRU) policy. When the cache hits its limit (e.g., 1024MB on Quest 2, 1536MB on Quest 3), it automatically unloads the oldest, unused textures to make room for new ones.

---

## 4. Texture Size Calculation (`CalculateActualSizeBytes`)

The LRU cache needs an accurate size in MB for every texture it stores. This is calculated by `CabinetTextureCache.CalculateActualSizeBytes(Texture2D)` using a **manual width × height × bytes-per-pixel formula**, accounting for the texture format and mipmap chain (×1.33 multiplier when mipmaps are present).

### Why not `Profiler.GetRuntimeMemorySizeLong()`?

`Profiler.GetRuntimeMemorySizeLong()` was tried and **must not be used here**. The reason:

After `Apply(false, makeNoLongerReadable: true)` is called (which happens for every texture loaded from the disk cache and after compression), Unity destroys the CPU-side copy of the texture data and retains only the GPU copy. `GetRuntimeMemorySizeLong()` can only measure CPU/native memory — **it returns 0 for any GPU-only texture**.

This was confirmed by log analysis on a release Meta Quest APK (2026-03-31): every disk-cached texture was being stored in the LRU as 0.00 MB, making eviction completely non-functional for those textures and allowing memory to grow unbounded.

The manual formula uses only `tex.width`, `tex.height`, and `tex.format` — all of which remain available on the `Texture2D` object even after the CPU copy is released — so it works correctly in all cases.

---

## 5. Summary of Key Files
*   **`Assets/curif/LibRetroWrapper/CabinetTextureCache.cs`**: The orchestrator. Handles downloading, dimension verification, async GPU resizing, memory caching, and triggering the disk save.
*   **`Assets/curif/LibRetroWrapper/TextureDiskCache.cs`**: Handles the low-level binary I/O for saving and loading the `.aojv1` pre-compressed texture files.
*   **`Assets/curif/LibRetroWrapper/GpuRgb565Converter.cs` / `GpuAlphaCheck.cs`**: Auxiliary tools for advanced GPU-based texture manipulation, primarily used when trying to optimize alpha channels or convert to lower-precision 16-bit formats for older hardware.
