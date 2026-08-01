# Lighting a Room

How to light an Age of Joy room so it looks good **and** so the arcade cabinets — which are
instantiated at runtime, after the scene is already baked — are lit correctly.

This document covers scene/bake-time lighting. For lights that are *animated or addressed from
AGEBasic at runtime*, see [roomlight_system.md](./roomlight_system.md).

---

## 1. The hard constraint: one per-pixel light

Age of Joy ships for Quest. `ProjectSettings/QualitySettings.asset` sets **Android → quality level 3
("High")**:

| Setting | Value |
|---|---|
| `pixelLightCount` | **1** |
| `shadows` | Hard shadows only |
| `shadowDistance` | 40 |
| `shadowCascades` | 2 |
| `shadowmaskMode` | **Shadowmask** (not Distance Shadowmask) |

**One** real-time per-pixel light. That single budget is the root of almost every lighting problem in
this project: put two Mixed or Realtime lights in a room and only one of them gets the per-pixel
slot — the other silently degrades to per-vertex/SH and effectively disappears. Raising Pixel Light
Count is not an option on Quest.

The editor uses the **Standalone** default quality level (**5 = "Ultra"**), which has
`shadowmaskMode: DistanceShadowmask`. That makes the Scene view show *real-time* mixed lighting on
static geometry — lighting the build will never show. **Set the editor Quality level to High before
judging a room's lighting.**

---

## 2. The rule: room lights are Baked, dynamic objects use Light Probes

> **Every fixture in a room is `Light Mode: Baked`. Every room has a Light Probe Group.**

| | Static geometry (walls, floor, props) | Cabinets & other runtime objects |
|---|---|---|
| Lit by | The lightmap | Light Probes (SH) |
| Per-pixel cost | **zero** | **zero** |
| Number of lights | unlimited | unlimited |
| Shadows | baked into the lightmap | none (use a blob/AO decal if needed) |

Because Baked lights are never rendered in real time, `pixelLightCount: 1` cannot drop one of them.
A room can have six baked lamps and all six show up. This is what makes the arcade rooms possible.

**Why not Mixed?** In Shadowmask mode a Mixed light bakes into the lightmap for static geometry
*but still renders in real time onto dynamic objects* — so each Mixed light competes for the single
per-pixel slot. With two Mixed lamps in a room, one of them lights nothing. Light Probes are also
weaker with Mixed lights: probes only store the **indirect** contribution of a Mixed light, whereas
they store the **full** contribution of a Baked light.

**When Mixed is acceptable:** at most **one** light per room, when you specifically want a moving
falloff/specular response on a cabinet as the player approaches. Set that light's
`Render Mode: Important` so it deterministically claims the per-pixel slot, and keep every other
light Baked.

**When Realtime is required:** a light whose intensity or color is changed at runtime (AGEBasic
neon flicker, `LightManagerController`). **Changing the intensity of a Baked light at runtime does
nothing** — its contribution lives in the lightmap. If a fixture must be script-driven, either make
that light Realtime with `Render Mode: Not Important` (per-vertex, cheap, no per-pixel slot), or
pair a baked lamp with a small separate realtime light for the animated part.

---

## 3. Setting up the lights in a room

1. **Place the fixtures.** For the Backrooms rooms, use
   `Assets/Resources/Architecture/Backrooms/light.prefab` (a Point Light + fixture mesh +
   `BackgroundNoise` AudioSource). Typical values: Intensity 1, Range 4.5–5.5, Hard shadows,
   Bounce Intensity 1.
2. **Set Light Mode to Baked in the prefab**, not on the instances. Light Mode is serialized as
   `m_Lightmapping` and per-instance overrides survive prefab edits — an instance that overrides it
   to Mixed will ignore the prefab forever. Right-click any existing `m_Lightmapping` override on an
   instance and **Revert** it.
3. **Verify the bulb's world position.** If the Light is a child with a local offset, the fixture's
   *rotation* moves the bulb. A bulb that ends up at or above the ceiling is occluded by the ceiling
   mesh and contributes almost nothing to the bake — the classic "this lamp doesn't light the floor"
   bug. Select the Light child and check its world Y against the ceiling before baking.
4. **Mark the room static.** All architecture needs **Contribute GI** on. Watch out for prefab roots
   that ship non-static: e.g. `Backrooms/Wall.prefab`'s root has
   `m_StaticEditorFlags: 0` while its `plinth` child is fully static, so wall instances need the flag
   set in the scene.
