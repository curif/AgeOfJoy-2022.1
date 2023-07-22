10 REM Replace each cabinet in all rooms with a random cabinet
20 LET totalRooms = RoomCount()
30 LET totalCabinetsDB = CabDbCount()

40 REM Loop through each room
50 FOR roomIndex = 0 TO totalRooms - 1
60     LET currentRoomName = RoomGetName(roomIndex)
70     LET totalCabinetsRoom = CabRoomCount()

80     REM Loop through each cabinet in the current room
90     FOR cabinetIndex = 0 TO totalCabinetsRoom - 1
100        LET randomIndex = INT(RND(1, totalCabinetsDB)) - 1
110        LET newCabinetName = CabDbGetName(randomIndex)

120        REM Replace the cabinet in the current room with the random cabinet
130        CALL CabDBReplace(currentRoomName, cabinetIndex, newCabinetName)
140     NEXT cabinetIndex

150 NEXT roomIndex

160 END
