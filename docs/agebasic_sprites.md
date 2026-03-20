# Working with Sprites in AGEBasic

AGEBasic features a fully hardware-accelerated, software-composited sprite engine designed specifically for the Age of Joy CRT screens. It allows you to draw images over the background without permanently modifying the background pixels, handling Z-ordering and transparency automatically.

## Understanding the Sprite Engine

Unlike standard drawing commands (`DPSET`, `DLINE`, `PRINT`) which permanently overwrite the screen's background buffer, the Sprite Engine works on an active **compositing layer**. 

Every time you call the `SHOW` command, the engine takes the permanent background and draws all active sprites on top of it. 
This means you **do not need to clear the screen or erase the old sprite** when moving it. Just update its coordinates, call `SHOW`, and the engine handles restoring the background perfectly.

## 1. Preparing Your Sprites
*   Sprites must be standard `.png` or `.jpg` files.
*   Transparency (Alpha channel) in PNG files is fully supported. Semi-transparent pixels will blend smoothly with the background and other sprites behind them.
*   For the best performance, place the image files in the same directory as your `.bas` script.

## 2. Loading a Sprite

Loading a texture from disk into memory takes time. To prevent VR stutter, AGEBasic loads sprites **asynchronously** in the background.

Use the `SPRITELOAD` command to start the process, and `SPRITESTATUS()` to wait for it to finish.

```basic
10 REM Start loading the file "ship.png" and assign it the reference name "PLAYER"
20 SPRITELOAD "PLAYER", "ship.png"

30 REM Wait in a loop until the sprite is fully loaded into memory
40 IF SPRITESTATUS("PLAYER") = 0 THEN SLEEP 0.05 : GOTO 40

50 PRINT 0, 0, "Sprite is ready!"
```

## 3. Drawing and Moving Sprites

Once loaded, use the `SPRITE` command to add it to the active screen.

**Syntax:** `SPRITE "name", x, y, z`
*   **`name`**: The reference name you gave it in `SPRITELOAD`.
*   **`x`, `y`**: The pixel coordinates on the screen (0,0 is the top-left).
*   **`z`**: The sorting layer. Sprites with higher Z values are drawn *in front* of sprites with lower Z values.

### Moving a Sprite (Animation)
To move a sprite, simply call `SPRITE` again with new coordinates. You must call `SHOW` to update the screen.

```basic
10 SPRITELOAD "BALL", "ball.png"
20 IF SPRITESTATUS("BALL") = 0 THEN SLEEP 0.1 : GOTO 20

30 REM Move the ball across the screen horizontally
40 FOR X = 0 TO 200 STEP 2
50   SPRITE "BALL", X, 100, 1
60   SHOW
70   SLEEP 0.02
80 NEXT X
```
*Notice there is no `CLS` in the loop. The background is preserved automatically.*

## 4. Layering (Z-Index)

The `Z` parameter determines which sprite is drawn on top when they overlap.
```basic
10 SPRITELOAD "BACKGROUND", "clouds.png"
20 SPRITELOAD "SHIP", "fighter.png"
30 IF SPRITESTATUS("BACKGROUND") = 0 THEN SLEEP 0.1 : GOTO 30
40 IF SPRITESTATUS("SHIP") = 0 THEN SLEEP 0.1 : GOTO 40

50 REM Draw clouds on layer 1 (back)
60 SPRITE "BACKGROUND", 50, 50, 1

70 REM Draw ship on layer 5 (front). It will overlap the clouds.
80 SPRITE "SHIP", 60, 60, 5
90 SHOW
```

## 5. Removing Sprites
When you no longer want a sprite to be drawn on the screen (e.g., an enemy explodes), use `SPRITEREMOVE`.

```basic
10 SPRITEREMOVE "SHIP"
20 SHOW
```
The sprite is removed from the screen, but it remains in the rapid memory cache. If you call `SPRITE` again later, it will reappear instantly without needing to use `SPRITELOAD` again.

## 6. Collision Detection Events

