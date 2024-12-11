
10 SETCOLORSPACE "zx"
20 LET cabRoomPos = 0
40 LETS cabsCount, cabsDBCount = CabRoomCount(), CabDBCount()
50 LETS width, height = ScreenWidth(), ScreenHeight()
60 LET lineEmpty = width * " "
70 LETS dicMember, pos, cabToSearch, changed = 0, 0, "", 0
75 LETS dic, dicMatrix1, dicMatrix2, dicMatrix3, dicMatrix4 = 
     "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", "012345678","9ABCDEFGH","IJKLMNOPQ","RSTUVWXYZ"
80 LETS dicPos, funct, cursor = 0,0,0 '0 = select room pos, 1=select dic letter, 2=select cab, 3=assing
90 LETS cabList, matrixcol, matrixrow, matrixidx = "", 0, 0, 0
100 LET cpuspeed = GetCPU()
110 LET columnWidth = width / 4

200 ' init screen
210 CLS
220 FGCOLOR "blue" : PRINTLN "Room cabinet configuration " + RoomName() : RESETCOLOR
250 GOSUB 5200
260 GOSUB 5500
270 GOSUB 9000

2000 IF ControlActive("JOYPAD_RIGHT") THEN LET CabRoomPos = CabRoomPos + IIF(cabRoomPos < cabsCount - 1, 1, 0)
     ELSE IF ControlActive("JOYPAD_LEFT") THEN LET CabRoomPos = CabRoomPos + IIF(cabRoomPos > 0, -1, 0)
     ELSE IF ControlActive("JOYPAD_DOWN") THEN GOSUB 9050 : GOTO 2050
     ELSE IF ControlActive("JOYPAD_Y") THEN END
2010 GOSUB 5200
2020 REM SLEEP 0.05
2030 GOTO 2000

2050 LETS left, right = ControlActive("JOYPAD_LEFT"), ControlActive("JOYPAD_RIGHT") 
2053 IF right THEN LET dicPos = dicPos + IIF(dicPos < 36, 1, 0)
     ELSE IF left THEN LET dicPos = dicPos - IIF(dicPos > 0, 1, 0)
     ELSE IF ControlActive("JOYPAD_B") THEN LET cabToSearch = cabToSearch + SUBSTR(dic, dicPos, 1) : GOSUB 6000
     ELSE IF ControlActive("JOYPAD_A") THEN LET cabToSearch = "" : GOSUB 6000
     ELSE IF AND(ControlActive("JOYPAD_X"), cabToSearch != "")
          THEN LET cabToSearch = SUBSTR(cabToSearch, 0, LEN(cabToSearch) - 1) : GOSUB 6000
     ELSE IF ControlActive("JOYPAD_Y") THEN END

2055 LETS up, down = ControlActive("JOYPAD_UP"), ControlActive("JOYPAD_DOWN")
2058 IF AND(dicPos<9, up) THEN LET cursor = 1 : GOSUB 5550 : GOSUB 9000 : GOTO 2000
     ELSE IF AND(dicPos>26, down) THEN LET cursor = 1 : GOSUB 5550 : GOSUB 9100 : GOTO 2100
     ELSE IF up THEN LET dicPos = dicPos - 9
     ELSE IF down THEN LET dicPos = dicPos + 9

2060 IF OR(left, right, up, down) THEN GOSUB 5500
2070 LET cursor = 1 - cursor : GOSUB 5550
2080 REM SLEEP 0.1
2090 GOTO 2050

2100 IF ControlActive("JOYPAD_UP") THEN LET cursor = 1 : GOSUB 7000 : GOSUB 9050 : GOTO 2050
     ELSE IF ControlActive("JOYPAD_RIGHT") THEN GOSUB 7500
     ELSE IF ControlActive("JOYPAD_LEFT") THEN GOSUB 7600
     ELSE IF ControlActive("JOYPAD_Y") THEN END
     ELSE IF ControlActive("JOYPAD_B") THEN GOSUB 9500
2110 LET cursor = 1 - cursor: GOSUB 7000
2120 REM SLEEP 0.1
2130 GOTO 2100

5200 REM SHOW ACTUAL CABINET TO CHANGE
5210 LET lineCabNum = "CAB #" + STR(cabRoomPos) + ":"
5220 LET cursor = 1 - cursor
5230 PRINT 0,1, lineEmpty, 0, 0
5240 PRINT 0,1, lineCabNum, cursor, 0
5250 PRINT LEN(lineCabNum) + 1, 1, CabRoomGetName(cabRoomPos), 0, 0
5270 SHOW
5280 RETURN

5500 REM SHOW CAB TO SEARCH MATRIX
5510 FGCOLOR "WHITE" : BGCOLOR "magenta"
5520 PRINT 0,2, dicMatrix1, 0, 0 :
     PRINT 0,3, dicMatrix2, 0, 0 :
     PRINT 0,4, dicMatrix3, 0, 0 :
     PRINT 0,5, dicMatrix4, 0, 0
5530 RESETCOLOR
5540 RETURN

5549 REM Show matrix letter (cursor)
5550 LETS row, col = INT(dicPos/9), MOD(dicPos, 9)
5560 PRINT col, row+2, SUBSTR(dic, dicPos, 1), cursor
5570 RETURN

