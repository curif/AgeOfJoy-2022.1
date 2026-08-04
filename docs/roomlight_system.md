# Room Light System

Age of Joy has **two parallel lighting layers** that together define how a room looks at runtime:

| Layer | Component | Drives | Update behavior |
|---|---|---|---|
| **Per-scene named lights** | `LightManagerController` on individual `Light` GameObjects (tagged `"Light"`) | The neon signs, ceiling lamps, spotlights, fixtures baked into a room | **Instant snap** — set value, GPU sees it next frame |
| **Global user light** | A single `UserLightManager` carrying one vertex-rendered directional `Light` | Overall room *mood* / ambient tint set by the player or the scripts | **Cross-faded** over a duration (default 1s) |

Both layers are independent: changing the global mood light does not move the named fixtures, and vice-versa. They composite naturally in the renderer.

---

## 1. Why two layers

**Per-scene lights** are the lights an environment artist places — a neon over a cabinet, a spotlight on a marquee, a flickering bulb. They have specific positions and are part of the room's identity. Scripts (AGEBasic) need to address them by name and change them *immediately* — a neon flicker or a strobe effect can't lerp.

**The global user light** is a single directional light intended as a cheap, mobile-friendly mood layer. It is **vertex-shaded directional**, which is essentially free on Quest hardware. Its job is to let the player (or a room's YAML config) raise the apparent ambient level and tint of a room.

> **Important:** the global light is **additive**. The player can crank the room brighter or shift its color, but cannot turn baked lighting *down*. Treat it as "mood on top," not "master dimmer."

---

## 2. Layer 1 — per-scene named lights

### `LightManagerController` ([Assets/curif/LibRetroWrapper/LightManagerController.cs](../Assets/curif/LibRetroWrapper/LightManagerController.cs))

Attached to any Unity `Light` you want addressable from scripts. Requirements:

1. The GameObject must have a Unity `Light` component on the **same** GameObject.
2. The GameObject must be **tagged `"Light"`** (this is the discovery mechanism — AGEBasic uses `GameObject.FindGameObjectsWithTag("Light")`).
3. A `RoomConfiguration` must be assigned in the inspector so the controller can build a unique name.

On `Start()` the controller builds:

```
LightName = $"{roomConfiguration.Room.ToUpper()}:{gameObject.name.ToUpper()}"
// e.g. "ROOM005:NEON_LEFT"
```

This `ROOM:NAME` form is the stable identifier exposed to AGEBasic. The class exposes three operations — all of which write directly to the underlying `Light` with no interpolation:

```csharp
public void SetIntensity(float newIntensity);   // instant
public void SetColor(Color newColor);            // instant
public float GetIntensity();
```

### When to use this layer

- Scripts that flash, blink, strobe, or react to gameplay events on a specific fixture.
- Anything that needs to be addressable by name.
- Per-fixture color or brightness changes that should happen *right now*.

---

## 3. Layer 2 — the global user light

### `UserLightManager` ([Assets/geometrizer/scripts/UserLightManager.cs](../Assets/geometrizer/scripts/UserLightManager.cs))

There is exactly one of these in the scene hierarchy — a GameObject named `"UserLightManager"` with a directional `Light` (vertex render mode for cost). Lookup is by name: `GameObject.Find("UserLightManager")`, lazy-cached on the caller.

The only mutation entry point is:

```csharp
public void ApplyUserLightSettings(RGBColor color, float? intensity = null,
                                   float transitionDuration = 1f);
```

It cancels any in-flight transition and starts a coroutine `TransitionLightSettings` that `Mathf.Lerp`s the intensity and `Color.Lerp`s the color from current to target over `transitionDuration` seconds. Passing `null` for color or intensity holds the current value.

A second overload accepts `UnityEngine.Color` and wraps it into an `RGBColor` with intensity 0.

### The four input sources

```
                                      ┌─────────────────────────┐
   Room YAML (config.light) ─────────▶│                         │
                                      │                         │
   World triggers (Color/Intensity) ─▶│    UserLightManager     │──▶ directional Light
                                      │   (cross-fade coroutine)│
   In-VR Configuration cabinet ──────▶│                         │
                                      │                         │
   AGEBasic SETGLOBALLIGHT ──────────▶│                         │
                                      └─────────────────────────┘
```

Each source is described below.

---

## 4. Source A — Room YAML + reflection coupling

### Data model

[`ConfigInformation.UserLightSettings`](../Assets/curif/LibRetroWrapper/ConfigInformation.cs) (lines 45-50):

```csharp
public class UserLightSettings : ConfigInformationBase
{
    public RGBColor color = null;
    public float intensity = 0;
}
```

This is the YAML schema (`light.color` + `light.intensity`) merged from the global config and the room's own `.yaml` under `ConfigManager.ConfigDir`. [`RoomConfiguration`](../Assets/curif/LibRetroWrapper/RoomConfiguration.cs) loads it on scene init and fires `OnRoomConfigChanged` when the file is edited at runtime.

### `PF_ReflectionChangeTrigger` — the room-entry environment trigger

Every room scene drops one instance of [`Assets/geometrizer/PF_ReflectionChangeTrigger.prefab`](../Assets/geometrizer/PF_ReflectionChangeTrigger.prefab) — a box-shaped trigger volume covering the room (or its doorway). It is the mechanism that makes "this room has its own look" true at runtime.

#### Why it exists

**Rooms are loaded additively, but Unity's environment settings are global.** [`GateController`](../Assets/curif/LibRetroWrapper/GateController.cs) (line 207) and [`TeleportationController`](../Assets/curif/LibRetroWrapper/TeleportationController.cs) (line 44) bring rooms in with `LoadSceneMode.Additive`, so a room coexists at runtime with the gallery and with any neighbouring rooms already streamed in.

`RenderSettings.skybox`, `RenderSettings.customReflectionTexture` and the ambient environment are **process-global, not per-scene**. Whichever scene was loaded or activated last wins for all of them, and reflection/light probe blending does not follow the player across an additive scene boundary. Unity offers no built-in "the environment of the scene I am standing in" concept under this loading model.

The only reliable signal for *which* room the player is actually in is therefore the player's physical position. Crossing into the volume is that signal: at that moment the room pushes its own cubemap, skybox and mood light into the shared render state. Without this trigger a room would simply inherit whatever environment the previously visited room left behind.

#### What the prefab carries

| Component | Role |
|---|---|
| `BoxCollider` | `isTrigger`, GameObject on layer `Player`, with `m_IncludeLayers` restricted to that layer so physics never reports non-player contacts. Resize per room in the scene instance. |
| [`ReflectionChangeTrigger`](../Assets/geometrizer/scripts/ReflectionChangeTrigger.cs) | Reflection cubemap swap **plus** the room's mood light. |
| [`SkyboxChangeTrigger`](../Assets/geometrizer/scripts/SkyboxChangeTrigger.cs) | Assigns `newSkyboxMaterial` to `RenderSettings.skybox` and calls `DynamicGI.UpdateEnvironment()`. |

Bundling all three concerns on one GameObject is **deliberate**: when the player crosses a doorway, reflections, sky and mood light must change together as a single visual beat rather than popping at separate moments.

#### `ReflectionChangeTrigger` behaviour

Despite the name, the script does two jobs on `OnTriggerEnter` (guarded by `other.CompareTag("Player")`):

1. **Swap the reflection cubemap** — assigns `newReflectionCubemap` to `RenderSettings.customReflectionTexture` and calls `DynamicGI.UpdateEnvironment()`, skipping the work if the cubemap is already current.
2. **Apply the user-light settings from YAML** — reads `roomConfiguration.Configuration.light` and calls `userLightManager.ApplyUserLightSettings(color, intensity)`. It first forces `light.color.intensity = 0` so brightness is driven by the separate `intensity` field rather than by the intensity baked into the `RGBColor`.

Its lifecycle covers the cases where the player never crosses the volume, or edits the config while standing inside it:

- **`Start()`** — if the room YAML has no `light` block, it applies the inspector fallback `RoomStartingColor` / `intensity` (the dev-time default); otherwise it applies the YAML values immediately, so the room already looks right before the player reaches the trigger.
- **`OnEnable` / `OnDisable`** — subscribe and unsubscribe `OnRoomConfigChanged` on the `RoomConfiguration`, guarded by `isListenerAdded`. Editing the room's `.yaml` at runtime re-applies the light live, with no scene reload.
- **`Start()` fallback lookup** — if `userLightManager` is not wired in the inspector (it is null on the prefab), it resolves via `GameObject.Find("UserLightManager")`.

#### Wiring a room instance

Per-scene overrides on the prefab instance — Room001 ([Assets/Scenes/Room001.unity](../Assets/Scenes/Room001.unity), around line 20586) is the reference example:

- `BoxCollider` `m_Size` / `m_Center` — sized to the room.
- `roomConfiguration` — **must** point at the scene's own `RoomConfiguration` object; left null, the room falls back to `RoomStartingColor` forever and ignores its YAML.
- `RoomStartingColor` / `intensity` — dev-time fallback only (Room001 uses black / 0).
- `newReflectionCubemap`, `newSkyboxMaterial` — usually inherited from the prefab; override for a room with a distinct look.

> **Gotchas.** The serialized `lightTransitionDuration` field is **dead** — every call site omits the duration argument and takes `ApplyUserLightSettings`'s own `1f` default, which coincidentally matches the prefab value, so changing it in the inspector does nothing. And because `userLightManager` is resolved by name, renaming the `UserLightManager` GameObject breaks the light silently (see §8).

---

## 5. Source B — player-activated world triggers

Two near-identical scripts let the level designer drop in-world "switches" that cycle through preset values. The activation pattern is shared:

- The player's `GrabVolumeSmall` collider enters the trigger.
- The configured `InputActionReference` (a controller button) is performed.
- An index advances through a preset array and the new value is pushed to `UserLightManager`.
- Optional: an `AudioSource` plays and a target `Animation` plays.

### `ColorSwitchTrigger` ([Assets/curif/LibRetroWrapper/ColorSwitchTrigger.cs](../Assets/curif/LibRetroWrapper/ColorSwitchTrigger.cs))

Cycles a `Color[] Colors` array. Each activation advances `colorIdx` and calls `ApplyUserLightSettings(Colors[colorIdx], transitionDuration: lightTransitionDuration)`.

### `LightIntensityTrigger` ([Assets/curif/LibRetroWrapper/LightIntensityTrigger.cs](../Assets/curif/LibRetroWrapper/LightIntensityTrigger.cs))

Cycles a `float[] Intensity` array. Same pattern, but pushes intensity only, holding the current color.

Both expose a custom inspector with a "Simulate" button so a designer can preview the cycle without entering play mode.

---

## 6. Source C — the in-VR Configuration cabinet

[`ConfigurationController.cs:1369-1463`](../Assets/curif/UI/ConfigurationController.cs#L1369) implements the Lights submenu of the in-VR configuration cabinet.

Widgets (`GenericOptionsInteger` / `GenericOptionsDecimal`):

- `lightColorR`, `lightColorG`, `lightColorB` — 0-255 each
- `lightIntensity` — 0.0-2.5 in 0.1 steps
- `lightColor` — named-color picker that fills the RGB widgets from `ScreenGenerator.ColorMap`

On **save**, `LightUpdateConfigurationFromWidgets` writes the values into `config.light.color` / `config.light.intensity` and persists via `configHelper.Save(isGlobalConfigurationWidget.value, config)` — either to the per-room YAML or the global YAML depending on the toggle. The `RoomConfiguration` then picks up the change through its file watcher and the next `ReflectionChangeTrigger` entry (or the active trigger's re-listen path) re-applies it.

`LightAdjustColorWidgets` draws a live RGB swatch on the CRT so the player sees the result before saving.

---

## 7. Source D — AGEBasic

AGEBasic exposes both layers to user scripts. The implementations live in [`Assets/curif/LibRetroWrapper/basic/functions/lights.cs`](../Assets/curif/LibRetroWrapper/basic/functions/lights.cs).

- **Per-scene lights** (operate on `LightManagerController` by `"ROOM:NAME"`): `GETLIGHTS`, `LIGHTSCOUNT`, `GETLIGHTINTENSITY`, `SETLIGHTINTENSITY`, `SETLIGHTCOLOR`.
- **Global user light** (operates on `UserLightManager`): `GETGLOBALLIGHT`, `SETGLOBALLIGHT`.

See the source file above for argument signatures, and [`AGEBasicTests/core/lights.bas`](../AGEBasicTests/core/lights.bas) + [`AGEBasicTests/core/addlightintensity.bas`](../AGEBasicTests/core/addlightintensity.bas) for working examples.

> AGEBasic's per-scene setters are instant. AGEBasic's `SETGLOBALLIGHT` triggers the standard cross-fade. This asymmetry is intentional — see §8.

---

## 8. Behavior notes

- **Snap vs. fade is intentional.** Per-scene `SetIntensity` / `SetColor` write the `Light` field directly so scripted effects (a neon flicker, a strobe, a beat-synced pulse) are exact. The global mood light cross-fades because it represents atmosphere, which should never pop.
- **Global light is additive.** It only raises apparent illumination — it cannot dim baked lighting. If a room feels too bright with the user light off, the fix is in the room's bake or in its per-scene `Light` intensities, not in the global.
- **Discovery is tag-based.** Per-scene lights are discovered by `GameObject.FindGameObjectsWithTag("Light")`. A scene `Light` without the `"Light"` tag is invisible to AGEBasic. A non-light GameObject with the `"Light"` tag will be skipped (the `LightManagerController` null-check filters it).
- **Naming requires `RoomConfiguration`.** A `LightManagerController` whose `roomConfiguration` reference is null early-returns from `Start()` and never sets `LightName`. Such a light is then unaddressable from AGEBasic — confirm the reference is wired in the prefab/scene.
- **`UserLightManager` is found by name.** Several callers (`ColorSwitchTrigger`, `LightIntensityTrigger`, `ReflectionChangeTrigger`, AGEBasic's global functions) do `GameObject.Find("UserLightManager")` as a fallback when their serialized reference is null. Renaming the GameObject breaks this chain silently — failures appear as "intensity changes do nothing."
- **Single in-flight transition.** `ApplyUserLightSettings` cancels any running coroutine and starts a fresh one. Two triggers firing in the same frame don't pile up; the second wins.
- **Vertex directional for a reason.** The global light is set to vertex render mode. It's cheap precisely because it never participates in per-pixel shading. Switching it to "Important" / pixel render mode would defeat the cost model.

---

## 9. File map

| Concern | File |
|---|---|
| Per-light controller | [Assets/curif/LibRetroWrapper/LightManagerController.cs](../Assets/curif/LibRetroWrapper/LightManagerController.cs) |
| Global user light | [Assets/geometrizer/scripts/UserLightManager.cs](../Assets/geometrizer/scripts/UserLightManager.cs) |
| Room entry trigger (light + reflection) | [Assets/geometrizer/scripts/ReflectionChangeTrigger.cs](../Assets/geometrizer/scripts/ReflectionChangeTrigger.cs) |
| Player-activated color switch | [Assets/curif/LibRetroWrapper/ColorSwitchTrigger.cs](../Assets/curif/LibRetroWrapper/ColorSwitchTrigger.cs) |
| Player-activated intensity switch | [Assets/curif/LibRetroWrapper/LightIntensityTrigger.cs](../Assets/curif/LibRetroWrapper/LightIntensityTrigger.cs) |
| YAML schema | [Assets/curif/LibRetroWrapper/ConfigInformation.cs](../Assets/curif/LibRetroWrapper/ConfigInformation.cs) (`UserLightSettings`) |
| Room config loader | [Assets/curif/LibRetroWrapper/RoomConfiguration.cs](../Assets/curif/LibRetroWrapper/RoomConfiguration.cs) |
| In-VR Lights menu | [Assets/curif/UI/ConfigurationController.cs:1369-1463](../Assets/curif/UI/ConfigurationController.cs#L1369) |
| AGEBasic functions | [Assets/curif/LibRetroWrapper/basic/functions/lights.cs](../Assets/curif/LibRetroWrapper/basic/functions/lights.cs) |
| AGEBasic examples | [AGEBasicTests/core/lights.bas](../AGEBasicTests/core/lights.bas), [AGEBasicTests/core/addlightintensity.bas](../AGEBasicTests/core/addlightintensity.bas) |
