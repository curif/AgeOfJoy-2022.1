# MR → VR: stick locomotion dead after phone booth return

**Status:** implemented (pending Quest verification)  
**Branch context:** `fix/v0.5-2` / `0.5.0`  
**Audience:** agent / QA verifying VR move controls after phone-booth return  
**Do not regress:** booth orientation, floor placement, WorldLock / Meta Reset View colocation (card 4)

---

## Fix applied (2026-07-11)

### 1. `ChangeControls` resume hardened
- Resume path always re-applies providers/actions (no silent skip when flag already false).
- Re-enables `actionBasedContinuousTurnProvider` (was left off after MR suspend).
- Re-enables `CharacterController` on the move provider if disabled.
- Logs `locomotion restored move=… continuousTurn=…`.

### 2. Phone-booth `EnableVrModeAndLocomotion` handoff
Order now mirrors safe parts of `EnterVRCoroutine`:
1. Passthrough off  
2. `SetMode(VR)`  
3. `MRSceneHost.SuspendForVr()` — disables MRUK (stops WorldLock)  
4. `RestoreXrOriginTrackingOffsetFromAppStart()` — **CameraFloorOffset only** (no app-start player teleport)  
5. Resume VR systems except locomotion → delay → `ResumePlayerLocomotionForVr`

### 3. `MRCameraRigShim` push gate
WorldLock → `CameraFloorOffset` push only while `CurrentMode` is `MR` or `MR_EDIT`. In VR the shim follows XROrigin again.

### 4. Snapshot correct pose → reapply after handoff (VR return displacement)
After walking in MR, booth return can look correct briefly then shove position/rotation ~1s later (dirty `CameraFloorOffset` + late handoff). Fix:

1. After arrival explosion: snapshot locomotion-root world pos/rot (`snapshot good pose`)
2. Before offset restore: snapshot **head** world position (root XZ alone is wrong after room-scale walk)
3. `SuspendForVr` + `RestoreXrOriginTrackingOffsetFromAppStart`
4. Translate root so the head returns; restore snapshotted root rotation (not head look yaw)
5. After locomotion resume: reapply for 3 frames; again after **1s** (`reapplied good pose (*)`)

**Do not** force `OrientPlayerYawToFacePhone` on return — preserve natural facing.

---

## Symptom (original)

After **phone booth travel MR → VR**, the player arrives in the gallery but **left-stick move (and often turn) does not work**.  
Physical walking / HMD tracking may still work.

**Out of scope:** Quick Travel (removed).

---

## What must stay working (baseline)

| Feature | Notes |
|---------|--------|
| Phone booth facing / spawn in MR | `PlaceOnMrFloor` + WorldLock push in MR only |
| Floor spawn near player | commit `f16b41e3` |
| Wall placement / posters | commit `67fdf7b4` |
| Meta Reset View / WorldLock colocation | push active **only in MR mode** |
| Locomotion suspended while in MR | intentional via `MRVrSystemsGate` |

---

## Test plan

- [ ] VR → MR via handset: booth upright, phone facing player, floor snap OK  
- [ ] Meta Reset View in MR: walls/floor stay colocated (WorldLock)  
- [ ] MR → VR via handset: player inside/near gallery booth (not exterior spawn)  
- [ ] **Left stick move works** within ~1s of arrival  
- [ ] Walk in MR then return: pose stays correct after ~1s (no late shove)  
- [ ] Facing matches natural orientation (backs to phone in MR → backs in VR)  
- [ ] Snap / continuous turn works as before MR  
- [ ] Log contains `locomotion restored move=True`  
- [ ] Placement ray / config cabinet still OK on next MR entry  
- [ ] Standard `EnterVR()` (if used) still resumes locomotion  

## Log hints

- `[ChangeControls] locomotion restored move=True continuousTurn=…`  
- `MRVrSystemsGate.ResumePlayerLocomotionForVr`  
- `EnterVRFromPhoneBoothCoroutine` / `EnableVrModeAndLocomotion done`  
- `snapshot good pose` / `reapplied good pose (*)`  
- `restored CameraFloorOffset localPos=…`  
- `SuspendForVr — MR children disabled`  

---

## Approaches that already failed (do not repeat)

1. Restoring **app-start player world/local position** on booth return (teleports outside booth).  
2. Suppressing WorldLock **rotation** only while `PlayerInside` booth.  
3. Forcing booth/player yaw rewrites while debugging locomotion.  
4. Reintroducing Quick Travel.

---

## Related docs

- `MIXED_REALITY_DESIGN.md`  
- `AGENTS.md` → specialist MR (`70-mixed-reality`)  
- Skill: `.cursor/skills/implement-mr-phase/SKILL.md`
