5 call DebugMode(1)
10 REM Replace each cabinet in all rooms with a random cabinet
20 LET totalRooms = RoomCount()
30 LET totalCabinetsDB = CabDbCount()
35 LET cont = 0
37 LET playerRoom = RoomName()

40 REM Loop through each room
50 FOR roomIndex = 0 TO totalRooms - 1
60     LET currentRoomName = RoomGetName(roomIndex)
65     LET countReplaced = 0
70     gosub 500

80     REM there is no way to know how many cabinets can hold a room, so it assumes 60 max.
81     rem obviously it will assing more than the room capacity.
90     FOR cabinetIndex = 0 TO 59
95         print 20,3, "#" + str(cabinetIndex)

100        REM is a cabinet assigned? we need one to proceed to change it.
110        if CabDBGetAssigned(currentRoomName, cabinetIndex) = "" then goto 170

120        LET randomIndex = INT(RND(1, totalCabinetsDB)) - 1
121        LET newCabinetName = CabDbGetName(randomIndex)
122        if newCabinetName = "" then goto 170

130        rem change the database by assigning the cabinet to the old position
140        if CabDBAssign(currentRoomName, cabinetIndex, newCabinetName) = 0 then goto 990

145        LET countReplaced = countReplaced + 1
146        print 0, 4 + MOD(countReplaced, 10), "#" + str(cabinetIndex) + " by DB #" + str(randomIndex) + ": " + str(newCabinetName) + "        "

149        rem change in current Room if it is the same to see it inmediatly
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
540 print 0,3, "room:" + currentRoomName, 1, 0
550 show
560 return

990 print 0,19, "assignment error", 1
995 print 0,20, "room:" + currentRoomName + "#" + str(cabinetIndex) + "cab:" + newCabinetName

10000 print 0, 23, "replaced: " + str(cont)
10010 print 0, 24, "PRESS B to end", 1
10050 IF ControlActive("JOYPAD_B") THEN END
10060 goto 10050
