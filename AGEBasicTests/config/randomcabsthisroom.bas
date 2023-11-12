10 REM replace cabinets in this room by a random selected one.

20 CLS
25 print 0,0, "random replace cabinets"

30 LET totalCabinetsDB = CabDbCount()
40 LET playerRoom = RoomName()
50 LET totalCabinetsRoom = CabRoomCount()

90 FOR cabinetIndex = 0 TO totalCabinetsRoom - 1
100   LET randomIndex = INT(RND(1, totalCabinetsDB)) - 1
110   LET newCabinetName = CabDbGetName(randomIndex)

112   if newCabinetName = "" then goto 170

115   print 0, 4+cabinetIndex, "#" + str(cabinetIndex) + " by DB #" +str(randomIndex) + ": " + str(newCabinetName) + "        "

120   REM Replace the cabinet in the current room with the random cabinet

129   REM is a cabinet assigned? we need one to proceed to change it.
130   if CabDBGetAssigned(playerRoom, cabinetIndex) = "" then goto 170

139   rem change the database by assining the cabinet on the old position
140   CALL CabDBAssign(playerRoom, cabinetIndex, newCabinetName)
150   call CabRoomReplace(cabinetIndex, newCabinetName)

170 NEXT cabinetIndex

190 CALL CabDBSave()

10010 print 0, 24, "PRESS B to end", 1
10050 IF ControlActive("JOYPAD_B") THEN END
10060 goto 10050
