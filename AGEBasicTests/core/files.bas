10 let files = getFiles("C:\Users\curif\desarr\AgeOfJoy-2022.1\AGEBasicTests\core", ":", 1)
20 CLS
30 for f = 0 to countMembers(files, ":") - 1
40      print 0, 1 + mod(f, 15), getmember(files, f, ":")
50 next f
60 show
