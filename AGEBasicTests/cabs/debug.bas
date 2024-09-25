10 LET path = DebugPath()
20 LET testcab = CombinePath(path, "test.log")
21 LET total = ""
30 LET file = FileOpen(testcab, "R")
40 IF AND(type(file) = "NUMBER", file = -1) THEN GOTO 1000

50 LET outputPath = CombinePath(path, "filteredTest.log") 
60 LET filteredFile = FileOpen(outputPath, "W")
70 IF AND(type(filteredFile) = "NUMBER", filteredFile = -1) THEN GOTO 1100

100 IF FileEOF(file) THEN GOTO 300
110 LET line = FileRead(file)
120 IF StringMatch(line, "ERROR") THEN GOTO 200
130 IF StringMatch(line, "Exception") THEN GOTO 200
131 let total = total + line
140 GOTO 100

200 CALL FileWrite(filteredFile, line)
210 GOTO 100

300 LET EOF = "true"
305 CALL FileClose(file)
310 CALL FileClose(filteredFile)
320 END

1000 LET ERROR = "test.log not found or open error"
1010 END

1100 LET ERROR = "cant open filteredTest.log to append"
1110 END