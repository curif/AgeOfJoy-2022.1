# MR Custom Object — `object.yaml` v1

Each placeable custom prop is a **package folder**:

```
{BaseDir}/MR/Custom Objects/<PackageName>/
├── object.yaml      ← this file (required)
├── <model>.glb      ← required (path from model.file)
└── *.wav / *.ogg    ← optional (path from audio.file)
```

Editor: `%UserProfile%\cabs\MR\Custom Objects\`  
Quest: `/sdcard/Android/data/com.curif.AgeOfJoy/MR/Custom Objects/`

The **folder name** (`Fan`, `Lamp`, …) is the stable `packageName` in `MR/objects-layout.yaml`.  
Field `name:` should match the folder; if omitted, the folder name is used.

Pose (position/rotation in the room) is **not** stored here — only in `MR/objects-layout.yaml`.

---

## Minimal valid file

```yaml
version: 1

model:
  file: fan.glb
```

---

## Full reference (v1)

```yaml
version: 1

# --- Identity ---
name: Fan                  # defaults to folder name
displayName: Ventoinha     # CRT menu label
author: you                # optional, display only

# --- Mesh (required) ---
model:
  file: fan.glb            # relative to package folder
  scale: 1.0               # default uniform scale at spawn
  offset: { x: 0, y: 0, z: 0 }       # optional, metres, local
  rotation: { x: 0, y: 0, z: 0 }     # optional, euler degrees, local

# --- Placement ray (MRPlacementProfile) ---
placement:
  surfaceType: 1           # see table below
  facingAxis: 0
  allowStickRotation: true
  stickRotationAxis: 0
  stickRotationSpeed: 90
  providesAnchor: false    # true = runtime MRPlacementAnchor tag
  anchorTarget: Top        # optional GLB child; omit = root

# --- Collision (optional) ---
collision:
  mode: mesh               # mesh | box | none
  convex: true             # mesh mode only

# --- Spatial audio (optional) ---
audio:
  file: fan.wav
  loop: true
  volume: 0.6
  playOnAwake: true
  distance:
    min: 0.5
    max: 4.0

# --- Runtime behaviours (optional) ---
components:
  - rotator
  # - video
  # - grab
  # - light

rotator:
  target: Blades
  axis: y
  speed: 180

# video:
#   file: screen.mp4
#   target: Screen
#   loop: true
#   volume: 0.8

# light:
#   target: Bulb
#   type: point
#   intensity: 2.0
#   range: 4.0
#   color: { r: 1.0, g: 0.75, b: 0.4 }
```

---

## `components`

List of behaviour ids to attach when the object spawns. Each id has a **root-level config block** with the same name.

| Id | Block | Purpose |
|----|-------|---------|
| `rotator` | `rotator:` | Spin a child mesh continuously |
| `grab` | `grab:` | XR grab — one hand or two hands |
| `video` | `video:` | Play a video file on a child screen mesh |
| `animator` | `animator:` | Play an embedded GLB glTF animation clip |
| `light` | `light:` | Attach a Unity point or spot light |

Unknown ids in `components` log a warning.

### `rotator`

Rotates a **child transform** from the GLB hierarchy in **local space**.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `target` | string | — | Child name (e.g. `Blades`). Required. |
| `axis` | string | `y` | Local axis: `x`, `y`, `z` or `-x`, `-y`, `-z` |
| `speed` | float | `180` | Degrees per second |

Example (fan):

```yaml
components:
  - rotator

rotator:
  target: Blades
  axis: y
  speed: 180
```

The GLB must contain a node named `Blades` (case-insensitive match).

### `grab`

Enables VR grab on the object (XRI). When released, can snap back to the MR placement pose.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `twoHands` | bool | `false` | `true` = both hands on left/right handles; `false` = single-hand grab on the object |
| `returnOnRelease` | bool | `true` | Return to placement pose when grab ends |
| `hideHands` | bool | `true` | Hide controller/hand mesh while grabbing |
| `target` | string | — | One-hand: optional GLB child used as **grip pivot** (position + rotation snap to the hand). Omit = package root. Two-hands: grab root for handles. |
| `returnDurationSeconds` | float | `0.15` | Ease-back duration; `0` = instant snap |

**One hand** (e.g. TV, flashlight). While held, the pivot’s pose matches the hand:

```yaml
components:
  - grab

grab:
  twoHands: false
  returnOnRelease: true
  # target: Grip   # optional — GLB empty/node oriented for how it sits in the hand
```

**Two hands** (e.g. handheld console — like PortableGames):

```yaml
components:
  - grab

