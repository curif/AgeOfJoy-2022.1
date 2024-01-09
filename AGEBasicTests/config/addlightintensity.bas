10 REM Increase Light Intensity
20 LET lights = GetLights()
30 LET count = CountMembers(lights, "|")

40 FOR i = 0 TO count - 1
50   LET lightInfo = GetMember(lights, i, "|")
60   LET intensity = GetLightIntensity(lightInfo)

70   REM Increase intensity by 30%
80   LET newIntensity = intensity + (intensity * 0.3)

90   REM Ensure the intensity doesn't exceed 10 (maximum intensity)
100  IF newIntensity > 10 THEN LET newIntensity = 10

110  REM Set the new intensity
120  call SetLightIntensity(lightInfo, newIntensity)
130 NEXT i

140 SHOW
150 END
