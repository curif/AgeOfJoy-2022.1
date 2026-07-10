# Flycast cabinets — `description.yaml` options

Age of Joy can run Sega Dreamcast / NAOMI / Atomiswave games through the **Flycast** core (hardware-rendered, Vulkan). 

On compatibility: performance seems smooth on the Quest 3 hardware, with systems in place to smoothly frame-pace the 60fps of the console to the 72fps refresh of the headset. If you are curious as to whether or not a game runs, it may be faster to do a test-run on a PC via Retroarch and Flycast, or by sideloading Retroarch on the Quest 3, finding optimal settings. Resolution defaults to 640x480 (progressive frame via VGA box emulation) and can be set higher but is not recommended due to performance concerns.

A cabinet opts in with:

```yaml
core: flycast
```

This page documents the cabinet-author options that are **specific to the Flycast core**. For the general cabinet schema (geometry, screen, ROMs, materials, controls) see *CDL — the Cabinet Description Language*. Options here are ignored by the software cores (`mame2003+`, `fbneo`, …).

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
| `false` *(default)* | the **digital d-pad** | fighting games, platformers, anything digital (MVC, Soul Calibur) |
| `true` | the **analog stick** *and analog triggers* | racing / analog games (Sega Rally 2, Crazy Taxi, Daytona) |

**What this does:** a Quest controller only has an analog thumbstick — there is no old-school physical D-pad. By default Age of Joy maps the Quest's thumbstick to the Dreamcast **d-pad**, which is what most games want. Other games like racing games, flying games, etc expect **analog** stick for smooth input. Setting `analog-stick: true` switches this one cabinet over to analog.

This only applies to the Quest stick! If you're using a Bluetooth controller your Analog Pad and D-Pad will be mapped as normal.

### What `analog-stick: true` maps

| Physical control (Quest / gamepad) | Dreamcast input |
|---|---|
| Left thumbstick | Analog stick (steering) |
| Right trigger | Right analog trigger — **accelerate** |
| Left trigger | Left analog trigger — **brake** |

All the normal buttons (A/B/X/Y, Start, coin) keep working as usual.

### Notes & behavior

- **External gamepads (Xbox, etc.):** a real gamepad has *both* a d-pad and an analog stick, and both reach the Dreamcast at once — the d-pad drives the DC d-pad, the stick drives the DC analog stick — regardless of this setting. `analog-stick` only changes what the **Quest thumbstick** (a single physical control) does.
- On Quest with `analog-stick: true`, the thumbstick still also nudges the digital d-pad. This is harmless for racing games (they steer on the analog axis; the d-pad is only used for menus), so both are sent. If a specific game misbehaves, let us know.
- The triggers only produce an analog value in `analog-stick: true` mode. In the default d-pad mode the triggers act as their normal digital buttons.

### Example — a racing cabinet

```yaml
name: Sega Rally 2
year: 1999
core: flycast
rom: segarally2.chd
input:
  analog-stick: true
crt:
  # …screen geometry…
```

### Example — a light gun cabinet

A light gun is **not** part of the `input:` block — it's its own top-level `light-gun:` block, and a gun cabinet needs no `input:` block at all. The minimum is `active: true` plus a gun `model:`:

```yaml
name: House of the Dead 2
year: 1998
core: flycast
rom: hotd2.zip
light-gun:
  active: true
  gun:
    model: gun.glb          # the 3D model held in the player's hand
crt:
  # …screen geometry…
```

The engine wires up the rest automatically: port 0 is declared a light gun before the ROM loads, the VR aim raycast is pushed to the core each frame, and the coin slot is delivered as the gun's coin input. Optional tuning lives under `gun:` (`invert-pointer`, `adjust-sight: {horizontal, vertical}`) and `crt:` (`invertx` / `inverty`, `border-size-x` / `border-size-y`) if a game's aim needs calibrating.

Two flycast-specific caveats:

- In light-gun mode flycast reads **only** `LIGHTGUN_*` inputs on that port — `JOYPAD_*` mappings are ignored. If you add a custom `controllers:` block, remap `LIGHTGUN_TRIGGER`, `LIGHTGUN_START`, etc., not the joypad ids.
- Single player only for now (port 0); the other ports stay as joypads.

---

## Where do I put my files?

For a `core: flycast` cabinet, files are located in three places under the app data folder (`/sdcard/Android/data/com.curif.AgeOfJoy/`):

| What | Location | Notes |
|---|---|---|
| **Game image** (`.chd`, `.cdi`, `.zip`, …) | `downloads/dc/<rom>` | the `rom:` value; falls back to `downloads/<rom>` |
| **BIOS + nvmem** | `system/dc/` | `dc_boot.bin`, `dc_flash.bin`; NAOMI/Atomiswave bios zips, custom game BIOSs (`naomi.zip`, `awbios.zip`, `hod2bios.zip`, …); `dc_nvmem.bin` |
| **VMU + savestates** | `system/dc/saves/` | per-game memory-card saves |

