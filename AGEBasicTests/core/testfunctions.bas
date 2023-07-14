5 LET ERROR = ""
10 LET a = ABS ( -1 ) + 2
20 if ( a != 3 ) THEN LET ERROR = "ABS FAILS"
30 IF ( ERROR == "" AND ABS ( 1 ) != 1 )  THEN LET ERROR = "ABS FAILS 2"
40 IF ( ERROR == "" AND MAX ( 1 , 2 ) != 2 )  THEN LET ERROR = "MAX FAILS"
50 IF ( ERROR == "" AND MAX ( 2 , 1 ) != 2 )  THEN LET ERROR = "MAX FAILS 2"
60 IF ( ERROR == "" AND MAX ( 1 , 1 ) != 1 )  THEN LET ERROR = "MAX FAILS 3"
70 LET THREE = MAX ( 2 , 1 ) + 1
80 IF ( ERROR == "" AND THREE != 3 )  THEN LET ERROR = "THREE FAIL"
90 LET TWO = MIN ( 2 , 1 ) + 1
100 IF ( ERROR == "" AND TWO != 2 )  THEN LET ERROR = "TWO FAIL"

500 let lowercase = LCASE ( "PEPE" )
510 let uppercase = UCASE ( "pepe" )
520 if (  ERROR == "" AND lowercase != "pepe" ) THEN LET ERROR = "LCASE"
530 if (  ERROR == "" AND uppercase != "PEPE" ) THEN LET ERROR = "UCASE"

1000 LET random = RND ( 0 , 1 )
1010 LET randabs = ABS ( RND ( 0 , 1 ) + RND ( 2 , 3 ) )
1020 IF ( ERROR == "" AND ( randabs < 0 OR randabs > 4 ) ) THEN LET ERROR = "randabs line 1010"
