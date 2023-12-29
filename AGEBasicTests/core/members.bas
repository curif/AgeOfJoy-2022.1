10 let a="AGE:of:joy"
20 if getmember(a,0, ":") != "AGE" then let error="age"
30 if getmember(a,1, ":") != "of" then let error="of"
40 if getmember(a,2, ":") != "joy" then let error="joy"
50 if exists("error") then end

90 CLS
100 for f = 0 to countMembers(a, ":") - 1
110   print 0, f, getmember(a, f, ":")
120 next f
130 show