Both roots use the same `dc/` folder name (matching the RetroArch-standard flycast BIOS layout). The BIOS is **global**, shared by every flycast cabinet — it is *not* part of a cabinet's own folder; a cabinet directory (`cabinetsdb/<name>/`) only holds the model, textures, video and `description.yaml`. Every flycast platform (Dreamcast, NAOMI, Atomiswave) reads its BIOS from `system/dc/`.

Dreamcast games use the HLE bios and do not require a BIOS file. Naomi and Atomiswave games do require one, however. Note that some games (HOTD2) require a secondary bios file that should also be placed in /system/dc.

The emulator's settings have been set to "safe" defaults that work with most tested games. It's also set to use HLE BIOS emulation, so no Dreamcast BIOS file is needed for Dreamcast games. This can be changed to use the official bios; see the section below.

---

## SOMETHING WENT WRONG!

If you're testing on the Flycast (Dreamcast / NAOMI) cores and something misbehaves — the screen stays black after you drop a coin, or the game never
starts — there's an automatic log that tells you what goofed.

**Where it is:** on the headset, browse to

```
Android/data/<the app>/Logs/flycast.log
```


---

## `environment:` — flycast core options (per cabinet)

Any flycast (`reicast_*`) core option can be overridden per cabinet through the `environment:` block. The full option id is `prefix` + `_` + property name, so for flycast use `prefix: reicast`:

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

**Only the options you list are changed.** Everything else keeps the shared safe defaults in `downloads/dc/Flycast.opt` — the YAML values are layered on top of those defaults at launch (YAML wins). Nothing is written to disk; the merge happens in memory per game load, so a cabinet that sets only `broadcast` keeps every other default (cable type, analog triggers, DSP, freeplay, …) exactly as-is.

Use this for games that need non-default settings — PAL timing, forcing `reicast_force_freeplay`, `reicast_hle_bios`, `reicast_digital_triggers`, etc. The option ids and their accepted values are the standard flycast/libretro ones (the same keys you see in `Flycast.opt`).

> Values must be the exact strings the core expects (e.g. `cable_type: "VGA"` / `"TV (RGB)"` / `"TV (Composite)"`), quoted as in `Flycast.opt`.

### Full option reference

