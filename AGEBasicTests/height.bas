1 REM C:\Users\curif\desarr\ageofjoy.0.5.1\AgeOfJoy-2022.1\AGEBasicTests
10 cls
20 PRINT 0,0, "PLAYER HEIGHT TOOL", 1
30 PRINT 0,1, "Height:"

40 LET H = PlayerGetHeight()
50 PRINT 8,1, str(H)

60 IF ControlActive("KEYBOARD_W") THEN LET H = H + 0.1 : GOTO 100
70 IF AND(H > 0.1, ControlActive("KEYBOARD_s")) THEN LET H = H - 0.1  : GOTO 100
80 SLEEP 0.3
90 GOTO 40

100 CALL PlayerSetHeight(H)
110 GOTO 80
