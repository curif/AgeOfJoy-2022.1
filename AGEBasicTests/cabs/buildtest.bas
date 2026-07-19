2 REM --- buildtest.bas: edit description.yaml + reload the workshop test cabinet ---
3 REM Load a cabinet in the workshop first (drop a test.zip once), then run this
4 REM program (copy it to the AGEBasic folder, run from the Configuration Room or
5 REM the workshop debug console). Do NOT rename this to debug.bas: the workshop
6 REM debug console reruns debug.bas whenever test.log changes, and an unconditional
7 REM WorkshopReload() there would loop forever (reload -> test.log rewritten -> rerun).

10 LET cab = "test"
20 LET n = CabDbGetInfo(cab, "parts.count")

30 PRINT "cabinet: " + CabDbGetInfo(cab, "name")
40 PRINT "parts: " + n

50 LET yearBefore = CabDbGetInfo(cab, "year")
60 PRINT "year before: " + yearBefore

70 IF CabDbSetInfo(cab, "year", 1985) = 0 THEN PRINT "CABDBSETINFO failed, see console/log" : END

80 PRINT "year after: " + CabDbGetInfo(cab, "year")

90 REM nested write with auto-created color block, visible after reload
100 IF n = "" THEN GOTO 140
110 IF n < 1 THEN GOTO 140

120 PRINT "editing part: " + CabDbGetInfo(cab, "parts[0].name")
130 CALL CabDbSetInfo(cab, "parts[0].color.intensity", 2)

140 IF WorkshopReload() = 1 THEN PRINT "workshop reload requested..." ELSE PRINT "no active workshop test cabinet (run in the workshop)"

150 END
