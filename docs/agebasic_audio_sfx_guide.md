# AGEBasic Sound & SFX Guide

How to add sound effects and music to your AGEBasic cabinet, from finding
ready-made audio to creating it yourself.

---

## 1. Understanding Your Audio Options

AGEBasic has four audio systems. Choose the right one for the job:

| Situation | System | Format |
|---|---|---|
| Background music throughout the arcade | Global Music Player | MP3 / OGG |
| Cabinet button click, coin slot, mechanical sound | Cabinet Parts Audio | WAV / OGG |
| One-shot SFX (explosion, laser, pickup) | Cabinet Parts Audio | WAV / OGG |
| C64 chiptune background music | SID Player | `.sid` |
| C64 chiptune SFX alongside music | SID Player (second instance) | `.sid` |

For most SFX needs — explosions, laser shots, power-ups — **Cabinet Parts Audio
with an OGG file is the simplest and most flexible approach.**

---

## 2. SID Files — What They Are and How to Get Them

### What a SID file actually is

A `.sid` file is **not audio**. It is a small C64 program: actual 6502 machine
code that runs a Commodore 64 SID chip emulator, telling the chip's three
oscillators, filters, and envelopes what to do in real time.

Because of this:
- You cannot convert an MP3 or WAV to SID — they are fundamentally different things.
- You cannot generate a SID with AI music tools like Suno.
- A SID file must be composed and programmed specifically for the C64 hardware.

### What you can do with SID files

Many SID files contain **multiple sub-songs** in a single file. A game SID
rip will often have the title music as sub-song 1, level music as sub-song 2,
and all the in-game sound effects (explosions, shots, pick-ups) as sub-songs
3 onward.

```basic
10 SIDLOAD "sfx", COMBINEPATH(AGEBASICPATH(), "music/uridium.sid")
20 LET N = SIDCOUNT("sfx")       ' how many sub-songs?
30 FOR I = 1 TO N
40   SIDPLAY "sfx", I            ' audition each one
50   SLEEP 3
60 NEXT I
```

Use this to iterate through sub-songs and find the explosion you need.

### Where to find SID files

| Source | URL | Notes |
|---|---|---|
| **HVSC** (High Voltage SID Collection) | `hvsc.c64.org` | 50,000+ files; look in `GAMES/` for SFX |
| **CSDb** (C64 Scene Database) | `csdb.dk` | Search "sound effects" or "sfx" |
| **DeepSID** (web player) | `deepsid.chordian.net` | Browse and audition SIDs in the browser without downloading |
| **SID Museum** | `sid.pm` | Smaller, curated |

**Good games to look for SFX sub-songs:**
- *Uridium* — space ship engine, explosions
- *R-Type* — full arcade SFX bank
- *Nemesis / Gradius* — laser shots, explosions
- *Commando* — gunshots, grenades
- *Space Taxi* — engine sounds, voices

### Creating a new SID file

This requires programming in 6502 assembly language for the C64 SID chip.
It is not a beginner task, but free tools exist:

- **GoatTracker 2** (cross-platform) — the most popular C64 music/SFX tracker
  for PC. Can export proper `.sid` files. Has a dedicated SFX mode for
  programming single-shot sound effects.
- **SidWizard** — tracker that runs on real C64 hardware or in VICE emulator.
- **VICE emulator** (`vice-emu.sourceforge.io`) — run a full C64 environment
  on PC to compose and test SID programs.

GoatTracker workflow for a simple explosion SFX:
1. Create a new instrument with `NOISE` waveform.
2. Set a fast attack, short decay, zero sustain, fast release.
3. Add a pitch sweep downward (frequency decreasing rapidly).
4. Export as `.sid`.

---

## 3. OGG/WAV Files — The Easier Path for SFX

For one-shot sound effects, Cabinet Parts Audio with OGG files is simpler
than SID and gives you access to a vast library of ready-made sounds.

### Using SFX in AGEBasic

Your cabinet needs a `speaker` part declared in `description.yaml`. Then:

```basic
10 CABPARTSAUDIOFILE "speaker", COMBINEPATH(CABINETPATH(), "sounds/explosion.ogg")
20 CABPARTSAUDIOLOOP "speaker", 0
30 CABPARTSAUDIOVOLUME "speaker", 0.8
40 CABPARTSAUDIODISTANCE "speaker", 0.5, 5.0
50 ONEVENT ONCUSTOM("explode") GOTO 1000
60 END

1000 CALL CABPARTSAUDIOPLAY("speaker")
1010 END
```

