10 LET partList = CabPartsList("|")
20 FOR idx = CountMembers(partList, "|") to CabPartsCount()
30    LET member = GetMember(partList, idx, "|")
40    LET partname = CabPartsName(idx) 
50    CALL LOG(member +" " + partname)
60    IF member <> partname THEN GOTO 1000
70 NEXT idx
80 CALL LOG("part.bas verification members OK!")
90 END

1000 CALL LOGERROR(str(idx) + "member error:" + member + " is not " + partname)