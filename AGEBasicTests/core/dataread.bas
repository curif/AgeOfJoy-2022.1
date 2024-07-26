10 DATA "storage1", 10, 20, 30
20 DATA "storage1", 40, 50, &FF
30 DATA "complex", "a", "a"+"b", 10+10

100 READ "storage1", a,b,c,d,e,f
110 IF a <> 10 || b <> 20 || c <> 30 || 
       d <> 40 || e <> 50 || f <> 255 THEN GOTO 500

200 READ "complex", c1, c2, c3
210 RESTORE "complex", 1
220 READ "complex", x
230 IF c1 <> "a" || c2 <> "ab" || c3 <> 20 || x <> "ab" THEN GOTO 600

250 LET MESSAGE = "TEST OK - NO ERRORS"
260 END

500 LET ERROR = "error reading storage1"
510 END

600 LET ERROR = "error reading complex storage"
610 END
