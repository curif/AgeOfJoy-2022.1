10 REM Initialize and read music directory content

20 LET musicDirectory = MusicPath()
30 LET fileList = getFiles(musicDirectory, ":", 0)
40 LET fileTotal = CountMembers(fileList, ":")
50 LET filesPerPage = 10
60 LET currentPage = 0
70 LET totalPages = fileTotal / filesPerPage
80 IF MOD(fileTotal, filesPerPage) > 0 THEN LET totalPages = INT(totalPages + 1)
85 LETS width, height = screenWidth(), ScreenHeight()
90 LET emptyline = " " * (width - 1)
100 LETS currentIndex, windowChanged, rowStart, screenRowDebug = 0, 0, 5, height - 1

110 LET SelectedList = ""
120 LET option = -1
125 LETS volume, minDB, maxDB = AudioMusicGetVolume(), -80, 0

130 CLS
140 PRINT 0,0, ("\64" * 15) + " JUKEBOX " + ("\64" * 16), 1, 0
150 PRINT 0,2, "B=SELECT Y=OPTIONS A=EXIT", 0,0
160 PRINT 0,3, "up/down/left/right to move",0,0
170 PRINT 0,4, "-" * width, 0,0 
180 PRINT 0,rowStart + filesPerPage, "-" * width, 0,0 

190 GOSUB 4500 'print menu
195 GOSUB 7530 'volume indicator
196 GOSUB 2000 'debug line
200 GOSUB 1000 'print window

230 REM Input loop for navigation and selection
250 IF ControlActive("JOYPAD_RIGHT") THEN GOSUB 700
    ELSE IF ControlActive("JOYPAD_LEFT")  THEN GOSUB 600 
    ELSE IF ControlActive("JOYPAD_DOWN")  THEN GOSUB 800
    ELSE IF ControlActive("JOYPAD_UP")  THEN GOSUB 900
    ELSE IF ControlActive("JOYPAD_B") THEN GOSUB 1600  ' select/unselect option
    ELSE IF ControlActive("JOYPAD_Y") THEN GOSUB 5000 ' main menu
    ELSE IF ControlActive("JOYPAD_A") THEN END  ' exit jukebox
    
320 ' Refresh display if page or selection changes
330 IF windowChanged THEN GOTO 200
350 SLEEP 0.1
360 GOTO 250  ' Repeat input loop

600 ' handling page left
610 IF currentPage <= 0 THEN RETURN
620 LET currentPage = currentPage - 1
630 LETS currentIndex, windowChanged = currentPage * filesPerPage, 1
640 GOSUB 2000 'debug line
650 RETURN

700 ' handling page right
710 IF currentPage >= totalPages - 1 THEN RETURN
720 LET currentPage = currentPage + 1
730 LETS currentIndex, windowChanged = currentPage * filesPerPage, 1
740 GOSUB 2000 'debug line
750 RETURN 

800 ' handling line down
810 IF currentIndex >= endIndex THEN RETURN
820 LETS currentIndex, windowChanged = currentIndex + 1, 1
830 RETURN

900 'handling line up
910 if currentIndex <= startIndex THEN RETURN
920 LETS currentIndex, windowChanged = currentIndex - 1, 1
930 RETURN

1000 ' Display file window
1020 LET startIndex = currentPage * filesPerPage
1030 LET endIndex = MIN(startIndex + filesPerPage - 1, fileTotal - 1)
1050 LETS row, maxrow = rowStart, rowStart + filesPerPage

1060 ' Display loop for file names ======
1070 FOR i = startIndex TO endIndex
1080     LET fileName = GetMember(fileList, i, ":")
1090     PRINT 0, row, IIF(MusicExist(fileName), "\117 ", "  ") + 'warning: the filename could start with a number
                       SUBSTR(fileName + emptyline, 0, width - 2), (i = currentIndex), 0
1100     LET row = row + 1
1110 NEXT i
1120 LET windowChanged = 0
1130 IF row < maxrow THEN GOSUB 1500
1140 SHOW
1150 RETURN

1500 ' fill window available space
1510 for fill = row to maxrow
1520   print 0, fill, emptyline, 0, 0
1530 next fill
1540 RETURN

1600 ' Subroutine to select/unselect a music file (add/remove from jukebox queue)
1610 LET LastSelected = GetMember(fileList, currentIndex, ":")
1620 LET fileExistsInPlayer = MusicExist(LastSelected) 
1630 IF fileExistsInPlayer THEN CALL MusicRemove(LastSelected) ELSE CALL MusicAdd(LastSelected)
1640 PRINT 0, rowStart + currentIndex - startIndex, IIF(NOT(fileExistsInPlayer), "\117", " ")
1650 RETURN

