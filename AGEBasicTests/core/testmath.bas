5 LET ERROR=""
10 LET a=ABS(-1) + 2
20 if (a != 3) THEN LET ERROR="ABS FAILS"
30 IF (ERROR="" AND ABS(1) != 1)  THEN LET ERROR="ABS FAILS 2"
40 IF (ERROR="" AND MAX(1, 2) != 2)  THEN LET ERROR="MAX FAILS"
50 IF (ERROR="" AND MAX(2, 1) != 2)  THEN LET ERROR="MAX FAILS 2"
60 IF (ERROR="" AND MAX(1, 1) != 1)  THEN LET ERROR="MAX FAILS 3"
70 LET THREE=MAX(2, 1) + 1
80 IF (ERROR="" AND THREE != 3)  THEN LET ERROR="THREE FAIL"
90 LET TWO=MIN(2, 1) + 1
100 IF (ERROR="" AND TWO != 2)  THEN LET ERROR="TWO FAIL"


300 LET sinVal=SIN (30 * 3.1416 / 180)
310 REM Compare sinVal with the expected value (calculate using a calculator)
320 IF (sinVal < 0.499 OR sinVal > 0.501) THEN LET ERROR="SIN(30) ERROR"

1000 LET random=RND (0, 1)
1010 LET randabs=ABS(RND (0, 1) + RND (2, 3))
1020 IF (ERROR="" AND (randabs < 0 OR randabs > 4)) THEN LET ERROR="randabs line 1010"

1030 LET integer=Int(10.5)
1040 IF ERROR="" AND integer != 10 then LET ERROR="INT fails"
