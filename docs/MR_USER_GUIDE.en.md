# Age of Joy — User Guide: Mixed Reality (MR)

Welcome to the Age of Joy Mixed Reality (MR) guide.

Here you will learn how to set up, customize, and enjoy MR mode — turning your real environment into a true arcade.

If this is your first time with MR, we recommend following this guide in order.

---

## Table of contents

1. [What is Mixed Reality?](#1-what-is-mixed-reality)
2. [How to enter MR](#2-how-to-enter-mr)
3. [How to leave MR](#3-how-to-leave-mr)
4. [What changes in MR](#4-what-changes-in-mr)
5. [Requirements](#5-requirements)
6. [Setting up your room (Space Setup)](#6-setting-up-your-room-space-setup)
7. [First steps](#7-first-steps)
8. [Configuration cabinet (CRT)](#8-configuration-cabinet-crt)
9. [Placement ray](#9-placement-ray)
10. [Arcade cabinets (CABINETS)](#10-arcade-cabinets-cabinets)
11. [Custom objects](#11-custom-objects)
12. [Posters](#12-posters)
13. [Room skin](#13-room-skin)
14. [Official objects](#14-official-objects)
15. [Portable Games](#15-portable-games)
16. [Phone booth](#16-phone-booth)
17. [Settings](#17-settings)
18. [DEBUG and HELP](#18-debug-and-help)
19. [Where files are stored](#19-where-files-are-stored)
20. [AOJ MR Studio](#20-aoj-mr-studio)
21. [Current limitations](#21-current-limitations)

---

## 1. What is Mixed Reality?

**Mixed Reality (MR)** lets you turn your own home into an arcade room.

You will use the real space around you to build your own collection of arcade machines and decorative objects.

In MR you can place:

- Arcade cabinets
- 3D objects
- Posters
- Lighting
- Room skins
- Other official objects

Everything is placed directly on the floor, walls, tables, and ceiling of your home.

Age of Joy uses your Quest room map via **Meta MRUK**, automatically detecting surfaces such as:

- Floor
- Walls
- Ceiling
- Tables

## 2. How to enter MR

Entering Mixed Reality is simple.

In the outdoor area (gallery), approach the **phone booth** and pick up the **handset**.

This starts the transition from VR to MR, including travel effects, fade, and passthrough activation.

---

## 3. How to leave MR

To return to VR mode, use the phone booth **handset** again.

If the booth is hidden, you can show it again via the **PHONE BOOTH** menu on the [configuration cabinet](#8-configuration-cabinet-crt).

---

## 4. What changes in MR

When you enter Mixed Reality, some parts of the game work differently.

### You see your real room

Passthrough stays active throughout the experience, so virtual objects blend into your home environment.

### Traditional locomotion is disabled

In MR you do not walk through Age of Joy’s virtual rooms. Instead, you use your physical space to explore and interact with objects.

### You build your own room

Arcade machines from VR rooms do not appear in Mixed Reality. All decoration is customized by you through the [configuration cabinet](#8-configuration-cabinet-crt).

### Adapted lighting

Environment lighting is tuned to match your real space. You can also configure automatic lighting when entering MR.

### Your room is saved

Object positions are saved between sessions, so you can continue exactly where you left off.

---

## 5. Requirements

Before using Mixed Reality, make sure all requirements below are met.

| Requirement | Description |
|-------------|-------------|
| **Meta Quest with passthrough** | Required to see the real environment |
| **Mapped room** | Allows correct object placement |
| **At least one wall and floor detected** | Required for placement and Effect Mesh |
| **Age of Joy launched at least once** | Automatically creates the folders used by MR |

---

## 6. Setting up your room (Space Setup)

The first time you enter Mixed Reality, Quest may ask you to map your room.

When that happens, **Space Setup** opens automatically. Walk around following the headset instructions until the process is complete.

This map is used to identify the surfaces where objects can be placed.

---

## 7. First steps

After entering Mixed Reality for the first time, follow this sequence:

1. Launch Age of Joy.
2. [Enter Mixed Reality](#2-how-to-enter-mr) using the phone booth handset.
3. If prompted, complete [Space Setup](#6-setting-up-your-room-space-setup).
4. [Place the configuration cabinet](#9-placement-ray) on a wall — only needed the first time.
5. Open the cabinet by **inserting a coin in the slot**.
6. Go to **CABINETS** and add your first arcade machine.
7. Use **CUSTOM OBJECTS** and **OFFICIAL OBJECTS** to decorate.
8. If needed, adjust machine scale in **CONFIG → Adjustments**.

After this initial setup, your room is saved automatically for future sessions.

---

## 8. Configuration cabinet (CRT)

The **configuration cabinet** is the control center of Mixed Reality.

Through it you add, remove, move, and configure every object in your room. The cabinet is mounted on a **wall** and has a small CRT screen that shows all MR menus.

All **Add**, **Rem**, **Move**, and **Opts** actions are done on the **configuration cabinet CRT** — not on arcade machines or props placed in your room.

### Opening the menu

To open the configuration panel:

1. Take the **coin** on your **wrist** (left hand).
2. Insert it in the cabinet **slot**, as you would on any arcade machine.

The menu opens immediately.

### Closing the menu

To close the panel:

1. Select **EXIT**.
2. Press **A** to confirm.

While the [placement ray](#9-placement-ray) is active (Add or Move), the panel closes. After you confirm or cancel, **insert another coin** to open the CRT again.

### Menu controls

| Button | Function |
|--------|----------|
| **Up / Down** | Navigate menu options |
| **L / R** | Switch between available actions (Add, Rem, Move, …) |
| **A** | Confirm the selected option |
| **B** | Go back to the previous menu or cancel the current action |

### Adding, moving, and removing objects

Regardless of object type (machines, posters, lights, or custom objects), the flow is always the same:

1. Open the configuration cabinet (insert a coin).
2. Enter the category you want (**A**).
3. Choose the object with **Up** and **Down**.
4. Use **L** or **R** to select the action.
5. Press **A** to confirm.

| Action | Description |
|--------|-------------|
| **Add** | Adds a new copy of the object to the room |
| **Rem** | Removes the object from the room |
| **Move** | Repositions an object already placed |
| **Opts** | Shows all copies of that object for individual actions (move or adjust) |

Whenever an object is added or moved, the **placement ray** opens automatically.

The number next to the object name shows how many copies are already in your room.

### Menu structure

```
MR CONFIGURATION
├── PHONE BOOTH     → show or hide the phone booth
├── CABINETS        → arcade game catalog in MR
├── CUSTOM OBJECTS  → custom objects, posters, and room skins
├── OFFICIAL OBJECTS→ lights and official props
├── CONFIG          → move cabinet, global adjustments, effect mesh
├── DEBUG           → MR errors by date
├── HELP            → controls and in-headset guide
└── EXIT            → close panel
```

#### PHONE BOOTH

Lets you show or hide the phone booth used to enter and leave Mixed Reality.

#### CABINETS

Contains all arcade machines available to be placed in your room.

#### CUSTOM OBJECTS

Brings together all user-created custom content, including 3D objects, posters, and room skins.

#### OFFICIAL OBJECTS

Contains official Age of Joy objects, such as lighting, fans, Portable Games, and other props included with the game.

#### CONFIG

Brings together general Mixed Reality settings: reposition the cabinet, adjust global machine scale, and view the environment mesh (Effect Mesh).

#### DEBUG

Shows debug information and error logs related to Mixed Reality.

#### HELP

Opens the help guide available directly inside the headset.

#### EXIT

Closes the configuration panel and returns to the game.

---

## 9. Placement ray

Whenever an object needs to be placed or repositioned, the game shows the **placement ray**.

This lets you see exactly where the object will go before you confirm.

### Ray colors

- 🟢 **Green:** valid position
- 🔴 **Red:** invalid position

Press **A** to confirm or **B** to cancel.

When allowed, use the **right thumbstick** (move it left/right) to rotate the object before placing it.

### Surface types

| Surface | Typical use |
|---------|-------------|
| **Floor** | Arcade cabinets, furniture, TVs |
| **Wall** | Posters, fans, configuration cabinet |
| **Ceiling** | Lamps |
| **Table** | Small objects and Portable Games |
| **Object** | On top of another placed object |
| **Free3D** | Anywhere in space |

### Placing the configuration cabinet

The first time you enter Mixed Reality, the game automatically asks where to install the configuration cabinet.

Point the ray at a **wall** and confirm with **A**.

This position is saved for future sessions. To move it later, use **CONFIG → Move Config** ([section 17](#17-settings)).

---

## 10. Arcade cabinets (CABINETS)

The **CABINETS** menu lists all arcade machines available in Age of Joy.

Each game can have one machine in your room, and all work normally after placement.

### Adding a machine

1. Open the configuration cabinet.
2. Enter **CABINETS**.
3. Choose the game.
4. Select **Add** with **L** or **R**.
5. Press **A**.
6. Place the machine with the [placement ray](#9-placement-ray).
7. Rotate with the **right thumbstick** if you want.
8. Press **A** to finish.

### Removing a machine

1. Select the machine in the list.
2. Choose **Rem**.
3. Press **A**.

### Moving a machine

To change the position of an installed machine:

- Select **Move**, or
- Open **Opts** to see all copies of that machine, pick the copy you want, and use **Move** to reposition it.

### Individual adjustments

Inside **Opts**, you can also use **Tune** to make small specific adjustments to that machine, without changing the others.

---

## 11. Custom objects

Besides official objects, Mixed Reality supports **3D packages** created by you or the community.

On the headset they appear under **CUSTOM OBJECTS → Others** on the configuration cabinet CRT. Once created, use **Add**, **Rem**, **Move**, **Opts**, and **Tune** like any other category ([section 8](#8-configuration-cabinet-crt)). **Tune** adjusts the scale of one placed copy without affecting the others.

### Creating packages with AOJ MR Studio

Custom objects are **packages**: a **`.glb`** model plus a config file managed by **[AOJ MR Studio](#20-aoj-mr-studio)** (Windows desktop app). You do **not** need to copy files manually on the Quest — the Studio uploads everything over USB.

Each package contains:

- **`object.yaml`** — name, MR placement, components (behaviours)
- **`<model>.glb`** — 3D mesh
- **Optional files** — video (`.mp4`), audio (`.wav`), etc., referenced by components

#### Requirements

| Item | Why |
|------|-----|
| **Windows** PC with AOJ MR Studio | Editor and upload to Quest |
| **Meta Quest** in developer mode, USB debugging on | ADB connection |
| **USB cable** (or working ADB setup) | Transfer packages |
| **Age of Joy** launched at least once on Quest | Creates MR data folders |

Download the zip from the [latest release](https://github.com/ramiro-github/aoj-mr-studio/releases/latest) (**do not** clone the repository). Extract the folder and run **`AOJ MR Studio.exe`** — no Python install required.

#### Step by step — new object

1. Connect Quest to the PC and open **AOJ MR Studio**. The app connects to the headset automatically (*Connecting to Meta Quest…*), then opens **Home** with the connection status. If it fails, check the USB cable and use **Reconnect**.
2. On **Home**, click **Open Custom Objects**. The app lists packages in the headset’s **Custom Objects** folder.
3. Click **Create folder** and name the package (e.g. `Ventoinha`, `TV_Sala`). That name appears in the game’s **CUSTOM OBJECTS** menu.
4. **Double-click** the new folder to open the package editor.
5. Click **Add model** and pick a **`.glb`** on your computer. The model uploads to Quest; if missing, the Studio creates a default **`object.yaml`** pointing at that GLB.
6. Adjust **Placement** (MR placement) — see table below.
7. Add **Components** (behaviours) if needed — see table below. Use **Add component**, pick a type, and fill the fields; the Studio suggests GLB child names when possible.
8. Click **Save to Meta Quest**. The updated `object.yaml` is written to the package on the headset.
9. In MR, open the configuration cabinet → **CUSTOM OBJECTS → Others** → select the package → **Add** → place with the [placement ray](#9-placement-ray).

To edit an existing package: **Open Custom Objects** → double-click the folder → change Placement/Components → **Save to Meta Quest**.

The editor also shows the `object.yaml` text for reference; the recommended flow is the **Placement** and **Components** panels, which update the file automatically.

#### Placement (MR placement)

Defines where the object can go in your room and how it behaves in the placement ray.

| Studio field | What it does |
|--------------|--------------|
| **surfaceType** | Allowed surface: floor, wall, ceiling, table, on another object, or free (see [section 9](#9-placement-ray)) |
| **allowStickRotation** | Lets you rotate with the **right thumbstick** before confirming position |
| **stickRotationAxis / stickRotationSpeed** | Axis and speed of that rotation |
| **providesAnchor** | This object accepts **other props on top** (e.g. shelf, TV) |
| **anchorTarget** | GLB child used as the surface (e.g. `Top`); empty = model root |

#### Components (behaviours)

Components are behaviours attached when the object spawns in MR. In the Studio, add one row per component and configure its fields.

| Component | Purpose | Main fields |
|-----------|---------|-------------|
| **grab** | Grab the object in VR | **twoHands** — one or two hands; **returnOnRelease** — snap back when released; **hideHands** — hide hands while grabbing; **target** — GLB child (optional) |
| **video** | Play video on a model screen | **file** — `.mp4` in the package; **target** — GLB child mesh (e.g. `Screen`); **loop**, **volume** |
| **rotator** | Spin a part of the model continuously | **target** — GLB child (e.g. `Blades`); **axis** — x/y/z; **speed** — degrees per second |
| **animator** | Play an animation embedded in the GLB | **clip** — animation name in the file; **target** — bone/mesh (optional); **loop**, **speed** |

**Tips:**

- **target** names must match parts of the **`.glb`** (the Studio lists suggestions after reading the model).
- **video**: prefer **MP4 H.264** (`yuv420p`) on Quest; put the file in the same package folder in the Studio before saving.
- **grab** with **twoHands** requires holding both handles — useful for handheld-style props.
- One object can have **multiple components** (e.g. `rotator` + `grab`).

#### In the game (after creating the package)

1. Insert a coin in the configuration cabinet.
2. **CUSTOM OBJECTS → Others** → pick the package.
3. **Add** / **Rem** / **Move** / **Opts** / **Tune** — same flow as [section 8](#8-configuration-cabinet-crt).

The game may include an example package on first use so you can try the workflow.

---

## 12. Posters

Posters work similarly to custom objects.

Place images in `MR/Posters/`. They then appear under **CUSTOM OBJECTS → Posters**.

Posters can be added, removed, repositioned, and resized.

Size adjustments are done through **Tune**, letting you change a poster’s scale individually without affecting the others.

---

## 13. Room skin

**Room skins** change how your room surfaces look using the Quest room map.

Apply different materials to create custom environments without manually editing walls or floors.

Room skins are under **CUSTOM OBJECTS → Room Skin**. Files go in `MR/Room Skins/`.

Select a room skin and press **A** to apply it.

Unlike other objects, a room skin applies directly to the room mesh — it does **not** use the placement ray.

---

## 14. Official objects

**Official objects** are items created and shipped with Age of Joy. They are under **OFFICIAL OBJECTS** on the configuration cabinet CRT.

The menu has two categories.

### Lights

Contains all lamps available for Mixed Reality.

Besides adding or removing lamps, you can configure **automatic lighting** when entering MR.

Available actions: **Add**, **Rem**, **Move**.

### Others

Brings together the other official objects in the game, such as fans (**PF_Fan**), **Portable Games**, and other props added in future updates.

These objects use the same placement flow as the other categories.

---

## 15. Portable Games

**Portable Games** is a handheld console that lets you play, right in your hands, any title you already have a **core** and **ROM** for — the **same files** as Age of Joy VR.

### Installing Portable Game

To add the device to your room:

1. Open the configuration cabinet.
2. Go to **OFFICIAL OBJECTS → Others**.
3. Select **Portable Games**.
4. Choose **Add** and press **A**.
5. Place the stand on a **table** with the [placement ray](#9-placement-ray).

After installation, the console rests on the stand until you use it.

### How to use

To pick up the device:

1. Hold **both handles** at once, one in each hand.

The console lifts off the stand and the main menu appears on screen.

When you **release either hand**:

- the menu closes;
- any running game stops;
- the console returns to the stand automatically.

The device only works while held with **both hands**.

### Navigation

| Control | Function |
|---------|----------|
| **Left thumbstick** (up / down) | Move through lists |
| **A** (right hand) | Confirm |
| **B** (right hand) | Back |

### Choosing a game

1. Pick a LibRetro **core**.
2. Select a **ROM** for that core.
3. Press **A** to start the game.

ROMs come from the same folders as Age of Joy VR: **`cores/`** and **`downloads/`** on the device.

If no games appear, check that files are installed in **`cores/`** and **`downloads/`**.

### During gameplay

Controls follow the LibRetro mapping for the selected core.

Press **Y** (left hand) to exit the game and return to the ROM list.

---

## 16. Phone booth

The **phone booth** switches between VR and Mixed Reality.

By default it stays **visible** after you enter MR. You can hide it temporarily to free up space.

### Show or hide the booth

1. Open the configuration cabinet (insert a coin).
2. Go to **PHONE BOOTH** and press **A**.
3. Choose **Show** or **Hide** with **L** or **R**.
4. Confirm with **A**.

When the booth is shown again, it returns to the last position used.

> **Important:** the booth must be **visible** to [return to VR mode](#3-how-to-leave-mr).

---

## 17. Settings

The **CONFIG** menu holds general Mixed Reality settings.

### Move Config

Lets you change the configuration cabinet position. When you select this option, the placement ray opens so you can choose a new wall.

### Adjustments

Lets you adjust **all** arcade machines in the room at once:

- **Global scale** of the machines
- **Height (Offset Y)**

Default scale is **1.00**. Use these adjustments if machines look larger or smaller than expected in your environment.

### Mesh

Shows the mesh Quest created during room mapping (floor = green, wall = orange, table = yellow, ceiling = blue).

This view is useful to check that all surfaces were detected correctly. If a surface is missing or wrong, run [Space Setup](#6-setting-up-your-room-space-setup) again.

---

## 18. DEBUG and HELP

Age of Joy includes two useful tools to help while using Mixed Reality.

### DEBUG

The **DEBUG** menu shows diagnostic information and MR error logs. This information can help identify configuration problems or provide details to the developer when reporting a bug.

### HELP

The **HELP** menu brings together a quick guide to controls and main Mixed Reality features. You can consult it anytime without removing the headset.

---

## 19. Where files are stored

All Mixed Reality data is saved automatically on your headset.

You do not need to move or edit files manually during normal use of the game.

| Folder | Content |
|--------|---------|
| `MR/Custom Objects/` | Custom objects |
| `MR/Posters/` | Posters |
| `MR/Room Skins/` | Room skins |

If you also use [AOJ MR Studio](#20-aoj-mr-studio), these folders can sync automatically to your computer, making content creation and management easier.

In addition to those files, the game also automatically saves:

- arcade machine positions;
- object positions;
- environment settings;
- user adjustments.

Your room will be exactly as you left it the next time you start Age of Joy.

---

## 20. AOJ MR Studio

**AOJ MR Studio** is the official **Windows** tool to create and send MR content to Quest over USB (ADB). You do not need to edit headset folders by hand.

**Download:** [latest release](https://github.com/ramiro-github/aoj-mr-studio/releases/latest) — download **`AOJ-MR-Studio-vX.Y.Z-win64.zip`**, extract the **`AOJ MR Studio`** folder, and run **`AOJ MR Studio.exe`**. **Do not** use `git clone`; the GitHub repo is for developers only.

### What the Studio does today

| Feature | Description |
|---------|-------------|
| **Home** | Automatic Quest connection, headset status, built-in MR manual (English / pt-BR), **Open Custom Objects** and **Reconnect** buttons |
| **Custom Objects** | Create package folders, upload `.glb`, edit placement and components, save `object.yaml` on Quest |
| **Quest connection** | **Reconnect** on Home; **Check device** on the package list to test ADB |

The full workflow for **custom objects** is in [section 11](#11-custom-objects) (Placement, Components **grab** / **video** / **rotator** / **animator**).

Posters and room skins are still added through the game or the flows in [sections 12](#12-posters) and [13](#13-room-skin); broader Studio support may arrive in future versions.

### Quick requirements

- Quest in **developer mode**, **USB debugging** on
- **Age of Joy** run at least once on the headset
- USB cable to the PC (the installer bundles `adb`)

---

## 21. Current limitations

Mixed Reality is still evolving; new features will be added as Age of Joy develops.

Current limitations include:

- No VR room locomotion in MR.
- Room scan required for precise placement.
- Some features may change between versions.
- New objects and tools will arrive in future updates.
- Small mapping differences may occur depending on your environment and Space Setup quality.

If you run into problems, use the **DEBUG** menu to gather information and send a report with as much detail as possible.

---

## Conclusion

Try different combinations of machines, objects, posters, and lighting to create a unique space fully integrated with your home.

Mixed Reality is built to grow with the community. New objects, features, and tools will keep arriving, making each update another chance to personalize your experience.

**Have fun and enjoy Age of Joy Mixed Reality!**
