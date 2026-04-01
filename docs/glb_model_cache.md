# GLB Model Cache — Architecture and Memory Tracking

## Overview

`CabinetFactory` loads `.glb` cabinet models via GLTFUtility and stores them in an LRU cache
(`ConfigManager.CabinetCache`, budget 512 MB). Cached templates are inactive `GameObject`s that
are instantiated each time a cabinet needs that model — avoiding repeated disk I/O and parse
time across room transitions.

---

## Why Disk File Size Is Wrong

The original code used `FileInfo.Length` (disk bytes) as the cache size:

```csharp
ConfigManager.CabinetCache.Add(cacheKey, model, fileInfo.Length / (1024f * 1024f));
```

A 50 MB `.glb` file expands to 200–500 MB in unified RAM after mesh and texture decode. The LRU
cache therefore believed it was managing ~512 MB while actual allocation was 2–5 GB. **Eviction
never fired.**

---

## Why `Profiler.GetRuntimeMemorySizeLong()` Is Also Wrong

Same failure mode documented in `CabinetTextureCache.cs:550–562`: after GPU upload the CPU copy
is discarded (`tex.Apply(false, true)`). `GetRuntimeMemorySizeLong()` cannot see GPU-mapped
memory — it returns 0 for every texture that went through this path.

On Meta Quest (Snapdragon XR2 / XR2 Gen 2) the CPU and GPU share the same LPDDR5 pool (UMA).
There is no discrete VRAM. The LRU budget protects this shared pool regardless of where buffers
are mapped.

**Do not use `Profiler.GetRuntimeMemorySizeLong()` for GameObjects with GPU-uploaded data.**

---

## Manual Size Formula (`CalculateGameObjectSizeBytes`)

`CabinetFactory.CalculateGameObjectSizeBytes()` sums the two dominant memory consumers:

### Meshes

```
vertexCount × GetVertexBufferStride(stream 0)     // vertex buffer
+ GetIndexCount(0) × (UInt16 → 2, UInt32 → 4)    // index buffer
```

`vertexCount` and `indexFormat` are geometry metadata that remain valid even after
`Mesh.UploadMeshData(true)` discards the CPU copy. By default Unity retains both a CPU copy and
a GPU-mapped copy until `UploadMeshData(true)` is called, so the formula counts the full
allocation.

Shared meshes are deduplicated by instance ID.

### Textures

Delegates to `CabinetTextureCache.CalculateActualSizeBytes()` which uses
`width × height × bpp × mipmap_multiplier`. This formula is correct regardless of where the
CPU copy lives. See `docs/texture_caching_and_compression.md` for the full format table.

---

## GLB Texture Compression

GLTFUtility decodes embedded GLB textures to `RGBA32` — the same starting format as cabinet art.
Immediately after load, `CabinetFactory` calls `Texture2D.Compress(false)` on each readable
texture (ETC2 on Android), then `tex.Apply(false, true)` to upload to the GPU and free the CPU
copy. This gives a 4–8× GPU memory reduction per texture.

Gated on `DeviceController.originalTextures` — the same flag that controls compression in
`CabinetTextureCache`. Textures whose dimensions are not multiples of 4 are skipped (ETC2
requirement).

**`CalculateGameObjectSizeBytes` must be called after compression** so that compressed sizes
are measured, not the original `RGBA32` sizes.

---

## Persisted Size Cache (`metadata.yaml`)

Walking a `GameObject` to sum mesh and texture sizes is cheap but not free. To avoid repeating
it on every load of the same GLB version, the result is persisted in `metadata.yaml`:

```yaml
hashes:
  1942williams.glb: c0ffb166d62df8cfa099d0f96b1f7c10

modified_at:
  1942williams.glb: "2025-11-04T08:32:17Z"

sizes_mb:
  c0ffb166d62df8cfa099d0f96b1f7c10: 187.4
```

`sizes_mb` is keyed by **hash** (not filename). If the GLB is replaced, the hash changes, the
old size entry no longer matches, and the size is recalculated automatically.

### Hash Freshness (`verifyAndRefreshHash`)

`CabinetMetadata.init()` computes MD5 hashes the first time `metadata.yaml` is created and
stores the file's UTC modification timestamp in `modified_at`. On subsequent loads,
`verifyAndRefreshHash()` checks:

1. Read `File.GetLastWriteTimeUtc()` — cheap, no file read.
2. Compare to `modified_at[filename]`.
3. **Timestamps match** → hash presumed valid, return `true`.
4. **Timestamps differ** → re-compute MD5, update `hashes` and `modified_at`, clear the stale
   `sizes_mb` entry, save `metadata.yaml`. Return `false` (size must be recalculated).

This keeps expensive MD5 computation rare — only on actual file change.

---

## GLTFUtility Leaked `GameObject` Workaround

`GLTFUtility.Importer.LoadAsync()` calls `GetRoot()` twice for GLBs with 2+ top-level nodes
(common in Blender exports). `GetRoot()` creates a `new GameObject("Root")` wrapper each call.
The second call steals all children from the first. The first wrapper is left empty and leaked
into the scene.

**Workaround:** snapshot the root `GameObject` instance IDs of `FixedScene` (where GLB loads
happen) before the load; after the load, destroy any new root that is not the model returned by
the importer. Between the snapshot and the scan only GLTFUtility activity can add scene-root
objects, so `go != model` is the only guard needed.

---

## LRU Budget

| Constant | Value | Notes |
|---|---|---|
| `ConfigManager.CabinetCache` | 512 MB | Quest 2 baseline |

Consider increasing to 1024 MB for Quest 3 after collecting real-device logs, following the same
pattern as `CACHE_SIZE_Q3` in `CabinetTextureCache`.
