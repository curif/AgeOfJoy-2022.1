10 REM Replace each cabinet in all rooms with a random cabinet
20 LET totalRooms = RoomCount()
30 LET totalCabinetsDB = CabDbCount()
35 LET cont = 0
37 LET playerRoom = RoomName()

40 REM Loop through each room
50 FOR roomIndex = 0 TO totalRooms - 1
60     LET currentRoomName = RoomGetName(roomIndex)
70     LET totalCabinetsRoom = CabDBCountInRoom(currentRoomName)
72     if (totalCabinetsRoom = 0) then goto 180

75     gosub 500

80     REM Loop through each cabinet in the current room
90     FOR cabinetIndex = 0 TO totalCabinetsRoom - 1

100        LET randomIndex = INT(RND(1, totalCabinetsDB)) - 1
110        LET newCabinetName = CabDbGetName(randomIndex)

112        if newCabinetName = "" then goto 170

115        print 0, 4+cabinetIndex, "#" + str(cabinetIndex) + " by DB #" +str(randomIndex) + ": " + str(newCabinetName) + "        "

120        REM Replace the cabinet in the current room with the random cabinet

129        REM is a cabinet assigned? we need one to proceed to change it.
130        if CabDBGetAssigned(currentRoomName, cabinetIndex) = "" then goto 170

139        rem change the database by assining the cabinet on the old position
140        CALL CabDBAssign(currentRoomName, cabinetIndex, newCabinetName)

149        rem change in current playerRoom if it is the same to see it inmediatly
150        if playerRoom = currentRoomName then call CabRoomReplace(cabinetIndex, newCabinetName)

160        let cont=cont+1

170    NEXT cabinetIndex
180 NEXT roomIndex

190 CALL CabDBSave()
200 goto 10000

500 REM show main info
510 CLS
520 print 0,1, "Rooms: " + str(totalRooms), 0, 0
530 print 0,2, "Cabinets in DB:" + str(totalCabinetsDB), 0, 0
540 print 0,3, "room:" + currentRoomName + " cabs:" + str(totalCabinetsRoom), 1, 0
550 show
560 return

10000 print 0, 23, "replaced: " + str(cont)
10010 print 0, 24, "PRESS B to end", 1
10050 IF ControlActive("JOYPAD_B") THEN END
10060 goto 10050
