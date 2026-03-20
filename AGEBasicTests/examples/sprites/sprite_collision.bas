10 REM ============================================================
20 REM Sprite Collision Demo
30 REM Two balls move diagonally, bounce off walls and off each
40 REM other. ONSPRITECOLLISION reverses both balls on contact.
50 REM ============================================================
60 CALL SETCPU(10)
70 CLS
80 PRINT 0, 0, "Loading sprites..."
90 SHOW

100 SPRITELOAD "BACK", "sprite_back.png"
110 SPRITELOAD "BALL1", "sprite.png"
120 SPRITELOAD "BALL2", "sprite.png"

130 IF SPRITESTATUS("BACK") = 0 || SPRITESTATUS("BALL1") = 0 || SPRITESTATUS("BALL2") = 0 THEN SLEEP 0.1 : GOTO 130

140 REM Screen pixel dimensions for boundary checks
150 LET SW = DSCREENWIDTH()
160 LET SH = DSCREENHEIGHT()

170 REM Initial positions: BALL1 top-left moving right-down,
180 REM                    BALL2 bottom-right moving left-up
190 LET X1 = 10         : LET Y1 = 10
200 LET DX1 = 4         : LET DY1 = 3
210 LET X2 = SW - 40    : LET Y2 = SH - 40
220 LET DX2 = -3        : LET DY2 = -2

230 REM COLLIDING flag: 0 = not colliding, used to avoid re-triggering
240 LET COLLIDING = 0

245 REM Frame counter: auto-exit after 600 ticks (~30 seconds at 0.05s/tick)
246 LET FRAMES = 0
247 LET MAXFRAMES = 600

250 REM Draw background at Z=0, balls at Z=2
260 SPRITE "BACK", 0, 0, 0
270 SPRITE "BALL1", X1, Y1, 2
280 SPRITE "BALL2", X2, Y2, 2
290 SHOW

300 REM Register events and hand off to the event loop
310 ONEVENT ONTIMER(0.05) GOTO 1000
320 ONEVENT ONSPRITECOLLISION("BALL1", "BALL2") GOTO 2000
330 ONEVENT ONSPRITECOLLISIONEND("BALL1", "BALL2") GOTO 3000
335 ONEVENT ONCONTROL("JOYPAD_START_0", "pressed") GOTO 4000
340 END

REM ============================================================
REM ONTIMER handler: move both balls and bounce off screen edges
REM ============================================================
1000 LET X1 = X1 + DX1 : LET Y1 = Y1 + DY1
1010 LET X2 = X2 + DX2 : LET Y2 = Y2 + DY2

1020 REM Bounce BALL1 off left/right walls
1030 IF X1 < 0 THEN LET X1 = 0 : LET DX1 = ABS(DX1)
1040 IF X1 > SW - 20 THEN LET X1 = SW - 20 : LET DX1 = -ABS(DX1)

1050 REM Bounce BALL1 off top/bottom walls
1060 IF Y1 < 0 THEN LET Y1 = 0 : LET DY1 = ABS(DY1)
1070 IF Y1 > SH - 20 THEN LET Y1 = SH - 20 : LET DY1 = -ABS(DY1)

1080 REM Bounce BALL2 off left/right walls
1090 IF X2 < 0 THEN LET X2 = 0 : LET DX2 = ABS(DX2)
1100 IF X2 > SW - 20 THEN LET X2 = SW - 20 : LET DX2 = -ABS(DX2)

1110 REM Bounce BALL2 off top/bottom walls
1120 IF Y2 < 0 THEN LET Y2 = 0 : LET DY2 = ABS(DY2)
1130 IF Y2 > SH - 20 THEN LET Y2 = SH - 20 : LET DY2 = -ABS(DY2)

1140 SPRITE "BALL1", X1, Y1, 2
1150 SPRITE "BALL2", X2, Y2, 2
1160 SHOW

1170 REM Auto-exit after MAXFRAMES ticks
1180 LET FRAMES = FRAMES + 1
1190 IF FRAMES >= MAXFRAMES THEN SHUTDOWN
1200 END

REM ============================================================
REM ONSPRITECOLLISION: balls begin overlapping — reverse both
REM ============================================================
2000 LET DX1 = -DX1 : LET DY1 = -DY1
2010 LET DX2 = -DX2 : LET DY2 = -DY2
2020 LET COLLIDING = 1
2030 END

REM ============================================================
REM ONSPRITECOLLISIONEND: balls separated
REM ============================================================
3000 LET COLLIDING = 0
3010 END

REM ============================================================
REM ONCONTROL: player pressed Start — exit immediately
REM ============================================================
4000 SHUTDOWN
