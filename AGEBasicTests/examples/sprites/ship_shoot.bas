10 REM ============================================================
20 REM Ship Shoot Demo
30 REM SHIP1 is the stationary enemy at screen centre, 1/3 from top.
40 REM SHIP2 is yours: move left/right with D-Pad.
50 REM Press A to fire a missile upward. Hit SHIP1 for an explosion!
60 REM Press X to quit.
70 REM Required sprites: sprite_back.png, sprite.png, misil.png, explosion.png
80 REM ============================================================
90 CALL SETCPU(10)
100 BGCOLOR "black" : FGCOLOR "yellow" : CLS
110 PRINT 0, 0, "Loading sprites...                      "
120 SHOW

130 SPRITELOAD "BACK",  "sprite_back.png"
140 SPRITELOAD "SHIP1", "sprite.png"
150 SPRITELOAD "SHIP2", "sprite.png"
160 SPRITELOAD "MISIL", "misil.png"
170 SPRITELOAD "EXPLOS", "explosion.png"

180 IF SPRITESTATUS("BACK") = 0 || SPRITESTATUS("SHIP1") = 0 || SPRITESTATUS("SHIP2") = 0 || SPRITESTATUS("MISIL") = 0 || SPRITESTATUS("EXPLOS") = 0 THEN SLEEP 0.1 : GOTO 180

190 LET SW = DSCREENWIDTH()
200 LET SH = DSCREENHEIGHT()

210 REM Sprite size estimate for clamping (~24px)
220 LET SSIZE = 24
230 LET STEPMOV = 2

240 REM SHIP1: stationary at horizontal centre, 1/3 from top
250 LET X1 = INT(SW / 2) - INT(SSIZE / 2)
260 LET Y1 = INT(SH / 3)

270 REM SHIP2: bottom centre, player-controlled left/right only
280 LET X2 = INT(SW / 2) - INT(SSIZE / 2)
290 LET Y2 = SH - 40

300 REM Missile state
310 LET MX = 0
320 LET MY = 0
330 LET MACTIVE = 0

340 REM Explosion re-entry guard and hit counter
350 LET EXPLODING = 0
360 LET HITS = 0

370 FGCOLOR "yellow"
380 PRINT 0, 0, "HITS: 0   SHOOT:A  MOVE:L/R  QUIT:X  "

390 REM Background starts at y=16 so char row 0 stays visible for status
400 SPRITE "BACK",  0, 16, 0
410 SPRITE "SHIP1", X1, Y1, 2
420 SPRITE "SHIP2", X2, Y2, 2
425 SPRITE "EXPLOS", X1, Y1, 2 : SPRITEDISABLE "EXPLOS"
430 SHOW

440 ONEVENT ONCONTROL("JOYPAD_LEFT",  "held")    GOTO 1000
450 ONEVENT ONCONTROL("JOYPAD_RIGHT", "held")    GOTO 1050
460 ONEVENT ONCONTROL("JOYPAD_A",     "pressed") GOTO 1100
470 ONEVENT ONTIMER(0.05)                        GOTO 1200
480 ONEVENT ONSPRITECOLLISION("MISIL", "SHIP1")  GOTO 2000
490 ONEVENT ONCONTROL("JOYPAD_X",     "pressed") GOTO 4000
500 END

999  REM Move SHIP2 left
1000 LET X2 = X2 - STEPMOV
1010 IF X2 < 0 THEN LET X2 = 0
1020 SPRITE "SHIP2", X2, Y2, 2
1030 SHOW
1040 END

1049 REM Move SHIP2 right
1050 LET X2 = X2 + STEPMOV
1060 IF X2 > SW - SSIZE THEN LET X2 = SW - SSIZE
1070 SPRITE "SHIP2", X2, Y2, 2
1080 SHOW
1090 END

1099 REM Shoot missile — guard: only one missile at a time, no shooting during explosion
1100 IF MACTIVE = 1 THEN END
1110 IF EXPLODING = 1 THEN END
1120 LET MX = X2 + INT(SSIZE / 2) - 2
1130 LET MY = Y2 - SSIZE
1140 LET MACTIVE = 1
1150 SPRITE "MISIL", MX, MY, 3
1160 SHOW
1170 END

1199 REM ONTIMER(0.05): advance missile 4px upward each tick (~80px/sec)
1200 IF MACTIVE = 0 THEN END
1210 LET MY = MY - 4
1220 IF MY < 16 THEN SPRITEDISABLE "MISIL" : LET MACTIVE = 0 : SHOW : END
1230 SPRITE "MISIL", MX, MY, 3
1240 SHOW
1250 END

1999 REM ONSPRITECOLLISION: missile struck SHIP1 — play explosion
2000 IF EXPLODING = 1 THEN END
2010 LET EXPLODING = 1
2020 SPRITEDISABLE "MISIL" : LET MACTIVE = 0
2030 LET HITS = HITS + 1
2040 SPRITEDISABLE "SHIP1"
2050 SPRITEENABLE "EXPLOS"
2060 SHOW
2070 FGCOLOR "red"
2080 PRINT 0, 0, "HITS: " + STR(HITS) + " *** HIT! ***              "
2090 SHOW
2100 SLEEP 2
2110 SPRITEDISABLE "EXPLOS"
2120 SPRITEENABLE "SHIP1"
2130 FGCOLOR "yellow"
2140 PRINT 0, 0, "HITS: " + STR(HITS) + "   SHOOT:A  MOVE:L/R  QUIT:X"
2150 SHOW
2160 LET EXPLODING = 0
2170 END

3999 REM Exit
4000 SHUTDOWN