grab:
  twoHands: true
  returnOnRelease: true
  hideHands: true
```

With `twoHands: true`, invisible `HandleLeft` / `HandleRight` colliders are created on the grab root; **both** must be held. Releasing either hand ends the grab and returns the object if `returnOnRelease` is true.

### `video`

Plays a video file on a **child mesh** from the GLB (e.g. TV screen). Uses Unity `VideoPlayer` + `RenderTexture` on the target `Renderer`.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `file` | string | — | Video in package folder (`.mp4`, `.webm`, `.mov`) |
| `target` | string | — | Child mesh name (e.g. `Screen`). Required. |
| `loop` | bool | `true` | Loop playback |
| `playOnAwake` | bool | `true` | Start when object spawns |
| `volume` | float | `1` | Spatial audio volume; `0` = silent |
| `invertX` | bool | `false` | Flip texture horizontally |
| `invertY` | bool | `false` | Flip texture vertically |

Example (TV):

```yaml
components:
  - video

video:
  file: screen.mp4
  target: Screen
  loop: true
  playOnAwake: true
  volume: 0.8
```

The GLB must contain a node named `Screen` (case-insensitive) with a `Renderer`. Place `screen.mp4` beside `object.yaml` in the package folder.

### `animator`

Plays an **embedded glTF animation** from the GLB (skeletal or keyframe). Uses Unity legacy `Animation` on the target transform.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `clip` | string | — | Animation name in the GLB (glTF `animations[].name`). Required. |
| `target` | string | — | Optional GLB child; omit = auto-detect first `Animation`/`Animator` child |
| `loop` | bool | `true` | Loop playback |
| `playOnAwake` | bool | `true` | Start when object spawns |
| `speed` | float | `1` | Playback speed multiplier |

Example (character with walk cycle):

```yaml
components:
  - animator

animator:
  clip: Walk
  target: Character
  loop: true
  playOnAwake: true
  speed: 1.0
```

The GLB must contain an animation named `Walk` (case-insensitive). This is **not** the same as `rotator` (continuous spin) or `placement.allowStickRotation` (right stick before placement).

### `light`

Adds a runtime Unity `Light` when the object spawns. Independent of CRT **GLOBAL LIGHT** (auto sun). Prefer soft values on Quest (`shadows: false`).

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `target` | string | — | Optional GLB child for the light transform; omit = package root |
| `type` | string | `point` | `point` or `spot` |
| `intensity` | float | `2` | Light intensity |
| `range` | float | `4` | Reach in metres (`point` / `spot`) |
| `color` | RGB | white | `{ r, g, b }` in 0–1 |
| `spotAngle` | float | `60` | Outer cone degrees (`spot` only) |
| `innerSpotAngle` | float | `30` | Inner cone degrees (`spot` only) |
| `shadows` | bool | `false` | Soft shadows when `true` (costly on Quest) |

**Spot direction:** local **+Z** of `target` (empty/node in the GLB). For a handheld flashlight, combine with `grab` and aim `target` along the beam.

Example (ceiling lamp — point):

```yaml
components:
  - light

light:
  target: Bulb
  type: point
  intensity: 1.5
  range: 3.5
  color: { r: 1.0, g: 0.85, b: 0.6 }
  shadows: false
```

Example (flashlight — spot + grab):

```yaml
components:
  - grab
  - light

grab:
  twoHands: false
  returnOnRelease: true

light:
  target: BeamOrigin
  type: spot
  intensity: 3.0
  range: 8.0
  spotAngle: 35
  innerSpotAngle: 20
  color: { r: 1.0, g: 0.95, b: 0.85 }
  shadows: false
```

#### Recommended video format (Quest + Windows Editor)

Use this profile for custom object TVs — same stack as Unity `VideoPlayer` (Media Foundation on Windows, MediaPlayer on Quest):

| Setting | Recommended |
|---------|-------------|
| Container | **`.mp4`** only |
| Video codec | **H.264 / AVC** (`libx264`) |
| Profile | **Baseline** or **Main** |
| Pixel format | **`yuv420p`** (required) |
| Resolution | **1280×720** or **1920×1080** (avoid 4K on Quest) |
| Frame rate | **30 fps** constant |
| Audio | **AAC-LC**, 48 kHz, stereo — or **no audio** (safest for TV props) |
| Mux | **`faststart`** (`moov` atom at start of file) |

**Avoid:** H.265/HEVC, VP9, AV1, `.webm`, `.mkv`, `.mov` from random exporters, variable frame rate, downloads with odd audio mux (often causes Windows error **`0xc00d36e6`** / *Getting duration*).

**TV props (recommended):** export **video only** and set `volume: 0` — simplest and most reliable.

**FFmpeg — full re-encode (video + AAC audio):**

```bash
ffmpeg -i input.mp4 -c:v libx264 -profile:v baseline -level 3.1 -pix_fmt yuv420p -r 30 -vf "scale=1280:720" -c:a aac -b:a 128k -ar 48000 -ac 2 -movflags +faststart screen.mp4
```

**FFmpeg — video only (best for TV, no audio issues):**

```bash
ffmpeg -i input.mp4 -c:v libx264 -profile:v baseline -pix_fmt yuv420p -r 30 -vf "scale=1280:720" -an -movflags +faststart screen.mp4
```

```yaml
video:
  file: screen.mp4
  target: Screen
  volume: 0
