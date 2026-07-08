# Flycast cabinets — `description.yaml` options

Age of Joy can run Sega Dreamcast / NAOMI / Atomiswave games through the **Flycast** core
(hardware-rendered, Vulkan). A cabinet opts in with:

```yaml
core: flycast
```

This page documents the cabinet-author options that are **specific to the Flycast core**. For the
general cabinet schema (geometry, screen, ROMs, materials, controls) see
*CDL — the Cabinet Description Language*. Options here are ignored by the software cores
(`mame2003+`, `fbneo`, …).

---

## `input:` — controller options

```yaml
core: flycast
input:
  analog-stick: true      # default: false
```

### `analog-stick`

Chooses what the **thumbstick** drives on the emulated Dreamcast controller.

| Value | Thumbstick drives | Use for |
|---|---|---|
| `false` *(default)* | the **digital d-pad** | fighting games, platformers, anything digital (Street Fighter, Marvel vs Capcom, Soul Calibur) |
| `true` | the **analog stick** *and the analog triggers* | racing / analog games (Sega Rally 2, Crazy Taxi, F355) |

**Why this exists:** a Quest controller only has a thumbstick — there is no physical d-pad. By default
Age of Joy maps that thumbstick to the Dreamcast **d-pad**, which is what most games want. Racing
games, however, expect a proportional **analog** stick for smooth steering. Setting
`analog-stick: true` switches this one cabinet over to analog.

### What `analog-stick: true` maps

| Physical control (Quest / gamepad) | Dreamcast input |
|---|---|
| Left thumbstick | Analog stick (steering) |
| Right trigger | Right analog trigger — **accelerate** |
| Left trigger | Left analog trigger — **brake** |

All the normal buttons (A/B/X/Y, Start, coin) keep working as usual.

### Notes & behavior

- **External gamepads (Xbox, etc.):** a real gamepad has *both* a d-pad and an analog stick, and
  both reach the Dreamcast at once — the d-pad drives the DC d-pad, the stick drives the DC analog
  stick — regardless of this setting. `analog-stick` only changes what the **Quest thumbstick**
  (a single physical control) does.
- On Quest with `analog-stick: true`, the thumbstick still also nudges the digital d-pad. This is
  harmless for racing games (they steer on the analog axis; the d-pad is only used for menus), so
  both are sent. If a specific game misbehaves, let us know.
- The triggers only produce an analog value in `analog-stick: true` mode. In the default d-pad mode
  the triggers act as their normal digital buttons.

### Example — a racing cabinet

```yaml
name: Sega Rally 2
year: 1998
core: flycast
rom: segarally2.chd
input:
  analog-stick: true
crt:
  # …screen geometry…
```

---

## Where files live on the device

For a `core: flycast` cabinet, files come from three places under the app data folder
(`/sdcard/Android/data/com.curif.AgeOfJoy/`):

| What | Location | Notes |
|---|---|---|
| **Game image** (`.chd`, `.cdi`, `.zip`, …) | `downloads/dc/<rom>` | the `rom:` value; falls back to `downloads/<rom>` |
| **BIOS + nvmem** | `system/dc/` | `dc_boot.bin`, `dc_flash.bin`; NAOMI/Atomiswave bios zips (`naomi.zip`, `awbios.zip`, `hod2bios.zip`, …); `dc_nvmem.bin` |
| **VMU + savestates** | `system/dc/saves/` | per-game memory-card saves |

Both roots use the same `dc/` folder name (matching the RetroArch-standard flycast BIOS layout).
The BIOS is **global**, shared by every flycast cabinet — it is *not* part of a cabinet's own
folder; a cabinet directory (`cabinetsdb/<name>/`) only holds the model, textures, video and
`description.yaml`. Every flycast platform (Dreamcast, NAOMI, Atomiswave) reads its BIOS from
`system/dc/`.

## Related

- Light-gun cabinets (`light-gun:`) — see *controllers* docs; a gun cabinet does **not** need an
  `input:` block.
- Engine internals for this core live in `docs/vulkan_cores_build.md` and the source in
  `Assets/curif/LibRetroWrapper/FlycastCore.cs` / `PdLibretro.cs`.

> **Maintainers:** when you add or change a Flycast-specific YAML option, update this file. It is the
> author-facing contract for `core: flycast`.