5. **Add the Light Probe Group** (next section).
6. **Bake:** save the scene, then `Window → Rendering → Lighting → Generate Lighting`, then save
   again. The bake output goes to `Assets/Scenes/<SceneName>/` (`LightingData.asset`,
   `Lightmap-N_comp_light.exr`, `_comp_dir.png`, `_comp_shadowmask.png`).

> Baking without saving the scene first produces a lightmap that does not match the saved scene —
> the light modes and static flags in the file will disagree with what was baked, and the room will
> look wrong on next load. Save, bake, save.

---

## 4. Light Probes — lighting the pop-up cabinets

Cabinets are instantiated by `CabinetFactory` **after** the bake, so they can never be in the
lightmap. Their only source of room light is the Light Probe Group. **A room without a probe group
has unlit cabinets, no matter how the lights are configured.**

### How sampling works

Probes are tetrahedralized into a volume. A dynamic renderer samples **one point** — the center of
its bounds (about mid-height of a cabinet), or the transform assigned to its `Probe Anchor`. That
point must be **inside the probe hull**; outside the hull the object falls back to the nearest probe
or flat ambient, which is the usual reason "I added probes and nothing changed".

### Placement rules

- **Bracket, don't mark.** Do not place a probe *at* the cabinet. Place 4 probes in a rough square
  0.5–1 m outside its footprint, at **two heights**: ~0.3 m and ~2.0–2.2 m. The sample point at
  ~1 m is then enclosed and gets a real vertical gradient.
- **Keep 20–30 cm clear of geometry.** A probe inside a wall, floor or ceiling bakes nearly black
  and drags everything near it dark.
- **Density follows change, not area.** Cluster probes under each lamp and at the edges of each
  light pool; uniform mid-room areas need very few.
- **Cover the whole walkable area**, not just the cabinet slots — held/carried props and any other
  dynamic object read the same probes, and holes in the hull cause visible brightness pops as
  objects move.
- Set each cabinet renderer to `Light Probes: Blend Probes`, **Contribute GI off**. If a tall
  cabinet's bounds center sits at an unhelpful height, add a child empty and assign it as
  `Probe Anchor`.

### Building the grid quickly

The probe editor places every new probe at the group's **local origin**, stacked on top of the
previous one — pressing *Add Probe* repeatedly looks like it does nothing. Instead:

1. Create one Light Probe Group and keep the default 8 probes (a cube).
2. Drag those 8 into one cell: ~2 × 2 m, floor-to-ceiling.
3. `Ctrl+D` the **GameObject** and move the copy to tile the room. All Light Probe Groups in a scene
   are merged at bake time, so a dozen duplicated cells is a valid dense grid.
4. Add one extra cell centered under each lamp and at each cabinet slot.

Requirements for the group itself: **scale (1,1,1)**, no scaled parent (a zero on any axis collapses
all probes into one point; a squashed parent like a scaled `Floor` object distorts the grid), Scene
view **Gizmos** enabled with *Light Probe Group* checked, and **Edit Light Probes** toggled on in the
inspector to select or see them.

### Reflection probes

Rooms should also carry at least one baked **Reflection Probe**, otherwise Environment Reflections
fall back to the (unset) custom cubemap and metallic/smooth cabinet parts reflect black. One
box-shaped probe covering the room at 128 resolution is enough.

---

## 5. "Age of Joy Lighting Settings"

`Assets/Scenes/Age of Joy Lighting Settings.lighting` is the shared Lighting Settings asset assigned
in the Lighting window (`m_LightingSettings` in the scene). Assign it to every room so all scenes
bake consistently. Variants exist (`… Production`, `… v2`, `…_2048px`) — the plain
**Age of Joy Lighting Settings** is the one the Backrooms scenes use.

| Setting | Value | Why |
|---|---|---|
| Baked Global Illumination | **On** | The whole strategy depends on the lightmap. |
| Realtime Global Illumination | Off | Not supported on Quest. |
| Lightmapper | **Progressive GPU** | Bake speed only; does not affect the result. |
| Lightmap Resolution | **40** texels/unit | Room-scale sweet spot. Raise locally with a renderer's *Scale In Lightmap* instead of raising this globally. |
| Lightmap Padding | 2 | |
| Max Lightmap Size | **1024** | One atlas per small room; keeps texture memory sane on Quest. |
| Lightmap Compression | High Quality | |
| Ambient Occlusion | **On**, Max Distance 1, Indirect 1, Direct 0 | Contact darkening in corners — cheap depth for the flat Backrooms geometry. |
| Directional Mode | **Combined Directional** | Produces the `_comp_dir` map; needed for normal-mapped surfaces. |
| **Mixed Lighting → Lighting Mode** | **Shadowmask** | Matches the Android quality level. Must not be Subtractive or Baked Indirect. |
| Direct/Indirect Samples | 32 / 512 | |
| Environment Samples | 256 | |
| Bounces | 2 | |
| Filtering | Auto (Gaussian) | |
| Light Probe Sample Multiplier | 4 | Better probe quality — matters here, since probes light every cabinet. |