```

**Windows note:** Films & TV / Media Player may play an MP4 fine while Unity `VideoPlayer` logs `0xc00d36e6` (*Getting duration*) — different APIs. The MR player uses **APIOnly** mode (same idea as cabinet `GameVideoPlayer`) and treats that duration warning as non-fatal if frames arrive. Re-encode only if playback still fails.

---

## `placement.surfaceType`

| Value | Surface   | Typical use        |
|------:|-----------|--------------------|
| 0     | Floor     | furniture, TV stand |
| 1     | Wall      | fan, poster, shelf  |
| 2     | Ceiling   | lamp, hanger        |
| 3     | Free3D    | floating prop       |
| 4     | Table     | small object on desk (MRUK scan) |
| 5     | Object    | on another placed prop (`providesAnchor` parent) |

### `placement.providesAnchor` / `placement.anchorTarget`

When `providesAnchor: true`, the loader tags a collider at spawn with **`MRPlacementAnchor`** (runtime — do not set the tag in the GLB). Other props with `surfaceType: 5` (Object) snap to that surface via the placement ray.

| Field | Meaning |
|-------|---------|
| `providesAnchor` | This prop is a placement surface for other objects |
| `anchorTarget` | GLB child name for the anchor collider; omit for package root |

Pose of the child is stored in `objects-layout.yaml` as local coordinates relative to the parent (`anchorPlacementId` + `anchorPoint`), not in `object.yaml`.

## `placement.facingAxis`

| Value | Axis    |
|------:|---------|
| 0     | +Z (default mesh forward) |
| 1     | −Z      |
| 2     | +X      |
| 3     | −X      |

## `placement.stickRotationAxis`

| Value | Axis        | Effect on Quest        |
|------:|-------------|------------------------|
| 0     | WorldYaw    | Spin on floor (turntable) |
| 1     | WorldPitch  | Tilt forward/back      |
| 2     | WorldRoll   | Roll around forward Z  |

## `collision.mode`

| Mode   | Behaviour |
|--------|-----------|
| `mesh` | `MeshCollider` (convex if `convex: true`) on first mesh |
| `box`  | `BoxCollider` from renderer bounds |
| `none` | No collider generated |

If `collision` is omitted, defaults to `mesh` with `convex: true` when no collider exists on the GLB.

## `audio`

Supported extensions: `.wav`, `.mp3`, `.ogg` (same as cabinet parts).  
Uses spatial audio (`spatialBlend = 1`) and `SpatializerMixer` when available.

If `audio` is omitted, the object is silent.

---

## Link to scene layout

When the user places the object in MR, an entry is written to `MR/objects-layout.yaml`:

```yaml
version: 2
props:
  - id: fan-wall-001
    source: custom
    packageName: Fan
    scale: 1
    surfaceType: 1
    position: { x: 0, y: 1.2, z: 0 }
    rotation: { x: 0, y: 0, z: 0, w: 1 }
    anchorUuid: "..."
```

`packageName` must match the folder under `Custom Objects/`.

---

## Package examples in repo

| Folder | Template |
|--------|----------|
| `Assets/ramiro/CustomObjectTemplate/Fan/` | Wall + loop audio |
| `Assets/ramiro/CustomObjectTemplate/Lamp/` | Ceiling, no audio |
| `Assets/ramiro/CustomObjectTemplate/TV/` | Floor, video on `Screen`, one-hand grab |

Copy a template folder into `MR/Custom Objects/` and add the `.glb` (and `.wav` if used).

On first run (when `Custom Objects/` has no packages), the app creates:

```
MR/Custom Objects/Example/object.yaml
```

Add `example.glb` beside that file to spawn it from the ENVIRONMENT menu.
