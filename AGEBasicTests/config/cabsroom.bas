5 CLS
10 LET count = CabRoomCount()
20 PRINT 0,0, "there are " +str(count) + " room's cabinets " + ROOMNAME()
30 PRINT 0,1, "and " + STR(CabDBCount()) + " cabinets in the database" ' show cabs
40 if (count = 0) then goto 10020 

100 let idx = 0
110 print 0,2+idx, "#" + str(idx) + ": " + CabRoomGetName(idx)
120 let idx = idx + 1
130 if (idx>=count) then goto 10020 ' last cab
140 goto 110

10010 ' wait for user to press B to end
10020 PRINT 0, 24, "PRESS B to end", 1, 0
10030 SHOW
10050 IF(ControlActive("JOYPAD_B")) THEN end
10060 goto 10050
