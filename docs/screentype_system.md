# Screen Type System (CRT Prefabs)

Every cabinet that has a display picks a **screen type** in its `description.yaml`:

```yaml
crt:
  type: 19i           # ← this string
  orientation: vertical
```

The string is looked up in a static prefab registry, and the matching prefab is instantiated into the cabinet at runtime. That prefab carries the screen mesh, its renderer, an `AudioSource`, an `AGEBasic` engine, a `GameVideoPlayer`, and one of three runtime controllers. The choice of `crt.type` therefore determines both the **shape** of the screen and which **controller** runs the cabinet.

> This document covers only the *geometry* axis (the prefab shape and which controller is attached). The independent *shader* axis (`crt.screen.shader` — `crt`, `crtlod`, `projector`, etc.) is documented separately; see [`ShaderScreen.cs`](../Assets/curif/LibRetroWrapper/ShaderScreen.cs).

---

## 1. The factory

[`CRTsFactory.cs`](../Assets/curif/LibRetroWrapper/CRTsFactory.cs) is a static class with a single `Dictionary<string, GameObject>` populated in a static constructor. Each entry is a `Resources.Load<GameObject>` of a prefab in [`Assets/Resources/Cabinets/PreFab/CRTs/`](../Assets/Resources/Cabinets/PreFab/CRTs/).

```csharp
objects.Add("19i", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen19i"));
// ...
public static GameObject Instantiate(string type, Vector3 position,
                                     Quaternion rotation, Transform parent)
{
    if (!objects.ContainsKey(type))
        throw new Exception($"[CRTFactory] screen type {type} doesn't exists");
    return GameObject.Instantiate(objects[type], position, rotation, parent);
}
```

Implications worth keeping in mind:

- **All screen prefabs are loaded into memory at first access** — the static constructor runs the moment `CRTsFactory` is touched. They live for the process lifetime.
- **The key is case-sensitive in the dictionary**, but `Cabinet.addCRT` lowercases the YAML value before lookup. Author-side strings in YAML should be lowercase to be safe.
- **The catalog is closed at compile time.** A new screen type means: drop a prefab into `Resources/Cabinets/PreFab/CRTs/`, add a line to the static constructor, rebuild.

---

## 2. The catalog

| `crt.type` | Prefab | Description |
|---|---|---|
| `19i` | `screen19i.prefab` | Standard 19-inch arcade CRT — the default and the most common case. |
| `19i-fresnel` | `screen19i_fresnel.prefab` | Starblade-style CRT with a Fresnel-bulged front. Used for cabinets whose real-world screen had a deliberate optical curvature on the glass. |
| `32i` | `screen32i.prefab` | 32-inch CRT for larger cabinets. |
| `50i` | `screen50i.prefab` | 50-inch big-screen for projection-style cabinets. |
| `circle` | `screen_circle.prefab` | Round screen for vector / oscilloscope-style games. |
| `square` | `screen_square.prefab` | 1:1 aspect square screen. |
| `19i-1x2` | `screen19i_1x2.prefab` | One column × two rows — vertically stacked monitors. **Punch-Out!!** layout. |
| `19i-2x1` | `screen19i_2x1.prefab` | Two columns × one row — horizontally stacked monitors. Not Punch-Out. |
| `19i-3x1` | `screen19i_3x1.prefab` | Three monitors wide × one tall. Used for **Buggy Boy**. |
| `19i-3x1-18deg` | `screen19i_3x1_18deg.prefab` | Same as `19i-3x1` but the outer two panels are toed in 18° to bend the image around the player. |
| `dome-concave` | `screen_dome_concave.prefab` | Inward-curving dome — player sits inside the curve. Simulator / cockpit cabinets. |
| `dome-convex` | `screen_dome_convex.prefab` | Outward-bulging dome — rarer, for cabinets whose real screen bulged outward toward the player. |
| `19i-agebasic` | `screen19iAGEBasic.prefab` | Same 19-inch shape as `19i`, but the prefab carries `AGEBasicScreenController` instead of `LibretroScreenController`. Use this for cabinets that run AGEBasic scripts on a screen without a libretro emulator. |
| `no-crt` | `noScreen.prefab` | No screen at all. Used for pinball / mechanical / sound-only cabinets that need coin handling and AGEBasic but have no display. Carries `AGEBasicCabinetController`. |
| `custom` | *(reuses `screen19i.prefab`)* | **Author-supplied screen mesh.** No dedicated prefab — the screen shape is a named part in the cabinet `.glb` (`crt.mesh: <part-name>`). See §9. |

