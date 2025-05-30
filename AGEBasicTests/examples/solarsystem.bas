10 REM AGEBasic Solar System Simulation
20 REM Demonstrates drawing circles, setting colors, and animating movement
30 REM using trigonometry and timed updates.

40 REM --- 1. Constants and Initial Values ---
50 LET PI = 3.14159265   ' A good approximation for PI
60 LET TWOPI = PI * 2    ' Full circle in radians (360 degrees)

70 REM Calculate the center of the screen in pixels
80 LET SCX = DScreenWidth() / 2
90 LET SCY = DScreenHeight() / 2

100 REM Sun Properties
110 LET SUN_RADIUS = 20
120 LET ORANGE = ARRAY(255, 165, 0)

130 REM Planet Properties
140 LET PLANET_ORBIT_RADIUS = 80
150 LET PLANET_RADIUS = 10
160 LET BLUE = GETCOLOR("blue")
170 LET PLANET_ANGLE = 0 ' Initial angle for planet (radians)
180 LET PLANET_ANGLE_INC = TWOPI / 60 ' Planet completes orbit in 60 seconds

190 REM Moon Properties
200 LET MOON_ORBIT_RADIUS = 20
210 LET MOON_RADIUS = 4
220 LET GREY = ARRAY(150, 150, 150)
230 LET MOON_ANGLE = 0 ' Initial angle for moon (radians)
240 LET MOON_ANGLE_INC = TWOPI / 15 ' Moon completes orbit in 15 seconds (relative to planet)

250 REM Orbit Line Color (thin lines)
260 LET WHITE = GETCOLOR("white")

270 REM Simulation Loop Parameters
280 LET TOTAL_STEPS = 120 ' Run for 120 seconds (2 minutes)

290 REM --- 2. Main Drawing Loop ---
300 SETCOLORSPACE "msx" ' Set a classic retro color palette (e.g., MSX)

320 FOR CURRENT_STEP = 0 TO TOTAL_STEPS
  
340   REM Clear everything for redraw in each step (DRAW_FLAG = 0 for CLS is implicit)
350   CLS
360   PRINT 0,0, "STEP:" + STR(CURRENT_STEP)
370   REM --- Draw Orbits (not filled, DRAW_FLAG = 0 to defer update) ---
380   REM Planet Orbit around Sun
390   DCIRCLE ARRAY(SCX, SCY), PLANET_ORBIT_RADIUS, WHITE, 0
400
410   REM --- Calculate Planet Position ---
420   LET PLANET_PX = SCX + PLANET_ORBIT_RADIUS * COS(PLANET_ANGLE)
430   LET PLANET_PY = SCY + PLANET_ORBIT_RADIUS * SIN(PLANET_ANGLE)
440
450   REM Moon Orbit around Planet
460   DCIRCLE ARRAY(PLANET_PX, PLANET_PY), MOON_ORBIT_RADIUS, WHITE, 0
470
480   REM --- Calculate Moon Position (relative to Planet) ---
490   LET MOON_PX = PLANET_PX + MOON_ORBIT_RADIUS * COS(MOON_ANGLE)
500   LET MOON_PY = PLANET_PY + MOON_ORBIT_RADIUS * SIN(MOON_ANGLE)
510
520   REM --- Draw Celestial Bodies (filled, DRAW_FLAG = 0 to defer update) ---
530   REM Draw Sun
540   DCIRCLE ARRAY(SCX, SCY), SUN_RADIUS, ORANGE, 1, ORANGE, 0
550
560   REM Draw Planet
570   DCIRCLE ARRAY(PLANET_PX, PLANET_PY), PLANET_RADIUS, BLUE, 1, BLUE, 0
580
590   REM Draw Moon
600   DCIRCLE ARRAY(MOON_PX, MOON_PY), MOON_RADIUS, GREY, 1, GREY, 0
610
620   REM --- Update Screen and Angles ---
630   SHOW ' Display all drawn elements at once
640
650   REM Update angles for the next frame
660   LET PLANET_ANGLE = PLANET_ANGLE + PLANET_ANGLE_INC
670   LET MOON_ANGLE = MOON_ANGLE + MOON_ANGLE_INC
680
690   REM Keep angles within 0 to 2*PI (optional, prevents large numbers)
700   IF PLANET_ANGLE >= TWOPI THEN LET PLANET_ANGLE = PLANET_ANGLE - TWOPI
710   IF MOON_ANGLE >= TWOPI THEN LET MOON_ANGLE = MOON_ANGLE - TWOPI
720
730   SLEEP 1 
740
750 NEXT CURRENT_STEP

760 CLS ' Clear screen at the very end
770 SHOW
780 END