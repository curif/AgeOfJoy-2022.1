10 rem player test
20 let original = PlayerGetHeight()

100 cls

110 print 0, 0, "PRESS B to change your height"
120 print 0, 1, "and pres B again"
130 print 0, 2, "to back"

210 IF ControlActive("JOYPAD_B") THEN goto 500
220 goto 210

500 call PlayerSetHeight(original + 0.25)
510 IF ControlActive("JOYPAD_B") THEN goto 600
520 goto 510

600 call PlayerSetHeight(original)
610 cls
