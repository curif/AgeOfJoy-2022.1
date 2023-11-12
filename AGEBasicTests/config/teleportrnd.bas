10 REM Program to teleport the player to a random room
20 LET totalRooms = RoomCount()
30 LET low = 0
40 LET high = totalRooms

50 REM Generate a random number between low and high-1
60 LET randomRoomIndex = INT(RND(low, high))

70 REM Get the name of the random room
80 LET randomRoomName = RoomGetName(randomRoomIndex)

90 REM Teleport the player to the random room
100 CALL RoomTeleport(randomRoomName)

110 END
