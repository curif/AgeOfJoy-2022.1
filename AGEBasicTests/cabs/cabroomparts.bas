5 call DebugMode(1)
10 REM Test CABROOMPARTS* - manipulating a cabinet OTHER than the one running this script.
20 REM Edit TARGETPOS to a position, in the current room, different from this cabinet's own position.
30 LET TARGETPOS = 1
40 LET COUNT = CABROOMPARTSCOUNT(TARGETPOS)
50 CALL LOG("target position " + STR(TARGETPOS) + " parts count: " + STR(COUNT))
60 IF COUNT = 0 THEN GOTO 1000
70 LET PARTNAME = CABROOMPARTSNAME(TARGETPOS, 0)
80 CALL LOG("target part 0 name: " + PARTNAME)
90 LET BEFORE = CABROOMPARTSGETROTATION(TARGETPOS, 0, "Y")
100 CALL LOG("rotation Y before: " + STR(BEFORE))
110 CALL CABROOMPARTSSETROTATION(TARGETPOS, 0, "Y", 45)
120 LET AFTER = CABROOMPARTSGETROTATION(TARGETPOS, 0, "Y")
130 CALL LOG("rotation Y after: " + STR(AFTER))
140 IF AFTER <> 45 THEN GOTO 2000
150 CALL LOG("cabroomparts.bas verification OK!")
160 END

1000 CALL LOGERROR("target position " + STR(TARGETPOS) + " has no cabinet loaded - pick another position")
1010 END

2000 CALL LOGERROR("rotation did not stick: expected 45, got " + STR(AFTER))
2010 END