6000 REM SHOW CABS
6010 IF cabToSearch != "" THEN GOTO 6040
6020 GOSUB 6500
6030 PRINT 11, 3, " " * (width - 11), 0, 0
6034 SHOW
6038 RETURN

6040 CALL SetCPU(100)
6050 LET cabList = CabDBSearch(cabToSearch, "|")
6060 GOSUB 6500
6064 PRINT 11, 3, " " * (width - 19 - LEN(cabToSearch)), 0, 0
6065 PRINT 11, 3, "Search: " + cabToSearch , 0, 0
6070 LETS y, col = 7, 0
6080 FOR idx = 0 to CountMembers(cabList, "|") - 1
6090   PRINT col * columnWidth, y, SUBSTR(GetMember(cabList, idx, "|"), 0, columnWidth - 1) , 0, 0
6100   let col = col + 1
6110   IF col < 4 THEN GOTO 6150 
6120   LET col = 0
6130   LET y = y + 1
6140   IF y > height THEN GOTO 6160
6150 NEXT idx
6160 SHOW
6180 LETS matrixidx, matrixcol, matrixrow = 0,0,0
6190 LET cursor = 1 : GOSUB 7000
6200 CALL SetCPU(cpuspeed)
6210 RETURN

6500 REM Clean area
6510 FOR scr = 7 to height - 2
6520   PRINT 0, scr, lineEmpty, 0, 0
6530 NEXT scr
6540 RETURN

7000 REM show selected cabinet in matrix
7020 PRINT matrixcol * columnWidth, 7 + matrixrow,  
     SUBSTR(GetMember(cabList, matrixidx, "|"), 0, columnWidth - 1), cursor
7030 RETURN

7500 REM next cabinet in matrix
7510 LET cursor = 0 : GOSUB 7000
7520 LET members = CountMembers(cabList, "|")
7530 IF matrixidx = members - 1 THEN RETURN
7540 LETS matrixidx, matrixcol = matrixidx + 1, matrixcol + 1
7550 IF matrixcol > 3 THEN LETS matrixcol, matrixrow = 0, matrixrow + 1
7560 IF matrixrow > members / 4 THEN LET matrixrow = members / 4
7580 RETURN

7600 REM previous cabinet in matrix
7610 LET cursor = 0 : GOSUB 7000
7620 LETS matrixidx, matrixcol = matrixidx - 1, matrixcol - 1
7630 IF matrixidx < 0 THEN LETS matrixidx, matrixcol, matrixrow = 0,0,0 : 
                           LET cursor = 1 : GOSUB 7000
                           RETURN 
7650 IF matrixcol < 0 THEN LETS matrixcol, matrixrow = 3, matrixrow - 1
7660 IF matrixrow < 0 THEN LET matrixrow = 0
7670 RETURN


9000 REM change to cabinet position selection
9010 LET funct = 0
9020 LET helpMessage = " \235 \236 CHANGE CABINET - \234 NEXT, Y:END"
9030 GOTO 9160
9050 REM change to letter selection
9060 LET funct = 1
9070 LET helpMessage = " \235 \236 \233 \234 B:ADD, A:DEL, X:CLEAR, Y:END"
9080 GOTO 9160
9100 REM change to cabinet matrix select
9110 LET funct = 2
9120 LET helpMessage = " \235 \236, B:ASSIGN, \233 PREV, Y:END"
9130 GOTO 9160

9160 REM print help line
9170 FGCOLOR "WHITE" : BGCOLOR "BLUE"
9190 PRINT 0, height - 1, lineEmpty, 0, 0
9200 PRINT 0, height - 1, helpMessage, 0, 0
9210 RESETCOLOR
9220 SHOW
9230 RETURN

9500 REM REPLACE CABINET
9510 GOSUB 6500
9515 LET helpMessage = "Press B to assign, X to cancel"
9516 GOSUB 9160
9520 PRINT 3, 10, "REPLACE CABINET", 0, 0
9530 PRINT 3, 12, "CABINET POSITION #" + STR(cabRoomPos), 0, 0
9540 PRINT 3, 13, "BY CABINET:", 0, 0
9550 PRINT 3, 14, GetMember(cabList, matrixidx, "|"), 0, 0
9560 FGCOLOR "WHITE" : BGCOLOR "red"
9580 PRINT 3, 16, " B: REPLACE ", 0, 0
9590 RESETCOLOR
9600 PRINT 15, 16, " X: CANCEL", 0, 0
9610 SHOW
 
9700 IF ControlActive("JOYPAD_X") THEN GOTO 9800
     ELSE IF ControlActive("JOYPAD_B") THEN GOTO 9900
     ELSE IF ControlActive("JOYPAD_Y") THEN END
9710 SLEEP 0.1
9720 GOTO 9700

9800 GOSUB 6060
9810 GOSUB 9000
9820 GOSUB 5200
9830 RETURN

9900 REM ASSIGN CABINET
9910 CALL CabDBAssign(RoomName(), cabRoomPos, GetMember(cabList, matrixidx, "|"))
9920 CALL CabRoomReplace(cabRoomPos, GetMember(cabList, matrixidx, "|"))
9930 GOSUB 6500
9940 PRINT 10, 10, "* ASSIGNED *", 1, 0
9950 SHOW
9960 REM CALL CabDBSave()
9970 SLEEP 1
9980 GOTO 9800