Every option below can be set under `environment: properties:` (drop the `reicast_` prefix — it's supplied by `prefix: reicast`). The **AoJ default** column is the current shipped baseline (`downloads/dc/Flycast.opt`); anything you don't list keeps that value.

**System & boot**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `region` | **USA** | Japan / USA / Europe / Default |
| `language` | **English** | Japanese / English / German / French / Spanish / Italian / Default |
| `broadcast` | **NTSC** | NTSC / PAL / PAL_N / PAL_M / Default |
| `cable_type` | **VGA** | VGA / TV (RGB) / TV (Composite) |
| `hle_bios` | **enabled** | disabled / enabled |
| `boot_to_bios` | **disabled** | disabled / enabled |
| `force_freeplay` | **enabled** | disabled / enabled |
| `allow_service_buttons` | **disabled** | disabled / enabled |
| `force_wince` | **disabled** | disabled / enabled |
| `dc_32mb_mod` | **disabled** | disabled / enabled |
| `gdrom_fast_loading` | **disabled** | disabled / enabled |

**Video & rendering**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `internal_resolution` | **640x480** | 320x240, 640x480, 800x600, … 12800x9600 (27 steps) |
| `screen_rotation` | **horizontal** | horizontal / vertical |
| `alpha_sorting` | **per-triangle (normal)** | per-strip (fast, least accurate) / per-triangle (normal) / per-pixel (accurate) |
| `oit_abuffer_size` | **512MB** | 512MB / 1GB / 2GB / 4GB |
| `oit_layers` | **32** | 8 / 16 / 32 / 64 / 96 / 128 |
| `emulate_framebuffer` | **disabled** | disabled / enabled |
| `enable_rttb` | **disabled** | disabled / enabled |
| `mipmapping` | **enabled** | disabled / enabled |
| `fog` | **enabled** | disabled / enabled |
| `anisotropic_filtering` | **4** | off / 2 / 4 / 8 / 16 |
| `texture_filtering` | **0** | 0 (default) / 1 (force nearest) / 2 (force linear) |
| `pvr2_filtering` | **disabled** | disabled / enabled |
| `native_depth_interpolation` | **disabled** | disabled / enabled |
| `fix_upscale_bleeding_edge` | **enabled** | disabled / enabled |
| `widescreen_hack` | **disabled** | disabled / enabled |
| `widescreen_cheats` | **disabled** | disabled / enabled |

**Performance & timing**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `threaded_rendering` | **enabled** | disabled / enabled |
| `auto_skip_frame` | **disabled** | disabled / some / more |
| `frame_skipping` | **disabled** | disabled / 1 / 2 / 3 / 4 / 5 / 6 |
| `delay_frame_swapping` | **disabled** | disabled / enabled |
| `detect_vsync_swap_interval` | **disabled** | disabled / enabled |
| `sh4clock` | **200** | 100–500 MHz (steps of 10; underclock/overclock) |

**Audio**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `enable_dsp` | **enabled** | disabled / enabled |
| `volume_modifier_enable` | **enabled** | disabled / enabled |

**Input**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `analog_stick_deadzone` | **15%** | 0% / 5% / 10% / 15% / 20% / 25% / 30% |
| `trigger_deadzone` | **0%** | 0% / 5% / 10% / 15% / 20% / 25% / 30% |
| `digital_triggers` | **disabled** | disabled / enabled |
| `enable_purupuru` | **enabled** | disabled / enabled |

> `digital_triggers` must stay `disabled` for the `input: analog-stick` gas/brake to work as analog.

**Custom textures**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `custom_textures` | **disabled** | disabled / enabled |
| `preload_custom_textures` | **disabled** | disabled / enabled |
| `texupscale` | **1** | 1 / 2 / 4 / 6 |
| `texupscale_max_filtered_texture_size` | **256** | 256 / 512 / 1024 |
| `dump_textures` | **disabled** | disabled / enabled |
| `dump_replaced_textures` | **disabled** | disabled / enabled |

**Light gun**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `lightgun1_crosshair` … `lightgun4_crosshair` | **disabled** | disabled / White / Red / Green / Blue |
| `lightgun_crosshair_size_scaling` | **100%** | 50%–300% (steps of 10%) |
| `show_lightgun_settings` | **disabled** | enabled / disabled |

**Networking**

| Option (`reicast_…`) | AoJ default | Values |
|---|---|---|
| `dcnet` | **enabled** | disabled / enabled |
| `upnp` | **enabled** | disabled / enabled |
| `emulate_bba` | **disabled** | disabled / enabled |
| `network_output` | **disabled** | disabled / enabled |

**Peripherals & VMU** (rarely needed for cabinets — the on-screen VMU overlay is not shown in AoJ)

- `device_port{1-4}_slot1` — VMU / Purupuru / DreamPotato / None (default VMU)
- `device_port{1-4}_slot2` — VMU / Purupuru / None (default Purupuru)
- `per_content_vmus` — disabled / VMU A1 / All VMUs (default disabled)
- `vmu_sound` — disabled / enabled · `linked_vmu_storage` — disabled / enabled
- On-screen VMU overlay family (per index 1-4): `vmu{n}_screen_display`, `vmu{n}_screen_position` (Upper/Lower Left/Right), `vmu{n}_screen_size_mult` (1x-5x), `vmu{n}_screen_opacity` (10%-100%), `vmu{n}_pixel_on_color` / `vmu{n}_pixel_off_color` (29 named colors, e.g. `DEFAULT_ON 00`, `WHITE 28`), plus `show_vmu_screen_settings`.

## When a cabinet doesn't boot

Every flycast boot writes to `Logs/flycast.log` under the app data folder, whether or not debug mode is on. If the game refuses to load, that file now carries the reason the **core itself** gave, followed by the tail of the native boot trace and a listing of `system/dc/`. A missing arcade BIOS, for instance, reads:

```
ERROR  [LibretroFlycastCore.Start] pdlr_start FAILED — retro_load_game('…/toyfight.zip') failed — the core said: Error: cannot load BIOS naomi
```

Note that each arcade game names **its own** BIOS set — `toyfight` wants `naomi.zip`, `hotd2` wants `hod2bios.zip` — so having `naomi.zip` installed does not cover every NAOMI cabinet. Flycast looks for the BIOS ROMs inside the game's own zip first, then a parent romset zip beside it, and only then `system/dc/<set>.zip`; a merged romset that carries its own BIOS therefore needs no separate file at all.

By default only warnings and errors from the core are captured. To capture its full boot chatter (every `INFO` line — ROM loading, region detection, cart mapping), drop a file named `verbose.txt` next to the game in `downloads/dc/` containing a single digit:

| `verbose.txt` | Captures |
|---|---|
| `0` | everything, including `DEBUG` |
| `1` | `INFO` and above — the full boot trace |
| `2` | `WARN` and above (the default) |
| `3` | errors only |

This needs no rebuild, and it's the same trick as the pacing knobs (`displaylock.txt`, `backpressure.txt`, `governor.txt`) that live in the same folder. Turning on the app's debug mode raises the capture level to `INFO` on its own.

## Related

- Light-gun cabinets (`light-gun:`) — see *controllers* docs; a gun cabinet does **not** need an `input:` block.
- Engine internals for this core live in `docs/vulkan_cores_build.md` and the source in `Assets/curif/LibRetroWrapper/LibretroFlycastCore.cs` / `LibretroHWBridge.cs`.

> **Maintainers:** when you add or change a Flycast-specific YAML option, update this file. It is the author-facing contract for `core: flycast`.
