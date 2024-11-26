10 LET count = CabRoomCount()
20 LET cursor = 0
30 LET actualRoom = 0

100 SETCOLORSPACE "zx": CLS
110 FGCOLOR "blue" : PRINTLN "Cabinet database configuration" : RESETCOLOR
120 GOSUB 510 

200 IF ControlActive("JOYPAD_RIGHT") 
     THEN LET actualRoom = actualRoom + IIF(actualRoom < count - 1, 1, 0) : 
              GOSUB 510
     ELSE IF ControlActive("JOYPAD_LEFT") 
          THEN LET actualRoom = actualRoom + IIF(actualRoom > 0, -1, 0) : 
              GOSUB 510
     ELSE IF ControlActive("JOYPAD_DOWN") THEN GOTO 1000
     ELSE IF ControlActive("JOYPAD_Y") THEN END
210 GOSUB 500
220 GOTO 200

500 PRINT 0, 1, "#" + STR(actualRoom) + "  ", cursor, 0 : LET cursor = 1 - cursor : RETURN

510 PRINT 6, 1, RoomGetName(actualRoom),0,0
520 PRINT 0, 2, RoomGetDesc(actualRoom),0,0
530 SHOW
540 RETURN

1000 END