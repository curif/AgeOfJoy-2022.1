10 REM Replace each cabinet in all rooms with a random cabinet
20 LET totalRooms = RoomCount()
30 LET totalCabinetsDB = CabDbCount()
35 LET cont = 0

40 REM Loop through each room
50 FOR roomIndex = 0 TO totalRooms - 1
60     LET currentRoomName = RoomGetName(roomIndex)
70     LET totalCabinetsRoom = CabDBCountInRoom(currentRoomName)
72     if (totalCabinetsRoom = 0) then goto 170

75     gosub 500

80     REM Loop through each cabinet in the current room
90     FOR cabinetIndex = 0 TO totalCabinetsRoom - 1

100        LET randomIndex = INT(RND(1, totalCabinetsDB)) - 1
110        LET newCabinetName = CabDbGetName(randomIndex)

115        print 0, 4+cabinetIndex, "#" + str(cabinetIndex) + " by DB #" +str(randomIndex) + ": " + str(newCabinetName) + "        "

120        REM Replace the cabinet in the current room with the random cabinet
130        if not(CabDBDelete(currentRoomName, cabinetIndex)) then goto 1000
140        if not(CabDBAdd(currentRoomName, cabinetIndex, newCabinetName)) then goto 1000

150        let cont=cont+1
160     NEXT cabinetIndex

170 NEXT roomIndex

180 CALL CabDBSave()
190 goto 10000

500 REM show main info
510 CLS
520 PRINT 0,1, "Rooms: " + str(totalRooms), 0, 0
530 print 0,2, "Cabinets in DB:" + str(totalCabinetsDB), 0, 0
540 Print 0,3, "room:" + currentRoomName + " cabs:" + str(totalCabinetsRoom), 1, 0
550 show
560 return

1000 CLS
1010 print 0,0, "ERROR"
1020 print 0,1, "room:" + currentRoomName
1030 print 0,2, "cabinet #" + str(randomIndex)
1040 print 0,3, "new cabinet: " + newCabinetName
1050 print 0,4, "------"

10000 print 0, 23, "replaced: " + str(cont)
10010 PRINT 0, 24, "PRESS B to end", 1
10050 IF(CONTROLACTIVE("JOYPAD_B")) THEN END
10060 goto 10050
