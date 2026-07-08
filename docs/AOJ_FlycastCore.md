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

---

## `environment:` — flycast core options (per cabinet)

Any flycast (`reicast_*`) core option can be overridden per cabinet through the `environment:`
block. The full option id is `prefix` + `_` + property name, so for flycast use `prefix: reicast`:

```yaml
core: flycast
rom: mygame.chd
environment:
  prefix: reicast
  properties:
    broadcast: "PAL"              # → reicast_broadcast
    force_freeplay: "disabled"    # → reicast_force_freeplay
    cable_type: "TV (RGB)"        # → reicast_cable_type
```

**Only the options you list are changed.** Everything else keeps the shared safe defaults in
`downloads/dc/Flycast.opt` — the YAML values are layered on top of those defaults at launch (YAML
wins). Nothing is written to disk; the merge happens in memory per game load, so a cabinet that
sets only `broadcast` keeps every other default (cable type, analog triggers, DSP, freeplay, …)
exactly as-is.

Use this for games that need non-default settings — PAL timing, a different `cable_type`, forcing
`reicast_force_freeplay`, `reicast_hle_bios`, `reicast_digital_triggers`, etc. The option ids and
their accepted values are the standard flycast/libretro ones (the same keys you see in
`Flycast.opt`).

> Values must be the exact strings the core expects (e.g. `cable_type: "VGA"` / `"TV (RGB)"` /
> `"TV (Composite)"`), quoted as in `Flycast.opt`.

### Full option reference

Every option below can be set under `environment: properties:` (drop the `reicast_` prefix — it's
supplied by `prefix: reicast`). The **AoJ default** column is the current shipped baseline
(`downloads/dc/Flycast.opt`); anything you don't list keeps that value.

**System & boot**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `region` | Japan / USA / Europe / Default | USA |
| `language` | Japanese / English / German / French / Spanish / Italian / Default | English |
| `broadcast` | NTSC / PAL / PAL_N / PAL_M / Default | NTSC |
| `cable_type` | VGA / TV (RGB) / TV (Composite) | VGA |
| `hle_bios` | disabled / enabled | enabled |
| `boot_to_bios` | disabled / enabled | disabled |
| `force_freeplay` | disabled / enabled | enabled |
| `allow_service_buttons` | disabled / enabled | disabled |
| `force_wince` | disabled / enabled | disabled |
| `dc_32mb_mod` | disabled / enabled | disabled |
| `gdrom_fast_loading` | disabled / enabled | disabled |

**Video & rendering**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `internal_resolution` | 320x240, 640x480, 800x600, … 12800x9600 (27 steps) | 640x480* |
| `screen_rotation` | horizontal / vertical | horizontal |
| `alpha_sorting` | per-strip (fast, least accurate) / per-triangle (normal) / per-pixel (accurate) | per-triangle (normal) |
| `oit_abuffer_size` | 512MB / 1GB / 2GB / 4GB | 512MB |
| `oit_layers` | 8 / 16 / 32 / 64 / 96 / 128 | 32 |
| `emulate_framebuffer` | disabled / enabled | disabled |
| `enable_rttb` | disabled / enabled | disabled |
| `mipmapping` | disabled / enabled | enabled |
| `fog` | disabled / enabled | enabled |
| `anisotropic_filtering` | off / 2 / 4 / 8 / 16 | 4 |
| `texture_filtering` | 0 (default) / 1 (force nearest) / 2 (force linear) | 0 |
| `pvr2_filtering` | disabled / enabled | disabled |
| `native_depth_interpolation` | disabled / enabled | disabled |
| `fix_upscale_bleeding_edge` | disabled / enabled | enabled |
| `widescreen_hack` | disabled / enabled | disabled |
| `widescreen_cheats` | disabled / enabled | disabled |

\* compile-time default; not pinned in AoJ's `Flycast.opt`.

**Performance & timing**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `threaded_rendering` | disabled / enabled | enabled |
| `auto_skip_frame` | disabled / some / more | disabled |
| `frame_skipping` | disabled / 1 / 2 / 3 / 4 / 5 / 6 | disabled |
| `delay_frame_swapping` | disabled / enabled | disabled |
| `detect_vsync_swap_interval` | disabled / enabled | disabled |
| `sh4clock` | 100–500 MHz (steps of 10; underclock/overclock) | 200 |

**Audio**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `enable_dsp` | disabled / enabled | enabled |
| `volume_modifier_enable` | disabled / enabled | enabled |

**Input**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `analog_stick_deadzone` | 0% / 5% / 10% / 15% / 20% / 25% / 30% | 15% |
| `trigger_deadzone` | 0% / 5% / 10% / 15% / 20% / 25% / 30% | 0% |
| `digital_triggers` | disabled / enabled | disabled |
| `enable_purupuru` | disabled / enabled | enabled |

> `digital_triggers` must stay `disabled` for the `input: analog-stick` gas/brake to work as analog.

**Custom textures**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `custom_textures` | disabled / enabled | disabled |
| `preload_custom_textures` | disabled / enabled | disabled |
| `texupscale` | 1 / 2 / 4 / 6 | 1 |
| `texupscale_max_filtered_texture_size` | 256 / 512 / 1024 | 256 |
| `dump_textures` | disabled / enabled | disabled |
| `dump_replaced_textures` | disabled / enabled | disabled |

**Light gun**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `lightgun1_crosshair` … `lightgun4_crosshair` | disabled / White / Red / Green / Blue | disabled |
| `lightgun_crosshair_size_scaling` | 50%–300% (steps of 10%) | 100% |
| `show_lightgun_settings` | enabled / disabled | disabled |

**Networking**

| Option (`reicast_…`) | Values | AoJ default |
|---|---|---|
| `dcnet` | disabled / enabled | enabled |
| `upnp` | disabled / enabled | enabled |
| `emulate_bba` | disabled / enabled | disabled |
| `network_output` | disabled / enabled | disabled |

**Peripherals & VMU** (rarely needed for cabinets — the on-screen VMU overlay is not shown in AoJ)

- `device_port{1-4}_slot1` — VMU / Purupuru / DreamPotato / None (default VMU)
- `device_port{1-4}_slot2` — VMU / Purupuru / None (default Purupuru)
- `per_content_vmus` — disabled / VMU A1 / All VMUs (default disabled)
- `vmu_sound` — disabled / enabled · `linked_vmu_storage` — disabled / enabled
- On-screen VMU overlay family (per index 1-4): `vmu{n}_screen_display`, `vmu{n}_screen_position`
  (Upper/Lower Left/Right), `vmu{n}_screen_size_mult` (1x-5x), `vmu{n}_screen_opacity` (10%-100%),
  `vmu{n}_pixel_on_color` / `vmu{n}_pixel_off_color` (29 named colors, e.g. `DEFAULT_ON 00`,
  `WHITE 28`), plus `show_vmu_screen_settings`.

## Related

- Light-gun cabinets (`light-gun:`) — see *controllers* docs; a gun cabinet does **not** need an
  `input:` block.
- Engine internals for this core live in `docs/vulkan_cores_build.md` and the source in
  `Assets/curif/LibRetroWrapper/FlycastCore.cs` / `PdLibretro.cs`.

> **Maintainers:** when you add or change a Flycast-specific YAML option, update this file. It is the
> author-facing contract for `core: flycast`.