Preferred format: **OGG Vorbis** — smaller than WAV, no licensing issues
unlike MP3, and well supported by Unity.

---

## 4. Finding Free SFX Online

### Sound databases

| Source | URL | Notes |
|---|---|---|
| **Freesound** | `freesound.org` | Largest free SFX library; search "explosion", "laser", etc. Filter by license (CC0 = no attribution needed) |
| **Kenney** | `kenney.nl/assets` | Pre-packaged retro/arcade SFX packs, all CC0, WAV format, game-ready |
| **OpenGameArt** | `opengameart.org` | Game-focused; good retro SFX section |
| **Soundsnap** | `soundsnap.com` | Professional quality; subscription-based |
| **BBC Sound Effects** | `sound-effects.bbcrewind.co.uk` | Free for personal/non-commercial use |

For arcade games, **Kenney** is the best starting point — download the
"Sci-Fi Sounds" or "Interface Sounds" pack and you get dozens of ready-to-use
WAV files including explosions, laser shots, and power-ups, all for free with
no strings attached.

### Retro SFX generators (browser-based, free, instant)

These tools generate classic arcade-style 8-bit sounds with one click:

| Tool | URL | Notes |
|---|---|---|
| **BFXR** | `bfxr.net` | Click "Explosion" → instant 8-bit explosion. Export as WAV. Best for retro style. |
| **SFXR** | `sfxr.me` | Original version of BFXR. Simpler interface. |
| **jfxr** | `jfxr.frozenfractal.com` | Web port of BFXR, no install needed |
| **ChipTone** | `sfbgames.itch.io/chiptone` | More control; exports WAV |

**BFXR workflow for an explosion:**
1. Open `bfxr.net`
2. Click the **Explosion** button — it generates a random one instantly
3. Click **Randomize** a few times until you like it
4. Adjust the sliders (Start Frequency, Decay, etc.) to taste
5. Click **Export WAV**
6. Convert to OGG with Audacity or ffmpeg

---

## 5. Creating SFX with AI Tools

### Suno

Suno (`suno.com`) generates music, not sound effects. It can sometimes produce
usable SFX with careful prompting, but results are inconsistent.

**Prompt template for an explosion:**
```
sound effect only, single explosion, cinematic boom, no music, no melody,
no vocals, isolated sfx, 2 seconds, silence after
```

Tips:
- Enable **Instrumental** mode (no lyrics toggle)
- Generate multiple times — it is not reliable for pure SFX
- Trim the result in Audacity to remove music bleed

For retro arcade style:
```
8-bit retro arcade explosion sound effect, single burst, no music, 2 seconds
```

Suno is a last resort for SFX. BFXR or Freesound will give better results
faster.

### Other AI audio tools

| Tool | Notes |
|---|---|
| **ElevenLabs Sound Effects** | `elevenlabs.io` — text-to-sfx, free tier, better than Suno for SFX |
| **Adobe Firefly Audio** | AI SFX generation, requires Adobe account |
| **Soundraw** | AI music generation, not ideal for SFX |

ElevenLabs Sound Effects is currently the most reliable AI tool for generating
a specific sound effect from a text description.

---

## 6. Converting and Preparing Audio

### Convert MP3/WAV to OGG

Using **ffmpeg** (free, command line):
```bash
ffmpeg -i explosion.mp3 -q:a 5 explosion.ogg
ffmpeg -i explosion.wav -q:a 5 explosion.ogg
```

Using **Audacity** (free, GUI):
1. File → Import → Audio
2. File → Export → Export as OGG Vorbis

### Trim silence

Keep SFX files short and tight — remove silence at the start and end.
In Audacity: Effect → Truncate Silence, or just select and delete manually.

### Recommended settings

| Setting | Value |
|---|---|
| Format | OGG Vorbis |
| Sample rate | 44100 Hz |
| Channels | Mono (SFX), Stereo (music) |
| Quality | 5 (OGG scale 0-10) |
| Length | As short as possible for SFX |

---

## 7. Decision Guide

```
I need a sound effect (explosion, laser, etc.)
│
├── I want authentic C64 chiptune style
│   ├── Search HVSC / CSDb / DeepSID for a game SID with SFX sub-songs
│   └── Or use GoatTracker to compose one → export .sid → SIDLOAD
│
└── I want any style
    ├── Instant retro SFX → BFXR (bfxr.net) → export WAV → convert OGG
    ├── Real recordings → Freesound / Kenney → download OGG/WAV
    ├── AI generated → ElevenLabs Sound Effects → download → convert OGG
    └── All of the above → CABPARTSAUDIOFILE + CABPARTSAUDIOPLAY
```
