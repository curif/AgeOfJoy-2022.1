10 LET path = CombinePath(AGEBasicPath(), "test.txt")
20 LET file = FileOpen(path, "W")
30 CALL FileWrite(file, "this is a test")
40 CALL FileClose(file)
50 IF NOT(FileExists(path)) THEN LET ERROR = "File missing after creation:" + path : END
60 IF FileDelete(path) THEN END
70 LET ERROR = "cannot delete the created file:" + path
