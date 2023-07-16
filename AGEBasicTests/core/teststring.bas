5 LET ERROR = ""

10 LET a = "Hello"
20 LET b = a + " " + "World"

25 IF ( b != "Hello World" ) THEN LET ERROR = " hello world error - line 20"

30 LET c = a OR b
40 IF ( c != 1 ) THEN LET ERROR = "a or b ERROR line 30"

200 LET lenA = LEN ( a )
210 IF ( lenA != 5 ) THEN LET ERROR = "LEN(a) ERROR"

500 let lowercase = LCASE ( "PEPE" )
510 let uppercase = UCASE ( "pepe" )
520 if (  ERROR == "" AND lowercase != "pepe" ) THEN LET ERROR = "LCASE"
530 if (  ERROR == "" AND uppercase != "PEPE" ) THEN LET ERROR = "UCASE"

260 LET trimmedB = TRIM ( b )
270 IF ( trimmedB != "Hello World" ) THEN LET ERROR = "TRIM(b) ERROR"

280 LET substrB = SUBSTR ( b , 6 , 5 )
290 IF ( substrB != "World" ) THEN LET ERROR = "SUBSTR(b) ERROR"

50 IF ( ERROR != "" ) THEN END

100 REM No errors

1000 END
