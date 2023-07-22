10 REM Replace each cabinet in all rooms with a random cabinet
20 LET totalCabinets = CabDbCount()

30 REM Loop through each room
40 LET totalRooms = RoomCount()
50 LET roomIndex = 0

60 REM Start of the loop through rooms
70 IF (roomIndex < totalRooms) THEN GOTO 100
80 END

100 REM Get the name of the current room
110 LET currentRoomName = RoomGetName(roomIndex)

120 REM Get the number of cabinets in the current room
130 LET cabRoomCount = CabDbCountInRoom(currentRoomName)

140 REM Loop through each cabinet in the current room
150 LET cabinetIndex = 0

160 REM Start of the loop through cabinets
170 IF (cabinetIndex < cabRoomCount) THEN GOTO 400
180 GOTO 500

400 REM Get a random index to select a cabinet from the database
410 LET randomIndex = RND(0, totalCabinets - 1)

420 REM Get the name of the randomly selected cabinet from the database
430 LET randomCabName = CabDbGetName(randomIndex)

440 REM Replace the cabinet in the current room with the randomly selected cabinet
450 CALL CabDBReplace(currentRoomName, cabinetIndex, randomCabName)

460 REM Move to the next cabinet in the current room
470 LET cabinetIndex = cabinetIndex + 1
480 GOTO 160

500 REM Move to the next room
510 LET roomIndex = roomIndex + 1
520 GOTO 60
