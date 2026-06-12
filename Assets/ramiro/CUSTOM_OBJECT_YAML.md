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

rotator:
  target: Blades
  axis: y
  speed: 180
```

---

## `components`

List of behaviour ids to attach when the object spawns. Each id has a **root-level config block** with the same name.

| Id | Block | Purpose |
|----|-------|---------|
| `rotator` | `rotator:` | Spin a child mesh continuously |
| `grab` | `grab:` | XR grab — one hand or two hands |

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
| `target` | string | — | Optional child name for grab root; omit = package root |
| `returnDurationSeconds` | float | `0.15` | Ease-back duration; `0` = instant snap |

**One hand** (e.g. TV, phone):

```yaml
components:
  - grab

grab:
  twoHands: false
  returnOnRelease: true
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

---

## `placement.surfaceType`

| Value | Surface   | Typical use        |
|------:|-----------|--------------------|
| 0     | Floor     | furniture, TV stand |
| 1     | Wall      | fan, poster, shelf  |
| 2     | Ceiling   | lamp, hanger        |
| 3     | Free3D    | floating prop       |
| 4     | Table     | small object on desk |

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
| `Assets/ramiro/CustomObjectTemplate/TV/` | Floor, one-hand grab |

Copy a template folder into `MR/Custom Objects/` and add the `.glb` (and `.wav` if used).

On first run (when `Custom Objects/` has no packages), the app creates:

```
MR/Custom Objects/Example/object.yaml
```

Add `example.glb` beside that file to spawn it from the ENVIRONMENT menu.
