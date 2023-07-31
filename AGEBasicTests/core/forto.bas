5 let last = ""
10 for y = 0 + 1 to 11 - 1
20      let last = last + ", " + str(y)
30 next y
40 if last != ", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10" then let error = "single loop failed"
50 if exists("error") then end

100 REM nested
110 let nested = ""
120 for j=0 to 5 ' external for
130    for h=0 to 3 ' nested
140         let nested = nested + ",[j=" + str(j) + ", h=" + str(h) +"]"
150    next h
160 next j

170 if nested != ",[j=0, h=0],[j=0, h=1],[j=0, h=2],[j=0, h=3],[j=1, h=0],[j=1, h=1],[j=1, h=2],[j=1, h=3],[j=2, h=0],[j=2, h=1],[j=2, h=2],[j=2, h=3],[j=3, h=0],[j=3, h=1],[j=3, h=2],[j=3, h=3],[j=4, h=0],[j=4, h=1],[j=4, h=2],[j=4, h=3],[j=5, h=0],[j=5, h=1],[j=5, h=2],[j=5, h=3]" then let ERROR = "nexted error"
180 if exists("error") then end 'finish when an error raise

200 REM STEP
210 let strstep = ""
220 for z = 1 - 1 to 100 / 10 step 1 + 1
230    let strstep = strstep + "," + str(z)
240 next z
250 if strstep != ",0,2,4,6,8,10" then let error = "step fails"


1800 END
