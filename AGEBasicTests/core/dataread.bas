10 DATA "storage1", 10, 20, 30
20 DATA "storage1", 40, 50, 60
30 DATA "complex", "a", "a"+"b", 10+10

100 READ "storage1", a,b,c,d,e,f
110 IF a <> 10 THEN GOTO 500
120 IF b <> 20 THEN GOTO 500
130 IF c <> 30 THEN GOTO 500
140 IF d <> 40 THEN GOTO 500
150 IF e <> 50 THEN GOTO 500
160 IF f <> 60 THEN GOTO 500

200 READ "complex", c1, c2, c3
210 IF c1 <> "a" THEN GOTO 600
220 IF c2 <> "ab" THEN GOTO 600
230 IF c3 <> 20 THEN GOTO 600

250 RESTORE "complex", 1
260 READ "complex", x
270 if x <> "ab" THEN GOTO 600

300 END

500 LET ERROR = "error reading storage1"
510 END

600 LET ERROR = "error reading complex storage"
610 END