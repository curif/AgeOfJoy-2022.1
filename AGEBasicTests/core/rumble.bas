10 rem rumble controls on trigger

20 cls
30 print 0, 0, "rumble test",0,0
40 print 0, 1, "trigger left and right",0,0
50 print 0, 2, "B button to exit",0,0

100 IF ControlActive("JOYPAD_B") THEN END
110 IF ControlActive("JOYPAD_L") THEN GOSUB 500
120 IF ControlActive("JOYPAD_R") THEN GOSUB 600
130 SHOW
140 GOTO 100

500 print 10, 5, "LEFT", 1, 0
610 print 10, 10, "     ", 1, 0
510 call ControlRumble("JOYPAD_LEFT_RUMBLE", 0.5, 0.5)
520 return

600 print 10, 10, "RIGHT", 1, 0
610 print 10, 5, "     ", 1, 0
620 call ControlRumble("JOYPAD_RIGHT_RUMBLE", 0.5, 0.5)
630 return
