
5 GOTO 35
10 REM esto es una prueba
15 REM LET a = 10 + 2
16 REM LET b = a + 1
17 REM LET c = 1 + 5 * 3
18 REM LET fabio = ( 5 * 3 ) + b - c
30 LET x=(3*2)-1
35 LET goto=99 + 1
40 GOTO goto
50 LET pepe=1

100 LET IN100 = 1

110 LET shouldbeone = 1 = 1
120 LET shouldbeonetoo = 3*2 >= 5
125 LET AND=1 AND 1
130 LET OR=1 OR 1
140 LET ANDcero = 1 AND 0
150 LET orcero = 0 OR 0
160 LET orceroaritm = 1 AND 0 OR 0

170 LET conditrue1 = 2 > 1
175 IF ( conditrue1 <> 1 ) THEN GOTO 1000

190 LET conditrue2 = 1 < 2 
195 IF (conditrue2 <> 1) THEN GOTO 1000

200 LET conditrue3 = 1 <= 1 
205 IF (conditrue3 <> 1)  THEN GOTO 1000

210 LET conditrue4 = 1 >= 1 
215 IF ( conditrue4 <> 1  ) THEN GOTO 1000

220 LET conditrue5 = 1 != 0 
225 IF ( conditrue5 <> 1  ) THEN GOTO 1000

230 LET conditrue6 = 1 <> 0 
235 IF ( conditrue6 <> 1  ) THEN GOTO 1000

240 IF ( conditrue6 <> 1 OR conditrue5 <> 1 ) THEN GOTO 1000

900 goto 1500

1000 LET error = 1
1010 END

1500 LET error = 0
2000 END