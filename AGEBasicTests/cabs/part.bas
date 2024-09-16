5 call DebugMode(1)
10 LET partList = CabPartsList("|")
15 CALL LOG(partList)
18 CALL LOG("member:" + str(CountMembers(partList, "|")) + " part count: " + str(CabPartsCount()))
20 FOR idx = 0 to CountMembers(partList, "|")
30    LET member = GetMember(partList, idx, "|")
40    LET partname = CabPartsName(idx)
50    CALL LOG(str(idx) + ": " + member + "  -  " + partname)
60    IF member <> partname THEN GOTO 1000
70 NEXT idx
80 CALL LOG("part.bas verification members OK!")
90 END

1000 CALL LOGERROR(str(idx) + " member error: member " + member + " is not " + partname)