2000 'show window position
2020 print 0,screenRowDebug, "page:" + str(currentPage) +
                             " totp:" + str(totalPages) +
                             " totf:" + str(fileTotal) +
                             " sel:" + str(MusicCount())+ "  ", 0, 0
2060 RETURN

4500 ' draw menu
4510 PRINT 0, 16, "options:", 0, 0
4520 PRINT 2, 17, "clear selections", option = 0, 0
4530 PRINT 2, 18, "select all in folder", option = 1, 0
4540 PRINT 2, 19, "go to first page", option = 2, 0
4550 PRINT 2, 20, "go to last page", option = 3, 0
4560 PRINT 2, 21, "EXIT JUKEBOX", option = 4, 0

4570 PRINT 24, 17, IIF(MusicLoopStatus(), "LOOP (OFF)", "LOOP (ON) "), option = 5, 0
4580 PRINT 24, 18, "RESET", option = 6, 0

4590 GOSUB 7500 'Draw volume
4600 LET redrawoptions = 0
4610 SHOW
4620 RETURN

5000 ' start menu -------------------
5010 LETS option, redrawoptions = 0, 0
5020 GOSUB 4500
5030 SLEEP 0.3 'to avoid repeat a button

5040 IF ControlActive("JOYPAD_DOWN") THEN LETS option, redrawoptions = MOD(option + 1, 7), 1 
     ELSE IF ControlActive("JOYPAD_UP") THEN LETS option, redrawoptions = MOD(option - 1 + 7, 7), 1
     ELSE IF ControlActive("JOYPAD_B") THEN GOSUB 5500 ' process selected menu option
     ELSE IF ControlActive("JOYPAD_Y") THEN GOTO 5100 'exit menu
     ELSE IF ControlActive("JOYPAD_RIGHT") THEN GOSUB 8000 ' vol up
     ELSE IF ControlActive("JOYPAD_LEFT")  THEN GOSUB 8500 ' vol down
5050 IF redrawoptions THEN GOSUB 4520 
5055 SLEEP 0.1
5060 GOTO 5040

5100 'exit menu
5110 LET option = -1
5120 GOSUB 4520
5130 RETURN

5500 IF option = 0 THEN GOSUB 6300
     ELSE IF option = 1 THEN GOSUB 6200
     ELSE IF option = 2 THEN GOSUB 6000
     ELSE IF option = 3 THEN GOSUB 6100
     ELSE IF option = 4 THEN END
     ELSE IF option = 5 THEN GOSUB 6500
     ELSE IF option = 6 THEN GOSUB 7000
5510 RETURN

6000 ' goto first page
6020 LET currentPage = 0
6030 LET currentIndex = 0
6040 GOSUB 2000 'show window position
6050 GOSUB 1000 'redraw window file
6060 RETURN

6100 ' go to last page
6110 LET currentPage = totalPages - 1
6120 LET currentIndex = currentPage * filesPerPage
6130 GOSUB 2000 'show window position
6140 GOSUB 1000 'redraw
6150 RETURN

6200 'add all files
6210 CALL MusicAddList(fileList, ":")
6220 LET currentPage = 0
6230 LET currentIndex = 0
6240 GOSUB 2000 'show window position
6250 GOSUB 1000 'redraw file window
6260 RETURN

6300 'clear all files
6310 CALL MusicClear()
6320 LET currentPage = 0
6330 LET currentIndex = 0
6340 GOSUB 2000 'show window position
6350 GOSUB 1000 'redraw
6360 RETURN

6500 'loop
6510 CALL MusicLoop(NOT(MusicLoopStatus()))
6520 LET redrawoptions = 1
6530 RETURN

7000 'loop
7010 CALL MusicReset()
7020 RETURN

7500 REM Draw Volume Indicator (volume is in dB) ---------------------------------------
7510 if volume = AudioMusicGetVolume() THEN RETURN ELSE LET volume = AudioMusicGetVolume()
7520 IF volume < minDB THEN LET volume = minDB
     ELSE IF volume > maxDB THEN LET volume = maxDB
7530 LET volumePercent = INT((volume - minDB) / (maxDB - minDB) * 100)
7540 LET volumeRatio = INT((volumePercent / 100) * (width - 5))
7550 LET indicator = "\126" * volumeRatio
7560 PRINT 0, height - 2, "vol " + indicator + " ", 0, 0
7570 RETURN

8000 ' volume up
8010 IF volume >= maxDB THEN RETURN
8020 LET volume = volume + 1
8030 CALL AudioMusicSetVolume(volume)
8040 GOSUB 7530
8050 SHOW
8060 RETURN

8500 ' volume down
8510 IF volume <= minDB THEN RETURN
8520 LET volume = volume - 1
8530 CALL AudioMusicSetVolume(volume)
8540 GOSUB 7530
8550 SHOW
8560 RETURN
