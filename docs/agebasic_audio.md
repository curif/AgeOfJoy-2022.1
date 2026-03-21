# AGEBasic Audio Reference

AGEBasic exposes four independent audio systems. Each targets a different use case and they can all run simultaneously.

```
┌─────────────────────────────────────────────────────────────────┐
│  Unity AudioMixer  (global volume buses)                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────────┐                  │
│  │  Game    │  │  Music   │  │  Ambience    │                   │
│  │  bus     │  │  bus     │  │  bus         │                   │
│  └────┬─────┘  └────┬─────┘  └──────┬───────┘                  │
│       │             │               │                           │
│  ┌────▼─────┐  ┌────▼──────────┐    │                          │
│  │ Cabinet  │  │ JukeBox       │    │                          │
│  │ AudioSrc │  │ (global MP3   │    │                          │
│  │ (SID,    │  │  playlist)    │    │                          │
│  │  video)  │  └───────────────┘    │                          │
│  │          │                       │                          │
│  │ CabParts │                       │                          │
│  │ AudioSrc │                       │                          │
│  └──────────┘                       │                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 1. Global Audio Mixer

Controls the master volume of three Unity audio buses. Values are in **decibels** (Unity AudioMixer format): `0` = full volume, `-80` = silence.

Available from any context (cabinet scripts, configuration room scripts).

| Function | Returns | Description |
|---|---|---|
| `AUDIOGAMEGETVOLUME()` | number (dB) | Get the Game bus volume |
| `AUDIOGAMESETVOLUME(db)` | 1 | Set the Game bus volume |
| `AUDIOMUSICGETVOLUME()` | number (dB) | Get the Music bus volume |
| `AUDIOMUSICSETVOLUME(db)` | 1 | Set the Music bus volume |
| `AUDIOAMBIENCEGETVOLUME()` | number (dB) | Get the Ambience bus volume |
| `AUDIOAMBIENCESETVOLUME(db)` | 1 | Set the Ambience bus volume |

**Example:**
```basic
10 REM Mute music bus, boost game audio
20 CALL AUDIOMUSICSETVOLUME(-80)
30 CALL AUDIOGAMESETVOLUME(0)
```

---

## 2. Global Music Player (JukeBox playlist)

A sequential MP3/OGG playlist that runs globally across the whole arcade, independent of any cabinet. The player is the `JukeBox` GameObject in the scene. Files are resolved relative to the **music directory** (`<config>/music/`).

All functions return `1` on success. The queue persists across scripts — use `MUSICCLEAR()` to reset it.

### Queue management

| Function | Description |
|---|---|
| `MUSICADD(filename)` | Append a file to the end of the queue. `filename` is relative to the music directory. |
| `MUSICREMOVE(filename)` | Remove a file from the queue. |
| `MUSICEXIST(filename)` | Returns `1` if the file is currently in the queue, `0` otherwise. |
| `MUSICCLEAR()` | Empty the entire queue. |
| `MUSICCOUNT()` | Returns the number of tracks in the queue. |
| `MUSICGETLIST(separator)` | Returns all queue entries as a single string separated by `separator`. |
| `MUSICADDLIST(list, separator)` | **Replaces** the current queue with files parsed from a separator-delimited string. |
| `MUSICADDLISTARRAY(array)` | **Replaces** the current queue with files from an AGEBasic array. |

### Playback control

| Function | Description |
|---|---|
| `MUSICPLAY()` | Start playing from the current queue position. |
| `MUSICNEXT()` | Skip to the next track. |
| `MUSICPREVIOUS()` | Go back to the previous track. |
| `MUSICRESET()` | Jump back to the first track without stopping. |
| `MUSICLOOP(bool)` | Enable (`1`) or disable (`0`) looping of the whole queue. |
| `MUSICLOOPSTATUS()` | Returns `1` if looping is enabled. |

**Example — build and start a playlist:**
```basic
10 CALL MUSICCLEAR()
20 CALL MUSICADD("ingame_theme.mp3")
30 CALL MUSICADD("boss_theme.mp3")
40 CALL MUSICLOOP(1)
50 CALL MUSICPLAY()
```

**Example — load from an array:**
```basic
10 LET TRACKS = ARRAY("track1.ogg", "track2.ogg", "track3.ogg")
20 CALL MUSICADDLISTARRAY(TRACKS)
30 CALL MUSICPLAY()
```

---

## 3. Cabinet Parts Audio

Each cabinet part declared with an `audio` speaker entry in `description.yaml` gets a `CabinetPartAudioController` component. This allows individual parts (e.g. speakers, buttons, slots) to play their own spatialized sounds independently.

The `part` argument is the part name string **or** the part index number.

| Function | Returns | Description |
|---|---|---|
| `CABPARTSAUDIOFILE(part, path)` | 1 | Assign an audio file to the part. Path is absolute or relative to the cabinet folder. |
| `CABPARTSAUDIOPLAY(part)` | 1 | Start playback on the part's AudioSource. |
| `CABPARTSAUDIOSTOP(part)` | 1 | Stop playback. |
| `CABPARTSAUDIOPAUSE(part)` | 1 | Pause playback. |
| `CABPARTSAUDIOVOLUME(part, volume)` | 1 | Set volume (0.0 – 1.0). |
| `CABPARTSAUDIOLOOP(part, bool)` | 1 | Enable (`1`) or disable (`0`) looping. |
| `CABPARTSAUDIODISTANCE(part, min, max)` | 1 | Set 3D audio rolloff distances in world units. |

The part **must be declared** with a `speaker` block in `description.yaml`. Calling these functions on a part without audio configuration throws a runtime error.

**Example — coin slot click sound:**
```basic
10 CABPARTSAUDIOFILE "coin-slot", combinepath(cabinetpath(), "sounds/coin.ogg")
20 CABPARTSAUDIOVOLUME "coin-slot", 0.8
30 CABPARTSAUDIODISTANCE "coin-slot", 0.5, 3.0
40 CABPARTSAUDIOLOOP "coin-slot", 0
50
60 ' Play on coin insert event
70 ONEVENT ONCONTROL("JOYPAD_SELECT_0", "pressed") GOTO 1000
80 END
90
1000 CALL CABPARTSAUDIOPLAY("coin-slot")
1010 END
```

---

## 4. SID Player

A Commodore 64 SID chip emulator built into AGEBasic. Plays `.sid` files (PSID/RSID format, versions 1–4) through the cabinet's existing AudioSource. Supports **multiple named instances** playing simultaneously — ideal for background music + overlapping sound effects.

**Availability:** requires a cabinet that has an AudioSource (i.e. `AGEBasicScreenController` or `AGEBasicCabinetController` cabinet types). Not available in the Configuration Room context.

### How it works

- Each `SIDLOAD` call allocates an independent 6502 CPU + SID chip pair identified by a name string.
- `OnAudioFilterRead` mixes all currently playing SID instances into the cabinet's audio stream at ~50 Hz (PAL).
- Multiple SIDs play simultaneously; control each one independently with its name.
- At most **3 active SIDs** is recommended to stay within Quest CPU budget.

### Commands (statements — no return value)

| Command | Syntax | Description |
|---|---|---|
| `SIDLOAD` | `SIDLOAD name, path` | Load a `.sid` file from disk. Synchronous — call during setup, not in the game loop. |
| `SIDLOADDATA` | `SIDLOADDATA name, storage` | Load a SID from bytes stored in a `DATA` list (embed SID data directly in the script). |
| `SIDPLAY` | `SIDPLAY name [, song]` | Start or restart playback. `song` is 1-based; omit to use the file's default sub-song. Calling again while playing **restarts** the SID — useful for one-shot sound effects. |
| `SIDSTOP` | `SIDSTOP name` | Stop playback. The instance remains loaded and ready to play again. |
| `SIDPAUSE` | `SIDPAUSE name` | Pause playback at the current position. |
| `SIDRESUME` | `SIDRESUME name` | Resume from the paused position. |
| `SIDUNLOAD` | `SIDUNLOAD name` | Free the instance. Always unload when done to reclaim memory. |
| `SIDVOLUME` | `SIDVOLUME name, vol` | Set volume for this instance. `vol` is 0–100. |

### Functions (return a value)

| Function | Returns | Description |
|---|---|---|
| `SIDSTATUS(name)` | number | `0` = not loaded · `1` = loaded/stopped · `2` = playing · `3` = paused |
| `SIDTITLE(name)` | string | Song title from the SID file header |
| `SIDAUTHOR(name)` | string | Composer name from the SID file header |
| `SIDRELEASED(name)` | string | Release / copyright string from the header |
| `SIDCOUNT(name)` | number | Total number of sub-songs in the file |
| `SIDDEFAULTSONG(name)` | number | The file's recommended starting sub-song (1-based) |

### Path helpers

Use `AGEBASICPATH()` and `COMBINEPATH()` to build portable paths:

```basic
10 LET MPATH = COMBINEPATH(AGEBASICPATH(), "music/Delta.sid")
```

Place `.sid` files in `<agebasicpath>/music/` (or any path reachable by the cabinet).

### Example — background music + one-shot SFX

```basic
10 CALL SETCPU(50)
20 REM Load music and SFX simultaneously
30 SIDLOAD "music", COMBINEPATH(AGEBASICPATH(), "music/theme.sid")
40 SIDLOAD "boom",  COMBINEPATH(AGEBASICPATH(), "music/explosion.sid")
50
60 SIDVOLUME "music", 70
70 SIDVOLUME "boom",  60
80 SIDPLAY "music"   REM background loops
90
100 REM Game loop
110 IF CONTROLACTIVE("JOYPAD_B_0") THEN SIDPLAY "boom", 1  REM re-trigger SFX over music
120 IF CONTROLACTIVE("JOYPAD_X_0") THEN GOTO 500
130 SLEEP 0.05
140 GOTO 100
150
500 SIDSTOP "music" : SIDUNLOAD "music"
510 SIDSTOP "boom"  : SIDUNLOAD "boom"
520 END
```

### Embedding SID data with DATA

For self-contained scripts that don't need an external file, paste raw PSID bytes as decimal values:

```basic
10 DATA "zap", 80, 83, 73, 68, ...  REM raw PSID bytes
20 SIDLOADDATA "sfx", "zap"
30 SIDPLAY "sfx"
```

### Volume headroom guideline

When mixing multiple SIDs, reduce individual volumes to avoid clipping:

| Active SIDs | Recommended max per-instance volume |
|---|---|
| 1 | 100 |
| 2 | 70 |
| 3 | 55 |

---

## Choosing the right audio system

| Situation | System to use |
|---|---|
| Arcade ambient music playing throughout the whole venue | **Global Music Player** |
| A cabinet part makes a click, buzz, or mechanical sound | **Cabinet Parts Audio** |
| A coin slot, flipper, or button needs its own 3D-spatialized sound | **Cabinet Parts Audio** |
| An AGEBasic game needs background music in the C64 chiptune style | **SID Player** |
| An AGEBasic game needs short sound effects (explosions, pickups, hits) | **SID Player** (separate named instance, re-trigger with `SIDPLAY`) |
| You need to duck or mute entire audio categories globally | **Global Audio Mixer** |
| Background music + chiptune game music at the same time | **Global Music Player** + **SID Player** |
