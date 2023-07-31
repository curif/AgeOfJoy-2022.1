10 CLS

20 LET x = 0
30 LET y = 0
35 LET inverted = 0

40 PRINT x, y, "test: "+STR(y), inverted
50 LET y = y+1
55 LET inverted = MOD(y, 2)

60 IF(y < 10) THEN GOTO 40

70 PRINT 0, y, "END"