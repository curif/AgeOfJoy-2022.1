5 LET ERROR = ""

10 LET a = "Hello"
20 LET b = a + " " + "World"

25 IF (ERROR="" AND  b!="Hello World") THEN LET ERROR = " hello world error - line 20"

30 LET c = a OR b
40 IF (ERROR="" AND c!=1) THEN LET ERROR = "a or b ERROR line 30"

200 LET lenA = LEN (a)
210 IF (ERROR="" AND lenA!=5) THEN LET ERROR = "LEN(a) ERROR"

500 let lowercase = LCASE ("PEPE")
510 let uppercase = UCASE ("pepe")
520 if (ERROR="" AND lowercase!="pepe") THEN LET ERROR = "LCASE"
530 if (ERROR="" AND uppercase!="PEPE") THEN LET ERROR = "UCASE"

2600 LET trimmedB = TRIM (b)
2700 IF (ERROR="" AND trimmedB!="Hello World") THEN LET ERROR = "TRIM(b) ERROR"

2800 LET substrB = SUBSTR (b , 6 , 5)
2900 IF (ERROR="" AND substrB!="World") THEN LET ERROR = "SUBSTR(b) ERROR"

5000 IF (ERROR!="") THEN END

10000 REM No errors

10001 END