Notes:

- The scene file also contains a legacy inline copy of these values in its `LightmapSettings` block.
  The referenced `.lighting` asset is what actually applies; ignore drift in the inline copy.
- Lighting Mode is only meaningful for Mixed lights. With the all-Baked setup it is inert, but keep
  it on **Shadowmask** so that a single Mixed light added later behaves the same as in the build.
- Bake resolution × room size drives atlas count. If a room suddenly needs several 1024 atlases,
  lower *Scale In Lightmap* on large low-detail surfaces (floor, ceiling) rather than raising Max
  Lightmap Size.

---

## 6. The Environment section

Per-scene, in `Window → Rendering → Lighting → Environment` (serialized as `RenderSettings` at the
top of the `.unity` file). Values below are the Backrooms rooms' configuration.

| Field | Value | Notes |
|---|---|---|
| Skybox Material | **None** | Interior rooms; no sky is ever visible. |
| Sun Source | **None** | No directional light in the arcade interiors. |
| Environment Lighting → Source | **Color** (`m_AmbientMode: 3`) | Flat ambient. A skybox source would add cost and leak daylight into a windowless room. |
| Ambient Color | ~`RGB(0.212, 0.227, 0.259)` | A dim cold grey. This is the **floor brightness** of the room: nothing is ever darker than this, and it's what an object outside the probe hull falls back to. Keep it low but non-zero so cabinets never go pure black. |
| Ambient Mode | Baked | |
| Environment Reflections → Source | Custom | With no cubemap assigned, reflections come from the scene's Reflection Probes only — so add one per room. |
| Resolution | 128 | |
| Compression / Intensity / Bounces | Auto / 1 / 1 | |
| Fog | **Off** | Per-pixel fog is not worth it on Quest; use baked darkness and light falloff instead. |

Tuning guidance:

- Ambient Color is the strongest single knob for a room's mood, and it is **free**. Raise it slightly
  if cabinets read as too dark; the lightmap keeps the lit/unlit contrast.
- `Subtractive Shadow Color` appears in the file but is unused in Shadowmask mode.
- Don't rely on ambient to fix an under-lit cabinet — that's a probe coverage problem. Fix the hull
  first.

---

## 7. Bake checklist

1. Editor Quality level = **High**.
2. All room fixtures = **Baked** (verify no per-instance `m_Lightmapping` overrides remain).
3. Every bulb's world position is *below* the ceiling and inside the room volume.
4. All architecture has **Contribute GI**; all runtime objects do **not**.
5. Light Probe Group present, identity scale, hull covers the walkable area and all cabinet slots.
6. At least one baked Reflection Probe.
7. Lighting Settings asset = **Age of Joy Lighting Settings**; Environment configured as above.
8. **Save scene → Generate Lighting → save scene.**
9. Check on device with Pixel Light Count at 1.

---

## 8. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Only one of two lamps lights the room; raising Pixel Light Count to 2 fixes it | Lamps are Mixed/Realtime and competing for the single per-pixel slot | Set both to Baked and rebake |
| Room looks right in the Scene view, wrong on device | Editor is on Ultra (Distance Shadowmask) | Switch the editor to quality level High |
| One lamp contributes nothing to the bake | Bulb is inside or above the ceiling mesh, or Range too small | Move the Light child; check the fixture's rotation isn't displacing it |
| Cabinets appear unlit / flat grey | No Light Probe Group, or the cabinet sits outside the probe hull | Add/extend the probe grid around the cabinet slots |
| Cabinets pop in brightness while moving | Gaps in the probe hull | Fill in probes; keep spacing even |
| Cabinet reflections are black | No Reflection Probe and Environment Reflections set to Custom with no cubemap | Bake a Reflection Probe in the room |
| Script changes a light's intensity and nothing happens | The light is Baked | Make it Realtime `Not Important`, or add a small realtime light alongside |
| Lightmap doesn't match the scene after a bake | Baked before saving | Save, rebake, save |
