10  CALL DebugMode(1) 'activate the debug mode
15  CLS

20  LET lights = GetLights()
30  REM Check if lights are present
40  IF LEN(lights) > 0 THEN GOTO 100

50  REM No lights available, end program
60  PRINT 0, 0, "No lights in the room", 0
70  END

80  REM Lights are present, proceed to change color
100 LET numLights = CountMembers(lights, "|")
110 FOR i = 0 TO numLights - 1
120     LET light = GetMember(lights, i, "|")
140     IF NOT(SetLightColor(light, 0, 0, 1)) THEN GOTO 180
150 NEXT i

160 PRINT 0, 23, "Lights changed to blue", 0
170 GOTO 10010

180 PRINT 0, 23, "ERROR setting light color #" + STR(i) + " " + light
190 goto 10010

10010 PRINT 0, 24, "PRESS B to end", 1
10050 IF ControlActive("JOYPAD_B") THEN END
10060 GOTO 10050
