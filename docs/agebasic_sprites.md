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

## Memory & Cleanup
When an AGEBasic script finishes executing or is stopped, the engine automatically clears all active sprites from the screen and memory. You do not need to manually remove them at the end of your program.