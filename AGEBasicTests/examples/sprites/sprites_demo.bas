10 REM Sprite Functionality Demo
20 CLS
30 PRINT 0, 0, "Initializing Sprite Demo..."
40 SHOW
50 SLEEP 1
60 
70 REM 1. Load the sprite from a PNG file
80 REM The 'name' (HERO) will be used to reference this texture in cache
90 PRINT 0, 2, "Loading sprite.png..."
100 SPRITELOAD "HERO", "sprite.png"
105 SPRITELOAD "BACKGROUND", "sprite_back.png"
110 
120 REM Wait for the sprite to be loaded (as it is asynchronous)
130 IF SPRITESTATUS("HERO") = 0 || SPRITESTATUS("BACKGROUND") = 0 THEN SLEEP 0.1 : GOTO 130
140 
150 PRINT 0, 3, "Sprites loaded and ready!"
160 SHOW
170 SLEEP 1
180 REM Draw BACKGROUND
185 SPRITE "BACKGROUND", 0, 0, 0
190 REM 2. Move the sprite across the screen
200 REM SPRITE name, x, y, z
210 REM x,y are pixel coordinates. z is the layer (higher = front).
220 FOR I = 0 TO 200 
250   SPRITE "HERO", I, 80, 5
260   SHOW
270   SLEEP 0.05
280 NEXT I
290 
300 REM 3. Demonstrate Z-layering (if you had a second sprite)
310 PRINT 0, 4, "Sprite reached destination."
320 SHOW
330 SLEEP 1
340 
350 REM 4. Remove the sprite
360 PRINT 0, 6, "Removing sprite..."
370 SPRITEREMOVE "HERO"
380 SHOW
390 SLEEP 1
400 
410 PRINT 0, 8, "Demo Finished."
420 SHOW
430 END