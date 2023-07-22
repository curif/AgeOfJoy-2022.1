5 LET ERROR = "" 
10 LETS a, b, c = 10, 20, 30
20 if a!=10 OR b!=20 OR c!=30 THEN LET ERROR = "LETS assignment error" + STR (a) + "/" + STR (b) + "/" + STR (c) 
30 LETS a = 50
40 if a!=50 THEN LET ERROR = "bas assignment error a!=50"  
50 LETS a, b, c = 10, ABS(-20) , 30
60 if a!=10 OR b!=20 OR c!=30 THEN LET ERROR = "LETS assignment error (abs) " + STR (a) + "/" + STR (b) + "/" + STR (c) 