The full list of valid keys is exposed at runtime as `CRTsFactory.objects.Keys` and is fed into `CabinetInformation.validate()` so a cabinet with an unknown `crt.type` is rejected with a clear error.

---

## 3. How a screen type becomes a screen in a cabinet

```
description.yaml          Cabinet 3D model              Runtime
─────────────────         ────────────────              ───────
crt:                      screen-mock-vertical    ──┐
  type: 19i               OR                        │
  orientation: vertical   screen-mock-horizontal  ──┤
  ...                                               │
                                                    ▼
                          Cabinet.addCRT()
                            ├─ find the screen-mock-{orientation} child
                            ├─ CRTsFactory.Instantiate(type, mock.position, …)
                            ├─ deactivate both screen-mocks
                            └─ wire the controller based on type
                                  ├─ "19i-agebasic"  → AGEBasicScreenController
                                  ├─ "no-crt"        → AGEBasicCabinetController
                                  └─ everything else → LibretroScreenController
```

### Step 1 — placeholder parts in the cabinet model

A cabinet `.glb` is expected to contain one or both of:

- `screen-mock-vertical` — a flat quad positioned where a vertically-oriented screen should sit
- `screen-mock-horizontal` — same, for horizontal orientation

These are non-rendered placeholders. `CabinetFactory.fromInformation` ([CabinetFactory.cs:551-580](../Assets/curif/LibRetroWrapper/CabinetFactory.cs#L551)) checks for their presence to decide whether the cabinet has a display at all.

### Step 2 — factory choice

`CabinetFactory.fromInformation` picks one of two paths:

```csharp
if (cbinfo.crt != null && (PartsExist("screen-mock-vertical") || PartsExist("screen-mock-horizontal")))
{
    cabinet.addCRT(cbinfo, ...);                  // standard display cabinet
}
else if (cbinfo.agebasic != null)
{
    cabinet.addController(cbinfo.pathBase, ...);   // no-display cabinet (uses no-crt)
}
```

A cabinet with no `crt:` block and no `agebasic:` block has no screen and no controller wired — it's just a decorative model.

### Step 3a — `Cabinet.addCRT` (standard path)

[Cabinet.cs:663](../Assets/curif/LibRetroWrapper/Cabinet.cs#L663). The key sequence:

```csharp
string CRTType = $"screen-mock-{orientation}";       // "screen-mock-vertical" or "...-horizontal"
GameObject CRT = GetPartController(CRTType).GameObject;

GameObject newCRT = CRTsFactory.Instantiate(
    type.ToLower(),                                  // <— the YAML crt.type, lowercased
    CRT.transform.position,
    CRT.transform.rotation,
    CRT.transform.parent);

newCRT.name = CRTName(Name, gameFile);               // "screen-<cabName>-<romFile>"
GetPartControllerOrNull("screen-mock-vertical")?.GameObject.SetActive(false);
GetPartControllerOrNull("screen-mock-horizontal")?.GameObject.SetActive(false);
```

The instantiated screen takes the placeholder's transform exactly, then both placeholders are deactivated. The new GameObject is renamed to a stable `screen-<cabinetName>-<gameFile>` form so `LibretroScreenController` and friends can find it across scene reloads.

Then, depending on the type:

```csharp
if (type.ToLower() == "19i-agebasic")
    /* configure AGEBasicScreenController fields */
else
    /* configure LibretroScreenController fields */
```

The 19i-agebasic prefab is the same screen mesh as `19i`, but its prefab has a different `MonoBehaviour` attached — that's the entire difference.

### Step 3b — `Cabinet.addController` (no-display path)

[Cabinet.cs:634](../Assets/curif/LibRetroWrapper/Cabinet.cs#L634). Triggered when there's no `crt:` block but `agebasic:` is set:

```csharp
GameObject newCRT = CRTsFactory.Instantiate(
    "no-crt", Vector3.zero, Quaternion.identity, gameObject.transform);
newCRT.name = CRTName(Name, "no-crt");
AGEBasicCabinetController agec = newCRT.GetComponent<AGEBasicCabinetController>();
/* configure agec fields */
```

`noScreen.prefab` has no `Renderer`, no `GameVideoPlayer` requirement, and no display surface. It's a logic-only host for the AGEBasic engine, coin slot, and lightgun targeting. Used for pinball-style and electromechanical cabinets.

---

## 4. The three controllers, one screen-type system

The screen type is what *selects* which controller runs a cabinet at runtime:

| Controller | Prefab(s) | When it runs |
|---|---|---|
| `LibretroScreenController` | Every prefab **except** `screen19iAGEBasic` and `noScreen` | The default — connects the screen to a libretro core, runs the emulator, manages attract-mode video, handles coin/light-gun input. |
| `AGEBasicScreenController` | `screen19iAGEBasic.prefab` | Display-bearing cabinets driven only by AGEBasic scripts (no emulation). Plays an attract video when idle, hands off to AGEBasic on coin insert. |
| `AGEBasicCabinetController` | `noScreen.prefab` | Display-less cabinets driven only by AGEBasic. Pinball, mechanical games, electromechanical curios. |

See [docs/agebasic_development_docs.md](agebasic_development_docs.md) §`AGEBasicScreenController.cs` and `AGEBasicCabinetController.cs` for the controller-side details.

---

## 5. Defaults and validation

[`CabinetInformation.CRT`](../Assets/curif/LibRetroWrapper/CabinetInformation.cs#L544):

```csharp
public class CRT
{
    public string type = "19i";
    public string orientation = "vertical";
    public string mesh;  // for type: custom — the screen part name inside the .glb (§9)
    public Screen screen = new Screen();
    public Geometry geometry = new Geometry();
    public string name;  // optional GameObject name override
    ...
}
```

- **Default `type`** is `19i`. A cabinet that declares an empty `crt:` block gets a vertical 19-inch screen.
- **Default `orientation`** is `vertical`. Anything other than `vertical` or `horizontal` fails validation.
- **`name`** lets a cabinet override the auto-generated `screen-<cabName>-<rom>` GameObject name. Useful when AGEBasic scripts want to address the screen by a stable custom name.

`validate()` rejects unknown `crt.type` values by checking the YAML string against `CRTsFactory.objects.Keys`. The error fires during cabinet load and surfaces in the cabinet's debug console.

---

## 6. Extending the catalog

To add a new screen type:

1. **Model the screen mesh** as a Unity prefab under `Assets/Resources/Cabinets/PreFab/CRTs/`. The prefab must include:
   - A `MeshRenderer` on the screen surface with **at least two material slots**:
     - **slot 0** — bezel / frame / glass-housing material (decorative; never touched at runtime).
     - **slot 1** — the screen surface material. `ShaderScreen.Factory` is called with the screen slot from every call site ([`LibretroScreenController`](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs#L220), [`AGEBasicScreenController`](../Assets/curif/LibRetroWrapper/AGEBasicScreenController.cs#L172), [`ConfigurationController`](../Assets/curif/UI/ConfigurationController.cs#L1533), [`CabinetDebugConsole`](../Assets/curif/LibRetroWrapper/CabinetDebugConsole.cs#L105)) and overwrites this slot with the active shader (CRT, projector, clean, etc.). The slot defaults to `1`; a prefab (or a runtime-built screen) can override it by carrying a `ScreenSurfaceInfo { materialSlot }` component — this is how `type: custom` collapses to a single-slot mesh (§9). `LibretroScreenController` reads it; `AGEBasicScreenController`/`ConfigurationController`/`CabinetDebugConsole` still use the fixed slot 1.
   - For lightgun-capable screens, the screen mesh must also have **submesh 1** as the screen surface. [`LightGunTarget.cs`](../Assets/curif/LibRetroWrapper/LightGunTarget.cs) reads `CRTRenderer.materials[slot].mainTexture` and `mesh.GetTriangles(submesh)` to do hit-to-UV mapping, where `slot`/`submesh` come from `ScreenSurfaceInfo` (default 1).
   - Whichever controller it should drive: `LibretroScreenController` (with its required components — `AudioSource`, `GameVideoPlayer`, `LibretroControlMap`, `basicAGE`, `CabinetAGEBasic`, `LightGunTarget`), `AGEBasicScreenController`, or `AGEBasicCabinetController`.
2. **Register the prefab** by adding a single line to the static constructor in [`CRTsFactory.cs`](../Assets/curif/LibRetroWrapper/CRTsFactory.cs):
   ```csharp
   objects.Add("my-new-shape", Resources.Load<GameObject>("Cabinets/PreFab/CRTs/screen_my_new_shape"));
   ```
3. **Cabinet authors** can now reference it: `crt: { type: my-new-shape, orientation: vertical }`. Validation picks it up automatically because `validate()` reads `CRTsFactory.objects.Keys`.

> Adding `19i-agebasic`-style controller variants for new shapes works the same way — duplicate the prefab, swap the controller component, register under a new key. The `Cabinet.addCRT` branch on `type == "19i-agebasic"` is the one place the controller choice is hard-coded; a third controller-variant key would require extending that conditional.

---

## 7. Behavior notes

- **Lowercase your YAML.** `Cabinet.addCRT` calls `type.ToLower()` before the dictionary lookup. The dictionary keys are already lowercase. If you accidentally type `19I` in YAML, it'll still work — but the safe convention is lowercase.
- **Both screen mocks get deactivated even if only one exists.** The null-conditional `GetPartControllerOrNull("...")?.GameObject.SetActive(false)` is intentional — a cabinet model only needs one of the two mocks, but having both is fine.
- **The prefab transform is overwritten on instantiate.** Position, rotation, and parent are all taken from the screen-mock placeholder. The prefab's own transform values don't matter at runtime.
- **`screen-mock-vertical` vs `-horizontal` is the cabinet author's call.** The same `crt.type` (e.g., `19i`) can be vertical or horizontal — the mock placement in the model defines which is which, and `orientation` in YAML just picks which mock to use.
- **`19i-agebasic` is the only special-cased type in `Cabinet.addCRT`.** All other types go through the `LibretroScreenController` path. `no-crt` is handled in a different method entirely (`addController`).
- **The factory has zero lifecycle management.** It just loads prefabs once and hands out instantiations. Cleanup of cabinet GameObjects (and their screens) is the caller's responsibility — see [docs/glb_model_cache.md](glb_model_cache.md) for the GLB cache that sits above this.
- **The screen surface lives at material slot 1 by default, not slot 0.** Shader call sites pass slot `1` to `ShaderScreen.Factory` (except `LibretroScreenController`, which reads `ScreenSurfaceInfo.materialSlot` and defaults to 1), and [`ShaderScreen.Activate`](../Assets/curif/LibRetroWrapper/ShaderScreen.cs#L52) does `display.materials[slot] = material`. A new screen prefab whose `MeshRenderer` has fewer than two material slots — or one whose screen surface is on slot 0 without a matching `ScreenSurfaceInfo` — will either throw `IndexOutOfRangeException` at activation or silently render the shader on the bezel. See section 6 for the full requirements. `type: custom` (§9) is the one built-in case that deliberately uses slot 0, and it attaches `ScreenSurfaceInfo(0,0)` to say so.

---

## 8. File map

| Concern | File |
|---|---|
| Screen-type registry | [Assets/curif/LibRetroWrapper/CRTsFactory.cs](../Assets/curif/LibRetroWrapper/CRTsFactory.cs) |
| All screen prefabs + materials | [Assets/Resources/Cabinets/PreFab/CRTs/](../Assets/Resources/Cabinets/PreFab/CRTs/) |
| YAML data model (`CRT`, `Screen`) | [Assets/curif/LibRetroWrapper/CabinetInformation.cs:544](../Assets/curif/LibRetroWrapper/CabinetInformation.cs#L544) |
| Standard wiring (instantiate + controller setup) | [Assets/curif/LibRetroWrapper/Cabinet.cs:663 `addCRT`](../Assets/curif/LibRetroWrapper/Cabinet.cs#L663) |
| No-display wiring | [Assets/curif/LibRetroWrapper/Cabinet.cs:634 `addController`](../Assets/curif/LibRetroWrapper/Cabinet.cs#L634) |
| Factory branch (with-display vs without) | [Assets/curif/LibRetroWrapper/CabinetFactory.cs:551](../Assets/curif/LibRetroWrapper/CabinetFactory.cs#L551) |
| Display-cabinet controller (libretro) | [Assets/curif/LibRetroWrapper/LibretroScreenController.cs](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs) |
| Display-cabinet controller (AGEBasic) | [Assets/curif/LibRetroWrapper/AGEBasicScreenController.cs](../Assets/curif/LibRetroWrapper/AGEBasicScreenController.cs) |
| No-display controller (AGEBasic) | [Assets/curif/LibRetroWrapper/AGEBasicCabinetController.cs](../Assets/curif/LibRetroWrapper/AGEBasicCabinetController.cs) |
| Shader system (separate axis) | [Assets/curif/LibRetroWrapper/ShaderScreen.cs](../Assets/curif/LibRetroWrapper/ShaderScreen.cs) |
| Screen-slot/submesh descriptor | [Assets/curif/LibRetroWrapper/ScreenSurfaceInfo.cs](../Assets/curif/LibRetroWrapper/ScreenSurfaceInfo.cs) |
| Custom-mesh build (mesh swap) | [Assets/curif/LibRetroWrapper/Cabinet.cs `buildCustomCRT`](../Assets/curif/LibRetroWrapper/Cabinet.cs) |

---

## 9. Custom screen meshes (`crt.type: custom`)

Instead of picking a built-in shape, a cabinet author can supply their **own screen mesh** — a wrap-around screen, an odd aspect ratio, a curved or angled panel — modeled directly in the cabinet `.glb`. It renders the emulator, plays attract video/audio, takes coins, runs AGEBasic, **and works with light guns**, exactly like a built-in screen.

```yaml
crt:
  type: custom
  mesh: my-screen        # a top-level part in the cabinet .glb; THIS mesh becomes the screen
  screen:
    shader: crt          # any screen shader: crt (default), crtlod, clean, projector, …
    invertx: false
    inverty: false
```

### How to use it (author steps)

1. **Model the screen** in Blender as a single object with **one material** (one submesh) and **UVs mapped 0–1** across the surface the emulator frame should fill — U left→right, V following the same convention as the built-in screens (flip later with `crt.screen.inverty` if it comes out upside-down). For a wrap-around screen, unwrap the curved face continuously across 0–1 U.
2. **Name that object** and export it as a **top-level part** in the cabinet `.glb` (a sibling of `bezel`, `marquee`, `coin-slot`, etc.).
3. In `description.yaml`, set `crt.type: custom` and `crt.mesh:` to that object's name.
4. **Do not also list the part in `parts:`.** It is *consumed* — deactivated and replaced by the live screen — so any material/art/`istarget`/`physical`/`speaker` you put on it there is discarded.
5. Configure the game as usual (`rom:`, `core:`, `video:`, `coinslot:`, …). **No `screen-mock-vertical/-horizontal` placeholder is needed** — the screen sits exactly where you modeled the part (WYSIWYG).

`orientation` is ignored (there is no mock to pick), and `crt.geometry` (scale %, ratio, rotation) still applies *on top of* the part's own transform if you need to nudge it.

### Light guns work

Custom screens are fully **light-gun compatible** — HotD2- / ninja-assault-style gun games hit-test correctly on a custom mesh, including curved and angled surfaces, with no extra author effort. Add the normal `light-gun:` block and it just works:

```yaml
light-gun:
  active: true
  gun:
    model: my-gun.glb
  # crt: { invertx: true, inverty: true }   # only if the AIM is mirrored vs. the image
```

Under the hood the same mesh becomes the gun-ray **collider**, and the hit point is converted to libretro gun coordinates by barycentrically interpolating the mesh's **UVs** — so wherever your 0–1 UVs map, the gun maps too. Requirements:

- **The mesh must be CPU-readable** (`Read/Write` enabled). GLB parts loaded at runtime are readable automatically, so there's nothing to toggle. This is the one hard requirement for the gun; if it were ever false, hits would silently miss.
- **Normals face the player**, and **UVs span 0–1** cleanly (the same UVs the shader uses).
- Gun-aim inversion is independent of the image: `light-gun.crt.invertx/inverty` correct the *aim*, `crt.screen.invertx/inverty` correct the *picture*.
- **On hardware cores (Flycast)** the screen texture is a `RenderTexture`, so the light-gun debug circle (`light-gun.debug.active`) can't draw on it — but the gun coordinates still register (they come from the mesh UVs, not the texture). Test by shooting the game, not by looking for the red dot.

### What you get for free

A custom screen is a full screen instance (see below), so everything a normal screen does comes along unchanged: the emulator, attract-mode video/audio, coin handling, AGEBasic, and the screen glow light. In particular there are **no separate audio/video objects to wire** — the screen's own `AudioSource` rides on the mesh, so game/attract audio is 3D-spatialized from wherever you placed it. (See §4 and [LibretroScreenController](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs) for the audio path.)

### How it works under the hood

There is **no `screenCustom.prefab`**. `custom` is registered in [`CRTsFactory`](../Assets/curif/LibRetroWrapper/CRTsFactory.cs) pointing at the existing **`screen19i`** prefab, and [`Cabinet.buildCustomCRT`](../Assets/curif/LibRetroWrapper/Cabinet.cs) does runtime "prefab surgery" on the instance:

1. Look up the author's part by name (error clearly if missing / no mesh / more than one submesh).
2. Instantiate `screen19i` at the part's transform and copy its local position/rotation/scale.
3. Swap `MeshFilter.sharedMesh` **and** `MeshCollider.sharedMesh` to the author's mesh (collider stays non-convex — required for `hit.triangleIndex`).
4. Collapse the two-slot material array to just the screen-surface material (drop the bezel slot).
5. Attach `ScreenSurfaceInfo { materialSlot = 0, submeshIndex = 0 }` so the shader (§6/§7) and `LightGunTarget` target slot/submesh 0 instead of the default 1.
6. Deactivate the donor part and fall through to the normal `LibretroScreenController` wiring.

Reusing the authored prefab (rather than bolting components onto the raw part) is deliberate: it keeps the full, correct component set — including an **authored `AudioSource`**, which Unity's `OnAudioFilterRead` needs to pull emulator audio on Quest (a runtime-added `AudioSource` stays silent there).

### Scope and limitations

- **Libretro cabinets only.** `type: custom` always wires the `LibretroScreenController` (emulator) path — including light guns and the Flycast HW path. Per-cabinet AGEBasic hooks work on it exactly as on any libretro screen.
- **No AGEBasic-only custom screen.** `custom` cannot substitute for `19i-agebasic` (a display driven purely by AGEBasic, no emulator) — there is no `custom-agebasic` type. Adding one would mean giving `AGEBasicScreenController` the same `ScreenSurfaceInfo` slot/submesh treatment `LibretroScreenController` got, plus a branch in `Cabinet.addCRT`.
- **One submesh** on the mesh, and the part must be **top-level** in the `.glb`. Failures (wrong submesh count, missing part) are reported through the normal cabinet load-error path.
