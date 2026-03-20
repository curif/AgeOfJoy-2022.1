10 REM ============================================================
20 REM Spaceship Collision Demo
30 REM SHIP1 sits in the centre (stationary).
40 REM SHIP2 is yours: move with D-Pad or WASD.
50 REM Line 0 shows status. Press Start or Esc to quit.
60 REM ============================================================
70 CALL SETCPU(10)
80 BGCOLOR "black" : FGCOLOR "yellow" : CLS
90 PRINT 0, 0, "Loading sprites...                      "
100 SHOW

110 SPRITELOAD "BACK",  "sprite_back.png"
120 SPRITELOAD "SHIP1", "sprite.png"
130 SPRITELOAD "SHIP2", "sprite.png"

140 IF SPRITESTATUS("BACK") = 0 || SPRITESTATUS("SHIP1") = 0 || SPRITESTATUS("SHIP2") = 0 THEN SLEEP 0.1 : GOTO 140

150 LET SW = DSCREENWIDTH()
160 LET SH = DSCREENHEIGHT()

170 REM Sprite size estimate for boundary clamping (~24 px for sprite.png)
180 LET SSIZE = 24
190 LET STEPMOV  = 4

200 REM SHIP1: fixed at screen centre
210 LET X1 = INT(SW / 2) - INT(SSIZE / 2)
220 LET Y1 = INT(SH / 2) - INT(SSIZE / 2)

230 REM SHIP2: starts lower-left, controlled by player
240 LET X2 = 20
250 LET Y2 = SH - 40

260 FGCOLOR "yellow"
270 PRINT 0, 0, "MOVE: DPAD/WASD        START/ESC: QUIT  "
290 SHOW

295 SLEEP 30

300 REM Background starts at y=8 so char row 0 stays visible for status text
310 SPRITE "BACK",  0, 16, 0
320 SPRITE "SHIP1", X1, Y1, 2
330 SPRITE "SHIP2", X2, Y2, 2


340 ONEVENT ONTIMER(0.05) GOTO 1000
350 ONEVENT ONSPRITECOLLISION("SHIP1", "SHIP2") GOTO 2000
360 ONEVENT ONSPRITECOLLISIONEND("SHIP1", "SHIP2") GOTO 3000
370 ONEVENT ONCONTROL("JOYPAD_X", "pressed") GOTO 4000
380 END

REM ============================================================
REM ONTIMER: poll controls, move SHIP2, clamp, redraw
REM ============================================================
1000 IF CONTROLACTIVE("JOYPAD_UP")    = 1 THEN LET Y2 = Y2 - STEPMOV
1010 IF CONTROLACTIVE("JOYPAD_DOWN")  = 1 THEN LET Y2 = Y2 + STEPMOV
1020 IF CONTROLACTIVE("JOYPAD_LEFT")  = 1 THEN LET X2 = X2 - STEPMOV
1030 IF CONTROLACTIVE("JOYPAD_RIGHT") = 1 THEN LET X2 = X2 + STEPMOV

1040 REM Clamp SHIP2 to visible area (y >= 8 to stay below status line)
1050 IF X2 < 0         THEN LET X2 = 0
1060 IF Y2 < 16        THEN LET Y2 = 16
1070 IF X2 > SW - SSIZE THEN LET X2 = SW - SSIZE
1080 IF Y2 > SH - SSIZE THEN LET Y2 = SH - SSIZE

1090 SPRITE "SHIP2", X2, Y2, 2
1100 SHOW
1110 END

REM ============================================================
REM ONSPRITECOLLISION: ships overlap (pixel-precise cell check)
REM ============================================================
2000 LET PAIRS = SPRITECOLLISIONCOUNT("SHIP1", "SHIP2")
2010 FGCOLOR "red"
2020 PRINT 0, 0, "  *** COLLISION DETECTED ***            "
2030 SHOW
2040 END

REM ============================================================
REM ONSPRITECOLLISIONEND: ships separated
REM ============================================================
3000 FGCOLOR "yellow"
3010 PRINT 0, 0, "MOVE: DPAD/WASD        START/ESC: QUIT  "
3020 SHOW
3030 END

REM ============================================================
REM Exit
REM ============================================================
4000 SHUTDOWN