AGEBasic can notify your script when two sprites overlap using the `ONEVENT` system. Collision uses a **two-phase approach** for precision:

1. **AABB pre-check** — fast rectangular overlap test to reject non-overlapping pairs instantly.
2. **Cell-map check** — the overlapping region is tested at a 4×4-pixel cell granularity. A cell is only "active" if it contains at least one non-transparent pixel (alpha > 10). Collision is confirmed only when an active cell from sprite A coincides with an active cell from sprite B.

This means a circular ball sprite in a square PNG will **not** trigger a collision when only the transparent corners overlap — only real art pixels count.

### `ONSPRITECOLLISION("spriteA", "spriteB")`

Fires **once** when the two sprites begin overlapping. It will not fire again while they remain in contact — only on the transition from not-overlapping to overlapping.

### `ONSPRITECOLLISIONEND("spriteA", "spriteB")`

Fires **once** when two previously overlapping sprites stop overlapping.

### Rules

*   Both sprites must be visible and fully loaded for a collision to be detected. If either sprite is not on screen, the result is always no collision.
*   Cell maps are built automatically (once per sprite texture, on first collision check) and cached — no setup needed.
*   Events only trigger when the interpreter is **idle** (no other script is running). Use `SLEEP` in long-running loops to give the event system time to evaluate.
*   Use `END` to finish the event handler block, not `SHUTDOWN` (which would kill all events).

### Querying Collision Details

After a `ONSPRITECOLLISION` event fires, you can inspect exactly which cells collided using two functions:

| Function | Returns | Description |
| :--- | :--- | :--- |
| `SPRITECOLLISIONCOUNT("nameA", "nameB")` | number | Count of colliding cell pairs from the last check |
| `SPRITECOLLISIONDATA("nameA", "nameB")` | array | Flat array `[colA, rowA, colB, rowB, ...]` for every colliding cell pair |

Each entry in `SPRITECOLLISIONDATA` is a pair of **sprite-local cell coordinates** (column and row in the 4×4 grid, where 0,0 is the top-left of the sprite). Multiply by 4 to convert to pixel offset within the sprite.

### Example: Ball Bouncing off a Wall

```basic
10 SPRITELOAD "BALL", "ball.png"
20 SPRITELOAD "WALL", "wall.png"
30 IF SPRITESTATUS("BALL") = 0 THEN SLEEP 0.05 : GOTO 30
40 IF SPRITESTATUS("WALL") = 0 THEN SLEEP 0.05 : GOTO 40

50 REM Place the wall and register collision events
60 SPRITE "WALL", 150, 0, 1
70 ONEVENT ONSPRITECOLLISION("BALL", "WALL") GOTO 200
80 ONEVENT ONSPRITECOLLISIONEND("BALL", "WALL") GOTO 300
90 END

100 REM Main game loop (started elsewhere or via another event)
110 FOR X = 0 TO 200
120   SPRITE "BALL", X, 80, 2
130   SHOW
140   SLEEP 0.02
150 NEXT X
160 END

200 REM Ball hit the wall — inspect which cells collided
210 CALL CABPARTSAUDIOPLAY("hit-sound")
220 LET PAIRS = SPRITECOLLISIONCOUNT("BALL", "WALL")
230 LET DATA = SPRITECOLLISIONDATA("BALL", "WALL")
240 FOR I = 0 TO PAIRS - 1
250   LET BALL_COL = DATA[I * 4]     : REM cell column of BALL
260   LET BALL_ROW = DATA[I * 4 + 1] : REM cell row of BALL
270   LET WALL_COL = DATA[I * 4 + 2] : REM cell column of WALL
280   LET WALL_ROW = DATA[I * 4 + 3] : REM cell row of WALL
290 NEXT I
300 END

400 REM Ball left the wall
410 END
```

## Memory & Cleanup
When an AGEBasic script finishes executing or is stopped, the engine automatically clears all active sprites from the screen and memory. You do not need to manually remove them at the end of your program.