10 REM List Lights
15 CLS
20 LET lights = GetLights()
30 LET count = CountMembers(lights, "|")
40 PRINT 0, 0, "Room ID"
50 PRINT 10, 0, "Light Name"
60 PRINT 30, 0, "Intensity"

70 FOR i = 0 TO count - 1
80   LET lightInfo = GetMember(lights, i, "|")
90   LET roomID = GetMember(lightInfo, 0, ":")
100  LET lightName = GetMember(lightInfo, 1, ":")
110  LET intensity = GetLightIntensity(lightInfo)

120  PRINT 0, i + 2, roomID
130  PRINT 10, i + 2, lightName
140  PRINT 30, i + 2, intensity
150 NEXT i

160 SHOW

10010 print 0, 24, "PRESS B to end", 1
10050 IF ControlActive("JOYPAD_B") THEN END
10060 goto 10050
