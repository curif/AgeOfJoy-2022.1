5 CALL DEBUGMODE(1)
10 CALL LOG("first log")
20 CALL LOGERROR("this is an error")
30 CALL LOGWARNING("this is a warning")
40 CALL ASSERT(1 = 1, "ASSERT not shown")
50 CALL ASSERT(1 = 2, "assert shown")
60 CALL DEBUGMODE(0)
70 CALL LOG("LOG not shown")
