10 REM Internet Archive search: list every pack tagged with the subject ageofjoy-cabinets.
20 LET SEARCH = IASEARCH("")
30 CLS
40 PRINT 0, 0, "Searching archive.org..."
50 SLEEP 0.5
60 IF IASTATUS(SEARCH) = "LOADING" THEN GOTO 50
70 IF IASTATUS(SEARCH) != "READY" THEN GOTO 400

100 LET PACKS = IAPACKS(SEARCH)
110 IF LEN(PACKS) = 0 THEN GOTO 410
120 CLS
130 PRINT 0, 0, "packs found: " + STR(LEN(PACKS))
140 SHOW
150 REM load every pack, then list title and cabinet count
160 FOR I = 0 TO LEN(PACKS) - 1
170   LET P = IALOAD(PACKS[I])
180 NEXT I
190 FOR I = 0 TO LEN(PACKS) - 1
200   SLEEP 0.5
210   IF IASTATUS(PACKS[I]) = "LOADING" THEN GOTO 200
220   IF IASTATUS(PACKS[I]) = "READY" THEN PRINT 0, 2 + I, PACKS[I] + " " + IAINFO(PACKS[I], "title") + " cabinets: " + STR(IAINFO(PACKS[I], "count")) ELSE PRINT 0, 2 + I, PACKS[I] + " " + IASTATUS(PACKS[I])
230 NEXT I
240 SHOW
250 PRINT 0, 24, "PRESS B to end", 1, 0
260 SHOW
270 IF ControlActive("JOYPAD_B") THEN END
280 SLEEP 0.1
290 GOTO 270

400 PRINT 0, 2, "search failed: " + IASTATUS(SEARCH)
405 SHOW
406 GOTO 250
410 PRINT 0, 2, "no packs tagged ageofjoy-cabinets"
415 SHOW
416 GOTO 250
