
10 SETCOLORSPACE "zx"
20 LET cabRoomPos = 0
30 DIM dicMatrix[4]
40 LETS cabsCount, cabsDBCount = CabRoomCount(), CabDBCount()
50 LETS width, height = ScreenWidth(), ScreenHeight()
60 LET lineEmpty = (width - 1) * " "
70 LETS dicMember, pos, cabToSearch, changed = 0, 0, "", 0
75 LETS dic, dicMatrix[0], dicMatrix[1], dicMatrix[2], dicMatrix[3] = 
     "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", "012345678","9ABCDEFGH","IJKLMNOPQ","RSTUVWXYZ"
80 LETS dicPos, funct, cursor, cabListLen = 0,0,0,0 '0 = select room pos, 1=select dic letter, 2=select cab, 3=assing
90 LETS matrixidx, scrollTop = 0, 0
100 LET cpuspeed = GetCPU()
110 LET columnWidth = width / 3
115 LET listRows = height - 8

200 ' init screen
210 CLS
220 FGCOLOR "blue" : PRINTLN "Room cabinet configuration " + RoomName() : RESETCOLOR
250 GOSUB 5200
260 GOSUB 5500
270 GOSUB 9000

280 REM register event handlers
290 ONEVENT ONCONTROL("JOYPAD_RIGHT", "pressed") GOTO 2010
300 ONEVENT ONCONTROL("JOYPAD_LEFT", "pressed") GOTO 2210
310 ONEVENT ONCONTROL("JOYPAD_UP", "pressed") GOTO 2410
320 ONEVENT ONCONTROL("JOYPAD_DOWN", "pressed") GOTO 2610
330 ONEVENT ONCONTROL("JOYPAD_Y", "pressed") GOTO 2810
340 ONEVENT ONCONTROL("JOYPAD_B", "pressed") GOTO 2910
350 ONEVENT ONCONTROL("JOYPAD_A", "pressed") GOTO 3110
360 ONEVENT ONCONTROL("JOYPAD_X", "pressed") GOTO 3210
370 ONEVENT ONTIMER(0.1) GOTO 3410
380 END

2000 REM JOYPAD_RIGHT
2010 IF funct = 0 THEN LET CabRoomPos = CabRoomPos + IIF(cabRoomPos < cabsCount - 1, 1, 0) : GOSUB 5200
     ELSE IF funct = 1 THEN LET dicPos = dicPos + IIF(dicPos < 36, 1, 0) : GOSUB 5500 : LET cursor = 1 : GOSUB 5550
2020 END

2200 REM JOYPAD_LEFT
2210 IF funct = 0 THEN LET CabRoomPos = CabRoomPos + IIF(cabRoomPos > 0, -1, 0) : GOSUB 5200
     ELSE IF funct = 1 THEN LET dicPos = dicPos - IIF(dicPos > 0, 1, 0) : GOSUB 5500 : LET cursor = 1 : GOSUB 5550
     ELSE IF funct = 2 THEN LET cursor = 1 : GOSUB 7000 : GOSUB 9050 : LET cursor = 1 : GOSUB 5550
2220 END

2400 REM JOYPAD_UP
2410 IF funct = 1 && dicPos < 9 THEN GOSUB 9000 : GOSUB 5200
     ELSE IF funct = 1 THEN LET dicPos = dicPos - 9 : GOSUB 5500 : LET cursor = 1 : GOSUB 5550
     ELSE IF funct = 2 && matrixidx <= 0 THEN LET cursor = 1 : GOSUB 7000 : GOSUB 9050 : LET cursor = 1 : GOSUB 5550
     ELSE IF funct = 2 THEN GOSUB 7600
2420 END

2600 REM JOYPAD_DOWN
2610 IF funct = 0 THEN GOSUB 9050 : GOSUB 5500 : LET cursor = 1 : GOSUB 5550
     ELSE IF funct = 1 && dicPos > 26 && cabListLen > 0 THEN LET cursor = 1 : GOSUB 5550 : GOSUB 9100 : LET cursor = 1 : GOSUB 7000
     ELSE IF funct = 1 && dicPos <= 26 THEN LET dicPos = dicPos + 9 : GOSUB 5500 : LET cursor = 1 : GOSUB 5550
     ELSE IF funct = 2 THEN GOSUB 7500
2620 END

2800 REM JOYPAD_Y
2810 SHUTDOWN

2900 REM JOYPAD_B
2910 IF funct = 1 THEN LET cabToSearch = cabToSearch + SUBSTR(dic, dicPos, 1) : GOSUB 6000
     ELSE IF funct = 2 THEN GOSUB 9500 : LET funct = 3
     ELSE IF funct = 3 THEN GOSUB 9900 : GOSUB 9800
2920 END

3100 REM JOYPAD_A
3110 IF funct = 1 THEN LET cabToSearch = "" : GOSUB 6000
3120 END

