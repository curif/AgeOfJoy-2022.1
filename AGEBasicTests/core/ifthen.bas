10 let a = 0 
20 if a = 0 then let msg = "A ok"

30 let b = 0
40 if b = 1 
   then let msg2 = "B NOT ok" 
   else let msg2 = "B is OK"

50 let c = 0
60 let msg3 = IIF (c = 0, 
                   "C is ok", 
                   "C is wrong")

70 if b = 1 
   then let msg3 = "B NOT ok" 
   else if b = 3 
        then let msg3 = "B is NOT OK (2)" 
        else let msg3 = "OK"

