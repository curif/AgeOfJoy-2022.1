5 LET ERROR=""
10 LET a=ABS(-1) + 2
20 if (a != 3) THEN LET ERROR="ABS FAILS"
30 IF AND(ERROR="" , ABS(1) != 1)  THEN LET ERROR="ABS FAILS 2"
40 IF AND(ERROR="", MAX(1, 2) != 2)  THEN LET ERROR="MAX FAILS"
50 IF AND(ERROR="", MAX(2, 1) != 2)  THEN LET ERROR="MAX FAILS 2"
60 IF AND(ERROR="", MAX(1, 1) != 1)  THEN LET ERROR="MAX FAILS 3"
70 LET THREE=MAX(2, 1) + 1
80 IF AND(ERROR="", THREE != 3)  THEN LET ERROR="THREE FAIL"
90 LET TWO=MIN(2, 1) + 1
100 IF AND(ERROR="", TWO != 2)  THEN LET ERROR="TWO FAIL"


300 LET sinVal=SIN (30 * 3.1416 / 180)
310 REM Compare sinVal with the expected value (calculate using a calculator)
320 IF OR(sinVal < 0.499 , sinVal > 0.501) THEN LET ERROR="SIN(30) ERROR"

1000 LET random=RND (0, 1)
1010 LET randabs=ABS(RND (0, 1) + RND (2, 3))
1020 IF AND(ERROR="" , OR(randabs < 0, randabs > 4)) THEN LET ERROR="randabs line 1010"

1030 LET integer=Int(10.5)
1040 IF AND(ERROR="", integer != 10) then LET ERROR="INT fails"

1050 LET double = Val("10.5")
1060 IF double != 10.5 THEN LET ERROR = "VAL fails"