3200 REM JOYPAD_X
3210 IF funct = 1 && cabToSearch != "" THEN LET cabToSearch = SUBSTR(cabToSearch, 0, LEN(cabToSearch) - 1) : GOSUB 6000
     ELSE IF funct = 3 THEN GOSUB 9800
3220 END

3400 REM ONTIMER blink handler
3410 IF funct = 0 THEN GOSUB 5200
     ELSE IF funct = 1 THEN LET cursor = 1 - cursor : GOSUB 5550
     ELSE IF funct = 2 THEN LET cursor = 1 - cursor : GOSUB 7000
3420 END

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
5520 PRINT 0,2, dicMatrix[0], 0, 0 :
     PRINT 0,3, dicMatrix[1], 0, 0 :
     PRINT 0,4, dicMatrix[2], 0, 0 :
     PRINT 0,5, dicMatrix[3], 0, 0
5530 RESETCOLOR
5540 RETURN

5549 REM Show matrix letter (cursor)
5550 LETS row, col = INT(dicPos/9), MOD(dicPos, 9)
5560 PRINT col, row+2, SUBSTR(dic, dicPos, 1), cursor
5570 RETURN

6000 REM SHOW CABS
6010 IF cabToSearch != "" THEN GOTO 6040
6015 LET cabListLen = 0
6020 GOSUB 6500
6030 PRINT 11, 3, " " * (width - 11), 0, 0
6034 SHOW
6038 RETURN

6040 CALL SetCPU(100)
6050 LET cabList = CabDBSearchArray(cabToSearch)
6055 LET cabListLen = LEN(cabList)
6060 GOSUB 6500
6064 PRINT 11, 3, " " * (width - 19 - LEN(cabToSearch)), 0, 0
6065 PRINT 11, 3, "Search: " + cabToSearch , 0, 0
6070 LETS matrixidx, scrollTop = 0, 0
6080 GOSUB 6700
6190 LET cursor = 1 : GOSUB 7000
6200 CALL SetCPU(cpuspeed)
6210 RETURN

6500 REM Clean area
6510 FOR scr = 7 to height - 2
6520   PRINT 0, scr, lineEmpty, 0, 0
6530 NEXT scr
6540 RETURN

6700 REM Render visible cab list (scrolling)
6710 GOSUB 6500
6720 FOR idx = 0 to MIN(listRows, cabListLen - scrollTop) - 1
6730   PRINT 0, 7 + idx, SUBSTR(cabList[scrollTop + idx], 0, columnWidth - 1), 0, 0
6740 NEXT idx
6750 SHOW
6760 RETURN

7000 REM show selected cabinet in list
7020 PRINT 0, 7 + (matrixidx - scrollTop), SUBSTR(cabList[matrixidx], 0, columnWidth - 1), cursor
7030 RETURN

7500 REM next cabinet in list (scroll down)
7510 LET cursor = 0 : GOSUB 7000
7520 LET members = LEN(cabList)
7530 IF matrixidx >= members - 1 THEN RETURN
7540 LET matrixidx = matrixidx + 1
7550 IF matrixidx < scrollTop + listRows THEN RETURN
7560 LET scrollTop = scrollTop + 1
7570 GOSUB 6700
7580 RETURN

7600 REM previous cabinet in list (scroll up)
7610 LET cursor = 0 : GOSUB 7000
7620 LET matrixidx = matrixidx - 1
7630 IF matrixidx >= scrollTop THEN RETURN
7640 LET scrollTop = scrollTop - 1
7650 GOSUB 6700
7660 RETURN


9000 REM change to cabinet position selection
9010 LET funct = 0
9020 LET helpMessage = " \235 \236 CHANGE CABINET - \234 NEXT, Y:END"
9030 GOTO 9160
9050 REM change to letter selection
9060 LET funct = 1
9070 LET helpMessage = " \235 \236 \233 \234 B:ADD, A:DEL, X:CLEAR, Y:END"
9080 GOTO 9160
9100 REM change to cabinet list select
9110 LET funct = 2
9120 LET helpMessage = " \233 \234, B:ASSIGN, \235 PREV, Y:END"
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
9550 PRINT 3, 14, cabList[matrixidx], 0, 0
9560 FGCOLOR "WHITE" : BGCOLOR "red"
9580 PRINT 3, 16, " B: REPLACE ", 0, 0
9590 RESETCOLOR
9600 PRINT 15, 16, " X: CANCEL", 0, 0
9610 SHOW
9615 RETURN

9800 GOSUB 6060
9810 GOSUB 9000
9820 GOSUB 5200
9830 RETURN

9900 REM ASSIGN CABINET
9910 CALL CabDBAssign(RoomName(), cabRoomPos, cabList[matrixidx])
9920 CALL CabRoomReplace(cabRoomPos, cabList[matrixidx])
9930 GOSUB 6500
9940 PRINT 10, 10, "* ASSIGNED *", 1, 0
9950 SHOW
9960 CALL CabDBSave()
9970 SLEEP 1
9980 RETURN
