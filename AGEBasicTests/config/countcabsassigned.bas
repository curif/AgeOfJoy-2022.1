10 REM Program to calculate the sum of cabinets assigned in all rooms
20 LET totalRooms = RoomCount()
30 LET sumOfCabinets = 0

40 FOR roomIndex = 0 TO totalRooms - 1
50     LET roomNameFromIndex = RoomGetName(roomIndex)
60     LET assignedCabinetsInRoom = CabDBCountInRoom(roomNameFromIndex)
70     LET sumOfCabinets = sumOfCabinets + assignedCabinetsInRoom
80 NEXT roomIndex

90 PRINT 0,0, "Sum of Cabinets Assigned: " + STR(sumOfCabinets)
100 END
