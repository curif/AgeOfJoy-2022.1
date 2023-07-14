5 LET ERROR = ""

10 LET a = "Hello"
20 LET b = a + " " + "World"

25 IF ( b != "Hello World" ) THEN LET ERROR = " hello world error - line 20"

30 LET c = a OR b
40 IF ( c != 1 ) THEN LET ERROR = "a or b ERROR line 30"

50 IF ( ERROR != "" ) THEN END

100 REM No errors

1000 END
