1 'PROGRAM TO READ THE test.log FILE THAT CONTAINS THE RESULT
2 'OF THE CABINET ANALISIS AND PRINT ONLY
3 'THE ERRORS FOUND ON SCREEN 

10 CLS
20 FGCOLOR "GREEN"
30 PRINT 0,0, ("\98" * 16) + " DEBUG " + ("\98" * 16), 1
45 PRINTLN ""
50 RESETCOLOR

60 LET testcab = CombinePath(DebugPath(), "test.log")
70 IF NOT(FileExists(testcab)) THEN GOTO 2000

80 LET file = FileOpen(testcab, "R")
90 IF AND(type(file) = "NUMBER", file = -1) THEN GOTO 1000

100 IF FileEOF(file) THEN GOTO 300
110 LET line = FileRead(file)
120 IF StringMatch(line, "ERROR") THEN GOTO 200
130 IF StringMatch(line, "Exception") THEN GOTO 200
140 GOTO 100

200 PRINTLN line
210 GOTO 100

300 CALL FileClose(file)
310 PRINTLN "END -------------------", 1
320 END

1000 PRINT 0, 10, "test.log not found or open error"
1010 END

2000 PRINT 0, 10, "test.log not found or not created yet"
2010 END